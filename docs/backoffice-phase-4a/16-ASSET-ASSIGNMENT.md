# 16 — Asset Assignment

## Entity
`AssetAssignment`: AssetId, EmployeeId, AssignedAt, ReturnedAt,
AssignedByEmployeeId, ConditionAtAssignment, ConditionAtReturn.

## Invariants
- Append-only history (never overwrite).
- Asset cannot be assigned to two active employees simultaneously.
- Returned assignment closes history (sets ReturnedAt).
- Disposed asset cannot be reassigned.

## Offboarding connection
Later/offboarding should show assets-not-returned before separation completes
(deferred — not blocked now).

## Access
`CanAssignAssets` (Admin/HR).
