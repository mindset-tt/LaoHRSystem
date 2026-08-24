using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class AccountingConfigDto
{
    public bool IsConfigured { get; set; }
    public int? ApControlAccountId { get; set; }
    public int? DefaultExpenseAccountId { get; set; }
    public int? CashAccountId { get; set; }
    public int? EmployeePayableAccountId { get; set; }
}

public class SetAccountingConfigRequest
{
    public int? ApControlAccountId { get; set; }
    public int? DefaultExpenseAccountId { get; set; }
    public int? CashAccountId { get; set; }
    public int? EmployeePayableAccountId { get; set; }
}

[Authorize]
[ApiController]
[Route("api/finance/settings")]
public class FinanceSettingsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;
    private readonly IAccountingConfigurationService _config;

    public FinanceSettingsController(
        LaoHRDbContext context,
        IFinanceAccessService access,
        IAccountingConfigurationService config)
    {
        _context = context;
        _access = access;
        _config = config;
    }

    [HttpGet]
    public async Task<ActionResult<AccountingConfigDto>> GetConfig()
    {
        if (!_access.CanManageFinance())
            return Forbid();

        var dto = new AccountingConfigDto
        {
            IsConfigured = await _config.IsConfiguredAsync(),
            ApControlAccountId = await GetSettingIntAsync("AP_CONTROL_ACCOUNT"),
            DefaultExpenseAccountId = await GetSettingIntAsync("DEFAULT_EXPENSE_ACCOUNT"),
            CashAccountId = await GetSettingIntAsync("CASH_ACCOUNT"),
            EmployeePayableAccountId = await GetSettingIntAsync("EMPLOYEE_PAYABLE_ACCOUNT"),
        };
        return dto;
    }

    [HttpPut]
    public async Task<IActionResult> SetConfig([FromBody] SetAccountingConfigRequest request)
    {
        if (!_access.CanManageFinance())
            return Forbid();

        if (request.ApControlAccountId.HasValue)
            await SetSettingAsync("AP_CONTROL_ACCOUNT", request.ApControlAccountId.Value.ToString());
        if (request.DefaultExpenseAccountId.HasValue)
            await SetSettingAsync("DEFAULT_EXPENSE_ACCOUNT", request.DefaultExpenseAccountId.Value.ToString());
        if (request.CashAccountId.HasValue)
            await SetSettingAsync("CASH_ACCOUNT", request.CashAccountId.Value.ToString());
        if (request.EmployeePayableAccountId.HasValue)
            await SetSettingAsync("EMPLOYEE_PAYABLE_ACCOUNT", request.EmployeePayableAccountId.Value.ToString());

        return NoContent();
    }

    private async Task<int?> GetSettingIntAsync(string key)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);
        return setting != null && int.TryParse(setting.SettingValue, out var v) ? v : null;
    }

    private async Task SetSettingAsync(string key, string value)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);
        if (setting == null)
        {
            _context.SystemSettings.Add(new SystemSetting { SettingKey = key, SettingValue = value });
        }
        else
        {
            setting.SettingValue = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
    }
}
