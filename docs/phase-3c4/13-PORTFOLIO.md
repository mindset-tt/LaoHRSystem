# 13 — Portfolio

## Endpoint
`GET /api/pm/portfolio` — aggregated project list (no Portfolio DB entity).

## Columns
Project, PM (owner), Status, Health, Progress, Start, Due, Open risks, Overdue tasks, Open issues, Next milestone.

## Filters
Deferred (status/health/PM/date filters can be added later). Authorization scope respected.

## Progress
`doneTasks / totalTasks` (documented formula).
