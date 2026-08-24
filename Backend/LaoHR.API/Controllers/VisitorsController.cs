using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class VisitorDto
{
    public int VisitorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

public class VisitDto
{
    public int VisitId { get; set; }
    public int VisitorId { get; set; }
    public string? VisitorName { get; set; }
    public int HostEmployeeId { get; set; }
    public string? HostName { get; set; }
    public int? FacilityId { get; set; }
    public string? Purpose { get; set; }
    public DateTime? ExpectedAt { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateVisitorRequest
{
    public string FullName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

public class CreateVisitRequest
{
    public int VisitorId { get; set; }
    public int HostEmployeeId { get; set; }
    public int? FacilityId { get; set; }
    public string? Purpose { get; set; }
    public DateTime? ExpectedAt { get; set; }
}

[Authorize]
[ApiController]
[Route("api/visitors")]
public class VisitorsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICorporateOperationsAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly INotificationService _notifications;

    public VisitorsController(
        LaoHRDbContext context,
        ICorporateOperationsAccessService access,
        ICurrentEmployeeService currentEmployee,
        INotificationService notifications)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<VisitorDto>>> GetVisitors(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanManageVisitors())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Visitors.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(v => v.FullName.ToLower().Contains(s) || (v.Company != null && v.Company.ToLower().Contains(s)));
        }

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => new VisitorDto
            {
                VisitorId = v.VisitorId,
                FullName = v.FullName,
                Company = v.Company,
                Phone = v.Phone,
                Email = v.Email,
                Notes = v.Notes,
            })
            .ToListAsync();

        return new PaginatedResponse<VisitorDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpPost]
    public async Task<ActionResult<Visitor>> CreateVisitor([FromBody] CreateVisitorRequest request)
    {
        if (!_access.CanManageVisitors())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest("FullName is required.");

        var visitor = new Visitor
        {
            FullName = request.FullName,
            Company = request.Company,
            Phone = request.Phone,
            Email = request.Email,
            Notes = request.Notes,
        };
        _context.Visitors.Add(visitor);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVisitors), new { }, visitor);
    }

    [HttpGet("visits")]
    public async Task<ActionResult<List<VisitDto>>> GetVisits(
        [FromQuery] int? hostEmployeeId = null,
        [FromQuery] string? status = null)
    {
        if (!_access.CanManageVisitors())
            return Forbid();

        var query = _context.Visits.AsNoTracking().AsQueryable();
        if (hostEmployeeId.HasValue)
            query = query.Where(v => v.HostEmployeeId == hostEmployeeId.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(v => v.Status == status);

        return await query
            .OrderByDescending(v => v.ExpectedAt)
            .Select(v => new VisitDto
            {
                VisitId = v.VisitId,
                VisitorId = v.VisitorId,
                VisitorName = v.Visitor != null ? v.Visitor.FullName : null,
                HostEmployeeId = v.HostEmployeeId,
                HostName = v.Host != null ? (v.Host.EnglishName ?? v.Host.LaoName) : null,
                FacilityId = v.FacilityId,
                Purpose = v.Purpose,
                ExpectedAt = v.ExpectedAt,
                CheckedInAt = v.CheckedInAt,
                CheckedOutAt = v.CheckedOutAt,
                Status = v.Status,
            })
            .ToListAsync();
    }

    [HttpPost("visits")]
    public async Task<ActionResult<Visit>> CreateVisit([FromBody] CreateVisitRequest request)
    {
        if (!_access.CanManageVisitors())
            return Forbid();

        var visit = new Visit
        {
            VisitorId = request.VisitorId,
            HostEmployeeId = request.HostEmployeeId,
            FacilityId = request.FacilityId,
            Purpose = request.Purpose,
            ExpectedAt = request.ExpectedAt,
            Status = "EXPECTED",
            CreatedByEmployeeId = _currentEmployee.GetCurrentEmployeeId(),
        };
        _context.Visits.Add(visit);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetVisits), new { }, visit);
    }

    [HttpPost("visits/{id:int}/check-in")]
    public async Task<IActionResult> CheckIn(int id)
    {
        if (!_access.CanManageVisitors())
            return Forbid();

        var visit = await _context.Visits.FirstOrDefaultAsync(v => v.VisitId == id);
        if (visit == null) return NotFound();
        if (visit.Status != "EXPECTED")
            return BadRequest($"Cannot check in a visit in '{visit.Status}' state.");

        visit.Status = "CHECKED_IN";
        visit.CheckedInAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notifications.NotifyEmployeeAsync(
            visit.HostEmployeeId, "VISITOR_ARRIVED", "Visitor arrived",
            "Your visitor has checked in.",
            "VISIT", visit.VisitId);

        return NoContent();
    }

    [HttpPost("visits/{id:int}/check-out")]
    public async Task<IActionResult> CheckOut(int id)
    {
        if (!_access.CanManageVisitors())
            return Forbid();

        var visit = await _context.Visits.FirstOrDefaultAsync(v => v.VisitId == id);
        if (visit == null) return NotFound();
        if (visit.Status != "CHECKED_IN")
            return BadRequest($"Cannot check out a visit in '{visit.Status}' state.");

        visit.Status = "CHECKED_OUT";
        visit.CheckedOutAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
