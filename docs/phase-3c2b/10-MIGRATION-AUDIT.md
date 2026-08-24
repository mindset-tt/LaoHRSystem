# 10 — Migration Audit

## Chain
```
InitialCreatePostgres
  ↓
AddApprovalEssMssNotifications
```

## `AddApprovalEssMssNotifications` review
- **New tables**: `AttendanceCorrections`, `Notifications` (purely additive).
- **New indexes**: `IX_AttendanceCorrections_AttendanceId`, `IX_AttendanceCorrections_EmployeeId_Status`, `IX_Notifications_UserId_IsRead_CreatedAt`.
- **No unexpected drops**: none.
- **Seed timestamp changes**: `UpdateData` on `Departments`/`Employees`/`Holidays`/`LeavePolicies`/`SystemSettings` `CreatedAt`/`UpdatedAt` — these are EF `HasData` seed timestamp refreshes (harmless, expected when the model snapshot is regenerated).
- **FK delete behavior**: `AttendanceCorrections` → `Attendances`/`Employees` use `Cascade` (a correction is meaningless without its parent). `Notifications` → `Users` uses `Cascade` (notifications die with the user). Approval history (`ApprovalRequest`/`Step`/`Action`) is NOT cascaded — preserved.
- **Approval history protected**: no migration touches approval tables.

## No rebase
`InitialCreatePostgres` was NOT regenerated. The chain is preserved as additive history.

## Real PostgreSQL
NOT RUN — Docker daemon not running; PostgreSQL at 10.233.141.2:5433 not reachable.
