using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class PurchaseRequestItemDto
{
    public int PurchaseRequestItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal EstimatedAmount { get; set; }
    public int? PreferredSupplierId { get; set; }
    public string? Notes { get; set; }
}

public class PurchaseRequestListItem
{
    public int PurchaseRequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int RequestedByEmployeeId { get; set; }
    public string? RequestedByName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Purpose { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalEstimatedAmount { get; set; }
    public string Currency { get; set; } = "LAK";
    public DateTime CreatedAt { get; set; }
}

public class PurchaseRequestDetail
{
    public int PurchaseRequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int RequestedByEmployeeId { get; set; }
    public string? RequestedByName { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
    public int? CostCenterId { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Purpose { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalEstimatedAmount { get; set; }
    public string Currency { get; set; } = "LAK";
    public DateTime CreatedAt { get; set; }
    public List<PurchaseRequestItemDto> Items { get; set; } = new();
}

public class CreatePurchaseRequestItem
{
    public string Description { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public int? PreferredSupplierId { get; set; }
    public string? Notes { get; set; }
}

public class CreatePurchaseRequestRequest
{
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
    public int? CostCenterId { get; set; }
    public int? BudgetId { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string? Purpose { get; set; }
    public string Currency { get; set; } = "LAK";
    public List<CreatePurchaseRequestItem> Items { get; set; } = new();
}

[Authorize]
[ApiController]
[Route("api/purchase-requests")]
public class PurchaseRequestsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IApprovalService _approval;
    private readonly INotificationService _notifications;
    private readonly INumberSequenceService _numbers;
    private readonly IBudgetService _budget;

    public PurchaseRequestsController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        ICurrentEmployeeService currentEmployee,
        IApprovalService approval,
        INotificationService notifications,
        INumberSequenceService numbers,
        IBudgetService budget)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _approval = approval;
        _notifications = notifications;
        _numbers = numbers;
        _budget = budget;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PurchaseRequestListItem>>> GetRequests(
        [FromQuery] string? status = null,
        [FromQuery] bool mineOnly = false,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.PurchaseRequests.AsNoTracking().AsQueryable();

        // Employees see only their own requests; Admin/HR see all.
        if (!_access.CanViewProcurement())
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId == null) return Forbid();
            query = query.Where(p => p.RequestedByEmployeeId == empId.Value);
        }
        else if (mineOnly)
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId.HasValue)
                query = query.Where(p => p.RequestedByEmployeeId == empId.Value);
        }

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.RequestNumber.ToLower().Contains(s) ||
                                     (p.Purpose != null && p.Purpose.ToLower().Contains(s)));
        }

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PurchaseRequestListItem
            {
                PurchaseRequestId = p.PurchaseRequestId,
                RequestNumber = p.RequestNumber,
                RequestedByEmployeeId = p.RequestedByEmployeeId,
                RequestedByName = p.RequestedBy != null ? (p.RequestedBy.EnglishName ?? p.RequestedBy.LaoName) : null,
                DepartmentId = p.DepartmentId,
                DepartmentName = p.Department != null ? p.Department.DepartmentName : null,
                RequiredDate = p.RequiredDate,
                Purpose = p.Purpose,
                Status = p.Status,
                TotalEstimatedAmount = p.TotalEstimatedAmount,
                Currency = p.Currency,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<PurchaseRequestListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PurchaseRequestDetail>> GetRequest(int id)
    {
        var p = await _context.PurchaseRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PurchaseRequestId == id);
        if (p == null) return NotFound();

        // IDOR: employees may only view their own requests.
        if (!_access.CanViewProcurement())
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId != p.RequestedByEmployeeId) return Forbid();
        }

        var items = await _context.PurchaseRequestItems.AsNoTracking()
            .Where(i => i.PurchaseRequestId == id)
            .Select(i => new PurchaseRequestItemDto
            {
                PurchaseRequestItemId = i.PurchaseRequestItemId,
                Description = i.Description,
                ItemId = i.ItemId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                EstimatedUnitPrice = i.EstimatedUnitPrice,
                EstimatedAmount = i.EstimatedAmount,
                PreferredSupplierId = i.PreferredSupplierId,
                Notes = i.Notes,
            })
            .ToListAsync();

        var requestedByName = await _context.Employees
            .Where(e => e.EmployeeId == p.RequestedByEmployeeId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        return new PurchaseRequestDetail
        {
            PurchaseRequestId = p.PurchaseRequestId,
            RequestNumber = p.RequestNumber,
            RequestedByEmployeeId = p.RequestedByEmployeeId,
            RequestedByName = requestedByName,
            DepartmentId = p.DepartmentId,
            ProjectId = p.ProjectId,
            CostCenterId = p.CostCenterId,
            RequiredDate = p.RequiredDate,
            Purpose = p.Purpose,
            Status = p.Status,
            TotalEstimatedAmount = p.TotalEstimatedAmount,
            Currency = p.Currency,
            CreatedAt = p.CreatedAt,
            Items = items,
        };
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseRequest>> CreateRequest([FromBody] CreatePurchaseRequestRequest request)
    {
        var requesterEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (requesterEmployeeId == null)
            return Unauthorized("No linked employee profile.");

        if (request.Items == null || request.Items.Count == 0)
            return BadRequest("At least one item is required.");

        var number = await _numbers.NextAsync("PR");

        var total = request.Items.Sum(i => i.Quantity * i.EstimatedUnitPrice);

        var pr = new PurchaseRequest
        {
            RequestNumber = number,
            RequestedByEmployeeId = requesterEmployeeId.Value,
            DepartmentId = request.DepartmentId,
            ProjectId = request.ProjectId,
            CostCenterId = request.CostCenterId,
            BudgetId = request.BudgetId,
            RequiredDate = request.RequiredDate,
            Purpose = request.Purpose,
            Status = "DRAFT",
            TotalEstimatedAmount = total,
            Currency = request.Currency,
        };

        foreach (var item in request.Items)
        {
            pr.Items.Add(new PurchaseRequestItem
            {
                Description = item.Description,
                ItemId = item.ItemId,
                Quantity = item.Quantity,
                Unit = item.Unit,
                EstimatedUnitPrice = item.EstimatedUnitPrice,
                EstimatedAmount = item.Quantity * item.EstimatedUnitPrice,
                PreferredSupplierId = item.PreferredSupplierId,
                Notes = item.Notes,
            });
        }

        _context.PurchaseRequests.Add(pr);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRequest), new { id = pr.PurchaseRequestId }, pr);
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var requesterEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (requesterEmployeeId == null) return Unauthorized("No linked employee profile.");

        var pr = await _context.PurchaseRequests
            .Include(p => p.Items)
            .FirstOrDefaultAsync(x => x.PurchaseRequestId == id);
        if (pr == null) return NotFound();

        if (pr.RequestedByEmployeeId != requesterEmployeeId.Value)
            return Forbid();

        if (pr.Status != "DRAFT")
            return BadRequest($"Cannot submit a request in '{pr.Status}' state.");

        pr.Status = "PENDING_APPROVAL";
        pr.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Approval routing: direct manager first, then Finance (Admin) for larger amounts.
        var requester = await _context.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == requesterEmployeeId.Value);
        var steps = new List<ApprovalStepDefinition>();
        if (requester?.ManagerId != null)
            steps.Add(new ApprovalStepDefinition { ResolverType = "DIRECT_MANAGER" });
        else
            steps.Add(new ApprovalStepDefinition { ResolverType = "ROLE", RoleName = "HR" });
        steps.Add(new ApprovalStepDefinition { ResolverType = "ROLE", RoleName = "Admin" });

        await _approval.CreateRequestAsync("PURCHASE_REQUEST", pr.PurchaseRequestId, requesterEmployeeId.Value, steps);

        if (requester?.ManagerId != null)
        {
            await _notifications.NotifyEmployeeAsync(
                requester.ManagerId.Value,
                "APPROVAL_REQUESTED",
                "New purchase request",
                $"{requester.EnglishName ?? requester.LaoName} submitted purchase request {pr.RequestNumber}.",
                "PURCHASE_REQUEST",
                pr.PurchaseRequestId);
        }

        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveActionRequest? body = null)
    {
        return await SetApprovalState(id, "APPROVED", body?.Notes);
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApproveActionRequest? body = null)
    {
        return await SetApprovalState(id, "REJECTED", body?.Notes);
    }

    private async Task<IActionResult> SetApprovalState(int id, string newStatus, string? notes)
    {
        if (!_access.CanApproveProcurement())
            return Forbid();

        var actorEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (actorEmployeeId == null) return Unauthorized("No linked employee profile.");

        var pr = await _context.PurchaseRequests.FirstOrDefaultAsync(x => x.PurchaseRequestId == id);
        if (pr == null) return NotFound();
        if (pr.Status != "PENDING_APPROVAL")
            return BadRequest($"Only PENDING_APPROVAL requests can be {newStatus.ToLower()}.");

        var approval = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.RequestType == "PURCHASE_REQUEST" && r.EntityId == id && r.Status == "PENDING");
        if (approval == null)
            return BadRequest("No pending approval for this request.");

        try
        {
            if (newStatus == "APPROVED")
            {
                var result = await _approval.ApproveAsync(approval.ApprovalRequestId, actorEmployeeId.Value, notes);
                if (result.Status == "APPROVED")
                {
                    // Budget enforcement: reserve against the linked budget (if any).
                    if (pr.BudgetId.HasValue)
                    {
                        await _budget.ReserveAsync(pr.BudgetId.Value, pr.TotalEstimatedAmount);
                    }

                    pr.Status = "APPROVED";
                    pr.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await _notifications.NotifyEmployeeAsync(
                        pr.RequestedByEmployeeId, "APPROVAL_APPROVED",
                        "Purchase request approved",
                        $"Your purchase request {pr.RequestNumber} was approved.",
                        "PURCHASE_REQUEST", pr.PurchaseRequestId);
                }
            }
            else
            {
                await _approval.RejectAsync(approval.ApprovalRequestId, actorEmployeeId.Value, notes);
                pr.Status = "REJECTED";
                pr.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await _notifications.NotifyEmployeeAsync(
                    pr.RequestedByEmployeeId, "APPROVAL_REJECTED",
                    "Purchase request rejected",
                    $"Your purchase request {pr.RequestNumber} was rejected.",
                    "PURCHASE_REQUEST", pr.PurchaseRequestId);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        return NoContent();
    }
}
