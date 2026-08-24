# 21 — Test Results

## Backend
- Build: PASS (0 errors, 5 pre-existing warnings)
- Tests: 134 passed / 0 failed
- Repeated full-suite: 5/5 green

## Frontend
- Typecheck: PASS
- Tests: 29 passed / 0 failed
- Production build: PASS
- Lint: 41 errors, 38 warnings (baseline; 0 new)

## New tests
- `PmPlanningServiceTests` (10): task hierarchy cycles, dependency cycles, workload/overallocation, health, portfolio.
- `PmAuthorizationTests` (4): non-member IDOR denial.
- Frontend: portfolio + capacity page tests (6 new).

## Determinism
0 intermittent failures across repeated runs.
