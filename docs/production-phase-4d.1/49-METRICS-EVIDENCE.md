# 49 — METRICS EVIDENCE

Phase 4D status: `Metrics: PARTIAL (logs+traces; no metrics endpoint)`.
This phase closes it.

## Implementation

- OpenTelemetry metrics pipeline added to `Program.cs`
  (`WithMetrics`) alongside the existing tracing:
  - `Microsoft.AspNetCore.Hosting` + `Microsoft.AspNetCore.Server.Kestrel`
    framework meters → HTTP request count, duration histogram, active
    requests, connection metrics
  - `OpenTelemetry.Instrumentation.Runtime` → GC collections, allocation
    rate, thread pool, JIT, exceptions (`dotnet_*` family)
  - `OpenTelemetry.Instrumentation.Process` → process CPU time,
    physical/virtual memory
  - `LaoHR.App` custom meter (`Metrics/AppMetrics.cs`):
    | Counter | Meaning |
    |---|---|
    | `laohr_audit_entries_dropped` | bounded audit channel overflow |
    | `laohr_audit_persist_failures` | audit batch write failures |
    | `laohr_background_jobs_failed` | retention / scheduled job errors |
    | `laohr_notifications_failed` | in-app notification write failures |
    | `laohr_uploads_rejected_total{reason}` | upload validation rejections |
- Export: **Prometheus pull endpoint** via
  `MapPrometheusScrapingEndpoint("/metrics")` — consistent with the local,
  no-SaaS observability stance.
- DB client duration: EF Core activity tracing already covers per-request DB
  time; Npgsql pool state is observable from PostgreSQL itself
  (pg_stat_activity) and is sampled by the load/soak harness and ops checks.

## Exposure protection

- The API container port is **not published** to the host in the production
  topology; `/metrics` is NOT routed by the reverse proxy:

```
$ curl -sk -o NUL -w "%{http_code}" https://localhost/metrics
404                                   <-- not reachable through the proxy

$ docker exec laohr-prodlike-caddy wget -qO- http://api:8080/metrics   # internal
HTTP/1.1 200 OK                       <-- scrape path for monitoring only
```

Operators must keep the API port off untrusted interfaces (documented in
the runbook); no JWT on the endpoint so in-network scrapers work.

## Validation that metrics actually change (spec §30)

A configured package without emitted data is not PASS. Live verification:

1. HTTP/runtime/process series present while serving traffic:
```
target_info{service_name="LaoHR.API",...} 1
dotnet_assembly_count{otel_scope_name="System.Runtime"} 182
dotnet_exceptions_total{...,error_type="SocketException"} 1
dotnet_gc_collections_total{otel_scope_name="System.Runtime",gc_heap_generation="gen2"} 4
http_server_active_requests{otel_scope_name="Microsoft.AspNetCore.Hosting",...}
```

2. Custom counter increments on real events:
   - before any rejection: no `laohr_uploads_rejected_total` series (zero-valued)
   - after two exe-renamed-PDF attempts:
```
laohr_uploads_rejected_total{otel_scope_name="LaoHR.App",reason="signature_mismatch"} 2
```

## Status

METRICS = **PASS** (endpoint live internally, series verified to change under
real traffic and real security events).
