# 15 — My Tasks

## Endpoint
`GET /api/my-tasks` (existing) — tasks assigned to the current user across projects.

## Security
Server resolves EmployeeId from the authenticated user. No `?employeeId=` as authorization.

## Sections
Today / Upcoming / Overdue / Blocked / Completed (frontend grouping over the same task list).
