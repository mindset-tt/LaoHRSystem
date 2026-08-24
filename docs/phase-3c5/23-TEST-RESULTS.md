# 23 — Test Results

## Backend
- Build: PASS (0 warnings / 0 errors)
- Tests: 142 passed / 0 failed
- Repeated full-suite: 5/5 green

## Frontend
- Typecheck: PASS
- Tests: 31 passed / 0 failed
- Production build: PASS
- Lint: 41 errors, 38 warnings (baseline; 0 new)

## New tests
- `HireConversionServiceTests` (4): employee mapping, idempotency, no-offer, duplicate-email.
- `RecruitmentWorkflowTests` (1): flagship E2E (requisition → hire → employee + onboarding).
- `RecruitmentIdorTests` (3): unrelated employee cannot view candidate/offer or hire.
- Frontend: recruitment page test (2).

## Determinism
0 intermittent failures across repeated runs.
