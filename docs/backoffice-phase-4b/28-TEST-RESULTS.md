# 28 — Test Results

## Backend
- Build: PASS (0 warnings, 0 errors).
- Tests: **195 PASS / 0 FAIL** (173 + 22 new).
- New tests: `AccountingServiceTests` (7), `AccountsPayableServiceTests` (6),
  `BudgetServiceTests` (+2), `FinanceAuthorizationTests` (7).

## Frontend
- Typecheck: PASS.
- Tests: **45 PASS / 0 FAIL** (39 + 6 new `financeAccounting.test.ts`).
- Build: PASS.
- Lint: 41 errors / 38 warnings (0 new).

## PostgreSQL 16
- Fresh migration: PASS (118 tables).
- Backup: PASS.
- Restore: PASS (118 tables).

## 5× backend gate
5/5 PASS (195 each).
