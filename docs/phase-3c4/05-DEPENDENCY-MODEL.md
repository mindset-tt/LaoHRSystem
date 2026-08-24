# 05 — Dependency Model

## Entity
`TaskDependency` (ProjectId, PredecessorTaskId, SuccessorTaskId, Type).

## Type
`FS` (finish-to-start) only. Other types (SS/FF/SF) deferred.

## Validation
- Self-dependency rejected.
- Duplicate rejected (409).
- Both tasks must belong to the project.
- Cycle rejected via `WouldCreateDependencyCycleAsync` (DFS over the project's dependency graph).

## Cross-project
Disallowed by default (both tasks must be in the same project).

## Tests
`PmPlanningServiceTests`: self, deep cycle (1→2→3 then 3→1), no-cycle allowed.
