# 02 — Target PM Architecture

## Principle
ONE Project, ONE Task source of truth, multiple views (List / Kanban / Gantt / My Tasks / Portfolio). ONE employee identity, multiple project roles.

## New entities (Phase 3C4)
- `ProjectTask.ParentTaskId` — self-referencing two-level hierarchy (summary → subtask).
- `TaskDependency` — finish-to-start (FS) dependency between tasks in the same project.
- `ProjectAssumption` — RAID assumption register.
- `ProjectDecision` — RAID decision log.

## New services
- `IProjectAccessService` — project-scoped authorization (view/edit/manage tasks/resources/risks).
- `IPmPlanningService` — hierarchy/dependency cycle validation, capacity/workload, project health, portfolio.

## New endpoints (`PmController`, `/api/pm`)
- `GET /projects/{id}/board` — kanban board.
- `POST /projects/{id}/board/move` — move task (server-validated).
- `GET/POST/DELETE /projects/{id}/dependencies` — task dependencies.
- `GET /projects/{id}/timeline` — gantt data.
- `GET /capacity` — resource workload.
- `GET /projects/{id}/health` — project health.
- `GET /portfolio` — portfolio overview.
- `GET/POST /projects/{id}/assumptions` — RAID assumptions.
- `GET/POST /projects/{id}/decisions` — RAID decisions.

## No duplicate system
No TaskV2/KanbanTask/GanttTask/PMTask. No second RAID Risk table. No Phase entity (Milestone + task hierarchy suffice). No Portfolio DB entity (aggregated view).

## Deferred (not justified yet)
- 4 dependency types (FS only for now).
- Critical path (data quality insufficient; show "unavailable" rather than fake).
- Cross-project dependencies (default disallowed).
- Project Phase entity.
- Change-management workflow.
