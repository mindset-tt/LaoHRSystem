# 03 — Migration Safety

## Additive-only rule
All new migrations are ADDITIVE. `Up()` must not contain `AlterColumn`,
`DropColumn`, `DropTable`, `DropForeignKey`, `RenameColumn`, or `Truncate`.

## Verified migrations
- `AddBackOfficeFoundation` (Phase 4A): 0 AlterColumn, 19 CreateTable.
- `AddBudgetEnforcementAndAssetTraceability` (Phase 4A.1): 0 AlterColumn,
  6 AddColumn, 3 CreateIndex (DropColumn only in `Down()`).

## Migration generation guard
After `dotnet ef migrations add`, scan the generated file for destructive
operations. The `NPGSQL_LEGACY_TIMESTAMP` env var MUST be unset during
`migrations add` (see 01-EF-TIMESTAMP-GOVERNANCE).

## Timestamp regression proof
A no-op `migrations add` against the current model produces zero timestamp
churn (verified: the budget/asset migration had 0 AlterColumn after clearing
the env var).
