# 02 — Integration Test Recovery

> Phase 3B workstream 3B.1. Result: 21 failures → 0 failures.

## Root cause

`System.InvalidOperationException: The logger is already frozen` from `Serilog.Extensions.Hosting.ReloadableLogger.Freeze()`.

The `Program.cs` used `CreateBootstrapLogger()` which creates a `ReloadableLogger`. When `WebApplicationFactory<Program>` re-runs the entry point during integration testing, `UseSerilog`'s own `ReloadableLogger` collides with the bootstrap logger, throwing "The logger is already frozen" before the host is built.

## Fix

Changed `CreateBootstrapLogger()` → `CreateLogger()` in `Program.cs`. A plain `Logger` (not `ReloadableLogger`) does not conflict with `UseSerilog`.

## Additional stale test fixes

After the host recovered, 6 tests still failed for other reasons:

| Test | Root cause | Fix |
|---|---|---|
| `GetEmployees_Authorized_ReturnsList` | Expected `List<Employee>` but API returns `PaginatedResponse<EmployeeListItem>` | Updated to deserialize `PaginatedResponse<EmployeeListItem>` |
| `GetHolidays_ReturnsList` | Same pagination mismatch | Updated to `PaginatedResponse<Holiday>` |
| `GetSettings_ReturnsOk` | No authentication (default-deny) | Added `AuthenticateAsync()` |
| `GetDocuments_ValidId_ReturnsOk` | No authentication | Added `AuthenticateAsync()` |
| `DownloadNssf_InvalidPeriod_ReturnsBadRequest` | No authentication | Added `AuthenticateAsync()` |
| `ClockIn_ValidRequest_ReturnsOk` | "employee" demo user not linked to Employee record → `GetCurrentEmployeeId()` threw | Linked "employee" user to first employee in `DbSeeder` |

## Result

- Before: 47 passed, 21 failed
- After: 78 passed, 0 failed (added 10 new tests in Phase 3B)

## Test host architecture

Kept the existing `CustomWebApplicationFactory : WebApplicationFactory<Program>` + `public partial class Program { }` pattern. No separate fake application, no middleware bypass, no security disabling. Integration tests run against the real pipeline.