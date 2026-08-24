using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class WorkOrderDto
{
    public int WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public int SourceId { get; set; }
    public string? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? AssignedEmployeeId { get; set; }
    public string? AssignedName { get; set; }
    public int? SupplierId { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public decimal? Cost { get; set; }
    public string? Currency { get; set; }
}

public class CreateWorkOrderRequest
{
    public string SourceType { get; set; } = "FACILITY";
    public int SourceId { get; set; }
    public string? Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = "MEDIUM";
    public int? AssignedEmployeeId { get; set; }
    public int? SupplierId { get; set; }
    public DateTime? ScheduledAt { get; set; }
}

public class UpdateWorkOrderRequest
{
    public string? Status { get; set; }
    public int? AssignedEmployeeId { get; set; }
    public string? Priority { get; set; }
    public decimal? Cost { get; set; }
    public string? Currency { get; set; }
}

[Authorize]
[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICorporateOperationsAccessService _access;
    private readonly INumberSequenceService _numbers;

    public WorkOrdersController(
        LaoHRDbContext context,
        ICorporateOperationsAccessService access,
        INumberSequenceService numbers)
    {
        _context = context;
        _access = access;
        _numbers = numbers;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<WorkOrderDto>>> GetWorkOrders(
        [FromQuery] string? status = null,
        [FromQuery] string? sourceType = null,
        [FromQuery] int? sourceId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewFacilities())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.WorkOrders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(w => w.Status == status);
        if (!string.IsNullOrEmpty(sourceType))
            query = query.Where(w => w.SourceType == sourceType);
        if (sourceId.HasValue)
            query = query.Where(w => w.SourceId == sourceId.Value);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new WorkOrderDto
            {
                WorkOrderId = w.WorkOrderId,
                WorkOrderNumber = w.WorkOrderNumber,
                SourceType = w.SourceType,
                SourceId = w.SourceId,
                Category = w.Category,
                Title = w.Title,
                Description = w.Description,
                Priority = w.Priority,
                Status = w.Status,
                AssignedEmployeeId = w.AssignedEmployeeId,
                AssignedName = w.AssignedTo != null ? (w.AssignedTo.EnglishName ?? w.AssignedTo.LaoName) : null,
                SupplierId = w.SupplierId,
                ScheduledAt = w.ScheduledAt,
                CompletedAt = w.CompletedAt,
                Cost = w.Cost,
                Currency = w.Currency,
            })
            .ToListAsync();

        return new PaginatedResponse<WorkOrderDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpPost]
    public async Task<ActionResult<WorkOrder>> CreateWorkOrder([FromBody] CreateWorkOrderRequest request)
    {
        if (!_access.CanManageWorkOrders())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required.");

        var workOrder = new WorkOrder
        {
            WorkOrderNumber = await _numbers.NextAsync("WO"),
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            Category = request.Category,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority,
            Status = "OPEN",
            AssignedEmployeeId = request.AssignedEmployeeId,
            SupplierId = request.SupplierId,
            ScheduledAt = request.ScheduledAt,
        };
        _context.WorkOrders.Add(workOrder);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWorkOrders), new { }, workOrder);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateWorkOrder(int id, [FromBody] UpdateWorkOrderRequest request)
    {
        if (!_access.CanManageWorkOrders())
            return Forbid();

        var wo = await _context.WorkOrders.FirstOrDefaultAsync(w => w.WorkOrderId == id);
        if (wo == null) return NotFound();

        if (request.Status != null)
        {
            if (request.Status is not ("OPEN" or "ASSIGNED" or "IN_PROGRESS" or "COMPLETED" or "CANCELLED"))
                return BadRequest("Invalid status.");
            wo.Status = request.Status;
            if (request.Status == "COMPLETED")
                wo.CompletedAt = DateTime.UtcNow;
        }
        if (request.AssignedEmployeeId.HasValue) wo.AssignedEmployeeId = request.AssignedEmployeeId;
        if (request.Priority != null) wo.Priority = request.Priority;
        if (request.Cost.HasValue) wo.Cost = request.Cost;
        if (request.Currency != null) wo.Currency = request.Currency;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}
