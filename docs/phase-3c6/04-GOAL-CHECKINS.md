# 04 — Goal Check-ins

## Model
`GoalCheckIn` (GoalId, ProgressPercent, Status, Comment, CreatedByEmployeeId, CreatedAt).

## History
Check-ins preserve historical progress (no overwrite). Each check-in also updates the goal's current ProgressPercent/Status.

## Authorization
`CanManageGoalAsync` (owner or manager).
