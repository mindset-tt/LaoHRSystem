using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

public class IssueListItem
{
    public int IssueId { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public string? TaskNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int ReporterId { get; set; }
    public string? ReporterName { get; set; }
    public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsOverdue { get; set; }
    public int CommentCount { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class IssueDetail
{
    public int IssueId { get; set; }
    public int ProjectId { get; set; }
    public int? TaskId { get; set; }
    public string? TaskNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int ReporterId { get; set; }
    public string? ReporterName { get; set; }
    public int? AssigneeId { get; set; }
    public string? AssigneeName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<IssueCommentDetail> Comments { get; set; } = new();
}

public class IssueCommentDetail
{
    public int IssueCommentId { get; set; }
    public int IssueId { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateIssueRequest
{
    public int? TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateIssueRequest
{
    public int? TaskId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
}

[Authorize]
[ApiController]
[Route("api/projects/{projectId:int}/[controller]")]
public class IssuesController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public IssuesController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<IssueListItem>>> GetIssues(
        int projectId,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] int? assigneeId = null,
        [FromQuery] bool mineOnly = false,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Issues.AsNoTracking().Where(i => i.ProjectId == projectId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);
        if (!string.IsNullOrEmpty(priority))
            query = query.Where(i => i.Priority == priority);
        if (assigneeId.HasValue)
            query = query.Where(i => i.AssigneeId == assigneeId.Value);
        if (mineOnly)
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId.HasValue)
                query = query.Where(i => i.AssigneeId == currentEmployeeId.Value || i.ReporterId == currentEmployeeId.Value);
        }
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(i => i.Title.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();

        var raw = await query
            .OrderByDescending(i => i.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new
            {
                i.IssueId,
                i.ProjectId,
                i.TaskId,
                TaskNumber = i.Task != null ? i.Task.TaskNumber : null,
                i.Title,
                i.Status,
                i.Priority,
                i.ReporterId,
                ReporterName = i.Reporter != null ? (i.Reporter.EnglishName ?? i.Reporter.LaoName) : null,
                i.AssigneeId,
                AssigneeName = i.Assignee != null ? (i.Assignee.EnglishName ?? i.Assignee.LaoName) : null,
                i.DueDate,
                i.ResolvedAt,
                i.UpdatedAt,
            })
            .ToListAsync();

        var ids = raw.Select(r => r.IssueId).ToList();
        var commentCounts = await _context.IssueComments
            .Where(c => ids.Contains(c.IssueId))
            .GroupBy(c => c.IssueId)
            .Select(g => new { IssueId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.IssueId, g => g.Count);

        var items = raw.Select(i => new IssueListItem
        {
            IssueId = i.IssueId,
            ProjectId = i.ProjectId,
            TaskId = i.TaskId,
            TaskNumber = i.TaskNumber,
            Title = i.Title,
            Status = i.Status,
            Priority = i.Priority,
            ReporterId = i.ReporterId,
            ReporterName = i.ReporterName,
            AssigneeId = i.AssigneeId,
            AssigneeName = i.AssigneeName,
            DueDate = i.DueDate,
            ResolvedAt = i.ResolvedAt,
            IsOverdue = i.DueDate.HasValue && i.DueDate.Value < DateTime.UtcNow && i.Status != "DONE" && i.Status != "CANCELLED",
            CommentCount = commentCounts.GetValueOrDefault(i.IssueId, 0),
            UpdatedAt = i.UpdatedAt,
        }).ToList();

        return new PaginatedResponse<IssueListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IssueDetail>> GetIssue(int projectId, int id)
    {
        var issue = await _context.Issues
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.IssueId == id && i.ProjectId == projectId);
        if (issue == null) return NotFound();

        var reporterName = await _context.Employees
            .Where(e => e.EmployeeId == issue.ReporterId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        string? assigneeName = null;
        if (issue.AssigneeId.HasValue)
        {
            assigneeName = await _context.Employees
                .Where(e => e.EmployeeId == issue.AssigneeId.Value)
                .Select(e => e.EnglishName ?? e.LaoName)
                .FirstOrDefaultAsync();
        }

        var taskNumber = issue.TaskId.HasValue
            ? await _context.ProjectTasks
                .Where(t => t.TaskId == issue.TaskId.Value)
                .Select(t => t.TaskNumber)
                .FirstOrDefaultAsync()
            : null;

        var comments = await _context.IssueComments
            .Where(c => c.IssueId == id)
            .OrderBy(c => c.CreatedAt)
            .Join(_context.Employees,
                c => c.AuthorId,
                e => e.EmployeeId,
                (c, e) => new IssueCommentDetail
                {
                    IssueCommentId = c.IssueCommentId,
                    IssueId = c.IssueId,
                    AuthorId = c.AuthorId,
                    AuthorName = e.EnglishName ?? e.LaoName,
                    Body = c.Body,
                    CreatedAt = c.CreatedAt,
                })
            .ToListAsync();

        return new IssueDetail
        {
            IssueId = issue.IssueId,
            ProjectId = issue.ProjectId,
            TaskId = issue.TaskId,
            TaskNumber = taskNumber,
            Title = issue.Title,
            Description = issue.Description,
            Status = issue.Status,
            Priority = issue.Priority,
            ReporterId = issue.ReporterId,
            ReporterName = reporterName,
            AssigneeId = issue.AssigneeId,
            AssigneeName = assigneeName,
            DueDate = issue.DueDate,
            ResolvedAt = issue.ResolvedAt,
            CreatedAt = issue.CreatedAt,
            UpdatedAt = issue.UpdatedAt,
            Comments = comments,
        };
    }

    [HttpPost]
    public async Task<ActionResult<Issue>> CreateIssue(int projectId, [FromBody] CreateIssueRequest request)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound("Project not found");

        var reporterId = GetCurrentEmployeeId() ?? 1;

        var issue = new Issue
        {
            ProjectId = projectId,
            TaskId = request.TaskId,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status ?? "TODO",
            Priority = request.Priority ?? "MEDIUM",
            ReporterId = reporterId,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"issueId\":{issue.IssueId},\"title\":\"{Escape(issue.Title)}\",\"action\":\"created\"}}");
        return CreatedAtAction(nameof(GetIssue), new { projectId, id = issue.IssueId }, issue);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateIssue(int projectId, int id, [FromBody] UpdateIssueRequest request)
    {
        var issue = await _context.Issues.FirstOrDefaultAsync(i => i.IssueId == id && i.ProjectId == projectId);
        if (issue == null) return NotFound();

        if (request.TaskId.HasValue) issue.TaskId = request.TaskId;
        if (request.Title != null) issue.Title = request.Title;
        if (request.Description != null) issue.Description = request.Description;
        if (request.Status != null) issue.Status = request.Status;
        if (request.Priority != null) issue.Priority = request.Priority;
        if (request.AssigneeId.HasValue) issue.AssigneeId = request.AssigneeId.Value == 0 ? null : request.AssigneeId;
        if (request.DueDate.HasValue) issue.DueDate = request.DueDate;

        if (request.Status == "DONE" && !issue.ResolvedAt.HasValue)
            issue.ResolvedAt = DateTime.UtcNow;
        if (request.Status != null && request.Status != "DONE" && issue.ResolvedAt.HasValue)
            issue.ResolvedAt = null;

        issue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"issueId\":{issue.IssueId},\"action\":\"updated\",\"status\":\"{issue.Status}\"}}");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteIssue(int projectId, int id)
    {
        var issue = await _context.Issues.FirstOrDefaultAsync(i => i.IssueId == id && i.ProjectId == projectId);
        if (issue == null) return NotFound();

        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"issueId\":{id},\"action\":\"deleted\"}}");
        return NoContent();
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<IssueCommentDetail>> AddComment(int projectId, int id, [FromBody] AddCommentRequest request)
    {
        var issue = await _context.Issues.FirstOrDefaultAsync(i => i.IssueId == id && i.ProjectId == projectId);
        if (issue == null) return NotFound("Issue not found");

        var authorId = GetCurrentEmployeeId() ?? 1;
        var comment = new IssueComment
        {
            IssueId = id,
            AuthorId = authorId,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.IssueComments.Add(comment);
        issue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var authorName = await _context.Employees
            .Where(e => e.EmployeeId == authorId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        await LogActivity(projectId, $"{{\"issueId\":{id},\"action\":\"commented\",\"commentId\":{comment.IssueCommentId}}}");

        return new IssueCommentDetail
        {
            IssueCommentId = comment.IssueCommentId,
            IssueId = comment.IssueId,
            AuthorId = comment.AuthorId,
            AuthorName = authorName ?? "Unknown",
            Body = comment.Body,
            CreatedAt = comment.CreatedAt,
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

    private async Task LogActivity(int projectId, string payloadJson)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            ProjectId = projectId,
            ActorId = GetCurrentEmployeeId() ?? 1,
            Action = "ISSUE_UPDATED",
            PayloadJson = payloadJson,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}

public class AddCommentRequest
{
    public string Body { get; set; } = string.Empty;
}