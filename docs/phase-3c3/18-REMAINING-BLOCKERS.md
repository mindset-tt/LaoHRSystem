# 18 — Remaining Blockers

## Production payroll (unchanged)
- PRODUCTION_PAYROLL_READY = NO
- OT divisor, leave carry-over, NSSF floor/base, bank format, visa/stay remain BLOCKED.

## Infrastructure (unchanged)
- Docker daemon not running; PostgreSQL not reachable → live migration/backup/restore/TLS NOT RUN.

## Frontend lint (pre-existing)
- 41 errors, 38 warnings (historical debt, unchanged).

## OpenTelemetry NU1902 warnings (pre-existing)
- Moderate-severity advisories on OpenTelemetry packages.

## Deferred (not blockers)
- Risk heatmap (probability×impact matrix) — deferred.
- CSV export for analytics — deferred until a specific report is requested.
- Sensitive-export audit logging — deferred to a dedicated audit pass.
- Materialized views / data warehouse — deferred until measured need.
