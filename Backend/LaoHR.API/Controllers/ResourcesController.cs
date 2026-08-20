using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

public class ResourceListItem
{
    public int ResourceId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int AllocationPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class ResourceDetail
{
    public int ResourceId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectCode { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int AllocationPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateResourceRequest
{
    public int EmployeeId { get; set; }
    public string Role { get; set; } = "CONTRIBUTOR";
    public int AllocationPercent { get; set; } = 100;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

public class UpdateResourceRequest
{
    public string? Role { get; set; }
    public int? AllocationPercent { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Notes { get; set; }
}

[Authorize]
[ApiController]
public class ResourcesController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public ResourcesController(LaoHRDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List resources — optionally filtered to a project. Useful for both the
    /// per-project "team" view and a cross-project allocation overview.
    /// </summary>
    [HttpGet("api/resources")]
    public async Task<ActionResult<PaginatedResponse<ResourceListItem>>> GetResources(
        [FromQuery] int? projectId = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] string? role = null,
        [FromQuery] bool activeOn = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var today = DateTime.UtcNow.Date;
        var query = _context.Resources.AsNoTracking().AsQueryable();

        if (projectId.HasValue)
            query = query.Where(r => r.ProjectId == projectId.Value);
        if (employeeId.HasValue)
            query = query.Where(r => r.EmployeeId == employeeId.Value);
        if (!string.IsNullOrEmpty(role))
            query = query.Where(r => r.Role == role);
        if (activeOn)
        {
            // Allocation active on the given date: started (or no start) AND not yet ended.
            query = query.Where(r =>
                (!r.StartDate.HasValue || r.StartDate.Value <= today) &&
                (!r.EndDate.HasValue || r.EndDate.Value >= today));
        }

        var total = await query.LongCountAsync();

        var items = await query
            .OrderBy(r => r.ProjectId).ThenBy(r => r.EmployeeId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Join(_context.Employees,
                r => r.EmployeeId,
                e => e.EmployeeId,
                (r, e) => new { Resource = r, Employee = e })
            .Join(_context.Projects,
                re => re.Resource.ProjectId,
                p => p.ProjectId,
                (re, p) => new ResourceListItem
                {
                    ResourceId = re.Resource.ResourceId,
                    ProjectId = re.Resource.ProjectId,
                    ProjectCode = p.Code,
                    ProjectName = p.Name,
                    EmployeeId = re.Resource.EmployeeId,
                    EmployeeName = re.Employee.EnglishName ?? re.Employee.LaoName,
                    Role = re.Resource.Role,
                    AllocationPercent = re.Resource.AllocationPercent,
                    StartDate = re.Resource.StartDate,
                    EndDate = re.Resource.EndDate,
                })
            .ToListAsync();

        return new PaginatedResponse<ResourceListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    /// <summary>
    /// Per-project list — same shape but scoped to a project.
    /// </summary>
    [HttpGet("api/projects/{projectId:int}/[controller]")]
    public async Task<ActionResult<PaginatedResponse<ResourceListItem>>> GetProjectResources(
        int projectId,
        [FromQuery] string? role = null,
        [FromQuery] bool activeOn = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        return await GetResources(projectId, null, role, activeOn, page, pageSize);
    }

    [HttpGet("api/projects/{projectId:int}/[controller]/{id:int}")]
    public async Task<ActionResult<ResourceDetail>> GetResource(int projectId, int id)
    {
        var resource = await _context.Resources
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ResourceId == id && r.ProjectId == projectId);
        if (resource == null) return NotFound();

        var info = await _context.Resources
            .Where(r => r.ResourceId == id)
            .Join(_context.Employees,
                r => r.EmployeeId,
                e => e.EmployeeId,
                (r, e) => new { Resource = r, EmployeeName = e.EnglishName ?? e.LaoName })
            .Join(_context.Projects,
                re => re.Resource.ProjectId,
                p => p.ProjectId,
                (re, p) => new { re.Resource, re.EmployeeName, ProjectCode = p.Code, ProjectName = p.Name })
            .FirstOrDefaultAsync();

        if (info == null) return NotFound();

        return new ResourceDetail
        {
            ResourceId = resource.ResourceId,
            ProjectId = resource.ProjectId,
            ProjectCode = info.ProjectCode,
            ProjectName = info.ProjectName,
            EmployeeId = resource.EmployeeId,
            EmployeeName = info.EmployeeName,
            Role = resource.Role,
            AllocationPercent = resource.AllocationPercent,
            StartDate = resource.StartDate,
            EndDate = resource.EndDate,
            Notes = resource.Notes,
            CreatedAt = resource.CreatedAt,
        };
    }

    [HttpPost("api/projects/{projectId:int}/[controller]")]
    public async Task<ActionResult<Resource>> CreateResource(int projectId, [FromBody] CreateResourceRequest request)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound("Project not found");

        var employeeExists = await _context.Employees.AnyAsync(e => e.EmployeeId == request.EmployeeId);
        if (!employeeExists) return BadRequest("Employee not found");

        // Reuse existing allocation if the same employee is already on the project.
        var existing = await _context.Resources
            .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.EmployeeId == request.EmployeeId);

        if (existing != null)
        {
            existing.Role = request.Role;
            existing.AllocationPercent = Math.Clamp(request.AllocationPercent, 0, 100);
            existing.StartDate = request.StartDate;
            existing.EndDate = request.EndDate;
            existing.Notes = request.Notes;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return existing;
        }

        var resource = new Resource
        {
            ProjectId = projectId,
            EmployeeId = request.EmployeeId,
            Role = request.Role,
            AllocationPercent = Math.Clamp(request.AllocationPercent, 0, 100),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"resourceId\":{resource.ResourceId},\"employeeId\":{resource.EmployeeId},\"role\":\"{resource.Role}\"}}");
        return CreatedAtAction(nameof(GetResource), new { projectId, id = resource.ResourceId }, resource);
    }

    [HttpPut("api/projects/{projectId:int}/[controller]/{id:int}")]
    public async Task<IActionResult> UpdateResource(int projectId, int id, [FromBody] UpdateResourceRequest request)
    {
        var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ResourceId == id && r.ProjectId == projectId);
        if (resource == null) return NotFound();

        if (request.Role != null) resource.Role = request.Role;
        if (request.AllocationPercent.HasValue) resource.AllocationPercent = Math.Clamp(request.AllocationPercent.Value, 0, 100);
        if (request.StartDate.HasValue) resource.StartDate = request.StartDate;
        if (request.EndDate.HasValue) resource.EndDate = request.EndDate;
        if (request.Notes != null) resource.Notes = request.Notes;

        resource.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"resourceId\":{id},\"action\":\"updated\"}}");
        return NoContent();
    }

    [HttpDelete("api/projects/{projectId:int}/[controller]/{id:int}")]
    public async Task<IActionResult> DeleteResource(int projectId, int id)
    {
        var resource = await _context.Resources.FirstOrDefaultAsync(r => r.ResourceId == id && r.ProjectId == projectId);
        if (resource == null) return NotFound();

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"resourceId\":{id},\"employeeId\":{resource.EmployeeId},\"action\":\"deleted\"}}");
        return NoContent();
    }

    private int? GetCurrentEmployeeId()
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return null;
        return _context.Users
            .Where(u => u.Username == username)
            .Select(u => u.EmployeeId)
            .FirstOrDefault();
    }

    private async Task LogActivity(int projectId, string payloadJson)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            ProjectId = projectId,
            ActorId = GetCurrentEmployeeId() ?? 1,
            Action = "RESOURCE_UPDATED",
            PayloadJson = payloadJson,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
    }
}