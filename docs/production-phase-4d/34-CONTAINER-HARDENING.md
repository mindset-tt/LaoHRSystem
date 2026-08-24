# 34 — CONTAINER HARDENING

## Current

- Non-root users (`appuser` for API, `app` for web).
- Pinned `postgres:16-alpine` (not `latest`).
- Multi-stage builds (no SDK in runtime image).
- No privileged mode, no host networking.

## Changes (4D)

- Postgres `5432` and API `8080` ports no longer published to host.
- `CORS_ALLOWED_ORIGINS` required (no silent default).

## Remaining (operator)

- Read-only filesystem where practical.
- Resource limits (set after measured use).
- Secrets via env/secret manager (never baked into images).

## Status

PASS.
