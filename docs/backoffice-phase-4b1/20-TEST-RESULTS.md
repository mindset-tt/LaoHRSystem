# 20 — Test Results

## Backend
- Build: PASS (0 warnings, 0 errors).
- Tests: **205 PASS / 0 FAIL** (195 + 10 new).
- New tests: `SegregationOfDutiesServiceTests` (4), `PostingServiceTests` (4),
  `FinanceAuthorizationTests` (+2 Finance-role tests).

## Frontend
- Typecheck: PASS.
- Tests: **48 PASS / 0 FAIL** (45 + 3 new).
- Build: PASS.
- Lint: 41 errors / 38 warnings (0 new).

## PostgreSQL 16
- Fresh migration: PASS (118 tables).
- Upgrade: PASS (data preserved + new columns).
- Backup: PASS.
- Restore: PASS (118 tables).

## 5× backend gate
5/5 PASS (205 each).
