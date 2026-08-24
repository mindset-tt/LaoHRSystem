# 25 — Backup Architecture

## Validated (real PostgreSQL 16)
- `pg_dump` custom format backup: PASS.
- Restore (clean drop → create → restore): PASS (84 tables).

## Strategy
- Full logical backup (pg_dump custom format) — validated.
- Physical backup (pg_basebackup) — not yet validated.
- WAL archiving for PITR — not yet configured (see 27-PITR-WAL).

## Schedule
- Daily full backup (off-peak).
- Retain N days (org policy).
- Off-site copy (encrypted).

## Follow-up
- Automate backup schedule (cron/systemd timer).
- Encrypt backups at rest.
- Test restore regularly (see 26-RESTORE-DRILL).
- Off-site replication.
