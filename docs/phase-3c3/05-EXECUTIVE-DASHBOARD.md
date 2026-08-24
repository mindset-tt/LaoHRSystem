# 05 — Executive Dashboard

## API
`GET /api/analytics/executive` (Admin/HR).

## Content
- KPI cards: Active Headcount, On Leave Today, Present Today, Pending Approvals, Pending Expenses, Active Projects, Projects At Risk.
- Attention panel: pending approvals, open high/critical risks, unapproved expenses.
- Headcount by department breakdown.
- Headcount/leave/expense trends (time series).

## Frontend
`/analytics/executive` — `KpiCard` grid + `BreakdownList` + attention list. Accessible (numeric values, not color-only).

## Authorization
Admin/HR only (403 otherwise).
