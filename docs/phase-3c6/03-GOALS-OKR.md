# 03 — Goals / OKR

## Model
`Goal` (EmployeeId, ManagerEmployeeId, ParentGoalId, Title, Description, GoalType, StartDate, DueDate, ProgressPercent, Status, CreatedByEmployeeId).

## Types
`Individual`, `Team`, `Organization` (minimal taxonomy).

## Alignment
`ParentGoalId` for org → dept → employee alignment (no strategy-management engine).

## Ownership
One accountable owner (EmployeeId). Collaborators deferred.

## Status
`DRAFT`, `ACTIVE`, `COMPLETED`, `CANCELLED` (clean workflow semantics; ON_TRACK/AT_RISK are health, not status).

## Progress
Manual 0-100 (no unexplained progress). Check-ins preserve history.

## Weight
Not added (no formal weighted calculation required).

## Authorization
Employee: own goals. Manager: team goals. HR: administer. Unrelated: 403.
