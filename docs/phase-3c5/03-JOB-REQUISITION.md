# 03 — Job Requisition

## Model
`JobRequisition` (RequisitionNumber, PositionId, DepartmentId, WorkLocationId, RequestedByEmployeeId, HiringManagerEmployeeId, Headcount, Reason, Justification, TargetStartDate, Priority, Status).

## Status lifecycle
`DRAFT → PENDING_APPROVAL → APPROVED → OPEN → FILLED/CLOSED` (+ ON_HOLD, CANCELLED).

## Position reuse
Requisition references `Position` (no duplicate title/department/manager master data).

## Headcount reason
`Replacement`, `Growth`, `Temporary`, `Other`.

## No workforce planning engine
Headcount request is a simple field, not a planning engine.
