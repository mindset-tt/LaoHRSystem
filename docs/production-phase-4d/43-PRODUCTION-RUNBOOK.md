# 43 — PRODUCTION RUNBOOK

## Start / stop / restart

- Start: `docker compose up -d` (postgres → api → web, health-gated).
- Stop: `docker compose down`.
- Restart: `docker compose restart <service>`.

## Deploy

See 29 (manual deployment).

## Migrate

`dotnet ef database update` (or startup `Migrate()`). Backup first. Review
migration for destructive ops.

## Backup

`scripts/backup.ps1` (DB) + copy uploads directory. Verify with `pg_restore --list`.

## Restore

`scripts/restore.ps1` (DB) + restore uploads directory. Run `Pg16RestoreSmokeTests`
or `production-smoke.ps1`.

## Health check

`GET /health/live` and `/health/ready`.

## Logs

`docker compose logs <service>`; file sink at `Logs/laohr-*.json` (if enabled).

## Disk check

Monitor Postgres data volume + uploads + logs + backups.

## DB connection check

`pg_isready`; `/health/ready`.

## Document storage check

Verify uploads directory exists/writable.

## Rollback

See 30.

## Incident escalation

See 44.
