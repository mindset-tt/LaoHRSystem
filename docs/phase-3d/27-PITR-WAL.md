# 27 — PITR / WAL

## Status: NOT CONFIGURED
Point-in-time recovery (PITR) via WAL archiving is not yet configured.

## Requirements
- Enable WAL archiving (`archive_mode=on`, `archive_command`).
- `wal_level=replica` (or logical).
- Continuous WAL shipping to off-site/archive storage.
- `pg_basebackup` for base backup.

## Follow-up
- Configure WAL archiving.
- Take base backup.
- Test PITR to a point in time.
- Document RPO (see 28-RPO-RTO).
