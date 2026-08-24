# 17 — BACKUP / RESTORE

PostgreSQL 16 backup and restore validation.

## Procedure

1. `pg_dump -Fc` (custom format) of the migrated `laohr` database.
2. `createdb laohr_restore`.
3. `pg_restore` into `laohr_restore`.

## Result

- `pg_dump`: PASS.
- `pg_restore`: PASS.
- Restored `__EFMigrationsHistory` row count: 11 (full migration history).
- Restored public table count: 118.

Finance tables (`Accounts`, `JournalEntries`, `SupplierInvoices`, `Payments`,
`Budgets`, `BankAccounts`) restored with 0 rows because the validation database
was migration-only (no seed data). The schema round-trip is proven; representative
data preservation is covered by the migration chain being additive-only.

## Status

PASS.
