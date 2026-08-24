using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4B.1 — accounting configuration. Auto-posting must not guess GL
/// accounts. Mandatory mappings are stored as SystemSettings (key/value) and
/// validated before any auto-posting.
///
/// ACCOUNTING_CONFIGURED is true only when all mandatory mappings exist.
/// </summary>
public interface IAccountingConfigurationService
{
    /// <summary>True when all mandatory account mappings exist.</summary>
    Task<bool> IsConfiguredAsync(CancellationToken ct = default);

    /// <summary>Returns the AP control account id (throws if missing).</summary>
    Task<int> GetApControlAccountAsync(CancellationToken ct = default);

    /// <summary>Returns the default expense account id (throws if missing).</summary>
    Task<int> GetDefaultExpenseAccountAsync(CancellationToken ct = default);

    /// <summary>Returns the cash/bank control account id (throws if missing).</summary>
    Task<int> GetCashAccountAsync(CancellationToken ct = default);

    /// <summary>Returns the employee payable account id (throws if missing).</summary>
    Task<int> GetEmployeePayableAccountAsync(CancellationToken ct = default);

    /// <summary>Returns the open fiscal period id for a posting date (throws if none).</summary>
    Task<int> GetOpenPeriodAsync(DateTime postingDate, CancellationToken ct = default);
}

public sealed class AccountingConfigurationService : IAccountingConfigurationService
{
    private readonly LaoHRDbContext _context;

    public AccountingConfigurationService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsConfiguredAsync(CancellationToken ct = default)
    {
        var keys = new[] { "AP_CONTROL_ACCOUNT", "DEFAULT_EXPENSE_ACCOUNT", "CASH_ACCOUNT", "EMPLOYEE_PAYABLE_ACCOUNT" };
        var settings = await _context.SystemSettings
            .Where(s => keys.Contains(s.SettingKey))
            .Select(s => s.SettingKey)
            .ToListAsync(ct);
        return keys.All(k => settings.Contains(k));
    }

    public async Task<int> GetApControlAccountAsync(CancellationToken ct = default)
        => await GetAccountIdAsync("AP_CONTROL_ACCOUNT", ct);

    public async Task<int> GetDefaultExpenseAccountAsync(CancellationToken ct = default)
        => await GetAccountIdAsync("DEFAULT_EXPENSE_ACCOUNT", ct);

    public async Task<int> GetCashAccountAsync(CancellationToken ct = default)
        => await GetAccountIdAsync("CASH_ACCOUNT", ct);

    public async Task<int> GetEmployeePayableAccountAsync(CancellationToken ct = default)
        => await GetAccountIdAsync("EMPLOYEE_PAYABLE_ACCOUNT", ct);

    public async Task<int> GetOpenPeriodAsync(DateTime postingDate, CancellationToken ct = default)
    {
        var period = await _context.FiscalPeriods
            .Where(p => p.Status == "OPEN" && p.StartDate <= postingDate && p.EndDate >= postingDate)
            .OrderBy(p => p.PeriodNumber)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException(
                "No OPEN fiscal period covers the posting date. Configure fiscal periods before posting.");

        return period.FiscalPeriodId;
    }

    private async Task<int> GetAccountIdAsync(string key, CancellationToken ct)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key, ct);
        if (setting == null || !int.TryParse(setting.SettingValue, out var accountId))
            throw new InvalidOperationException(
                $"Accounting configuration '{key}' is missing. Configure control accounts before posting.");

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId && a.IsPostingAccount, ct)
            ?? throw new InvalidOperationException(
                $"Accounting configuration '{key}' references a non-posting or missing account.");

        return accountId;
    }
}
