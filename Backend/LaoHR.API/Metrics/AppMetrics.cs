using System.Diagnostics.Metrics;

namespace LaoHR.API.Metrics;

/// <summary>
/// Phase 4D.1 — application metrics (Prometheus-compatible via OpenTelemetry).
///
/// Exposes lightweight operational counters for behaviors that are otherwise
/// only visible in logs. HTTP/runtime/process metrics come from the built-in
/// ASP.NET Core + System.Runtime + Process instrumentation meters; this class
/// carries the LaoHR-specific ones.
/// </summary>
public static class AppMetrics
{
    public const string MeterName = "LaoHR.App";

    private static readonly Meter Meter = new(MeterName);

    /// <summary>Audit entries dropped because the bounded channel was full.</summary>
    public static readonly Counter<long> AuditEntriesDropped = Meter.CreateCounter<long>(
        "laohr.audit.entries_dropped",
        unit: "{entry}",
        description: "Audit log entries dropped because the bounded audit channel was full.");

    /// <summary>Audit batches that failed to persist (dropped after retry-less write).</summary>
    public static readonly Counter<long> AuditPersistFailures = Meter.CreateCounter<long>(
        "laohr.audit.persist_failures",
        unit: "{batch}",
        description: "Audit log batches that failed to persist to the database.");

    /// <summary>Background job iterations that ended with an unexpected error.</summary>
    public static readonly Counter<long> BackgroundJobFailures = Meter.CreateCounter<long>(
        "laohr.background.jobs_failed",
        unit: "{error}",
        description: "Background job failures (retention, scheduled leave jobs, ...).");

    /// <summary>In-app notification writes that failed.</summary>
    public static readonly Counter<long> NotificationFailures = Meter.CreateCounter<long>(
        "laohr.notifications.failed",
        unit: "{notification}",
        description: "In-app notifications that failed to persist.");

    /// <summary>Uploads rejected by validation (size, type, or content signature).</summary>
    public static readonly Counter<long> UploadsRejected = Meter.CreateCounter<long>(
        "laohr.uploads.rejected",
        unit: "{upload}",
        description: "File uploads rejected by server-side validation.");
}
