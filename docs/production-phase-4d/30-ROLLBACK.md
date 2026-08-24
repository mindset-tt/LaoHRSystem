# 30 — ROLLBACK

## App rollback

Redeploy the previous build/version (previous image tag or git commit).

## Database rollback

Do NOT blindly use EF migration `Down()` in production. Prefer:
- Forward fix (new additive migration), or
- Restore from backup (if data loss is acceptable and the failure is severe).

## File storage rollback

Consider compatibility between app version and stored files (storage names are
generated; display names are metadata).

## Failed migration / failed startup

See incident runbook (44).

## Status

PASS (procedure documented).
