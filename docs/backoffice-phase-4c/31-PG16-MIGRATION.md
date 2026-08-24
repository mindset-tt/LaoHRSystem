# 31 — PG16 MIGRATION

## Migration

`20260824094126_AddCorporateOperations` — additive-only.

- `Up()`: 5 `AddColumn` (Expense.TravelRequestId; Contract.AutoRenew/CostCenterId/
  NoticePeriodDays/ProjectId) + 17 `CreateTable`.
- `Down()`: the reverse (DropTable/DropColumn) — normal.
- 0 unexpected `AlterColumn`, 0 `DropColumn`/`DropTable` in `Up()`.

## Timestamp guard

`NPGSQL_LEGACY_TIMESTAMP` was unset before `migrations add` (no legacy timestamp
distortion).

## Fresh + upgrade

Fresh full chain against `postgres:16-alpine`: PASS. Upgrade (pre-4C → 4C) is
additive-only, so prior rows are preserved.

## Status

PASS.
