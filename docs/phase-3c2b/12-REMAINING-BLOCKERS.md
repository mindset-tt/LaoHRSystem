# 12 — Remaining Blockers

## Production payroll (unchanged)
- PRODUCTION_PAYROLL_READY = NO
- OT hourly divisor, leave carry-over, NSSF floor/base, bank file format, visa/stay categories remain BLOCKED (no guessed statutory values).

## Infrastructure (unchanged)
- Docker daemon not running; PostgreSQL not reachable → live migration/backup/restore/TLS NOT RUN.

## Read-path authorization (deferred)
- `GET /api/leave?employeeId=`, `GET /api/expenses?employeeId=`, `GET /api/employeeLoans?employeeId=`, `GET /api/attendance/corrections/{id}` accept client-supplied ids without self-scoping. Deferred to a dedicated authorization pass.

## Frontend lint (pre-existing)
- 41 errors, 38 warnings (historical debt, unchanged).

## OpenTelemetry NU1902 warnings (pre-existing)
- Moderate-severity advisories on OpenTelemetry packages.
