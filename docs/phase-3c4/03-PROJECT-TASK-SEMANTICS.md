# 03 — Project / Task Semantics

## Task status (source of truth)
`TODO, IN_PROGRESS, BLOCKED, REVIEW, DONE, CANCELLED` (existing vocabulary, preserved).

## Task hierarchy
`ParentTaskId` self-reference. Two-level (summary → subtask). Cycles prevented server-side via `WouldCreateTaskCycleAsync` (walks parent chain).

## Board column vs status
Board columns map directly to status (lightest model). No configurable workflow designer.

## Task ordering
`SortOrder` integer. Move endpoint accepts optional `SortOrder`; no full resequencing on every drag (avoid renumbering).

## Overdue definition
`DueDate < now` AND status not in `{DONE, CANCELLED}`.

## Progress
Task `ProgressPercent` (manual 0-100). Project progress = `doneTasks / totalTasks` (documented formula, no hidden weighting).

## Date consistency
`StartDate <= DueDate` validated at the API boundary where set (existing behavior). Parent summary dates are NOT auto-derived (manual control).
