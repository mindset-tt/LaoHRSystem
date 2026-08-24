using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 3C2 — notification center. In-app notifications for the current user.
/// </summary>
[Authorize]
[ApiController]
[Route("api/notifications")]
public class NotificationsController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public NotificationsController(LaoHRDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var userIdStr = User.FindFirst("UserId")?.Value;
        if (int.TryParse(userIdStr, out var userId)) return userId;
        // Fallback: resolve via username
        var username = User.Identity?.Name;
        if (!string.IsNullOrEmpty(username))
        {
            var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Username == username);
            if (user != null) return user.UserId;
        }
        return 0;
    }

    /// <summary>My notifications (paged).</summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<Notification>>> GetMyNotifications(
        [FromQuery] bool? unreadOnly = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Ok(new PaginatedResponse<Notification>());

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Notifications.AsNoTracking().Where(n => n.UserId == userId);
        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new PaginatedResponse<Notification>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        });
    }

    /// <summary>Unread count (for navigation badge).</summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return Ok(0);

        return Ok(await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead));
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetCurrentUserId();
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == userId);

        if (notification == null) return NotFound();

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        return NoContent();
    }

    /// <summary>Mark all notifications as read.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetCurrentUserId();
        var unread = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in unread)
        {
            n.IsRead = true;
            n.ReadAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
        return NoContent();
    }
}