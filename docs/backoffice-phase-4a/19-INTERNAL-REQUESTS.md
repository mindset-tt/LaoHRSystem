# 19 — Internal Requests

## Entities
- `ServiceRequestCategory`: Code, Name, NameLao, IsActive (seeded: IT, FACILITIES,
  ADMIN, PROCUREMENT, HR, FINANCE).
- `ServiceRequest`: RequestNumber, RequesterEmployeeId, CategoryId, Subject,
  Description, Priority, Status, AssignedEmployeeId, DepartmentId, DueDate,
  CreatedAt, ResolvedAt.

## Status
`OPEN`, `IN_PROGRESS`, `ON_HOLD`, `RESOLVED`, `CLOSED`, `CANCELLED`.

## Priority
`LOW`, `MEDIUM`, `HIGH`, `URGENT`.

## Design
Lightweight `ServiceRequest` (NOT a BPMN/drag-drop workflow engine). Categories
are configurable. Employees create their own (self-service); Admin/HR manage.

## Numbering
`SR-{yyyy}-{000000}` via `NumberSequenceService`.

## Access
Self-service create + own-view for employees; Admin/HR manage all.
