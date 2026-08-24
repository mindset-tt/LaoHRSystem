# 20 — Performance / Index Review

## Added indexes (justified by access patterns)
- `ProjectTask.ParentTaskId` — hierarchy walk.
- `ProjectTask (ProjectId, DueDate)` — overdue/portfolio queries.
- `TaskDependency (ProjectId, PredecessorTaskId)` + `(ProjectId, SuccessorTaskId)` — dependency graph + cycle detection.
- `ProjectAssumption (ProjectId, Status)` — RAID list.
- `ProjectDecision (ProjectId)` — decision log.

## N+1 prevention
- Portfolio: single aggregation queries for tasks/risks/issues/milestones (no per-project loop).
- Capacity: single `GroupBy` over resources (no per-employee loop).
- Board: single task query + assignee join.

## No fabricated EXPLAIN
Real PostgreSQL unavailable; no fabricated query plans.

## No materialized views / warehouse
Normal queries first.
