# 14 — Development Plans

## Model
`DevelopmentPlan` (EmployeeId, ManagerEmployeeId, PeriodStart, PeriodEnd, Status) + `DevelopmentGoal` (DevelopmentPlanId, CompetencyId, Title, DesiredOutcome, TargetDate, Status).

## Status
Plan: `DRAFT`, `ACTIVE`, `COMPLETED`, `CANCELLED`. Goal: `PLANNED`, `IN_PROGRESS`, `COMPLETED`, `CANCELLED`.

## Continuity
Development plans may span performance cycles (not forced to close with cycle).

## Authorization
Self: own. Manager: team. HR: broader.
