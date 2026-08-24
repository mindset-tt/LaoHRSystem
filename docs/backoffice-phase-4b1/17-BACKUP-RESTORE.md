# 17 — Backup / Restore

## Result (PostgreSQL 16)
- Backup (pg_dump custom): PASS.
- Restore (drop → create → pg_restore): PASS — 118 tables.

## Verification
Finance tables (Journal, SupplierInvoice, Payment, Account, FiscalPeriod) present
after restore.

## Rule
Backup = PASS and Restore = PASS are required after schema changes.
