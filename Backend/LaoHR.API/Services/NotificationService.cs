using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C2 — in-app notification service. Creates notifications for AppUsers
/// (resolved from EmployeeId where needed). Notification failure must not roll
/// back the business action; callers should treat this as best-effort.
/// </summary>
public interface INotificationService
{
    /// <summary>Creates a notification for a specific AppUser.</summary>
    Task NotifyUserAsync(int userId, string type, string title, string? message, string? entityType, int? entityId, CancellationToken ct = default);

    /// <summary>Creates a notification for the AppUser linked to an employee (no-op if unlinked).</summary>
    Task NotifyEmployeeAsync(int employeeId, string type, string title, string? message, string? entityType, int? entityId, CancellationToken ct = default);
}

public sealed class NotificationService : INotificationService
{
    private readonly LaoHRDbContext _context;

    public NotificationService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task NotifyUserAsync(int userId, string type, string title, string? message, string? entityType, int? entityId, CancellationToken ct = default)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            EntityType = entityType,
            EntityId = entityId
        });
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception)
        {
            // Phase 4D.1 — count the failure, then preserve existing behavior
            // (callers treat notifications as best-effort and handle exceptions).
            Metrics.AppMetrics.NotificationFailures.Add(1);
            throw;
        }
    }

    public async Task NotifyEmployeeAsync(int employeeId, string type, string title, string? message, string? entityType, int? entityId, CancellationToken ct = default)
    {
        // Resolve the AppUser linked to this employee. No-op if unlinked.
        var userId = await _context.Users
            .AsNoTracking()
            .Where(u => u.EmployeeId == employeeId && u.IsActive)
            .Select(u => (int?)u.UserId)
            .FirstOrDefaultAsync(ct);

        if (userId.HasValue)
        {
            await NotifyUserAsync(userId.Value, type, title, message, entityType, entityId, ct);
        }
    }
}