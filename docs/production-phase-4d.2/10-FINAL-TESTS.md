# 10 — FINAL TEST EVIDENCE

All runs on final code (rotation + HSTS fix), 2026-08-25.

## Backend

| Run | Result |
|---|---|
| Build API + LicenseGen + Tests | 0 errors, 0 warnings |
| Test run 1 | **295 / 295** |
| Test run 2 | 295 / 295 |
| Test run 3 | 295 / 295 |
| Test run 4 | 295 / 295 |
| Test run 5 | 295 / 295 |

291 (4D.1) + 4 new rotation regression tests (`LicenseRotationTests`) = 295.
New tests are serialized into the same xUnit collection as
`LicenseServiceTests` (shared `public.key` CWD file).

## Frontend

| Check | Result |
|---|---|
| npm audit | **0 vulnerabilities** (0 critical / 0 high / 0 moderate) |
| vitest | **57 / 57 passed** (14 files) |
| tsc --noEmit | exit 0 PASS |
| eslint | 0 errors, 0 warnings |
| next build | exit 0 PASS |

## PostgreSQL 16 concurrency races (real PG16 containers, disposable)

Harness: `scripts/validate-finance-postgres.ps1` /
`scripts/validate-corporate-postgres.ps1` (each spins up postgres:16-alpine,
applies real migrations, runs `Pg16Concurrency` filter, destroys container).

Filter covers exactly these six race tests
(`Backend/LaoHR.Tests/Integration/Pg16/Pg16ConcurrencyTests.cs`):
payment double-pay race, journal single-post race, auto-post race,
closed-period rejection, room booking overlap race, vehicle booking overlap race.

| Suite | Result |
|---|---|
| validate-finance-postgres | **6 / 6 Passed!** (incl. Room + Vehicle races) |
| validate-corporate-postgres | **6 / 6 Passed!** (repeat) |

No schema change occurred in this phase → no migration created (per rule §28);
fresh-migration path exercised twice by the validators above.

## Live behavior checks

- License rotation matrix (real HTTP): see 02 — all six cases as specified.
- Security headers live retest incl. single-HSTS fix: see 08.
