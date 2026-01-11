using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Data;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Services;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LicenseController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    // We instantiate LicenseService manually or inject it. It's stateless so manual is fine.
    private readonly LicenseService _licenseService = new LicenseService();

    public LicenseController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var setting = await _context.SystemSettings.FindAsync("LICENSE_KEY");
        if (setting == null || string.IsNullOrEmpty(setting.SettingValue))
        {
            return Ok(new { status = "NoLicense" });
        }

        var data = _licenseService.VerifyLicense(setting.SettingValue);
        if (data == null)
        {
             return Ok(new { status = "Invalid" });
        }

        return Ok(new 
        { 
            status = "Active",
            customer = data.CustomerName,
            expires = data.ExpirationDate,
            type = data.Type,
            maxEmployees = data.MaxEmployees
        });
    }

    [HttpPost("activate")]
    public async Task<IActionResult> Activate([FromBody] ActivateRequest request)
    {
        var data = _licenseService.VerifyLicense(request.Key);
        if (data == null)
        {
            return BadRequest("Invalid License Key or Signature.");
        }

        if (data.ExpirationDate < DateTime.UtcNow)
        {
            return BadRequest("License has expired.");
        }

        var setting = await _context.SystemSettings.FindAsync("LICENSE_KEY");
        if (setting == null)
        {
            setting = new SystemSetting { SettingKey = "LICENSE_KEY", SettingValue = request.Key };
            _context.SystemSettings.Add(setting);
        }
        else
        {
            setting.SettingValue = request.Key;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "License Activated Successfully", customer = data.CustomerName });
    }
}

public class ActivateRequest
{
    public string Key { get; set; } = "";
}
