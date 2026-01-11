using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using LaoHR.Shared.Models;

namespace LaoHR.API.Data;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        context.ChangeTracker.DetectChanges();

        var entries = context.ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted)
            .ToList();

        var auditEntries = new List<AuditLog>();
        var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "SYSTEM";

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditLog) continue;

            var audit = new AuditLog
            {
                UserId = username,
                EntityName = entry.Entity.GetType().Name,
                Timestamp = DateTime.UtcNow,
                Action = entry.State.ToString().ToUpper()
            };

            var keyValues = new Dictionary<string, object>();
            var oldValues = new Dictionary<string, object>();
            var newValues = new Dictionary<string, object>();

            foreach (var property in entry.Properties)
            {
                string propertyName = property.Metadata.Name;
                if (property.Metadata.IsPrimaryKey())
                {
                    keyValues[propertyName] = property.CurrentValue!;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (property.CurrentValue != null)
                            newValues[propertyName] = property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        if (property.OriginalValue != null)
                            oldValues[propertyName] = property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            if (property.OriginalValue != null)
                                oldValues[propertyName] = property.OriginalValue;
                            if (property.CurrentValue != null)
                                newValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }

            audit.KeyValues = JsonSerializer.Serialize(keyValues);
            audit.OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues);
            audit.NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues);
            
            auditEntries.Add(audit);
        }

        if (auditEntries.Count > 0)
        {
            context.AddRange(auditEntries);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
