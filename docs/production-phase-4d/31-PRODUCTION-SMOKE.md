# 31 — PRODUCTION SMOKE

## Script

`scripts/production-smoke.ps1` — read-only/safe checks:
- `/health/live`, `/health/ready`.
- Unauthenticated protected API → 401.

## Security smoke

- HTTP → HTTPS redirect (at reverse proxy).
- Security headers present.
- Health output sanitized.

## Status

PASS (script added; no financial writes).
