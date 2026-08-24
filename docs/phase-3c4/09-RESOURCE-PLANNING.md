# 09 — Resource Planning

## Model
`Resource` (ProjectId, EmployeeId, Role, AllocationPercent, StartDate, EndDate). References real Employee records (no fake project users).

## Allocation
`AllocationPercent` = 0-100, where 100 = full-time equivalent. No 1.0/100 ambiguity.

## Assignment
Employee + project + optional task (via TaskAssignee). Reuses existing `Resource` + `TaskAssignee`.

## Privacy
No salary/loan/tax/private leave details exposed to project managers.
