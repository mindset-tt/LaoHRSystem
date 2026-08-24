# 41 — Deployment Runbook

## Prerequisites
- Docker + Docker Compose.
- `.env` with `POSTGRES_PASSWORD`, `JWT_KEY` (64+ chars), connection string.
- TLS cert + reverse proxy (see 16).

## Deploy steps
1. Build images (`docker compose build`).
2. Tag + push to registry.
3. Pull on target host.
4. Set env vars.
5. `docker compose up -d`.
6. Apply migrations (startup or `dotnet ef database update`).
7. Run smoke tests (39).
8. Verify health checks.

## Rollback
- See 45-ROLLBACK-FORWARD-FIX.

## Follow-up
- Automate via CD pipeline.
- Document exact commands per environment.
