using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C4 — PM planning operations: task hierarchy, dependencies, kanban
/// ordering, capacity/workload, project health, and portfolio. All operations
/// validate authorization and graph invariants server-side.
/// </summary>
public interface IPmPlanningService
{
    // Task hierarchy
    Task<bool> WouldCreateTaskCycleAsync(int taskId, int? parentTaskId, CancellationToken ct = default);

    // Dependencies
    Task<bool> WouldCreateDependencyCycleAsync(int projectId, int predecessorTaskId, int successorTaskId, CancellationToken ct = default);

    // Capacity / workload
    Task<List<ResourceWorkload>> GetResourceWorkloadAsync(CancellationToken ct = default);

    // Project health
    Task<ProjectHealth> GetProjectHealthAsync(int projectId, CancellationToken ct = default);

    // Portfolio
    Task<List<PortfolioItem>> GetPortfolioAsync(CancellationToken ct = default);
}

public sealed class ResourceWorkload
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int TotalAllocationPercent { get; set; }
    public bool IsOverallocated { get; set; }
    public int ActiveProjectCount { get; set; }
}

public sealed class ProjectHealth
{
    public int ProjectId { get; set; }
    public string ScheduleHealth { get; set; } = "ON_TRACK";   // ON_TRACK, AT_RISK, DELAYED
    public string RiskHealth { get; set; } = "GREEN";          // GREEN, AMBER, RED
    public string OverallHealth { get; set; } = "GREEN";       // GREEN, AMBER, RED
    public int OverdueTaskCount { get; set; }
    public int OpenCriticalRiskCount { get; set; }
    public int OpenIssueCount { get; set; }
    public int ProgressPercent { get; set; }
}

public sealed class PortfolioItem
{
    public int ProjectId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? OwnerName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProgressPercent { get; set; }
    public string Health { get; set; } = "GREEN";
    public int OpenRiskCount { get; set; }
    public int OverdueTaskCount { get; set; }
    public int OpenIssueCount { get; set; }
    public DateTime? NextMilestoneDate { get; set; }
}

public sealed class PmPlanningService : IPmPlanningService
{
    private readonly LaoHRDbContext _context;

    public PmPlanningService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<bool> WouldCreateTaskCycleAsync(int taskId, int? parentTaskId, CancellationToken ct = default)
    {
        if (parentTaskId == null) return false;
        if (parentTaskId.Value == taskId) return true;

        // Walk up the parent chain from the proposed parent; if we reach taskId, it's a cycle.
        var visited = new HashSet<int>();
        int? current = parentTaskId;
        while (current.HasValue && visited.Add(current.Value))
        {
            if (current.Value == taskId) return true;
            current = await _context.ProjectTasks
                .AsNoTracking()
                .Where(t => t.TaskId == current.Value)
                .Select(t => t.ParentTaskId)
                .FirstOrDefaultAsync(ct);
        }
        return false;
    }

    public async Task<bool> WouldCreateDependencyCycleAsync(int projectId, int predecessorTaskId, int successorTaskId, CancellationToken ct = default)
    {
        if (predecessorTaskId == successorTaskId) return true;

        // Build the dependency graph for the project and detect a cycle if we add
        // predecessor → successor. A cycle exists if successor can already reach predecessor.
        var edges = await _context.TaskDependencies
            .AsNoTracking()
            .Where(d => d.ProjectId == projectId)
            .Select(d => new { d.PredecessorTaskId, d.SuccessorTaskId })
            .ToListAsync(ct);

        var adjacency = new Dictionary<int, List<int>>();
        foreach (var e in edges)
        {
            if (!adjacency.ContainsKey(e.PredecessorTaskId)) adjacency[e.PredecessorTaskId] = new List<int>();
            adjacency[e.PredecessorTaskId].Add(e.SuccessorTaskId);
        }
        // Add the proposed edge.
        if (!adjacency.ContainsKey(predecessorTaskId)) adjacency[predecessorTaskId] = new List<int>();
        adjacency[predecessorTaskId].Add(successorTaskId);

        // DFS from successorTaskId: if we can reach predecessorTaskId, adding the edge creates a cycle.
        var stack = new Stack<int>();
        var seen = new HashSet<int>();
        stack.Push(successorTaskId);
        while (stack.Count > 0)
        {
            var node = stack.Pop();
            if (node == predecessorTaskId) return true;
            if (!seen.Add(node)) continue;
            if (adjacency.TryGetValue(node, out var neighbors))
            {
                foreach (var n in neighbors) stack.Push(n);
            }
        }
        return false;
    }

    public async Task<List<ResourceWorkload>> GetResourceWorkloadAsync(CancellationToken ct = default)
    {
        // Aggregate active allocations per employee in a single query (no N+1).
        var allocations = await _context.Resources
            .AsNoTracking()
            .Where(r => r.EndDate == null || r.EndDate >= DateTime.UtcNow)
            .GroupBy(r => r.EmployeeId)
            .Select(g => new
            {
                EmployeeId = g.Key,
                TotalAllocation = g.Sum(x => x.AllocationPercent),
                ActiveProjects = g.Count()
            })
            .ToListAsync(ct);

        var employeeIds = allocations.Select(a => a.EmployeeId).ToList();
        var names = await _context.Employees
            .AsNoTracking()
            .Where(e => employeeIds.Contains(e.EmployeeId))
            .Select(e => new { e.EmployeeId, Name = e.EnglishName ?? e.LaoName })
            .ToDictionaryAsync(e => e.EmployeeId, e => e.Name, ct);

        return allocations
            .Select(a => new ResourceWorkload
            {
                EmployeeId = a.EmployeeId,
                EmployeeName = names.GetValueOrDefault(a.EmployeeId, $"#{a.EmployeeId}"),
                TotalAllocationPercent = a.TotalAllocation,
                IsOverallocated = a.TotalAllocation > 100,
                ActiveProjectCount = a.ActiveProjects,
            })
            .OrderByDescending(w => w.TotalAllocationPercent)
            .ToList();
    }

    public async Task<ProjectHealth> GetProjectHealthAsync(int projectId, CancellationToken ct = default)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == projectId, ct);

        var overdueTasks = await _context.ProjectTasks
            .CountAsync(t => t.ProjectId == projectId && t.DueDate != null && t.DueDate < DateTime.UtcNow
                && t.Status != "DONE" && t.Status != "CANCELLED", ct);

        var openCriticalRisks = await _context.Risks
            .CountAsync(r => r.ProjectId == projectId && r.Status == "OPEN" && r.Priority == "CRITICAL", ct);

        var openIssues = await _context.Issues
            .CountAsync(i => i.ProjectId == projectId && i.Status != "DONE" && i.Status != "CANCELLED", ct);

        var totalTasks = await _context.ProjectTasks.CountAsync(t => t.ProjectId == projectId, ct);
        var doneTasks = await _context.ProjectTasks.CountAsync(t => t.ProjectId == projectId && t.Status == "DONE", ct);
        var progress = totalTasks == 0 ? 0 : (int)Math.Round(doneTasks * 100.0 / totalTasks);

        // Schedule health: DELAYED if project due date passed and not completed;
        // AT_RISK if any overdue task; else ON_TRACK.
        var scheduleHealth = "ON_TRACK";
        if (project != null && project.DueDate.HasValue && project.DueDate < DateTime.UtcNow && project.Status != "COMPLETED")
            scheduleHealth = "DELAYED";
        else if (overdueTasks > 0)
            scheduleHealth = "AT_RISK";

        // Risk health: RED if critical open risks, AMBER if any open risk, else GREEN.
        var riskHealth = "GREEN";
        if (openCriticalRisks > 0) riskHealth = "RED";
        else if (await _context.Risks.AnyAsync(r => r.ProjectId == projectId && r.Status == "OPEN", ct)) riskHealth = "AMBER";

        // Overall: RED if any dimension red, AMBER if any amber, else GREEN.
        var overall = "GREEN";
        if (scheduleHealth == "DELAYED" || riskHealth == "RED") overall = "RED";
        else if (scheduleHealth == "AT_RISK" || riskHealth == "AMBER") overall = "AMBER";

        return new ProjectHealth
        {
            ProjectId = projectId,
            ScheduleHealth = scheduleHealth,
            RiskHealth = riskHealth,
            OverallHealth = overall,
            OverdueTaskCount = overdueTasks,
            OpenCriticalRiskCount = openCriticalRisks,
            OpenIssueCount = openIssues,
            ProgressPercent = progress,
        };
    }

    public async Task<List<PortfolioItem>> GetPortfolioAsync(CancellationToken ct = default)
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => p.Status != "CANCELLED")
            .Select(p => new
            {
                p.ProjectId,
                p.Code,
                p.Name,
                p.Status,
                p.Priority,
                p.OwnerId,
                p.StartDate,
                p.DueDate,
            })
            .OrderBy(p => p.Status).ThenBy(p => p.Name)
            .ToListAsync(ct);

        var ids = projects.Select(p => p.ProjectId).ToList();

        var ownerIds = projects.Select(p => p.OwnerId).Distinct().ToList();
        var ownerNames = await _context.Employees
            .AsNoTracking()
            .Where(e => ownerIds.Contains(e.EmployeeId))
            .Select(e => new { e.EmployeeId, Name = e.EnglishName ?? e.LaoName })
            .ToDictionaryAsync(e => e.EmployeeId, e => e.Name, ct);

        var taskAgg = await _context.ProjectTasks
            .Where(t => ids.Contains(t.ProjectId))
            .GroupBy(t => t.ProjectId)
            .Select(g => new
            {
                ProjectId = g.Key,
                Total = g.Count(),
                Done = g.Sum(t => t.Status == "DONE" ? 1 : 0),
                Overdue = g.Sum(t => (t.DueDate != null && t.DueDate < DateTime.UtcNow && t.Status != "DONE" && t.Status != "CANCELLED") ? 1 : 0),
            })
            .ToDictionaryAsync(g => g.ProjectId, ct);

        var riskAgg = await _context.Risks
            .Where(r => ids.Contains(r.ProjectId) && r.Status == "OPEN")
            .GroupBy(r => r.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ProjectId, g => g.Count, ct);

        var issueAgg = await _context.Issues
            .Where(i => ids.Contains(i.ProjectId) && i.Status != "DONE" && i.Status != "CANCELLED")
            .GroupBy(i => i.ProjectId)
            .Select(g => new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.ProjectId, g => g.Count, ct);

        var nextMilestone = await _context.Milestones
            .Where(m => ids.Contains(m.ProjectId) && m.Status == "OPEN" && m.DueDate != null)
            .GroupBy(m => m.ProjectId)
            .Select(g => new { ProjectId = g.Key, Next = g.Min(m => m.DueDate!.Value) })
            .ToDictionaryAsync(g => g.ProjectId, g => (DateTime?)g.Next, ct);

        return projects.Select(p =>
        {
            taskAgg.TryGetValue(p.ProjectId, out var t);
            var overdue = t?.Overdue ?? 0;
            var openRisks = riskAgg.GetValueOrDefault(p.ProjectId, 0);
            var openIssues = issueAgg.GetValueOrDefault(p.ProjectId, 0);
            var progress = (t?.Total ?? 0) == 0 ? 0 : (int)Math.Round((t?.Done ?? 0) * 100.0 / (t?.Total ?? 1));

            var health = "GREEN";
            if (overdue > 0 || openRisks > 0) health = "AMBER";
            if (p.DueDate.HasValue && p.DueDate < DateTime.UtcNow && p.Status != "COMPLETED") health = "RED";

            return new PortfolioItem
            {
                ProjectId = p.ProjectId,
                Code = p.Code,
                Name = p.Name,
                Status = p.Status,
                Priority = p.Priority,
                OwnerName = ownerNames.GetValueOrDefault(p.OwnerId),
                StartDate = p.StartDate,
                DueDate = p.DueDate,
                ProgressPercent = progress,
                Health = health,
                OpenRiskCount = openRisks,
                OverdueTaskCount = overdue,
                OpenIssueCount = openIssues,
                NextMilestoneDate = nextMilestone.GetValueOrDefault(p.ProjectId),
            };
        }).ToList();
    }
}
