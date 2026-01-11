using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    
    // In production, use a proper user store/database
    private static readonly Dictionary<string, (string Password, string Role, string Name)> Users = new()
    {
        { "admin", ("admin123", "Admin", "System Administrator") },
        { "hr", ("hr123", "HR", "HR Manager") },
        { "employee", ("emp123", "Employee", "Employee User") }
    };
    
    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    /// <summary>
    /// Login and get JWT token
    /// </summary>
    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        // Validate credentials
        if (!Users.TryGetValue(request.Username.ToLower(), out var user))
            return Unauthorized(new { message = "Invalid username or password" });
        
        if (user.Password != request.Password)
            return Unauthorized(new { message = "Invalid username or password" });
        
        // Generate JWT token
        var token = GenerateJwtToken(request.Username, user.Role, user.Name);
        
        return Ok(new LoginResponse
        {
            Token = token,
            Username = request.Username,
            Role = user.Role,
            DisplayName = user.Name,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }
    
    /// <summary>
    /// Get current user info from token
    /// </summary>
    [HttpGet("me")]
    public ActionResult<UserInfo> GetCurrentUser()
    {
        // Get user from JWT claims
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
    /// Refresh token (placeholder)
    /// </summary>
    [HttpPost("refresh")]
    public ActionResult<LoginResponse> RefreshToken()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
        var name = User.FindFirst("DisplayName")?.Value ?? username;
        
        if (string.IsNullOrEmpty(username))
            return Unauthorized();
        
        var token = GenerateJwtToken(username, role, name!);
        
        return Ok(new LoginResponse
        {
            Token = token,
            Username = username,
            Role = role,
            DisplayName = name!,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }
    
    private string GenerateJwtToken(string username, string role, string displayName)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "LaoHRSystemSecretKey2024VeryLongKeyForSecurity!")
        );
        
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("DisplayName", displayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
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
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserInfo
{
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
