using LaoHR.Shared.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LaoHR.API.Jobs;

/// <summary>
/// Phase 6d — Data retention hosted service.
/// Periodically prunes:
///   - AuditLog rows older than <c>Retention:AuditLogDays</c>
///   - RefreshToken rows that have been revoked for more than
///     <c>Retention:RefreshTokenDays</c> days
///
/// Defaults are conservative (audit logs: 365d, refresh tokens: 30d). Set
/// <c>Retention:Enabled</c> to <c>true</c> to actually run; otherwise the
/// service no-ops so deployments can opt in.
/// </summary>
public class RetentionService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RetentionService> _log;
    private readonly IConfiguration _config;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public RetentionService(
        IServiceProvider services,
        ILogger<RetentionService> log,
        IConfiguration config)
    {
        _services = services;
        _log = log;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _config.GetValue("Retention:Enabled", false);
        if (!enabled)
        {
            _log.LogInformation("Retention service is disabled (Retention:Enabled=false). Skipping.");
            return;
        }

        var auditDays = _config.GetValue("Retention:AuditLogDays", 365);
        var refreshDays = _config.GetValue("Retention:RefreshTokenDays", 30);
        var targetHourUtc = _config.GetValue("Retention:RunAtHourUtc", 3);

        _log.LogInformation(
            "Retention service started. auditDays={AuditDays}, refreshDays={RefreshDays}, runAtHourUtc={Hour}",
            auditDays, refreshDays, targetHourUtc);

        DateTime? lastRun = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow;
                var today = now.Date;
                var shouldRunToday = lastRun == null || lastRun.Value.Date < today;
                var atTargetHour = now.Hour >= targetHourUtc;

                if (shouldRunToday && atTargetHour)
                {
                    await RunOnceAsync(auditDays, refreshDays, stoppingToken);
                    lastRun = now;
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _log.LogError(ex, "Retention sweep failed");
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (TaskCanceledException) { /* shutdown */ }
        }
    }

    private async Task RunOnceAsync(int auditDays, int refreshDays, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();

        var auditCutoff = DateTime.UtcNow.AddDays(-auditDays);
        var refreshCutoff = DateTime.UtcNow.AddDays(-refreshDays);

        // AuditLog: AuditLog.Timestamp is the canonical change timestamp.
        var auditDeleted = await db.AuditLogs
            .Where(a => a.Timestamp < auditCutoff)
            .ExecuteDeleteAsync(ct);

        // RefreshToken: drop revoked rows after the grace window. Active tokens
        // are never touched.
        var refreshDeleted = await db.RefreshTokens
            .Where(r => r.RevokedAt != null && r.RevokedAt < refreshCutoff)
            .ExecuteDeleteAsync(ct);

        _log.LogInformation(
            "Retention sweep complete: auditDeleted={Audit}, refreshDeleted={Refresh}",
            auditDeleted, refreshDeleted);
    }
}
