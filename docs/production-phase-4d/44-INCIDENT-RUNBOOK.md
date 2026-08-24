# 44 — INCIDENT RUNBOOK

## API down

1. Check `/health/live` (process) vs `/health/ready` (DB).
2. `docker compose ps` + `docker compose logs api`.
3. Restart API; verify readiness.

## PostgreSQL down

1. `pg_isready`; check container/process.
2. Check disk (WAL/data volume full).
3. Restart Postgres; verify `/health/ready`.

## Disk high

1. Identify consumer (Postgres data, uploads, logs, backups).
2. Rotate/archive logs; prune old backups.
3. Expand volume if needed.

## Backup failure

1. Check `PGPASSWORD` set; check `pg_dump` exit code.
2. Check disk space for the backup file.
3. Re-run and verify (`pg_restore --list`).

## Document storage failure

1. Check uploads directory exists/writable.
2. Check disk space.
3. Restore permissions/volume.

## Mass 500 errors

1. Check logs for exception class + traceId.
2. Check DB connectivity.
3. Roll back recent deploy if correlated.

## Authentication outage

1. Check JWT key/config (fail-fast would prevent startup).
2. Check rate limiter (429s).
3. Check DB (user table).

## Suspected account compromise

1. `logout-all` for the user (revokes all refresh tokens).
2. Rotate the user's password.
3. Review audit logs for the account.

## Escalation

Operator → on-call → (if statutory/legal) legal/accounting review.
