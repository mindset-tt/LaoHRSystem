# 10 — Payroll Analytics

## API
`GET /api/analytics/payroll` (Admin/HR only).

## Metrics (aggregate only)
Gross Payroll, Net Payroll, NSSF Employee, NSSF Employer, PIT; payroll by department; gross trend.

## Privacy
- No per-employee salary detail returned.
- Managers do NOT see employee salary.
- Aggregate totals only.

## No rule changes
Reports existing calculated payroll. Does NOT introduce or modify statutory calculations (OT divisor, NSSF floor/base, PIT brackets remain as-is).

## Historical rules
Uses existing finalized `SalarySlip` values; does not recalculate historical payroll with current rules.

## Government filing warning
A "NSSF Summary" is NOT an official LSSO submission file; PIT report is NOT an official filing format. Labeled clearly.
