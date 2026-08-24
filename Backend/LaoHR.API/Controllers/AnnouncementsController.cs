using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

public class AnnouncementListItem
{
    public int AnnouncementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleLao { get; set; }
    public string Severity { get; set; } = "INFO";
    public string Audience { get; set; } = "ALL";
    public bool IsPinned { get; set; }
    public DateTime? PublishFrom { get; set; }
    public DateTime? PublishUntil { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class AnnouncementDetail
{
    public int AnnouncementId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TitleLao { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? BodyLao { get; set; }
    public string Severity { get; set; } = "INFO";
    public string Audience { get; set; } = "ALL";
    public int? AudienceRoleId { get; set; }
    public int? AudienceDepartmentId { get; set; }
    public bool IsPinned { get; set; }
    public DateTime? PublishFrom { get; set; }
    public DateTime? PublishUntil { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsRead { get; set; }
}

public class CreateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string? TitleLao { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? BodyLao { get; set; }
    public string Severity { get; set; } = "INFO";
    public string Audience { get; set; } = "ALL";
    public int? AudienceDepartmentId { get; set; }
    public bool IsPinned { get; set; } = false;
    public DateTime? PublishFrom { get; set; }
    public DateTime? PublishUntil { get; set; }
}

public class UpdateAnnouncementRequest
{
    public string? Title { get; set; }
    public string? TitleLao { get; set; }
    public string? Body { get; set; }
    public string? BodyLao { get; set; }
    public string? Severity { get; set; }
    public string? Audience { get; set; }
    public int? AudienceDepartmentId { get; set; }
    public bool? IsPinned { get; set; }
    public DateTime? PublishFrom { get; set; }
    public DateTime? PublishUntil { get; set; }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public AnnouncementsController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AnnouncementListItem>>> GetAnnouncements(
        [FromQuery] string? severity = null,
        [FromQuery] bool? unreadOnly = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var now = DateTime.UtcNow;
        var employeeId = GetCurrentEmployeeId();

        var query = _context.Announcements.AsNoTracking().AsQueryable();

        // Only return items currently published.
        query = query.Where(a =>
            (a.PublishFrom == null || a.PublishFrom <= now) &&
            (a.PublishUntil == null || a.PublishUntil >= now));

        if (!string.IsNullOrEmpty(severity))
            query = query.Where(a => a.Severity == severity);

        var total = await query.LongCountAsync();

        // Single round-trip for read state of the current user across the page.
        var ids = await query
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => a.AnnouncementId)
            .ToListAsync();

        var readIds = employeeId.HasValue
            ? await _context.AnnouncementReads
                .AsNoTracking()
                .Where(ar => ar.EmployeeId == employeeId.Value && ids.Contains(ar.AnnouncementId))
                .Select(ar => ar.AnnouncementId)
                .ToListAsync()
            : new List<int>();

        var readSet = readIds.ToHashSet();

        var items = await query
            .Where(a => ids.Contains(a.AnnouncementId))
            .OrderByDescending(a => a.IsPinned)
            .ThenByDescending(a => a.CreatedAt)
            .Select(a => new AnnouncementListItem
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                TitleLao = a.TitleLao,
                Severity = a.Severity,
                Audience = a.Audience,
                IsPinned = a.IsPinned,
                PublishFrom = a.PublishFrom,
                PublishUntil = a.PublishUntil,
                AuthorId = a.AuthorId,
                AuthorName = a.Author != null ? (a.Author.EnglishName ?? a.Author.LaoName) : null,
                CreatedAt = a.CreatedAt,
                IsRead = readSet.Contains(a.AnnouncementId),
            })
            .ToListAsync();

        if (unreadOnly == true)
            items = items.Where(i => !i.IsRead).ToList();

        return new PaginatedResponse<AnnouncementListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AnnouncementDetail>> GetAnnouncement(int id)
    {
        var a = await _context.Announcements.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AnnouncementId == id);
        if (a == null) return NotFound();

        var author = await _context.Employees
            .Where(e => e.EmployeeId == a.AuthorId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        var employeeId = GetCurrentEmployeeId();
        var isRead = false;
        if (employeeId.HasValue)
        {
            isRead = await _context.AnnouncementReads
                .AnyAsync(ar => ar.AnnouncementId == id && ar.EmployeeId == employeeId.Value);
        }

        return new AnnouncementDetail
        {
            AnnouncementId = a.AnnouncementId,
            Title = a.Title,
            TitleLao = a.TitleLao,
            Body = a.Body,
            BodyLao = a.BodyLao,
            Severity = a.Severity,
            Audience = a.Audience,
            AudienceDepartmentId = a.AudienceDepartmentId,
            IsPinned = a.IsPinned,
            PublishFrom = a.PublishFrom,
            PublishUntil = a.PublishUntil,
            AuthorId = a.AuthorId,
            AuthorName = author,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            IsRead = isRead,
        };
    }

    [HttpPost]
    [Authorize(Roles = "Admin,HR")]
    public async Task<ActionResult<Announcement>> CreateAnnouncement([FromBody] CreateAnnouncementRequest request)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == null) return Forbid();

        var a = new Announcement
        {
            Title = request.Title,
            TitleLao = request.TitleLao,
            Body = request.Body,
            BodyLao = request.BodyLao,
            Severity = request.Severity,
            Audience = request.Audience,
            AudienceDepartmentId = request.AudienceDepartmentId,
            IsPinned = request.IsPinned,
            PublishFrom = request.PublishFrom,
            PublishUntil = request.PublishUntil,
            AuthorId = employeeId.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Announcements.Add(a);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAnnouncement), new { id = a.AnnouncementId }, a);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> UpdateAnnouncement(int id, [FromBody] UpdateAnnouncementRequest request)
    {
        var a = await _context.Announcements.FirstOrDefaultAsync(x => x.AnnouncementId == id);
        if (a == null) return NotFound();

        if (request.Title != null) a.Title = request.Title;
        if (request.TitleLao != null) a.TitleLao = request.TitleLao;
        if (request.Body != null) a.Body = request.Body;
        if (request.BodyLao != null) a.BodyLao = request.BodyLao;
        if (request.Severity != null) a.Severity = request.Severity;
        if (request.Audience != null) a.Audience = request.Audience;
        if (request.AudienceDepartmentId.HasValue) a.AudienceDepartmentId = request.AudienceDepartmentId;
        if (request.IsPinned.HasValue) a.IsPinned = request.IsPinned.Value;
        if (request.PublishFrom.HasValue) a.PublishFrom = request.PublishFrom;
        if (request.PublishUntil.HasValue) a.PublishUntil = request.PublishUntil;

        a.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,HR")]
    public async Task<IActionResult> DeleteAnnouncement(int id)
    {
        var a = await _context.Announcements.FirstOrDefaultAsync(x => x.AnnouncementId == id);
        if (a == null) return NotFound();
        _context.Announcements.Remove(a);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == null) return Forbid();

        var exists = await _context.AnnouncementReads
            .AnyAsync(ar => ar.AnnouncementId == id && ar.EmployeeId == employeeId.Value);
        if (exists) return NoContent();

        _context.AnnouncementReads.Add(new AnnouncementRead
        {
            AnnouncementId = id,
            EmployeeId = employeeId.Value,
            ReadAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
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
}