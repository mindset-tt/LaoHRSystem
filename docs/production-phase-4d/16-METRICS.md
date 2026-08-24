# 16 — METRICS

## Available

- Request rate / latency / status via Serilog request logging + OTel traces.
- EF Core query instrumentation (traces).
- Health/readiness status.

## Not yet implemented

- Prometheus-compatible metrics endpoint (optional, local-first).
- DB connection-pool / query-latency metrics (available via Npgsql counters if
  wired; not yet exposed).

## Status

PARTIAL — logs + traces provide request-level observability; a dedicated metrics
endpoint is a follow-up (not a P0 blocker).
