# 12 — PM / Risk Analytics

## API
`GET /api/analytics/pm`.

## Metrics
Active/Completed projects, open/overdue tasks, open/critical risks, open issues; project/task/risk/issue status distributions.

## Project health
No arbitrary "health score". Risk severity uses existing `Priority` (LOW/MEDIUM/HIGH/CRITICAL) and `Likelihood`/`Impact` (1-5) fields. No fabricated probability/impact.

## Risk heatmap
Deferred — only if a reliable probability×impact matrix is needed; current analytics use severity distribution.
