using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class ServiceRequestCategoryDto
{
    public int ServiceRequestCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public bool IsActive { get; set; }
}

public class ServiceRequestListItem
{
    public int ServiceRequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int RequesterEmployeeId { get; set; }
    public string? RequesterName { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedEmployeeId { get; set; }
    public string? AssignedName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServiceRequestDetail
{
    public int ServiceRequestId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public int RequesterEmployeeId { get; set; }
    public string? RequesterName { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedEmployeeId { get; set; }
    public string? AssignedName { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

public class CreateServiceRequestRequest
{
    public int CategoryId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "MEDIUM";
    public int? DepartmentId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateServiceRequestRequest
{
    public string? Status { get; set; }
    public int? AssignedEmployeeId { get; set; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
}

[Authorize]
[ApiController]
[Route("api/service-requests")]
public class ServiceRequestsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly INumberSequenceService _numbers;

    public ServiceRequestsController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        ICurrentEmployeeService currentEmployee,
        INumberSequenceService numbers)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _numbers = numbers;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<ServiceRequestCategoryDto>>> GetCategories()
    {
        return await _context.ServiceRequestCategories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new ServiceRequestCategoryDto
            {
                ServiceRequestCategoryId = c.ServiceRequestCategoryId,
                Code = c.Code,
                Name = c.Name,
                NameLao = c.NameLao,
                IsActive = c.IsActive,
            })
            .ToListAsync();
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ServiceRequestListItem>>> GetRequests(
        [FromQuery] string? status = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool mineOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.ServiceRequests.AsNoTracking().AsQueryable();

        // Employees see only their own; Admin/HR see all.
        if (!_access.CanManageServiceRequests())
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId == null) return Forbid();
            query = query.Where(s => s.RequesterEmployeeId == empId.Value);
        }
        else if (mineOnly)
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId.HasValue)
                query = query.Where(s => s.RequesterEmployeeId == empId.Value);
        }

        if (!string.IsNullOrEmpty(status))
            query = query.Where(s => s.Status == status);
        if (categoryId.HasValue)
            query = query.Where(s => s.CategoryId == categoryId.Value);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new ServiceRequestListItem
            {
                ServiceRequestId = s.ServiceRequestId,
                RequestNumber = s.RequestNumber,
                RequesterEmployeeId = s.RequesterEmployeeId,
                RequesterName = s.Requester != null ? (s.Requester.EnglishName ?? s.Requester.LaoName) : null,
                CategoryId = s.CategoryId,
                CategoryName = s.Category != null ? s.Category.Name : null,
                Subject = s.Subject,
                Priority = s.Priority,
                Status = s.Status,
                AssignedEmployeeId = s.AssignedEmployeeId,
                AssignedName = s.AssignedTo != null ? (s.AssignedTo.EnglishName ?? s.AssignedTo.LaoName) : null,
                DueDate = s.DueDate,
                CreatedAt = s.CreatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<ServiceRequestListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRequestDetail>> GetRequest(int id)
    {
        var s = await _context.ServiceRequests.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ServiceRequestId == id);
        if (s == null) return NotFound();

        if (!_access.CanManageServiceRequests())
        {
            var empId = _currentEmployee.GetCurrentEmployeeId();
            if (empId != s.RequesterEmployeeId) return Forbid();
        }

        var requesterName = await _context.Employees
            .Where(e => e.EmployeeId == s.RequesterEmployeeId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();
        var categoryName = await _context.ServiceRequestCategories
            .Where(c => c.ServiceRequestCategoryId == s.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();
        var assignedName = s.AssignedEmployeeId.HasValue
            ? await _context.Employees.Where(e => e.EmployeeId == s.AssignedEmployeeId.Value)
                .Select(e => e.EnglishName ?? e.LaoName).FirstOrDefaultAsync()
            : null;

        return new ServiceRequestDetail
        {
            ServiceRequestId = s.ServiceRequestId,
            RequestNumber = s.RequestNumber,
            RequesterEmployeeId = s.RequesterEmployeeId,
            RequesterName = requesterName,
            CategoryId = s.CategoryId,
            CategoryName = categoryName,
            Subject = s.Subject,
            Description = s.Description,
            Priority = s.Priority,
            Status = s.Status,
            AssignedEmployeeId = s.AssignedEmployeeId,
            AssignedName = assignedName,
            DepartmentId = s.DepartmentId,
            DueDate = s.DueDate,
            CreatedAt = s.CreatedAt,
            ResolvedAt = s.ResolvedAt,
        };
    }

    [HttpPost]
    public async Task<ActionResult<ServiceRequest>> CreateRequest([FromBody] CreateServiceRequestRequest request)
    {
        var requesterEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (requesterEmployeeId == null) return Unauthorized("No linked employee profile.");

        if (string.IsNullOrWhiteSpace(request.Subject))
            return BadRequest("Subject is required.");

        var number = await _numbers.NextAsync("SR");

        var sr = new ServiceRequest
        {
            RequestNumber = number,
            RequesterEmployeeId = requesterEmployeeId.Value,
            CategoryId = request.CategoryId,
            Subject = request.Subject,
            Description = request.Description,
            Priority = request.Priority,
            Status = "OPEN",
            DepartmentId = request.DepartmentId,
            DueDate = request.DueDate,
        };
        _context.ServiceRequests.Add(sr);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRequest), new { id = sr.ServiceRequestId }, sr);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRequest(int id, [FromBody] UpdateServiceRequestRequest request)
    {
        if (!_access.CanManageServiceRequests())
            return Forbid();

        var sr = await _context.ServiceRequests.FirstOrDefaultAsync(x => x.ServiceRequestId == id);
        if (sr == null) return NotFound();

        var actorId = _currentEmployee.GetCurrentEmployeeId();

        if (request.Status != null)
        {
            if (request.Status is not ("OPEN" or "IN_PROGRESS" or "ON_HOLD" or "RESOLVED" or "CLOSED" or "CANCELLED"))
                return BadRequest("Invalid status.");
            if (request.Status != sr.Status)
            {
                _context.ServiceRequestHistories.Add(new ServiceRequestHistory
                {
                    ServiceRequestId = id,
                    ChangeType = "STATUS_CHANGE",
                    FromValue = sr.Status,
                    ToValue = request.Status,
                    ChangedByEmployeeId = actorId,
                });
            }
            sr.Status = request.Status;
            if (request.Status == "RESOLVED")
                sr.ResolvedAt = DateTime.UtcNow;
        }
        if (request.AssignedEmployeeId.HasValue && request.AssignedEmployeeId != sr.AssignedEmployeeId)
        {
            _context.ServiceRequestHistories.Add(new ServiceRequestHistory
            {
                ServiceRequestId = id,
                ChangeType = "ASSIGNED",
                FromValue = sr.AssignedEmployeeId?.ToString(),
                ToValue = request.AssignedEmployeeId.Value.ToString(),
                ChangedByEmployeeId = actorId,
            });
            sr.AssignedEmployeeId = request.AssignedEmployeeId;
        }
        if (request.Priority != null && request.Priority != sr.Priority)
        {
            _context.ServiceRequestHistories.Add(new ServiceRequestHistory
            {
                ServiceRequestId = id,
                ChangeType = "PRIORITY_CHANGE",
                FromValue = sr.Priority,
                ToValue = request.Priority,
                ChangedByEmployeeId = actorId,
            });
            sr.Priority = request.Priority;
        }
        if (request.DueDate.HasValue) sr.DueDate = request.DueDate;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<List<ServiceRequestHistory>>> GetHistory(int id)
    {
        if (!_access.CanManageServiceRequests())
            return Forbid();

        return await _context.ServiceRequestHistories.AsNoTracking()
            .Where(h => h.ServiceRequestId == id)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }
}
