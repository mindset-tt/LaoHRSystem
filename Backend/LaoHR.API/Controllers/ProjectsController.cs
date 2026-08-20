using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

/// <summary>
/// Lightweight project list response — keeps payloads small for the dashboard.
/// </summary>
public class ProjectListItem
{
    public int ProjectId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Color { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public int MemberCount { get; set; }
    public int OpenTaskCount { get; set; }
    public int DoneTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public ProjectsController(LaoHRDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List projects with pagination, filters, and search.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ProjectListItem>>> GetProjects(
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] bool mineOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Projects.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(p => p.Priority == priority);

        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(p =>
                p.Code.ToLower().Contains(s) ||
                p.Name.ToLower().Contains(s));
        }

        if (mineOnly)
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId.HasValue)
            {
                // Owner OR a member
                query = query.Where(p =>
                    p.OwnerId == currentEmployeeId.Value ||
                    _context.ProjectMembers.Any(pm =>
                        pm.ProjectId == p.ProjectId && pm.EmployeeId == currentEmployeeId.Value));
            }
        }

        var total = await query.LongCountAsync();

        // Aggregate counts in a single round-trip per project (small N — pageSize capped).
        var raw = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.ProjectId,
                p.Code,
                p.Name,
                p.Status,
                p.Priority,
                p.Color,
                p.StartDate,
                p.DueDate,
                p.CompletedAt,
                p.OwnerId,
                OwnerName = p.Owner != null
                    ? (p.Owner.EnglishName ?? p.Owner.LaoName)
                    : null,
                p.CreatedAt,
                p.UpdatedAt,
            })
            .ToListAsync();

        var ids = raw.Select(r => r.ProjectId).ToList();

        // Single aggregation query — avoids N+1 over members and tasks.
        var memberCounts = await _context.ProjectMembers
            .Where(pm => ids.Contains(pm.ProjectId))
            .GroupBy(pm => pm.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ProjectId, g => g.Count);

        var taskCounts = await _context.ProjectTasks
            .Where(t => ids.Contains(t.ProjectId))
            .GroupBy(t => new { t.ProjectId, t.Status })
            .Select(g => new { g.Key.ProjectId, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var items = raw.Select(p => new ProjectListItem
        {
            ProjectId = p.ProjectId,
            Code = p.Code,
            Name = p.Name,
            Status = p.Status,
            Priority = p.Priority,
            Color = p.Color,
            StartDate = p.StartDate,
            DueDate = p.DueDate,
            CompletedAt = p.CompletedAt,
            OwnerId = p.OwnerId,
            OwnerName = p.OwnerName,
            CreatedAt = p.CreatedAt,
            MemberCount = memberCounts.GetValueOrDefault(p.ProjectId, 0),
            OpenTaskCount = taskCounts.Where(t => t.ProjectId == p.ProjectId && t.Status != "DONE" && t.Status != "CANCELLED").Sum(t => t.Count),
            DoneTaskCount = taskCounts.Where(t => t.ProjectId == p.ProjectId && t.Status == "DONE").Sum(t => t.Count),
        }).ToList();

        return new PaginatedResponse<ProjectListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    /// <summary>
    /// Get a single project with members, milestones, and tasks.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDetail>> GetProject(int id)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (project == null) return NotFound();

        var owner = await _context.Employees
            .Where(e => e.EmployeeId == project.OwnerId)
            .Select(e => new { e.EmployeeId, Name = e.EnglishName ?? e.LaoName })
            .FirstOrDefaultAsync();

        var members = await _context.ProjectMembers
            .Where(pm => pm.ProjectId == id)
            .Join(_context.Employees,
                pm => pm.EmployeeId,
                e => e.EmployeeId,
                (pm, e) => new ProjectMemberDetail
                {
                    ProjectMemberId = pm.ProjectMemberId,
                    EmployeeId = pm.EmployeeId,
                    EmployeeName = e.EnglishName ?? e.LaoName,
                    Role = pm.Role,
                    JoinedAt = pm.JoinedAt,
                })
            .ToListAsync();

        var milestones = await _context.Milestones
            .Where(m => m.ProjectId == id)
            .OrderBy(m => m.DueDate ?? DateTime.MaxValue)
            .Select(m => new MilestoneDetail
            {
                MilestoneId = m.MilestoneId,
                Name = m.Name,
                Description = m.Description,
                DueDate = m.DueDate,
                CompletedAt = m.CompletedAt,
                Status = m.Status,
            })
            .ToListAsync();

        var taskCounts = await _context.ProjectTasks
            .Where(t => t.ProjectId == id)
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Status, g => g.Count);

        return new ProjectDetail
        {
            ProjectId = project.ProjectId,
            Code = project.Code,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            Priority = project.Priority,
            Color = project.Color,
            StartDate = project.StartDate,
            DueDate = project.DueDate,
            CompletedAt = project.CompletedAt,
            OwnerId = project.OwnerId,
            OwnerName = owner?.Name,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            Members = members,
            Milestones = milestones,
            TaskCountByStatus = taskCounts,
        };
    }

    /// <summary>
    /// Create a new project. The current user becomes the owner.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject([FromBody] CreateProjectRequest request)
    {
        var currentEmployeeId = GetCurrentEmployeeId() ?? 1; // Fallback to admin if no linked employee

        var project = new Project
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Status = request.Status ?? "PLANNING",
            Priority = request.Priority ?? "MEDIUM",
            Color = request.Color,
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            OwnerId = request.OwnerId ?? currentEmployeeId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        // Auto-add the owner as a member with OWNER role.
        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.ProjectId,
            EmployeeId = project.OwnerId,
            Role = "OWNER",
            JoinedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        await LogActivity(project.ProjectId, null, currentEmployeeId, "CREATED", $"{{\"code\":\"{project.Code}\",\"name\":\"{project.Name}\"}}");

        return CreatedAtAction(nameof(GetProject), new { id = project.ProjectId }, project);
    }

    /// <summary>
    /// Update an existing project.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateProject(int id, [FromBody] UpdateProjectRequest request)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound();

        if (request.Name != null) project.Name = request.Name;
        if (request.Description != null) project.Description = request.Description;
        if (request.Status != null) project.Status = request.Status;
        if (request.Priority != null) project.Priority = request.Priority;
        if (request.Color != null) project.Color = request.Color;
        if (request.StartDate.HasValue) project.StartDate = request.StartDate;
        if (request.DueDate.HasValue) project.DueDate = request.DueDate;
        if (request.OwnerId.HasValue) project.OwnerId = request.OwnerId.Value;

        if (request.Status == "COMPLETED" && !project.CompletedAt.HasValue)
            project.CompletedAt = DateTime.UtcNow;
        if (request.Status != "COMPLETED" && project.CompletedAt.HasValue)
            project.CompletedAt = null;

        project.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogActivity(project.ProjectId, null, GetCurrentEmployeeId() ?? 1, "UPDATED_PROJECT", null);
        return NoContent();
    }

    /// <summary>
    /// Add or update a project member.
    /// </summary>
    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddMemberRequest request)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound();

        var existing = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == id && pm.EmployeeId == request.EmployeeId);

        if (existing != null)
        {
            existing.Role = request.Role;
            await _context.SaveChangesAsync();
        }
        else
        {
            _context.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = id,
                EmployeeId = request.EmployeeId,
                Role = request.Role,
                JoinedAt = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();
            await LogActivity(id, null, GetCurrentEmployeeId() ?? 1, "MEMBER_ADDED", $"{{\"employeeId\":{request.EmployeeId}}}");
        }
        return NoContent();
    }

    /// <summary>
    /// Remove a member.
    /// </summary>
    [HttpDelete("{id:int}/members/{employeeId:int}")]
    public async Task<IActionResult> RemoveMember(int id, int employeeId)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == id && pm.EmployeeId == employeeId);
        if (member == null) return NotFound();
        _context.ProjectMembers.Remove(member);
        await _context.SaveChangesAsync();
        await LogActivity(id, null, GetCurrentEmployeeId() ?? 1, "MEMBER_REMOVED", $"{{\"employeeId\":{employeeId}}}");
        return NoContent();
    }

    /// <summary>
    /// Get the activity timeline for a project.
    /// </summary>
    [HttpGet("{id:int}/activities")]
    public async Task<ActionResult<PaginatedResponse<ActivityListItem>>> GetActivities(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.ActivityLogs
            .AsNoTracking()
            .Where(a => a.ProjectId == id)
            .OrderByDescending(a => a.CreatedAt);

        var total = await query.LongCountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ActivityListItem
            {
                ActivityId = a.ActivityId,
                ProjectId = a.ProjectId,
                TaskId = a.TaskId,
                ActorId = a.ActorId,
                ActorName = a.Actor != null ? (a.Actor.EnglishName ?? a.Actor.LaoName) : null,
                Action = a.Action,
                PayloadJson = a.PayloadJson,
                CreatedAt = a.CreatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<ActivityListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
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

    private async Task LogActivity(int projectId, int? taskId, int actorId, string action, string? payloadJson)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            ProjectId = projectId,
            TaskId = taskId,
            ActorId = actorId,
            Action = action,
            PayloadJson = payloadJson,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
    }
}

public class ProjectDetail
{
    public int ProjectId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Color { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<ProjectMemberDetail> Members { get; set; } = new();
    public List<MilestoneDetail> Milestones { get; set; } = new();
    public Dictionary<string, int> TaskCountByStatus { get; set; } = new();
}

public class ProjectMemberDetail
{
    public int ProjectMemberId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

public class MilestoneDetail
{
    public int MilestoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class ActivityListItem
{
    public long ActivityId { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public int ActorId { get; set; }
    public string? ActorName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? PayloadJson { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateProjectRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Color { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? OwnerId { get; set; }
}

public class UpdateProjectRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public string? Color { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? OwnerId { get; set; }
}

public class AddMemberRequest
{
    public int EmployeeId { get; set; }
    public string Role { get; set; } = "MEMBER";
}