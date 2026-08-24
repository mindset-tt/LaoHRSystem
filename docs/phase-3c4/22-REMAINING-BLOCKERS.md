# 22 — Remaining Blockers

## Production payroll (unchanged)
- PRODUCTION_PAYROLL_READY = NO
- OT divisor, leave carry-over, NSSF floor/base, bank format, visa/stay remain BLOCKED.

## Infrastructure (unchanged)
- Docker daemon not running; PostgreSQL not reachable → live migration/backup/restore/TLS NOT RUN.

## Frontend lint (pre-existing)
- 41 errors, 38 warnings (historical debt, unchanged).

## OpenTelemetry NU1902 warnings (pre-existing)

## Deferred (not blockers)
- Critical path (data quality insufficient).
- 4 dependency types (FS only).
- Cross-project dependencies.
- Project Phase entity.
- Kanban optimistic concurrency token.
- Gantt drag-to-reschedule + zoom.
- Saved views / report designer.
- Sensitive-export audit logging.
