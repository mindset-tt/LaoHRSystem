# 32 — BACKUP / RESTORE APP VALIDATION

## Stronger than 4B.2

Phase 4B.2 restored an effectively empty finance DB. Phase 4C seeds representative
corporate rows before backup and verifies their exact values after restore.

## Procedure

1. Apply migrations to a disposable `postgres:16-alpine` DB.
2. Seed representative rows: Facility (FAC-001), Vehicle (V-001, odometer 1000),
   CorporateDocument (DOC-001, version 1).
3. `pg_dump -Fc` → `createdb` → `pg_restore`.
4. Verify restored rows by exact value.

## Result

- Backup: PASS.
- Restore: PASS.
- Representative restored data: PASS (FAC-001/Head Office, V-001/REG-001/1000,
  DOC-001/Policy/1 all preserved).
- Migration history: 12 rows restored.

## Application-level restore smoke

NOT RUN (no application host pointed at the restored DB in this phase). Marked
honestly; a stronger smoke is a Phase 4D (production readiness) item.

## Status

PASS (backup/restore + representative data); app-level smoke NOT RUN.
