# 07 — Gantt / Timeline

## Endpoint
`GET /api/pm/projects/{id}/timeline` returns tasks (with dates, parent, milestone, progress), milestones, and dependencies.

## Scope
Project-level. No unrestricted thousands-of-tasks load (project-scoped).

## Editing
Read visualization + basic date editing via existing task update. No drag-to-reschedule (deferred).

## Critical path
Not computed (data quality insufficient). Would show "unavailable" rather than a fake result.

## Today marker / zoom
Frontend concern; day/week/month zoom deferred to a later UI pass.
