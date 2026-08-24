# 02 — Read-Scope Security

## Scope service
`IDataScopeService` / `DataScopeService` is the single source of truth for read-path authorization.

Rules:
- **Admin / HR**: full organization scope.
- **Manager**: self + direct reports (MSS policy — direct reports only).
- **Employee**: self only.
- **Unlinked user**: no employee scope.

Methods: `IsPrivileged()`, `IsManagerAsync()`, `GetCurrentEmployeeId()`, `GetVisibleEmployeeIdsAsync()`, `CanViewEmployeeAsync()`, `IsSelf()`, `IsManagerOfAsync()`.

## Closed read-path IDOR (Phase 3C3)
- `GET /api/leave` — intersected with visible scope.
- `GET /api/leave/balance` — non-privileged forced to self.
- `GET /api/leave/calendar` — intersected with visible scope.
- `GET /api/leave/export` — intersected with visible scope.
- `GET /api/expenses` — intersected with visible scope.
- `GET /api/expenses/{id}` — `CanViewEmployeeAsync` (403 otherwise).
- `GET /api/employeeLoans` — intersected with visible scope.
- `GET /api/employeeLoans/{id}` — `CanViewEmployeeAsync` (403 otherwise).
- `GET /api/documents/employee/{employeeId}` — `CanViewEmployeeAsync` (403 otherwise).

## Export security
Exports use the same scope as list/dashboard (e.g. leave export intersects visible scope).

## Tests
- `ReadPathIdorTests` (6 tests): employee A cannot list/read employee B's leave/expense/loan/documents.
- `AnalyticsAuthorizationTests` (5 tests): role-based dashboard visibility.
