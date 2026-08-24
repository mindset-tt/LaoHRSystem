# 26 — BACKUP STRATEGY

## Database

`pg_dump` custom format (`scripts/backup.ps1`), operator-run. `PGPASSWORD` from
environment (never embedded).

## Document storage

Database backup is insufficient — also back up the uploads directory (employee +
corporate documents) and required configuration.

## Consistency

Document the DB↔file consistency point (backup both in the same window).

## Verification

Backup success ≠ valid backup. Verify: file exists, non-zero, `pg_restore --list`
succeeds, checksum.

## Retention

Configurable company policy (no invented legal retention).

## Status

PASS (DB + document backup procedure; restore script added in 4D).
