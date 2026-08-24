# 15 — Observability

> `VERIFIED` from `Program.cs` (Serilog, OpenTelemetry, health checks, audit).

## Logging

- **Serilog** bootstrap + host logger.
- Enrichers: `FromLogContext`, `EnvironmentName`, `MachineName`, `ThreadId`, `Application=LaoHR.API`.
- **Console sink**: `RenderedCompactJsonFormatter` (structured JSON).
- **Optional file sink** (`Serilog:WriteToFile`): daily rolling `Logs/laohr-.json`, 14-day retention, shared.
- **Request logging**: `UseSerilogRequestLogging` enriches Host, Scheme, UserAgent, ClientIP; never logs Authorization headers.
- Minimum level overrides: `Microsoft.AspNetCore` → Warning, `Microsoft.EntityFrameworkCore` → Warning.

## Metrics

- **None** — no Prometheus/OTLP metrics exporter configured. OpenTelemetry configured for **traces only**. `VERIFIED`.

## Tracing

- **OpenTelemetry** 1.9.0:
  - ASP.NET Core instrumentation (filters out `/health` paths).
  - HttpClient instrumentation.
  - EF Core instrumentation (`SetDbStatementForText = true`).
  - OTLP exporter when `OpenTelemetry:Otlp:Endpoint` configured (optional; blank by default = console-only/no export).
  - Resource: `AddService("LaoHR.API")`.

## Health checks

- `AddHealthChecks().AddNpgSql(...)` — tagged `"db"`, `"ready"`.
- `/health/live` — liveness, `AllowAnonymous`, no checks (always healthy if process alive).
- `/health/ready` — readiness, filters by `"ready"` tag (postgres), `AllowAnonymous`.
- Docker compose uses `/health/ready` for API healthcheck.

## Dashboards / alerting

- **None** — no Grafana/Prometheus/Datadog config. `VERIFIED` absent.

## Crash reporting

- **None** — no Sentry/exception tracker. Fatal startup errors logged via `Log.Fatal` + `Log.CloseAndFlush()`. `VERIFIED`.

## Audit logs

- `AuditLogInterceptor` captures entity changes on `SaveChanges` → `Channel<AuditLog>` → `AuditLogWriter` (background service, separate scope) → `AuditLogs` table.
- `AuditLogsController` (Admin-only) reads logs (paged, filter by entity).
- No UI for audit logs (API only).
- Retention: `RetentionService` prunes logs >365 days (disabled by default).

## Diagnosability assessment

- **Can production issues be diagnosed?** Partially. Structured logs (Serilog JSON) + traces (if OTLP configured) + health checks give a baseline. Missing: metrics, dashboards, alerting, crash reporting, centralized log aggregation. Without OTLP endpoint + a collector (Tempo/Jaeger) + log ship (Loki/ELK), traces and logs stay in-container. `INFERRED` — production diagnosis is possible but manual (kubectl logs).