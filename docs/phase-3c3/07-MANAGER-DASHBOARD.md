# 07 — Manager Dashboard

## API
`GET /api/analytics/my-team` (any user; scope = self + direct reports).

## Content
- KPI cards: Direct Reports, Present Today, On Leave Today, Pending Approvals.
- Team leave by status.
- Team attendance trend.

## Scope
Direct reports only (MSS policy). No manager id from client — resolved server-side.

## Frontend
`/analytics/my-team` — KPI grid + breakdown.

## Privacy
No salary/bank/tax/documents/medical data exposed.
