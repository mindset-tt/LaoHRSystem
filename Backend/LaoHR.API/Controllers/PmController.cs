using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 3C4 — PM planning endpoints: kanban board, dependencies, gantt,
/// capacity/workload, project health, portfolio, and RAID (assumptions/decisions).
/// </summary>
[Authorize]
[ApiController]
[Route("api/pm")]
public class PmController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IProjectAccessService _access;
    private readonly IPmPlanningService _planning;

    public PmController(LaoHRDbContext context, IProjectAccessService access, IPmPlanningService planning)
    {
        _context = context;
        _access = access;
        _planning = planning;
    }

    // ---- Kanban board ----

    /// <summary>Kanban board: tasks grouped by status, ordered by SortOrder.</summary>
    [HttpGet("projects/{projectId:int}/board")]
    public async Task<ActionResult<KanbanBoard>> GetBoard(int projectId)
    {
        if (!await _access.CanViewProjectAsync(projectId)) return Forbid();

        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.TaskId)
            .Select(t => new KanbanCard
            {
                TaskId = t.TaskId,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                DueDate = t.DueDate,
                ProgressPercent = t.ProgressPercent,
                MilestoneId = t.MilestoneId,
                ParentTaskId = t.ParentTaskId,
                IsOverdue = t.DueDate.HasValue && t.DueDate < DateTime.UtcNow && t.Status != "DONE" && t.Status != "CANCELLED",
                AssigneeNames = t.Assignees.Select(a => a.Employee != null ? (a.Employee.EnglishName ?? a.Employee.LaoName) : null).ToList(),
            })
            .ToListAsync();

        var columns = new[] { "TODO", "IN_PROGRESS", "BLOCKED", "REVIEW", "DONE" };
        var board = new KanbanBoard
        {
            Columns = columns.Select(c => new KanbanColumn
            {
                Status = c,
                Cards = tasks.Where(t => t.Status == c).ToList(),
            }).ToList(),
        };
        return Ok(board);
    }

    /// <summary>Move a task to a new status/order (server-validated).</summary>
    [HttpPost("projects/{projectId:int}/board/move")]
    public async Task<IActionResult> MoveTask(int projectId, [FromBody] MoveTaskRequest request)
    {
        if (!await _access.CanManageTasksAsync(projectId)) return Forbid();

        var task = await _context.ProjectTasks
            .FirstOrDefaultAsync(t => t.TaskId == request.TaskId && t.ProjectId == projectId);
        if (task == null) return NotFound();

        var validStatuses = new[] { "TODO", "IN_PROGRESS", "BLOCKED", "REVIEW", "DONE", "CANCELLED" };
        if (!validStatuses.Contains(request.Status))
            return BadRequest($"Invalid status: {request.Status}");

        var statusBefore = task.Status;
        task.Status = request.Status;
        if (request.SortOrder.HasValue) task.SortOrder = request.SortOrder.Value;

        if (request.Status == "DONE" && !task.CompletedAt.HasValue)
            task.CompletedAt = DateTime.UtcNow;
        if (request.Status != "DONE" && task.CompletedAt.HasValue)
            task.CompletedAt = null;

        task.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (statusBefore != request.Status)
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                ProjectId = projectId,
                TaskId = task.TaskId,
                ActorId = _access.GetCurrentEmployeeId() ?? 1,
                Action = "UPDATED_STATUS",
                PayloadJson = $"{{\"from\":\"{statusBefore}\",\"to\":\"{task.Status}\"}}",
            });
            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    // ---- Dependencies ----

    /// <summary>List dependencies for a project.</summary>
    [HttpGet("projects/{projectId:int}/dependencies")]
    public async Task<ActionResult<List<DependencyDto>>> GetDependencies(int projectId)
    {
        if (!await _access.CanViewProjectAsync(projectId)) return Forbid();

        return await _context.TaskDependencies
            .AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .Select(d => new DependencyDto
            {
                TaskDependencyId = d.TaskDependencyId,
                PredecessorTaskId = d.PredecessorTaskId,
                SuccessorTaskId = d.SuccessorTaskId,
                Type = d.Type,
            })
            .ToListAsync();
    }

    /// <summary>Add a finish-to-start dependency (cycle-validated).</summary>
    [HttpPost("projects/{projectId:int}/dependencies")]
    public async Task<ActionResult<DependencyDto>> AddDependency(int projectId, [FromBody] AddDependencyRequest request)
    {
        if (!await _access.CanManageTasksAsync(projectId)) return Forbid();

        if (request.PredecessorTaskId == request.SuccessorTaskId)
            return BadRequest("A task cannot depend on itself.");

        var bothExist = await _context.ProjectTasks
            .CountAsync(t => t.ProjectId == projectId
                && (t.TaskId == request.PredecessorTaskId || t.TaskId == request.SuccessorTaskId)) == 2;
        if (!bothExist) return BadRequest("Both tasks must belong to this project.");

        var duplicate = await _context.TaskDependencies
            .AnyAsync(d => d.ProjectId == projectId
                && d.PredecessorTaskId == request.PredecessorTaskId
                && d.SuccessorTaskId == request.SuccessorTaskId);
        if (duplicate) return Conflict("Dependency already exists.");

        if (await _planning.WouldCreateDependencyCycleAsync(projectId, request.PredecessorTaskId, request.SuccessorTaskId))
            return BadRequest("Adding this dependency would create a cycle.");

        var dep = new TaskDependency
        {
            ProjectId = projectId,
            PredecessorTaskId = request.PredecessorTaskId,
            SuccessorTaskId = request.SuccessorTaskId,
            Type = "FS",
        };
        _context.TaskDependencies.Add(dep);
        await _context.SaveChangesAsync();

        return Ok(new DependencyDto
        {
            TaskDependencyId = dep.TaskDependencyId,
            PredecessorTaskId = dep.PredecessorTaskId,
            SuccessorTaskId = dep.SuccessorTaskId,
            Type = dep.Type,
        });
    }

    /// <summary>Remove a dependency.</summary>
    [HttpDelete("projects/{projectId:int}/dependencies/{dependencyId:int}")]
    public async Task<IActionResult> RemoveDependency(int projectId, int dependencyId)
    {
        if (!await _access.CanManageTasksAsync(projectId)) return Forbid();

        var dep = await _context.TaskDependencies
            .FirstOrDefaultAsync(d => d.TaskDependencyId == dependencyId && d.ProjectId == projectId);
        if (dep == null) return NotFound();

        _context.TaskDependencies.Remove(dep);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---- Gantt / timeline ----

    /// <summary>Gantt data: tasks (with dates) + milestones for a project.</summary>
    [HttpGet("projects/{projectId:int}/timeline")]
    public async Task<ActionResult<GanttData>> GetTimeline(int projectId)
    {
        if (!await _access.CanViewProjectAsync(projectId)) return Forbid();

        var tasks = await _context.ProjectTasks
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .Select(t => new GanttTask
            {
                TaskId = t.TaskId,
                Title = t.Title,
                ParentTaskId = t.ParentTaskId,
                MilestoneId = t.MilestoneId,
                StartDate = t.StartDate,
                DueDate = t.DueDate,
                ProgressPercent = t.ProgressPercent,
                Status = t.Status,
            })
            .ToListAsync();

        var milestones = await _context.Milestones
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId)
            .Select(m => new GanttMilestone
            {
                MilestoneId = m.MilestoneId,
                Name = m.Name,
                DueDate = m.DueDate,
                Status = m.Status,
            })
            .ToListAsync();

        var dependencies = await _context.TaskDependencies
            .AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .Select(d => new DependencyDto
            {
                TaskDependencyId = d.TaskDependencyId,
                PredecessorTaskId = d.PredecessorTaskId,
                SuccessorTaskId = d.SuccessorTaskId,
                Type = d.Type,
            })
            .ToListAsync();

        return Ok(new GanttData { Tasks = tasks, Milestones = milestones, Dependencies = dependencies });
    }

    // ---- Capacity / workload ----

    /// <summary>Resource workload across all projects (Admin/HR or any manager).</summary>
    [HttpGet("capacity")]
    public async Task<ActionResult<List<ResourceWorkload>>> GetCapacity()
    {
        return Ok(await _planning.GetResourceWorkloadAsync());
    }

    // ---- Project health ----

    /// <summary>Project health (schedule/risk/overall).</summary>
    [HttpGet("projects/{projectId:int}/health")]
    public async Task<ActionResult<ProjectHealth>> GetHealth(int projectId)
    {
        if (!await _access.CanViewProjectAsync(projectId)) return Forbid();
        return Ok(await _planning.GetProjectHealthAsync(projectId));
    }

    // ---- Portfolio ----

    /// <summary>Portfolio overview of all non-cancelled projects.</summary>
    [HttpGet("portfolio")]
    public async Task<ActionResult<List<PortfolioItem>>> GetPortfolio()
    {
        return Ok(await _planning.GetPortfolioAsync());
    }

    // ---- RAID: assumptions ----

    [HttpGet("projects/{projectId:int}/assumptions")]
    public async Task<ActionResult<List<ProjectAssumption>>> GetAssumptions(int projectId)
    {
        if (!await _access.CanViewProjectAsync(projectId)) return Forbid();
        return await _context.ProjectAssumptions
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    [HttpPost("projects/{projectId:int}/assumptions")]
    public async Task<ActionResult<ProjectAssumption>> AddAssumption(int projectId, [FromBody] AddAssumptionRequest request)
    {
        if (!await _access.CanManageRisksAsync(projectId)) return Forbid();
        var a = new ProjectAssumption
        {
            ProjectId = projectId,
            Description = request.Description,
            OwnerId = request.OwnerId,
            Status = "OPEN",
        };
        _context.ProjectAssumptions.Add(a);
        await _context.SaveChangesAsync();
        return Ok(a);
    }

    // ---- RAID: decisions ----

    [HttpGet("projects/{projectId:int}/decisions")]
    public async Task<ActionResult<List<ProjectDecision>>> GetDecisions(int projectId)
    {
        if (!await _access.CanViewProjectAsync(projectId)) return Forbid();
        return await _context.ProjectDecisions
            .AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .OrderByDescending(d => d.DecisionDate ?? d.CreatedAt)
            .ToListAsync();
    }

    [HttpPost("projects/{projectId:int}/decisions")]
    public async Task<ActionResult<ProjectDecision>> AddDecision(int projectId, [FromBody] AddDecisionRequest request)
    {
        if (!await _access.CanManageRisksAsync(projectId)) return Forbid();
        var d = new ProjectDecision
        {
            ProjectId = projectId,
            Decision = request.Decision,
            Context = request.Context,
            OwnerId = request.OwnerId,
            DecisionDate = request.DecisionDate ?? DateTime.UtcNow,
        };
        _context.ProjectDecisions.Add(d);
        await _context.SaveChangesAsync();
        return Ok(d);
    }
}

// ---- DTOs ----

public sealed class KanbanCard
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int ProgressPercent { get; set; }
    public int? MilestoneId { get; set; }
    public int? ParentTaskId { get; set; }
    public bool IsOverdue { get; set; }
    public List<string?> AssigneeNames { get; set; } = new();
}

public sealed class KanbanColumn
{
    public string Status { get; set; } = string.Empty;
    public List<KanbanCard> Cards { get; set; } = new();
}

public sealed class KanbanBoard
{
    public List<KanbanColumn> Columns { get; set; } = new();
}

public sealed class MoveTaskRequest
{
    public int TaskId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? SortOrder { get; set; }
}

public sealed class DependencyDto
{
    public int TaskDependencyId { get; set; }
    public int PredecessorTaskId { get; set; }
    public int SuccessorTaskId { get; set; }
    public string Type { get; set; } = "FS";
}

public sealed class AddDependencyRequest
{
    public int PredecessorTaskId { get; set; }
    public int SuccessorTaskId { get; set; }
}

public sealed class GanttTask
{
    public int TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? ParentTaskId { get; set; }
    public int? MilestoneId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProgressPercent { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class GanttMilestone
{
    public int MilestoneId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class GanttData
{
    public List<GanttTask> Tasks { get; set; } = new();
    public List<GanttMilestone> Milestones { get; set; } = new();
    public List<DependencyDto> Dependencies { get; set; } = new();
}

public sealed class AddAssumptionRequest
{
    public string Description { get; set; } = string.Empty;
    public int? OwnerId { get; set; }
}

public sealed class AddDecisionRequest
{
    public string Decision { get; set; } = string.Empty;
    public string? Context { get; set; }
    public int? OwnerId { get; set; }
    public DateTime? DecisionDate { get; set; }
}
