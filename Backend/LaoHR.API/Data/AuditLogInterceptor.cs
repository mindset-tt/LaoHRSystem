using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Channels;
using LaoHR.Shared.Models;

namespace LaoHR.API.Data;

/// <summary>
/// Collects audit entries on save and enqueues them onto a <see cref="Channel{T}"/>
/// for an <see cref="IAuditLogWriter"/> worker. The user-facing transaction is no
/// longer coupled to the audit write — a failing audit cannot roll back the
/// caller's work, and a slow audit sink cannot delay the response.
/// </summary>
public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditLogChannel _channel;

    public AuditLogInterceptor(IHttpContextAccessor httpContextAccessor, IAuditLogChannel channel)
    {
        _httpContextAccessor = httpContextAccessor;
        _channel = channel;
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

                // Secret exclusion: never persist sensitive values into audit JSON.
                // This covers passwords, tokens, secrets, and full bank account
                // numbers / SWIFT codes. The audit still records that the field
                // changed (via the redacted marker) without leaking the value.
                var isSensitive = IsSensitiveProperty(propertyName);

                switch (entry.State)
                {
                    case EntityState.Added:
                        if (property.CurrentValue != null)
                            newValues[propertyName] = isSensitive ? Redacted : property.CurrentValue;
                        break;
                    case EntityState.Deleted:
                        if (property.OriginalValue != null)
                            oldValues[propertyName] = isSensitive ? Redacted : property.OriginalValue;
                        break;
                    case EntityState.Modified:
                        if (property.IsModified)
                        {
                            if (property.OriginalValue != null)
                                oldValues[propertyName] = isSensitive ? Redacted : property.OriginalValue;
                            if (property.CurrentValue != null)
                                newValues[propertyName] = isSensitive ? Redacted : property.CurrentValue;
                        }
                        break;
                }
            }

            audit.KeyValues = JsonSerializer.Serialize(keyValues);
            audit.OldValues = oldValues.Count == 0 ? null : JsonSerializer.Serialize(oldValues);
            audit.NewValues = newValues.Count == 0 ? null : JsonSerializer.Serialize(newValues);

            auditEntries.Add(audit);
        }

        // Enqueue rather than Add — the user transaction is no longer responsible
        // for persisting the audit row. The background writer drains the channel.
        foreach (var a in auditEntries)
        {
            _channel.TryWrite(a);
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>Marker written in place of a sensitive value in audit JSON.</summary>
    private const string Redacted = "[REDACTED]";

    /// <summary>
    /// Property names whose values must never be persisted into audit JSON.
    /// Covers credentials, tokens, secrets, and full banking identifiers.
    /// </summary>
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        // Credentials / tokens / secrets
        "PasswordHash",
        "Password",
        "TokenHash",
        "ReplacedByHash",
        "RefreshToken",
        "AccessToken",
        "Jwt",
        "Secret",
        "ApiKey",
        "ConnectionString",
        // Banking identifiers (full values are sensitive)
        "AccountNumber",
        "BankAccount",
        "Swift",
        "Iban",
    };

    private static bool IsSensitiveProperty(string propertyName)
        => SensitiveProperties.Contains(propertyName);
}

/// <summary>
/// Process-wide single-writer channel for audit entries. Bounded so a runaway
/// producer cannot OOM the host. When the channel is full the audit entry is
/// dropped and logged (better to lose a log line than to fail the user request).
/// </summary>
public interface IAuditLogChannel
{
    ChannelReader<AuditLog> Reader { get; }
    bool TryWrite(AuditLog entry);
}

public sealed class AuditLogChannel : IAuditLogChannel
{
    private readonly Channel<AuditLog> _channel;
    private readonly ILogger<AuditLogChannel> _logger;

    public AuditLogChannel(ILogger<AuditLogChannel> logger)
    {
        _logger = logger;
        _channel = Channel.CreateBounded<AuditLog>(new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });
    }

    public ChannelReader<AuditLog> Reader => _channel.Reader;

    public bool TryWrite(AuditLog entry)
    {
        if (_channel.Writer.TryWrite(entry)) return true;
        _logger.LogWarning("Audit channel full; dropping entry for {Entity}", entry.EntityName);
        return false;
    }
}

/// <summary>
/// Drains the channel and writes entries to the DB. Lives as a singleton in the
/// host so it survives scoped DbContext instances. Uses an injected scoped
/// factory per write to avoid capturing a disposed context.
/// </summary>
public sealed class AuditLogWriter : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogWriter> _logger;
    private readonly ChannelReader<AuditLog> _reader;

    public AuditLogWriter(IAuditLogChannel channel, IServiceScopeFactory scopeFactory, ILogger<AuditLogWriter> logger)
    {
        _reader = channel.Reader;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<AuditLog>(capacity: 256);
        while (!stoppingToken.IsCancellationRequested)
        {
            batch.Clear();
            try
            {
                // Wait for the first item so we don't busy-loop.
                if (!await _reader.WaitToReadAsync(stoppingToken)) break;
                while (batch.Count < 256 && _reader.TryRead(out var item))
                {
                    batch.Add(item);
                }
            }
            catch (OperationCanceledException) { break; }

            if (batch.Count == 0) continue;
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<LaoHR.Shared.Data.LaoHRDbContext>();
                db.AuditLogs.AddRange(batch);
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist {Count} audit entries", batch.Count);
                // Drop and continue. We cannot block the user path on this.
            }
        }
    }
}
