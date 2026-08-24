# 04 — Task Hierarchy

## Model
`ProjectTask.ParentTaskId` (nullable, self-referencing). Two-level hierarchy (summary → subtask).

## Cycle prevention
`WouldCreateTaskCycleAsync(taskId, parentTaskId)` walks the parent chain; rejects self-parent and deep cycles (A→B→C→A).

## Tests
`PmPlanningServiceTests`: self-parent, deep cycle, no-cycle allowed.

## Semantics
Parent summary dates are manually controlled (not auto-derived from children). No auto-rollup of progress.
