# 00 — Phase 3C2B Baseline

Recorded before any Phase 3C2B modifications.

## Backend
- Build: PASS (0 errors, 5 pre-existing warnings)
- Tests: 94 passed / 0 failed
- Test count: 94

## Frontend
- Typecheck: PASS
- Production build: PASS
- Automated tests: NOT CONFIGURED
- Lint: 41 errors, 38 warnings (pre-existing historical debt)

## Known issues
- `LeaveControllerTests.CreateLeave` observed flaky (shared InMemory DB race)
- Seeded username `hr` (2 chars) fails `LoginRequestValidator` (min 3 chars)
- MSS: team leave + team attendance NOT implemented
- Frontend tests: not configured

## Git
- Branch `master`, ahead of origin by 4 commits, substantial uncommitted work.
