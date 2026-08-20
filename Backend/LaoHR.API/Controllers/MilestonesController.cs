using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

public class MilestoneListItem
{
    public int MilestoneId { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int OpenTaskCount { get; set; }
    public int DoneTaskCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

[Authorize]
[ApiController]
[Route("api/projects/{projectId:int}/[controller]")]
public class MilestonesController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public MilestonesController(LaoHRDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List milestones for a project (paged).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<MilestoneListItem>>> GetMilestones(
        int projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Milestones
            .AsNoTracking()
            .Where(m => m.ProjectId == projectId);

        var total = await query.LongCountAsync();

        var raw = await query
            .OrderBy(m => m.DueDate ?? DateTime.MaxValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.MilestoneId,
                m.ProjectId,
                ProjectName = m.Project != null ? m.Project.Name : string.Empty,
                m.Name,
                m.Description,
                m.DueDate,
                m.CompletedAt,
                m.Status,
                m.CreatedAt,
            })
            .ToListAsync();

        var ids = raw.Select(r => r.MilestoneId).ToList();
        var counts = await _context.ProjectTasks
            .Where(t => t.MilestoneId.HasValue && ids.Contains(t.MilestoneId.Value))
            .GroupBy(t => new { t.MilestoneId, t.Status })
            .Select(g => new { g.Key.MilestoneId, g.Key.Status, Count = g.Count() })
            .ToListAsync();

        var items = raw.Select(m => new MilestoneListItem
        {
            MilestoneId = m.MilestoneId,
            ProjectId = m.ProjectId,
            ProjectName = m.ProjectName,
            Name = m.Name,
            Description = m.Description,
            DueDate = m.DueDate,
            CompletedAt = m.CompletedAt,
            Status = m.Status,
            CreatedAt = m.CreatedAt,
            OpenTaskCount = counts.Where(c => c.MilestoneId == m.MilestoneId && c.Status != "DONE" && c.Status != "CANCELLED").Sum(c => c.Count),
            DoneTaskCount = counts.Where(c => c.MilestoneId == m.MilestoneId && c.Status == "DONE").Sum(c => c.Count),
        }).ToList();

        return new PaginatedResponse<MilestoneListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    /// <summary>
    /// Create a milestone.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Milestone>> CreateMilestone(int projectId, [FromBody] CreateMilestoneRequest request)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound();

        var milestone = new Milestone
        {
            ProjectId = projectId,
            Name = request.Name,
            Description = request.Description,
            DueDate = request.DueDate,
            Status = request.Status ?? "OPEN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Milestones.Add(milestone);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMilestones), new { projectId }, milestone);
    }
}

public class CreateMilestoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Status { get; set; }
}