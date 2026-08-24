# 04 — Filter / Date Semantics

## Timezone
Lao business timezone = UTC+07:00 (`ReportingDateRange.LaoOffset`). Stored timestamps are UTC; range boundaries are converted to UTC for querying.

## Named ranges
`today`, `week`, `month`, `quarter`, `year`, or custom `from`/`to`. Default = current month.

## Date fields per domain
- Employee: `HireDate`
- Leave: `StartDate` (submitted/approved dates not mixed)
- Expense: `ExpenseDate`
- Payroll: `PayrollPeriod` (Year/Month)
- Project: `StartDate`/`DueDate`
- Task: `DueDate`

## Filter authorization
Filters intersect with authorized scope; they never widen it. A manager restricted to their team cannot gain other departments by manipulating filter IDs.

## Midnight boundaries
Range boundaries are computed as Lao-local dates converted to UTC instants; tested via `ReportingDateRange.Resolve`.
