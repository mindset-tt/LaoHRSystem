# 18 — PM Analytics Integration

## Existing (Phase 3C3)
`/api/analytics/pm` returns PM KPIs (active/completed projects, open/overdue tasks, open/critical risks, open issues) + status distributions.

## Phase 3C4 additions
- Portfolio endpoint (`/api/pm/portfolio`) with per-project health/progress/risk/overdue.
- Project health endpoint (`/api/pm/projects/{id}/health`).
- Capacity endpoint (`/api/pm/capacity`).

## Drilldown
Portfolio rows link to `/projects/{id}`. Health/overdue/risk counts are actionable (navigate to project workspace).

## No new analytics stack
Extends Phase 3C3 analytics; no separate BI.
