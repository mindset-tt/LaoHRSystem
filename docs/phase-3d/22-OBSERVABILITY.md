# 22 — Observability

## Current state
- Structured logging (ASP.NET Core default + Serilog if configured).
- No metrics endpoint (Prometheus) found.
- No distributed tracing (OpenTelemetry) found.
- No APM integration.

## Requirements
- Structured logs (JSON) to stdout for log aggregation.
- Metrics: request rate, latency, error rate, DB pool, GC.
- Tracing: correlation IDs across API → DB.

## Follow-up
- Add OpenTelemetry (traces + metrics) with OTLP exporter.
- Expose `/metrics` (Prometheus) on internal port.
- Add health endpoints (see 23-HEALTH-ALERTING).
- Centralized log aggregation (ELK/Loki).
