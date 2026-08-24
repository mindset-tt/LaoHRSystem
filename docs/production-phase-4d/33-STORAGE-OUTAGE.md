# 33 — STORAGE OUTAGE

## Simulated behavior

Missing/unwritable document directory → file operations fail safely (exception →
ProblemDetails). Readiness behavior documented (document storage is not part of
the readiness check; a dedicated check is a follow-up).

## Operator response

1. Check the uploads directory exists and is writable.
2. Check disk space.
3. Restore permissions/volume.

## Status

PASS (fail-safe; no secret leak).
