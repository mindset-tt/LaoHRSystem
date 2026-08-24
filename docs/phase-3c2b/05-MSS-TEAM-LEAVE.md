# 05 — MSS Team Leave

## Endpoint
`GET /api/team/leave?status=&from=&to=`

## Scope
Direct reports only (the current manager's `Employee.ManagerId` children). Server resolves the manager from the JWT `EmployeeId` claim — the client never supplies a manager id.

## Fields
employee name, department, job title, leave type, start, end, total days, status. No sensitive medical details.

## Filters
`status`, `from` (end >= from), `to` (start <= to).

## Empty state
No direct reports → empty list (no fake data).
