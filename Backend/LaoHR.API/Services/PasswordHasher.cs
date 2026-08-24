using System.Security.Cryptography;
using System.Text;

namespace LaoHR.API.Services;

/// <summary>
/// Password hashing with migration support.
/// Version 1: Legacy SHA-256 (unsalted, single-pass) — used by pre-Phase-3A code.
/// Version 2: PBKDF2-HMAC-SHA256 (salted, 600,000 iterations) — OWASP recommended for FIPS.
/// 
/// Migration strategy: On login, if the stored hash is version 1 and verification succeeds,
/// re-hash with PBKDF2 and update the stored hash + version. New users always get version 2.
/// </summary>
public static class PasswordHasher
{
    private const int Pbkdf2Iterations = 600_000; // OWASP recommendation for HMAC-SHA256
    private const int SaltSize = 16; // 128-bit salt
    private const int HashSize = 32; // 256-bit hash

    /// <summary>
    /// Hash a password using the current algorithm (PBKDF2, version 2).
    /// Format: "v2:{base64salt}:{base64hash}"
    /// </summary>
    public static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"v2:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <summary>
    /// Verify a password against a stored hash. Supports both legacy (v1) and current (v2) formats.
    /// Returns (success, needsRehash).
    /// </summary>
    public static (bool isValid, bool needsRehash) VerifyPasswordWithMigration(string password, string storedHash, int hashVersion)
    {
        // Version 1: Legacy SHA-256 (unsalted)
        if (hashVersion == 1 || (!storedHash.StartsWith("v2:") && hashVersion != 2))
        {
            var legacyValid = VerifyLegacySha256(password, storedHash);
            return (legacyValid, legacyValid); // If valid, needs rehash to v2
        }

        // Version 2: PBKDF2
        if (storedHash.StartsWith("v2:"))
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 3) return (false, false);
            var salt = Convert.FromBase64String(parts[1]);
            var expectedHash = Convert.FromBase64String(parts[2]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256, HashSize);
            var valid = CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
            return (valid, false); // Already v2, no rehash needed
        }

        return (false, false);
    }

    /// <summary>
    /// Legacy verification for backward compatibility (SHA-256, unsalted).
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        // If the hash is in v2 format, use the new verifier
        if (hash.StartsWith("v2:"))
        {
            var (valid, _) = VerifyPasswordWithMigration(password, hash, 2);
            return valid;
        }
        // Fall back to legacy SHA-256
        return VerifyLegacySha256(password, hash);
    }

    private static bool VerifyLegacySha256(string password, string hash)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var computed = Convert.ToBase64String(bytes);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed), Encoding.UTF8.GetBytes(hash));
    }
}
