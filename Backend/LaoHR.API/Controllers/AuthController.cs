using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
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
    
    public AuthController(IConfiguration configuration, LaoHRDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }
    
    /// <summary>
    /// Login and get JWT token
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // Ensure default admin exists (Seeding Logic)
        await SeedDefaultAdminAsync();

        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Username == request.Username);
            
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid username or password" });
            
        if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
             return Unauthorized(new { message = "Invalid username or password" });
        
        // Update Last Login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        // Generate JWT token
        var displayName = user.DisplayName ?? user.Employee?.EnglishName ?? user.Username;
        var token = GenerateJwtToken(user.Username, user.Role, displayName, user.EmployeeId);
        
        return Ok(new LoginResponse
        {
            Token = token,
            Username = user.Username,
            Role = user.Role,
            DisplayName = displayName,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }

    private async Task SeedDefaultAdminAsync()
    {
        if (!await _context.Users.AnyAsync())
        {
            var admin = new AppUser
            {
                Username = "admin",
                PasswordHash = PasswordHasher.HashPassword("admin123"),
                Role = "Admin",
                DisplayName = "System Administrator",
                IsActive = true
            };
            _context.Users.Add(admin);
            
            var hr = new AppUser
            {
                Username = "hr",
                PasswordHash = PasswordHasher.HashPassword("hr123"),
                Role = "HR",
                DisplayName = "HR Manager",
                IsActive = true
            };
             _context.Users.Add(hr);
             
            await _context.SaveChangesAsync();
        }
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
    /// Refresh token (placeholder)
    /// </summary>
    [HttpPost("refresh")]
    public ActionResult<LoginResponse> RefreshToken()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
        var name = User.FindFirst("DisplayName")?.Value ?? username;
        var empIdStr = User.FindFirst("EmployeeId")?.Value;
        int? empId = string.IsNullOrEmpty(empIdStr) ? null : int.Parse(empIdStr);
        
        if (string.IsNullOrEmpty(username))
            return Unauthorized();
        
        var token = GenerateJwtToken(username, role, name!, empId);
        
        return Ok(new LoginResponse
        {
            Token = token,
            Username = username,
            Role = role,
            DisplayName = name!,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        });
    }
    
    private string GenerateJwtToken(string username, string role, string displayName, int? employeeId)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "LaoHRSystemSecretKey2024VeryLongKeyForSecurity!")
        );
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role),
            new Claim("DisplayName", displayName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
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
