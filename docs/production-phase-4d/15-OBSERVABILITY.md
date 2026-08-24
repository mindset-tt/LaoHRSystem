# 15 — OBSERVABILITY

## Structured logging

Serilog (compact JSON console; optional daily-rolling file). Enriched with
EnvironmentName, MachineName, ThreadId, Application. Request logging adds
RequestHost/Scheme/UserAgent/ClientIP (never Authorization).

## OpenTelemetry

ASP.NET Core + HttpClient + EF Core instrumentation; OTLP exporter when
`OpenTelemetry:Otlp:Endpoint` is set (optional, local-first). No cloud requirement.

## Correlation

Request → trace (TraceId) → audit (traceId in ProblemDetails). Serilog request
logging includes elapsed + status.

## Status

PASS (local-first; OTLP optional).
