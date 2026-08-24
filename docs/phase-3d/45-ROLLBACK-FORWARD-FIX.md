# 45 — Rollback / Forward-Fix

## Strategy
- Prefer forward-fix (patch forward) over rollback for data migrations.
- Rollback only for stateless app code (revert image tag).

## Rollback (app)
- Revert to previous image tag.
- No DB rollback (migrations are forward-only — no rebase).

## Forward-fix (data)
- If a migration is bad, write a corrective migration (never rebase/rewrite history).

## DB rollback
- Restore from backup (see 42) if data corruption.

## Follow-up
- Tag every release (image + migration).
- Test rollback in rehearsal (38).
- Document decision tree (rollback vs forward-fix).
