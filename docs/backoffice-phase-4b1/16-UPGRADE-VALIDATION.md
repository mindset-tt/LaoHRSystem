# 16 — Upgrade Validation

## Test
Pre-4B.1 schema (up to `AddFinanceAccountingFoundation`) → seed synthetic
Supplier → apply `AddFinanceClosure` → verify data preserved + new columns.

## Result
PASS — synthetic supplier preserved; `JournalEntries.PostingPurpose` and
`ExpenseCategories.AccountId` added.

## Migration safety
`NPGSQL_LEGACY_TIMESTAMP` unset during `migrations add` (0 AlterColumn).
