using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class TravelRequestDto
{
    public int TravelRequestId { get; set; }
    public string TravelNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Currency { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateTravelRequestRequest
{
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Currency { get; set; }
}

[Authorize]
[ApiController]
[Route("api/travel")]
public class TravelController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICorporateOperationsAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly INumberSequenceService _numbers;
    private readonly IApprovalService _approval;
    private readonly INotificationService _notifications;

    public TravelController(
        LaoHRDbContext context,
        ICorporateOperationsAccessService access,
        ICurrentEmployeeService currentEmployee,
        INumberSequenceService numbers,
        IApprovalService approval,
        INotificationService notifications)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _numbers = numbers;
        _approval = approval;
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<TravelRequestDto>>> GetTravelRequests(
        [FromQuery] string? status = null,
        [FromQuery] bool mineOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.TravelRequests.AsNoTracking().AsQueryable();

        // Employees see only their own; Admin/HR/Finance see all.
        if (!_access.CanManageTravel())
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId == null) return Forbid();
            query = query.Where(t => t.EmployeeId == empId.Value);
        }
        else if (mineOnly)
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId.HasValue)
                query = query.Where(t => t.EmployeeId == empId.Value);
        }

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TravelRequestDto
            {
                TravelRequestId = t.TravelRequestId,
                TravelNumber = t.TravelNumber,
                EmployeeId = t.EmployeeId,
                EmployeeName = t.Employee != null ? (t.Employee.EnglishName ?? t.Employee.LaoName) : null,
                DepartmentId = t.DepartmentId,
                ProjectId = t.ProjectId,
                Purpose = t.Purpose,
                Destination = t.Destination,
                DepartureDate = t.DepartureDate,
                ReturnDate = t.ReturnDate,
                EstimatedCost = t.EstimatedCost,
                Currency = t.Currency,
                Status = t.Status,
                CreatedAt = t.CreatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<TravelRequestDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TravelRequestDto>> GetTravelRequest(int id)
    {
        var t = await _context.TravelRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TravelRequestId == id);
        if (t == null) return NotFound();

        if (!_access.CanManageTravel())
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId != t.EmployeeId) return Forbid();
        }

        var employeeName = await _context.Employees
            .Where(e => e.EmployeeId == t.EmployeeId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        return new TravelRequestDto
        {
            TravelRequestId = t.TravelRequestId,
            TravelNumber = t.TravelNumber,
            EmployeeId = t.EmployeeId,
            EmployeeName = employeeName,
            DepartmentId = t.DepartmentId,
            ProjectId = t.ProjectId,
            Purpose = t.Purpose,
            Destination = t.Destination,
            DepartureDate = t.DepartureDate,
            ReturnDate = t.ReturnDate,
            EstimatedCost = t.EstimatedCost,
            Currency = t.Currency,
            Status = t.Status,
            CreatedAt = t.CreatedAt,
        };
    }

    [HttpPost]
    public async Task<ActionResult<TravelRequest>> CreateTravelRequest([FromBody] CreateTravelRequestRequest request)
    {
        if (!_access.CanCreateTravel())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        if (string.IsNullOrWhiteSpace(request.Purpose) || string.IsNullOrWhiteSpace(request.Destination))
            return BadRequest("Purpose and Destination are required.");

        if (request.ReturnDate < request.DepartureDate)
            return BadRequest("Return date must be on or after departure date.");

        var travel = new TravelRequest
        {
            TravelNumber = await _numbers.NextAsync("TRV"),
            EmployeeId = empId.Value,
            DepartmentId = request.DepartmentId,
            ProjectId = request.ProjectId,
            Purpose = request.Purpose,
            Destination = request.Destination,
            DepartureDate = request.DepartureDate,
            ReturnDate = request.ReturnDate,
            EstimatedCost = request.EstimatedCost,
            Currency = request.Currency,
            Status = "DRAFT",
        };
        _context.TravelRequests.Add(travel);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTravelRequest), new { id = travel.TravelRequestId }, travel);
    }

    [HttpPost("{id:int}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        var travel = await _context.TravelRequests.FirstOrDefaultAsync(t => t.TravelRequestId == id);
        if (travel == null) return NotFound();
        if (travel.EmployeeId != empId.Value) return Forbid();
        if (travel.Status != "DRAFT")
            return BadRequest($"Cannot submit a travel request in '{travel.Status}' state.");

        // Server-side approver resolution: direct manager.
        await _approval.CreateRequestAsync("TRAVEL_REQUEST", id, empId.Value,
            new List<ApprovalStepDefinition>
            {
                new ApprovalStepDefinition { ResolverType = "DIRECT_MANAGER" },
            });

        travel.Status = "PENDING_APPROVAL";
        travel.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        if (!_access.CanApproveTravel())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        var travel = await _context.TravelRequests.FirstOrDefaultAsync(t => t.TravelRequestId == id);
        if (travel == null) return NotFound();
        if (travel.Status != "PENDING_APPROVAL")
            return BadRequest($"Cannot approve a travel request in '{travel.Status}' state.");

        travel.Status = "APPROVED";
        travel.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notifications.NotifyEmployeeAsync(
            travel.EmployeeId, "TRAVEL_APPROVED", "Travel approved",
            $"Your travel request {travel.TravelNumber} was approved.",
            "TRAVEL_REQUEST", travel.TravelRequestId);

        return NoContent();
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        if (!_access.CanApproveTravel())
            return Forbid();

        var travel = await _context.TravelRequests.FirstOrDefaultAsync(t => t.TravelRequestId == id);
        if (travel == null) return NotFound();
        if (travel.Status != "PENDING_APPROVAL")
            return BadRequest($"Cannot reject a travel request in '{travel.Status}' state.");

        travel.Status = "CANCELLED";
        travel.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notifications.NotifyEmployeeAsync(
            travel.EmployeeId, "TRAVEL_REJECTED", "Travel rejected",
            $"Your travel request {travel.TravelNumber} was rejected.",
            "TRAVEL_REQUEST", travel.TravelRequestId);

        return NoContent();
    }
}
