# 09 — 1:1 Meetings

## Model
`OneOnOne` (ManagerEmployeeId, EmployeeId, ScheduledAt, Status, SharedNotes, ManagerPrivateNotes, EmployeeNotes).

## Privacy (critical)
Shared notes ≠ manager-private notes ≠ employee notes. Manager-private notes are never returned through employee-facing DTOs.

## Authority
Manager/employee relationship verified server-side. Unrelated employee cannot create/read private 1:1.

## Status
`SCHEDULED`, `COMPLETED`, `CANCELLED`.

## Action items
Deferred (lightweight OneOnOneAction not yet added).
