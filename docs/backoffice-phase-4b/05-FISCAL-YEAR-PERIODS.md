# 05 — Fiscal Year / Periods

## Entities
- `FiscalYear`: Name, StartDate, EndDate, Status (OPEN/CLOSED). NOT assumed calendar year.
- `FiscalPeriod`: FiscalYearId, PeriodNumber, StartDate, EndDate, Status (OPEN/CLOSED/LOCKED).

## Period status semantics
- OPEN — accepts new postings.
- CLOSED — no new ordinary postings.
- LOCKED — no postings; corrections require authorized reopen or next-period adjustment.

## Closed-period control
`AccountingService.PostAsync` rejects posting to a non-OPEN period.

## Access
`CanManagePeriods` (Admin/Finance only).
