using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public SettingsController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _context.SystemSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
        return Ok(settings);
    }

    [HttpPut("{key}")]
    public async Task<IActionResult> UpdateSetting(string key, [FromBody] UpdateSettingDto dto)
    {
        var setting = await _context.SystemSettings.FindAsync(key);
        if (setting == null)
        {
            setting = new SystemSetting { SettingKey = key, SettingValue = dto.Value };
            _context.SystemSettings.Add(setting);
        }
        else
        {
            setting.SettingValue = dto.Value;
        }

        await _context.SaveChangesAsync();
        return Ok(setting);
    }
}

public class UpdateSettingDto
{
    public string Value { get; set; } = string.Empty;
}
