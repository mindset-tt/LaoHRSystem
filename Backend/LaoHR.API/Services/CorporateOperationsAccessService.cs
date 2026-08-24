using Microsoft.AspNetCore.Http;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4C — Corporate Operations authorization (capability-based).
///
/// Maps roles to corporate-operations capabilities in one place. Admin is a
/// superset; operational users (HR, Finance) gain specific capabilities without
/// needing system Admin. Ordinary Employees get self-service capabilities only.
///
/// Roles:
///   Admin    → full corporate operations (superset).
///   HR       → facilities, fleet, travel, service desk, contracts (operational).
///   Finance  → contracts (view/manage), corporate reports.
///   Employee → self-service (book rooms/vehicles, create travel/service requests,
///              view own records).
/// </summary>
public interface ICorporateOperationsAccessService
{
    bool IsAdmin();
    bool CanViewDocuments();
    bool CanManageDocuments();
    bool CanViewContracts();
    bool CanManageContracts();
    bool CanApproveContracts();
    bool CanManageServiceDesk();
    bool CanViewFacilities();
    bool CanManageFacilities();
    bool CanBookRooms();
    bool CanManageWorkOrders();
    bool CanViewFleet();
    bool CanManageFleet();
    bool CanBookVehicles();
    bool CanCreateTravel();
    bool CanApproveTravel();
    bool CanManageTravel();
    bool CanManageVisitors();
    bool CanViewCorporateReports();
}

public sealed class CorporateOperationsAccessService : ICorporateOperationsAccessService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorporateOperationsAccessService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private bool IsInRole(string role)
        => _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;

    public bool IsAdmin() => IsInRole("Admin");
    private bool IsHr() => IsInRole("HR");
    private bool IsFinance() => IsInRole("Finance");

    // Documents: Admin/HR/Finance manage; Employee can view own (enforced at controller).
    public bool CanViewDocuments() => IsAdmin() || IsHr() || IsFinance();
    public bool CanManageDocuments() => IsAdmin() || IsHr() || IsFinance();

    // Contracts: Admin/HR/Finance view+manage; approve is Admin/HR.
    public bool CanViewContracts() => IsAdmin() || IsHr() || IsFinance();
    public bool CanManageContracts() => IsAdmin() || IsHr() || IsFinance();
    public bool CanApproveContracts() => IsAdmin() || IsHr();

    // Service desk: Admin/HR manage; Employee self-service (controller-scoped).
    public bool CanManageServiceDesk() => IsAdmin() || IsHr();

    // Facilities: Admin/HR manage; broad read for Admin/HR/Finance.
    public bool CanViewFacilities() => IsAdmin() || IsHr() || IsFinance();
    public bool CanManageFacilities() => IsAdmin() || IsHr();

    // Rooms: any authenticated employee can book (self-service).
    public bool CanBookRooms() => true;

    // Work orders: Admin/HR manage.
    public bool CanManageWorkOrders() => IsAdmin() || IsHr();

    // Fleet: Admin/HR manage; Finance can view.
    public bool CanViewFleet() => IsAdmin() || IsHr() || IsFinance();
    public bool CanManageFleet() => IsAdmin() || IsHr();

    // Vehicles: any authenticated employee can book (self-service).
    public bool CanBookVehicles() => true;

    // Travel: any employee can create; Admin/HR approve; Admin/HR/Finance manage.
    public bool CanCreateTravel() => true;
    public bool CanApproveTravel() => IsAdmin() || IsHr();
    public bool CanManageTravel() => IsAdmin() || IsHr() || IsFinance();

    // Visitors: Admin/HR manage (reception/front desk).
    public bool CanManageVisitors() => IsAdmin() || IsHr();

    // Reports: Admin/HR/Finance.
    public bool CanViewCorporateReports() => IsAdmin() || IsHr() || IsFinance();
}
