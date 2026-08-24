# 00 — Phase 3C3 Baseline

Recorded before Phase 3C3 modifications.

## Backend
- Build: PASS (0 errors, 5 pre-existing warnings)
- Tests: 104 passed / 0 failed

## Frontend
- Typecheck: PASS
- Tests: 18 passed / 0 failed
- Production build: PASS
- Lint: 41 errors, 38 warnings (pre-existing)

## Known read-path IDOR gaps (from Phase 3C2B)
- `GET /api/leave?employeeId=`, `GET /api/expenses?employeeId=`, `GET /api/employeeLoans?employeeId=` accept client-supplied `employeeId` without self-scoping.
- `GET /api/documents/employee/{employeeId}` un-scoped.
- `GET /api/expenses/{id}`, `GET /api/employeeLoans/{id}` detail un-scoped.

## Migration chain
`InitialCreatePostgres` → `AddApprovalEssMssNotifications`

## Git
Branch `master`, substantial uncommitted work.
