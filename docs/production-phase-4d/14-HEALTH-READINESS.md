# 14 — HEALTH / READINESS

## Endpoints

- `/health/live` — liveness (no deps; healthy if process alive). `AllowAnonymous`.
- `/health/ready` — readiness (Postgres reachable, tagged `ready`). `AllowAnonymous`.

## Security

Health output is sanitized (no connection strings, stack traces, or topology).

## Docker

- API Dockerfile `HEALTHCHECK` uses `/health/live`; compose uses `/health/ready`.
  (Minor inconsistency documented; both are valid — liveness for the container,
  readiness for `depends_on`.)

## Status

PASS.
