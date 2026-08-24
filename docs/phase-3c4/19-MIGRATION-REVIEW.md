# 19 — Migration Review

## Chain (append-only)
```
InitialCreatePostgres
  → AddApprovalEssMssNotifications
  → AddPmPlanningAndDependencies
```

## `AddPmPlanningAndDependencies` review
- **AddColumn**: `ProjectTasks.ParentTaskId` (nullable, self-FK).
- **New tables**: `ProjectAssumptions`, `ProjectDecisions`, `TaskDependencies`.
- **New indexes**: `IX_ProjectTasks_ParentTaskId`, `IX_ProjectTasks_ProjectId_DueDate`, `IX_TaskDependencies_*`, `IX_ProjectAssumptions_*`, `IX_ProjectDecisions_ProjectId`.
- **No DROP / destructive ALTER**: none.
- **FK delete behavior**: `TaskDependencies` → `ProjectTasks`/`Projects` Cascade (a dependency is meaningless without its tasks). `ProjectAssumptions`/`ProjectDecisions` → `Projects` Cascade. `ParentTaskId` self-FK has no cascade (default Restrict) — deleting a parent with children is prevented.
- **Seed timestamp changes**: `UpdateData` on `Departments`/`Employees`/`Holidays`/etc. — EF `HasData` timestamp refresh (harmless).

## Project history preservation
Project history does not disappear when an employee becomes inactive (no cascade from Employee to Project/Task).
