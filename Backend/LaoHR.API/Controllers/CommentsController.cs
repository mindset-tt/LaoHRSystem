using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Controllers;

public class CommentItem
{
    public int EntityCommentId { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? ParentCommentId { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<CommentItem> Replies { get; set; } = new();
}

public class CreateCommentRequest
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int? ParentCommentId { get; set; }
    public string Body { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    public string Body { get; set; } = string.Empty;
}

public class CommentThreadSummary
{
    public string EntityType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int TotalComments { get; set; }
}

/// <summary>
/// Polymorphic comment thread. Single endpoint serves all features (project
/// tasks, issues, expenses, loans, etc.) by (entityType, entityId) pair.
/// Threading is one level deep via optional parentCommentId.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private static readonly HashSet<string> AllowedEntityTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "PROJECT", "TASK", "ISSUE", "EXPENSE", "LOAN", "RISK", "RESOURCE",
        // Phase 4C — corporate operations
        "CONTRACT", "SERVICE_REQUEST", "WORK_ORDER", "TRAVEL_REQUEST", "VEHICLE",
    };

    private readonly LaoHRDbContext _context;

    public CommentsController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet("{entityType}/{entityId:int}")]
    public async Task<ActionResult<List<CommentItem>>> GetThread(string entityType, int entityId)
    {
        if (!AllowedEntityTypes.Contains(entityType))
            return BadRequest($"Unsupported entityType '{entityType}'");

        var rows = await _context.EntityComments
            .AsNoTracking()
            .Where(c => c.EntityType.ToUpper() == entityType.ToUpper() && c.EntityId == entityId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        var authorIds = rows.Select(r => r.AuthorId).Distinct().ToList();
        var authors = await _context.Employees
            .AsNoTracking()
            .Where(e => authorIds.Contains(e.EmployeeId))
            .Select(e => new { e.EmployeeId, Name = e.EnglishName ?? e.LaoName })
            .ToDictionaryAsync(e => e.EmployeeId, e => e.Name);

        // Single round-trip: load everything, then assemble parents and replies in memory.
        var byId = rows.ToDictionary(r => r.EntityCommentId);
        var roots = new List<CommentItem>();
        foreach (var r in rows.Where(r => r.ParentCommentId == null))
        {
            roots.Add(ToItem(r, authors));
        }
        foreach (var r in rows.Where(r => r.ParentCommentId != null))
        {
            // Reply is attached to its parent's Replies list. Parent is always loaded.
            if (byId.TryGetValue(r.ParentCommentId!.Value, out var parent))
            {
                // Recompute parent item with replies seeded.
                var parentItem = ToItem(parent, authors);
                var existing = roots.FirstOrDefault(x => x.EntityCommentId == parent.EntityCommentId);
                if (existing != null) existing.Replies.Add(ToItem(r, authors));
                else
                {
                    parentItem.Replies.Add(ToItem(r, authors));
                    roots.Add(parentItem);
                }
            }
        }
        return roots;
    }

    [HttpGet("{entityType}/{entityId:int}/summary")]
    public async Task<ActionResult<CommentThreadSummary>> GetSummary(string entityType, int entityId)
    {
        if (!AllowedEntityTypes.Contains(entityType))
            return BadRequest($"Unsupported entityType '{entityType}'");
        var count = await _context.EntityComments
            .Where(c => c.EntityType.ToUpper() == entityType.ToUpper() && c.EntityId == entityId && c.DeletedAt == null)
            .LongCountAsync();
        return new CommentThreadSummary { EntityType = entityType, EntityId = entityId, TotalComments = (int)count };
    }

    [HttpPost]
    public async Task<ActionResult<CommentItem>> AddComment([FromBody] CreateCommentRequest request)
    {
        if (!AllowedEntityTypes.Contains(request.EntityType))
            return BadRequest($"Unsupported entityType '{request.EntityType}'");
        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("Body cannot be empty");
        if (request.Body.Length > 4000)
            return BadRequest("Body exceeds 4000 chars");

        var authorId = GetCurrentEmployeeId();
        if (authorId == null) return Forbid();

        if (request.ParentCommentId.HasValue)
        {
            var parent = await _context.EntityComments
                .FirstOrDefaultAsync(c => c.EntityCommentId == request.ParentCommentId.Value);
            if (parent == null || parent.EntityId != request.EntityId || !string.Equals(parent.EntityType, request.EntityType, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Parent comment not found in same thread");
        }

        var c = new EntityComment
        {
            EntityType = request.EntityType.ToUpper(),
            EntityId = request.EntityId,
            ParentCommentId = request.ParentCommentId,
            AuthorId = authorId.Value,
            Body = request.Body,
            CreatedAt = DateTime.UtcNow,
        };
        _context.EntityComments.Add(c);
        await _context.SaveChangesAsync();

        var authorName = await _context.Employees
            .Where(e => e.EmployeeId == c.AuthorId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        return new CommentItem
        {
            EntityCommentId = c.EntityCommentId,
            EntityType = c.EntityType,
            EntityId = c.EntityId,
            ParentCommentId = c.ParentCommentId,
            AuthorId = c.AuthorId,
            AuthorName = authorName,
            Body = c.Body,
            CreatedAt = c.CreatedAt,
        };
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] UpdateCommentRequest request)
    {
        var c = await _context.EntityComments.FirstOrDefaultAsync(x => x.EntityCommentId == id);
        if (c == null) return NotFound();

        var authorId = GetCurrentEmployeeId();
        if (authorId == null || c.AuthorId != authorId.Value) return Forbid();

        if (c.DeletedAt != null) return BadRequest("Comment was deleted");

        c.Body = request.Body;
        c.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var c = await _context.EntityComments.FirstOrDefaultAsync(x => x.EntityCommentId == id);
        if (c == null) return NotFound();

        var authorId = GetCurrentEmployeeId();
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("HR");
        if (authorId == null || (c.AuthorId != authorId.Value && !isAdmin)) return Forbid();

        // Soft-delete to preserve thread structure.
        c.DeletedAt = DateTime.UtcNow;
        c.Body = "[deleted]";
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static CommentItem ToItem(EntityComment c, Dictionary<int, string> authors)
    {
        return new CommentItem
        {
            EntityCommentId = c.EntityCommentId,
            EntityType = c.EntityType,
            EntityId = c.EntityId,
            ParentCommentId = c.ParentCommentId,
            AuthorId = c.AuthorId,
            AuthorName = authors.TryGetValue(c.AuthorId, out var n) ? n : null,
            Body = c.Body,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            DeletedAt = c.DeletedAt,
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
}