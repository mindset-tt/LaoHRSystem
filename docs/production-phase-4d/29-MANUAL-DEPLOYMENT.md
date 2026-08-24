# 29 — MANUAL DEPLOYMENT

## Pre-deploy checklist

1. `git status` clean; correct commit checked out.
2. Environment config + secrets set (`.env` from `.env.example`).
3. Backup taken (DB + documents).
4. Migration reviewed (additive-only; no destructive ops).
5. Vulnerability gate: CRITICAL 0, HIGH 0.
6. Backend tests + frontend tests + typecheck + build pass.
7. Disk space checked.

## Deploy steps

1. Pull approved commit.
2. Build images (`docker compose build`).
3. Database backup.
4. Apply migration (`dotnet ef database update` or via startup `Migrate()`).
5. Restart services (`docker compose up -d`).
6. Health checks (`/health/ready`).
7. Smoke tests (`scripts/production-smoke.ps1`).

## Downtime

Short downtime expected (restart). Zero-downtime is NOT claimed.

## Status

PASS (procedure documented).
