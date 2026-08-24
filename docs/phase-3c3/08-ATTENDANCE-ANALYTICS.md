# 08 — Attendance Analytics

## API
`GET /api/analytics/attendance?range=&from=&to=` (scoped).

## Metrics
Present, Late, Early Leave, Missing Clock Out (existing fields only — no new OT logic).

## Trend
Daily attendance count time series.

## Drilldown
Aggregates are small; detail lists are paginated (existing attendance list endpoint, now scoped).

## No new legal OT formulas
Only existing `IsLate`/`IsEarlyLeave`/`WorkHours` surfaced.
