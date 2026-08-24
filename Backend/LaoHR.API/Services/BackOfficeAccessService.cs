using Microsoft.AspNetCore.Http;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4A — Back Office authorization. Centralizes module-level access checks
/// for the new procurement/inventory/asset/finance/contract domains.
///
/// The platform currently has three roles (Admin, HR, Employee). Rather than
/// scatter role checks across controllers, this service maps roles to
/// back-office capabilities in one place. It is intentionally lightweight —
/// no separate permission table — but keeps domain boundaries explicit so a
/// future permission model can replace it without touching controllers.
///
/// Mapping:
///   Admin    → full back-office access (all modules, all actions).
///   HR       → operational modules (procurement, inventory, assets, contracts,
///              service requests) but NOT finance/payments.
///   Employee → self-service only (create requests, view own records).
/// </summary>
public interface IBackOfficeAccessService
{
    bool IsAdmin();
    bool IsHr();
    bool CanViewProcurement();
    bool CanManageProcurement();
    bool CanApproveProcurement();
    bool CanViewFinance();
    bool CanManageFinance();
    bool CanViewInventory();
    bool CanManageInventory();
    bool CanAdjustInventory();
    bool CanViewAssets();
    bool CanManageAssets();
    bool CanAssignAssets();
    bool CanViewContracts();
    bool CanManageContracts();
    bool CanManageServiceRequests();

    /// <summary>True if the user may see supplier bank/tax sensitive fields (Admin/Finance only).</summary>
    bool CanViewSupplierSensitive();
}

public sealed class BackOfficeAccessService : IBackOfficeAccessService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BackOfficeAccessService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsInRole(string role)
    {
        return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
    }

    public bool IsAdmin() => IsInRole("Admin");
    public bool IsHr() => IsInRole("HR");

    // Procurement: Admin + HR manage; Employee can create (self-service).
    public bool CanViewProcurement() => IsAdmin() || IsHr();
    public bool CanManageProcurement() => IsAdmin() || IsHr();
    public bool CanApproveProcurement() => IsAdmin() || IsHr();

    // Finance: Admin only (payments, AP, budgets are sensitive).
    public bool CanViewFinance() => IsAdmin();
    public bool CanManageFinance() => IsAdmin();

    // Inventory: Admin + HR.
    public bool CanViewInventory() => IsAdmin() || IsHr();
    public bool CanManageInventory() => IsAdmin() || IsHr();
    public bool CanAdjustInventory() => IsAdmin() || IsHr();

    // Assets: Admin + HR.
    public bool CanViewAssets() => IsAdmin() || IsHr();
    public bool CanManageAssets() => IsAdmin() || IsHr();
    public bool CanAssignAssets() => IsAdmin() || IsHr();

    // Contracts: Admin + HR.
    public bool CanViewContracts() => IsAdmin() || IsHr();
    public bool CanManageContracts() => IsAdmin() || IsHr();

    // Service requests: Admin + HR manage; Employee can create (self-service).
    public bool CanManageServiceRequests() => IsAdmin() || IsHr();

    // Supplier bank/tax sensitive fields: Admin only (Finance). HR does NOT
    // automatically gain supplier banking access — that is a Finance capability.
    public bool CanViewSupplierSensitive() => IsAdmin();
}
