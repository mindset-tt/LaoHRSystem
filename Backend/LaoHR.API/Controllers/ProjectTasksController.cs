using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

/// <summary>
/// Lightweight task list response — keeps payloads small for the board view.
/// </summary>
public class TaskListItem
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectCode { get; set; } = string.Empty;
    public int? MilestoneId { get; set; }
    public string? MilestoneName { get; set; }
    public string? TaskNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercent { get; set; }
    public int ReporterId { get; set; }
    public string? ReporterName { get; set; }
    public List<TaskAssigneeSummary> Assignees { get; set; } = new();
    public int CommentCount { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TaskAssigneeSummary
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

[Authorize]
[ApiController]
[Route("api/projects/{projectId:int}/[controller]")]
public class ProjectTasksController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IPmPlanningService _planning;

    public ProjectTasksController(LaoHRDbContext context, IPmPlanningService planning)
    {
        _context = context;
        _planning = planning;
    }

    /// <summary>
    /// List tasks for a project with pagination, filters, and search.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<TaskListItem>>> GetTasks(
        int projectId,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] int? milestoneId = null,
        [FromQuery] bool? assignedToMe = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrEmpty(priority))
            query = query.Where(t => t.Priority == priority);

        if (milestoneId.HasValue)
            query = query.Where(t => t.MilestoneId == milestoneId.Value);

        if (assignedToMe == true)
        {
            var me = GetCurrentEmployeeId();
            if (me.HasValue)
                query = query.Where(t => _context.TaskAssignees.Any(ta => ta.TaskId == t.TaskId && ta.EmployeeId == me.Value));
        }

        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(t => t.Title.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();

        var raw = await query
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.TaskId,
                t.ProjectId,
                ProjectName = t.Project != null ? t.Project.Name : string.Empty,
                ProjectCode = t.Project != null ? t.Project.Code : string.Empty,
                t.MilestoneId,
                MilestoneName = t.Milestone != null ? t.Milestone.Name : null,
                t.TaskNumber,
                t.Title,
                t.Status,
                t.Priority,
                t.StartDate,
                t.DueDate,
                t.CompletedAt,
                t.ProgressPercent,
                t.ReporterId,
                ReporterName = t.Reporter != null ? (t.Reporter.EnglishName ?? t.Reporter.LaoName) : null,
                t.CreatedAt,
                t.UpdatedAt,
            })
            .ToListAsync();

        var ids = raw.Select(r => r.TaskId).ToList();

        // Single aggregate query for assignees and comment counts.
        var assigneesRaw = await _context.TaskAssignees
            .Where(ta => ids.Contains(ta.TaskId))
            .Join(_context.Employees,
                ta => ta.EmployeeId,
                e => e.EmployeeId,
                (ta, e) => new
                {
                    ta.TaskId,
                    ta.EmployeeId,
                    EmployeeName = e.EnglishName ?? e.LaoName,
                    ta.Role,
                })
            .ToListAsync();

        var commentCounts = await _context.TaskComments
            .Where(c => ids.Contains(c.TaskId))
            .GroupBy(c => c.TaskId)
            .Select(g => new { TaskId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.TaskId, g => g.Count);

        var items = raw.Select(r => new TaskListItem
        {
            TaskId = r.TaskId,
            ProjectId = r.ProjectId,
            ProjectName = r.ProjectName,
            ProjectCode = r.ProjectCode,
            MilestoneId = r.MilestoneId,
            MilestoneName = r.MilestoneName,
            TaskNumber = r.TaskNumber,
            Title = r.Title,
            Status = r.Status,
            Priority = r.Priority,
            StartDate = r.StartDate,
            DueDate = r.DueDate,
            CompletedAt = r.CompletedAt,
            ProgressPercent = r.ProgressPercent,
            ReporterId = r.ReporterId,
            ReporterName = r.ReporterName,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            IsOverdue = r.DueDate.HasValue && r.DueDate < DateTime.UtcNow && r.Status != "DONE" && r.Status != "CANCELLED",
            Assignees = assigneesRaw
                .Where(a => a.TaskId == r.TaskId)
                .Select(a => new TaskAssigneeSummary { EmployeeId = a.EmployeeId, EmployeeName = a.EmployeeName, Role = a.Role })
                .ToList(),
            CommentCount = commentCounts.GetValueOrDefault(r.TaskId, 0),
        }).ToList();

        return new PaginatedResponse<TaskListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    /// <summary>
    /// Get a single task with assignees and recent comments.
    /// </summary>
    [HttpGet("{taskId:int}")]
    public async Task<ActionResult<TaskDetail>> GetTask(int projectId, int taskId)
    {
        var task = await _context.ProjectTasks
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TaskId == taskId && t.ProjectId == projectId);

        if (task == null) return NotFound();

        var assignees = await _context.TaskAssignees
            .Where(ta => ta.TaskId == taskId)
            .Join(_context.Employees,
                ta => ta.EmployeeId,
                e => e.EmployeeId,
                (ta, e) => new TaskAssigneeSummary
                {
                    EmployeeId = ta.EmployeeId,
                    EmployeeName = e.EnglishName ?? e.LaoName,
                    Role = ta.Role,
                })
            .ToListAsync();

        var comments = await _context.TaskComments
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .Join(_context.Employees,
                c => c.AuthorId,
                e => e.EmployeeId,
                (c, e) => new TaskCommentDetail
                {
                    CommentId = c.CommentId,
                    AuthorId = c.AuthorId,
                    AuthorName = e.EnglishName ?? e.LaoName,
                    Body = c.Body,
                    ParentCommentId = c.ParentCommentId,
                    CreatedAt = c.CreatedAt,
                })
            .ToListAsync();

        return new TaskDetail
        {
            TaskId = task.TaskId,
            ProjectId = task.ProjectId,
            MilestoneId = task.MilestoneId,
            TaskNumber = task.TaskNumber,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            StartDate = task.StartDate,
            DueDate = task.DueDate,
            CompletedAt = task.CompletedAt,
            ProgressPercent = task.ProgressPercent,
            EstimatedHours = task.EstimatedHours,
            ActualHours = task.ActualHours,
            ReporterId = task.ReporterId,
            Assignees = assignees,
            Comments = comments,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
        };
    }

    /// <summary>
    /// Create a new task in the project.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProjectTask>> CreateTask(int projectId, [FromBody] CreateTaskRequest request)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound();

        var reporterId = GetCurrentEmployeeId() ?? request.ReporterId ?? 1;

        // TaskNumber — simple "PRJ-<n>-<seq>" pattern.
        var seq = await _context.ProjectTasks.CountAsync(t => t.ProjectId == projectId) + 1;
        var task = new ProjectTask
        {
            ProjectId = projectId,
            MilestoneId = request.MilestoneId,
            ParentTaskId = request.ParentTaskId,
            TaskNumber = $"{project.Code}-{seq}",
            Title = request.Title,
            Description = request.Description,
            Status = request.Status ?? "TODO",
            Priority = request.Priority ?? "MEDIUM",
            StartDate = request.StartDate,
            DueDate = request.DueDate,
            EstimatedHours = request.EstimatedHours,
            ReporterId = reporterId,
            SortOrder = request.SortOrder ?? 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.ProjectTasks.Add(task);
        await _context.SaveChangesAsync();

        // Assignees
        if (request.AssigneeIds != null)
        {
            foreach (var empId in request.AssigneeIds)
            {
                _context.TaskAssignees.Add(new TaskAssignee
                {
                    TaskId = task.TaskId,
                    EmployeeId = empId,
                    Role = "ASSIGNEE",
                    AssignedAt = DateTime.UtcNow,
                });
            }
            await _context.SaveChangesAsync();
        }

        await LogActivity(projectId, task.TaskId, reporterId, "CREATED", $"{{\"title\":\"{task.Title}\"}}");
        return CreatedAtAction(nameof(GetTask), new { projectId, taskId = task.TaskId }, task);
    }

    /// <summary>
    /// Update a task — status, priority, description, progress, etc.
    /// </summary>
    [HttpPut("{taskId:int}")]
    public async Task<IActionResult> UpdateTask(int projectId, int taskId, [FromBody] UpdateTaskRequest request)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.TaskId == taskId && t.ProjectId == projectId);
        if (task == null) return NotFound();

        var statusBefore = task.Status;

        if (request.Title != null) task.Title = request.Title;
        if (request.Description != null) task.Description = request.Description;
        if (request.Status != null) task.Status = request.Status;
        if (request.Priority != null) task.Priority = request.Priority;
        if (request.MilestoneId.HasValue) task.MilestoneId = request.MilestoneId.Value == 0 ? null : request.MilestoneId.Value;
        if (request.ParentTaskId.HasValue)
        {
            int? newParent = request.ParentTaskId.Value == 0 ? (int?)null : request.ParentTaskId.Value;
            if (newParent.HasValue && await _planning.WouldCreateTaskCycleAsync(taskId, newParent.Value))
                return BadRequest("Assigning this parent would create a task hierarchy cycle.");
            task.ParentTaskId = newParent;
        }
        if (request.StartDate.HasValue) task.StartDate = request.StartDate;
        if (request.DueDate.HasValue) task.DueDate = request.DueDate;
        if (request.ProgressPercent.HasValue) task.ProgressPercent = Math.Clamp(request.ProgressPercent.Value, 0, 100);
        if (request.EstimatedHours.HasValue) task.EstimatedHours = request.EstimatedHours;
        if (request.ActualHours.HasValue) task.ActualHours = request.ActualHours;
        if (request.SortOrder.HasValue) task.SortOrder = request.SortOrder.Value;

        if (request.Status == "DONE" && !task.CompletedAt.HasValue)
            task.CompletedAt = DateTime.UtcNow;
        if (request.Status != null && request.Status != "DONE" && task.CompletedAt.HasValue)
            task.CompletedAt = null;

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var actorId = GetCurrentEmployeeId() ?? 1;
        if (request.Status != null && request.Status != statusBefore)
            await LogActivity(projectId, taskId, actorId, "UPDATED_STATUS", $"{{\"from\":\"{statusBefore}\",\"to\":\"{task.Status}\"}}");
        else
            await LogActivity(projectId, taskId, actorId, "UPDATED_TASK", null);

        return NoContent();
    }

    /// <summary>
    /// Add a comment to a task.
    /// </summary>
    [HttpPost("{taskId:int}/comments")]
    public async Task<ActionResult<TaskComment>> AddComment(int projectId, int taskId, [FromBody] AddCommentRequest request)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.TaskId == taskId && t.ProjectId == projectId);
        if (task == null) return NotFound();

        var authorId = GetCurrentEmployeeId() ?? 1;
        var comment = new TaskComment
        {
            TaskId = taskId,
            AuthorId = authorId,
            Body = request.Body,
            ParentCommentId = request.ParentCommentId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.TaskComments.Add(comment);
        await _context.SaveChangesAsync();
        await LogActivity(projectId, taskId, authorId, "COMMENTED", null);
        return Ok(comment);
    }

    /// <summary>
    /// Add or replace assignees on a task.
    /// </summary>
    [HttpPut("{taskId:int}/assignees")]
    public async Task<IActionResult> SetAssignees(int projectId, int taskId, [FromBody] SetAssigneesRequest request)
    {
        var task = await _context.ProjectTasks.FirstOrDefaultAsync(t => t.TaskId == taskId && t.ProjectId == projectId);
        if (task == null) return NotFound();

        var existing = await _context.TaskAssignees.Where(ta => ta.TaskId == taskId).ToListAsync();
        _context.TaskAssignees.RemoveRange(existing);

        foreach (var empId in request.EmployeeIds)
        {
            _context.TaskAssignees.Add(new TaskAssignee
            {
                TaskId = taskId,
                EmployeeId = empId,
                Role = "ASSIGNEE",
                AssignedAt = DateTime.UtcNow,
            });
        }
        await _context.SaveChangesAsync();
        await LogActivity(projectId, taskId, GetCurrentEmployeeId() ?? 1, "ASSIGNED", $"{{\"count\":{request.EmployeeIds.Count}}}");
        return NoContent();
    }

    /// <summary>
    /// Tasks assigned to the current user across all projects.
    /// </summary>
    [HttpGet("/api/my-tasks")]
    public async Task<ActionResult<PaginatedResponse<TaskListItem>>> GetMyTasks(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var me = GetCurrentEmployeeId();
        if (!me.HasValue)
            return new PaginatedResponse<TaskListItem> { Page = page, PageSize = pageSize };

        var query = _context.ProjectTasks
            .AsNoTracking()
            .Where(t => _context.TaskAssignees.Any(ta => ta.TaskId == t.TaskId && ta.EmployeeId == me.Value));

        if (!string.IsNullOrEmpty(status))
            query = query.Where(t => t.Status == status);

        return await BuildPagedAsync(query, page, pageSize);
    }

    private async Task<PaginatedResponse<TaskListItem>> BuildPagedAsync(IQueryable<ProjectTask> query, int page, int pageSize)
    {
        var total = await query.LongCountAsync();

        var raw = await query
            .OrderBy(t => t.DueDate ?? DateTime.MaxValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new
            {
                t.TaskId,
                t.ProjectId,
                ProjectName = t.Project != null ? t.Project.Name : string.Empty,
                ProjectCode = t.Project != null ? t.Project.Code : string.Empty,
                t.MilestoneId,
                MilestoneName = t.Milestone != null ? t.Milestone.Name : null,
                t.TaskNumber,
                t.Title,
                t.Status,
                t.Priority,
                t.StartDate,
                t.DueDate,
                t.CompletedAt,
                t.ProgressPercent,
                t.ReporterId,
                ReporterName = t.Reporter != null ? (t.Reporter.EnglishName ?? t.Reporter.LaoName) : null,
                t.CreatedAt,
                t.UpdatedAt,
            })
            .ToListAsync();

        var ids = raw.Select(r => r.TaskId).ToList();
        var assigneesRaw = await _context.TaskAssignees
            .Where(ta => ids.Contains(ta.TaskId))
            .Join(_context.Employees,
                ta => ta.EmployeeId,
                e => e.EmployeeId,
                (ta, e) => new
                {
                    ta.TaskId,
                    ta.EmployeeId,
                    EmployeeName = e.EnglishName ?? e.LaoName,
                    ta.Role,
                })
            .ToListAsync();
        var commentCounts = await _context.TaskComments
            .Where(c => ids.Contains(c.TaskId))
            .GroupBy(c => c.TaskId)
            .Select(g => new { TaskId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.TaskId, g => g.Count);

        var items = raw.Select(r => new TaskListItem
        {
            TaskId = r.TaskId,
            ProjectId = r.ProjectId,
            ProjectName = r.ProjectName,
            ProjectCode = r.ProjectCode,
            MilestoneId = r.MilestoneId,
            MilestoneName = r.MilestoneName,
            TaskNumber = r.TaskNumber,
            Title = r.Title,
            Status = r.Status,
            Priority = r.Priority,
            StartDate = r.StartDate,
            DueDate = r.DueDate,
            CompletedAt = r.CompletedAt,
            ProgressPercent = r.ProgressPercent,
            ReporterId = r.ReporterId,
            ReporterName = r.ReporterName,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            IsOverdue = r.DueDate.HasValue && r.DueDate < DateTime.UtcNow && r.Status != "DONE" && r.Status != "CANCELLED",
            Assignees = assigneesRaw
                .Where(a => a.TaskId == r.TaskId)
                .Select(a => new TaskAssigneeSummary { EmployeeId = a.EmployeeId, EmployeeName = a.EmployeeName, Role = a.Role })
                .ToList(),
            CommentCount = commentCounts.GetValueOrDefault(r.TaskId, 0),
        }).ToList();

        return new PaginatedResponse<TaskListItem>
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

public class TaskDetail
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public int? MilestoneId { get; set; }
    public string? TaskNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int ProgressPercent { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public int ReporterId { get; set; }
    public List<TaskAssigneeSummary> Assignees { get; set; } = new();
    public List<TaskCommentDetail> Comments { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TaskCommentDetail
{
    public int CommentId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? MilestoneId { get; set; }
    public int? ParentTaskId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal? EstimatedHours { get; set; }
    public int? ReporterId { get; set; }
    public int? SortOrder { get; set; }
    public List<int>? AssigneeIds { get; set; }
}

public class UpdateTaskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? MilestoneId { get; set; }
    public int? ParentTaskId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? ProgressPercent { get; set; }
    public decimal? EstimatedHours { get; set; }
    public decimal? ActualHours { get; set; }
    public int? SortOrder { get; set; }
}

public class AddCommentRequest
{
    public string Body { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
}

public class SetAssigneesRequest
{
    public List<int> EmployeeIds { get; set; } = new();
}