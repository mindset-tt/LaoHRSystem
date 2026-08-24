# 18 — Migration Review

## Migration
`AddFinanceClosure` — additive (0 AlterColumn, 2 AddColumn, 1 CreateIndex;
DropColumn only in `Down()`).

## Changes
- `JournalEntries.PostingPurpose` (string, nullable) — source uniqueness.
- `ExpenseCategories.AccountId` (int, nullable) — expense GL mapping.

## Discipline
Append-only history (no rebase). `NPGSQL_LEGACY_TIMESTAMP` unset during
`migrations add`. No destructive operations in `Up()`.

## Validation
Fresh migration on PostgreSQL 16: PASS (118 tables).
