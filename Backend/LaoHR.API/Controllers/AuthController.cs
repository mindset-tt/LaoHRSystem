using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly LaoHRDbContext _context;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly IHostEnvironment _environment;

    public AuthController(
        IConfiguration configuration,
        LaoHRDbContext context,
        IRefreshTokenService refreshTokens,
        IHostEnvironment environment)
    {
        _configuration = configuration;
        _context = context;
        _refreshTokens = refreshTokens;
        _environment = environment;
    }

    /// <summary>
    /// Login and get JWT + refresh token. Public endpoint.
    /// Demo users are seeded once at startup by DbSeeder — never as a side
    /// effect of an authentication request.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid username or password" });

        // Phase 3A — password hash migration: verify with version awareness, rehash if legacy
        var (isValid, needsRehash) = PasswordHasher.VerifyPasswordWithMigration(
            request.Password, user.PasswordHash, user.PasswordHashVersion);

        if (!isValid)
            return Unauthorized(new { message = "Invalid username or password" });

        // Migrate from SHA-256 (v1) to PBKDF2 (v2) on successful legacy login
        if (needsRehash)
        {
            user.PasswordHash = PasswordHasher.HashPassword(request.Password);
            user.PasswordHashVersion = 2;
        }

        // Update Last Login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var displayName = user.DisplayName ?? user.Employee?.EnglishName ?? user.Username;
        var accessToken = GenerateJwtToken(user.Username, user.Role, displayName, user.EmployeeId, user.UserId);
        var refresh = await _refreshTokens.IssueAsync(user.UserId, HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(new LoginResponse
        {
            Token = accessToken,
            RefreshToken = refresh.RawToken,
            Username = user.Username,
            Role = user.Role,
            DisplayName = displayName,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            RefreshExpiresAt = refresh.ExpiresAt,
        });
    }

    /// <summary>
    /// Get current user info from token
    /// </summary>
    [HttpGet("me")]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var name = User.FindFirst("DisplayName")?.Value;

        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        return Ok(new UserInfo
        {
            Username = username,
            Role = role ?? "Employee",
            DisplayName = name ?? username
        });
    }

    /// <summary>
    /// Phase 6c — Rotate a refresh token.
    /// The presented refresh token is revoked and a fresh access + refresh
    /// pair is returned. On replay detection, the entire user-issued family
    /// is revoked.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth-refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] RefreshRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.RefreshToken))
            return Unauthorized(new { message = "Missing refresh token" });

        var result = await _refreshTokens.RotateAsync(
            request.RefreshToken,
            HttpContext.Connection.RemoteIpAddress?.ToString());

        if (!result.Success || result.NewToken == null)
        {
            return Unauthorized(new { message = result.FailureReason ?? "refresh_failed" });
        }

        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.UserId == result.UserId);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "user_inactive" });

        var displayName = user.DisplayName ?? user.Employee?.EnglishName ?? user.Username;
        var accessToken = GenerateJwtToken(user.Username, user.Role, displayName, user.EmployeeId, user.UserId);

        return Ok(new LoginResponse
        {
            Token = accessToken,
            RefreshToken = result.NewToken.RawToken,
            Username = user.Username,
            Role = user.Role,
            DisplayName = displayName,
            ExpiresAt = DateTime.UtcNow.AddHours(8),
            RefreshExpiresAt = result.NewToken.ExpiresAt,
        });
    }

    /// <summary>
    /// Phase 6c — Logout: revoke the presented refresh token (single device).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request)
    {
        if (!string.IsNullOrWhiteSpace(request?.RefreshToken))
        {
            await _refreshTokens.RevokeAsync(request.RefreshToken, "logout");
        }
        return NoContent();
    }

    /// <summary>
    /// Phase 6c — Logout-everywhere: revoke all refresh tokens for the
    /// current user. Useful when an account is suspected of compromise.
    /// </summary>
    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var userIdClaim = User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(userIdClaim, out var employeeId))
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
            if (user != null)
            {
                await _refreshTokens.RevokeAllForUserAsync(user.UserId, "logout_all");
            }
        }
        return NoContent();
    }

    private string GenerateJwtToken(string username, string role, string displayName, int? employeeId, int? userId = null)
    {
        // Phase 4D — no silent fallback key in non-Development. Program.cs already
        // fails fast at startup, but this is defense-in-depth: if the key is ever
        // missing here in Production, throw rather than sign with a known key.
        var configuredKey = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            if (_environment.IsProduction() || _environment.IsStaging())
                throw new InvalidOperationException("JWT signing key is missing in Production/Staging.");
            configuredKey = "LaoHRSystemSecretKey2024VeryLongKeyForSecurity!";
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuredKey));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("DisplayName", displayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (userId.HasValue)
        {
            claims.Add(new Claim("UserId", userId.Value.ToString()));
        }

        if (employeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", employeeId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "LaoHRSystem",
            audience: _configuration["Jwt:Audience"] ?? "LaoHRFrontend",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    /// <summary>Server-issued, single-use refresh token. Phase 6c.</summary>
    public string? RefreshToken { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RefreshExpiresAt { get; set; }
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LogoutRequest
{
    public string? RefreshToken { get; set; }
}

public class UserInfo
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
