# 09 — Upgrade Migration Validation

## Result: PASS
- Created disposable DB `laohr_upgrade_test`.
- Migrated to `AddApprovalEssMssNotifications` (partial state = 2 migrations).
- Applied remaining 4 migrations to latest.
- 84 tables, all 6 migrations in history.

## Synthetic data
No synthetic data seeded (schema-only upgrade validation). Data-integrity upgrade with representative records is a follow-up (requires seed fixtures).

## Password migration
Legacy SHA-256 → PBKDF2 rehash-on-login is covered by existing `PasswordHasherTests` (unit). Real-DB password migration smoke test is a follow-up.
