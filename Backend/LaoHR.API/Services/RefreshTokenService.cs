using System.Security.Cryptography;
using System.Text;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 6c — Server-side refresh tokens with rotation + revocation.
///
/// Design constraints (LIGHTWEIGHT + SCALABLE):
/// - No extra NuGet packages — SHA-256 via System.Security.Cryptography.
/// - One hash record per token; the raw token never leaves the response.
/// - Rotation on every use: a refresh token is single-use. Using it issues a
///   new pair (access + refresh) and revokes the old refresh token.
/// - Replay detection: if a revoked token is presented again, the *entire
///   family* (this token and any descendants) is revoked — the simplest
///   trustworthy defence against token theft.
/// </summary>
public interface IRefreshTokenService
{
    Task<IssuedRefreshToken> IssueAsync(int userId, string? ipAddress, CancellationToken ct = default);
    Task<RefreshRotationResult> RotateAsync(string presentedToken, string? ipAddress, CancellationToken ct = default);
    Task RevokeAsync(string presentedToken, string reason, CancellationToken ct = default);
    Task RevokeAllForUserAsync(int userId, string reason, CancellationToken ct = default);
}

public record IssuedRefreshToken(string RawToken, DateTime ExpiresAt);

public class RefreshRotationResult
{
    public bool Success { get; init; }
    public string? FailureReason { get; init; }
    public IssuedRefreshToken? NewToken { get; init; }
    public int UserId { get; init; }
    public bool FamilyRevoked { get; init; }
}

public class RefreshTokenService : IRefreshTokenService
{
    // Tokens live 14 days; this is configurable via app settings in a future
    // change. We keep the constant local to avoid leaking it as a setting
    // operators would shorten to "now" accidentally.
    public static readonly TimeSpan Lifetime = TimeSpan.FromDays(14);

    private readonly LaoHRDbContext _db;
    private readonly ILogger<RefreshTokenService> _log;

    public RefreshTokenService(LaoHRDbContext db, ILogger<RefreshTokenService> log)
    {
        _db = db;
        _log = log;
    }

    public async Task<IssuedRefreshToken> IssueAsync(int userId, string? ipAddress, CancellationToken ct = default)
    {
        var raw = GenerateRawToken();
        var hash = HashToken(raw);
        var now = DateTime.UtcNow;

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            TokenHash = hash,
            IssuedAt = now,
            ExpiresAt = now.Add(Lifetime),
            CreatedByIp = ipAddress,
        });
        await _db.SaveChangesAsync(ct);

        return new IssuedRefreshToken(raw, now.Add(Lifetime));
    }

    public async Task<RefreshRotationResult> RotateAsync(string presentedToken, string? ipAddress, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(presentedToken))
            return new RefreshRotationResult { Success = false, FailureReason = "missing_token" };

        var hash = HashToken(presentedToken);
        var stored = await _db.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);

        if (stored == null)
        {
            // Unknown token — could be from a different deployment or forged.
            // Fail closed. We do NOT attempt to find a "family" because we
            // don't track family membership at this layer.
            _log.LogWarning("Refresh token presented that does not exist");
            return new RefreshRotationResult { Success = false, FailureReason = "unknown_token" };
        }

        // Revoked token presented again = possible replay. Revoke the entire
        // user-issued family as a precaution.
        if (stored.RevokedAt != null)
        {
            _log.LogWarning("Refresh token replay detected for user {UserId}", stored.UserId);
            await RevokeAllForUserAsync(stored.UserId, "replay_detected", ct);
            return new RefreshRotationResult
            {
                Success = false,
                FailureReason = "replay_detected",
                UserId = stored.UserId,
                FamilyRevoked = true,
            };
        }

        if (stored.ExpiresAt <= DateTime.UtcNow)
        {
            // Stale but not replay — silent fail. Don't touch the family.
            return new RefreshRotationResult { Success = false, FailureReason = "expired", UserId = stored.UserId };
        }

        // Issue new token.
        var raw = GenerateRawToken();
        var newHash = HashToken(raw);
        var now = DateTime.UtcNow;

        // Mark old token as replaced.
        stored.RevokedAt = now;
        stored.RevokedReason = "rotated";
        stored.ReplacedByHash = newHash;

        _db.RefreshTokens.Add(new RefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newHash,
            IssuedAt = now,
            ExpiresAt = now.Add(Lifetime),
            CreatedByIp = ipAddress,
        });
        await _db.SaveChangesAsync(ct);

        return new RefreshRotationResult
        {
            Success = true,
            NewToken = new IssuedRefreshToken(raw, now.Add(Lifetime)),
            UserId = stored.UserId,
        };
    }

    public async Task RevokeAsync(string presentedToken, string reason, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(presentedToken)) return;
        var hash = HashToken(presentedToken);
        var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);
        if (stored == null || stored.RevokedAt != null) return;
        stored.RevokedAt = DateTime.UtcNow;
        stored.RevokedReason = reason;
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(int userId, string reason, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var tokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var t in tokens)
        {
            t.RevokedAt = now;
            t.RevokedReason = reason;
        }
        if (tokens.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
            _log.LogInformation("Revoked {Count} refresh tokens for user {UserId}: {Reason}", tokens.Count, userId, reason);
        }
    }

    // ---- helpers ----------------------------------------------------------

    private static string GenerateRawToken()
    {
        // 32 bytes = 256 bits of entropy → base64url → ~43 chars.
        Span<byte> buf = stackalloc byte[32];
        RandomNumberGenerator.Fill(buf);
        return Base64UrlEncode(buf);
    }

    internal static string HashToken(string raw)
    {
        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(Encoding.UTF8.GetBytes(raw), hash);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string Base64UrlEncode(ReadOnlySpan<byte> data)
    {
        var s = Convert.ToBase64String(data);
        return s.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
