using LaoHR.API.Services;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/company-settings")]
[Authorize] // Phase 3A — explicit auth (fallback policy covers, but be explicit for clarity)
public class CompanySettingsController : ControllerBase
{
    private readonly ICompanySettingsService _service;

    public CompanySettingsController(ICompanySettingsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSettings()
    {
        var settings = await _service.GetSettingsAsync();
        return Ok(settings ?? new CompanySetting());
    }

    [HttpPut]
    [Authorize(Roles = "Admin,HR")] // Phase 3A — only Admin/HR can change company settings
    public async Task<IActionResult> UpdateSettings([FromBody] CompanySetting settings)
    {
        var updated = await _service.UpdateSettingsAsync(settings);
        return Ok(updated);
    }
}
