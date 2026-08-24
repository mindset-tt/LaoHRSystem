using Microsoft.AspNetCore.Http;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4B.1 — Finance authorization (capability-based).
///
/// System Administrator ≠ Accountant. Finance access is granted via the
/// "Finance" role (or "Admin" as a superset), NOT by making every accountant a
/// global Admin. HR, Procurement, and Warehouse do NOT gain finance access.
///
/// Roles:
///   Admin   → full system + finance access (superset).
///   Finance → finance/accounting capabilities only.
///   HR      → NO finance access (HR ≠ Finance).
///   Employee → no finance access.
/// </summary>
public interface IFinanceAccessService
{
    bool IsFinance();
    bool CanViewFinance();
    bool CanManageFinance();
    bool CanViewAp();
    bool CanCreateAp();
    bool CanApproveAp();
    bool CanViewPayments();
    bool CanCreatePayments();
    bool CanApprovePayments();
    bool CanViewAccounting();
    bool CanPostAccounting();
    bool CanViewCoa();
    bool CanManageCoa();
    bool CanManagePeriods();
    bool CanViewAr();
    bool CanManageAr();
    bool CanViewBankAccounts();
    bool CanManageBankAccounts();
    bool CanViewFinanceReports();
    bool CanExportFinance();
}

public sealed class FinanceAccessService : IFinanceAccessService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FinanceAccessService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsInRole(string role)
        => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

    /// <summary>True for the Finance role OR Admin (superset).</summary>
    public bool IsFinance() => IsInRole("Finance") || IsInRole("Admin");

    public bool CanViewFinance() => IsFinance();
    public bool CanManageFinance() => IsFinance();

    public bool CanViewAp() => IsFinance();
    public bool CanCreateAp() => IsFinance();
    public bool CanApproveAp() => IsFinance();

    public bool CanViewPayments() => IsFinance();
    public bool CanCreatePayments() => IsFinance();
    public bool CanApprovePayments() => IsFinance();

    public bool CanViewAccounting() => IsFinance();
    public bool CanPostAccounting() => IsFinance();
    public bool CanViewCoa() => IsFinance();
    public bool CanManageCoa() => IsFinance();
    public bool CanManagePeriods() => IsFinance();

    public bool CanViewAr() => IsFinance();
    public bool CanManageAr() => IsFinance();

    public bool CanViewBankAccounts() => IsFinance();
    public bool CanManageBankAccounts() => IsFinance();

    public bool CanViewFinanceReports() => IsFinance();
    public bool CanExportFinance() => IsFinance();
}
