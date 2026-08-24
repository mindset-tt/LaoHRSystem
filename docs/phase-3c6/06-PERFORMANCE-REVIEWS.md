# 06 — Performance Reviews

## Model
`PerformanceReview` (CycleId, EmployeeId, ManagerEmployeeId [snapshot], PositionId [snapshot], DepartmentId [snapshot], Status, OverallRating, SelfAchievements, SelfChallenges, ManagerComments, DevelopmentNeeds, timestamps).

## Manager snapshot
ManagerEmployeeId is snapshotted at review creation. If Employee.ManagerId changes later, the historical review stays with the original reviewer. New cycle uses current manager. Tested.

## Position snapshot
PositionId/DepartmentId snapshotted for review context (historical reviews don't appear under today's position).

## Status machine
`NOT_STARTED → SELF_REVIEW → MANAGER_REVIEW → FINALIZED → ACKNOWLEDGED`. Server validates transitions.

## Self review
Employee submits achievements/challenges (identity from auth, not client EmployeeId).

## Manager review
Manager submits rating/comments/development needs (identity from snapshotted reviewer).

## Finalize
Only MANAGER_REVIEW → FINALIZED. Finalized review is a stable historical record (reopen deferred).

## Acknowledgement
Employee acknowledges "I have seen this review" (NOT "I agree"). Optional comment.
