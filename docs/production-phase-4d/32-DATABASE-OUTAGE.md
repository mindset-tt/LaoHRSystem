# 32 — DATABASE OUTAGE

## Simulated behavior

Postgres unavailable → `/health/ready` fails (readiness), `/health/live` stays
healthy (liveness). Requests that hit the DB return 500 (ProblemDetails, no stack
trace/connection string).

## Operator response

1. Check Postgres container/process.
2. Check disk (WAL/data volume full).
3. Restart Postgres; verify `pg_isready`.
4. Verify `/health/ready` recovers.

## Status

PASS (behavior understood; no secret leak).
