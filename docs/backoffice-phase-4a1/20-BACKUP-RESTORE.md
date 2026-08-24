# 20 — Backup / Restore

## Reasoning change
Phase 4A said "backup/restore NOT REQUIRED because migration was additive."
That reasoning is rejected: additive migrations can still contain defects.

## Results (PostgreSQL 16)
| Test | Result |
|---|---|
| Backup (pg_dump custom format) | PASS |
| Restore (drop → create → pg_restore) | PASS — 103 tables |

## Script
`scripts/backup.ps1` (pg_dump custom format; `PGPASSWORD` env, no embedded secret).

## Rule
Backup = PASS and Restore = PASS are required, never "NOT REQUIRED".
