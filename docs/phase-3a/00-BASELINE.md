# 00 — Phase 3A Baseline

> Established 2026-08-21 before any Phase 3A changes beyond build fixes.

## Pre-existing build failures fixed (uncommitted code)

The repository had pre-existing build errors in uncommitted work. These were fixed as a prerequisite to establishing a baseline:

| # | File | Error | Fix |
|---|---|---|---|
| B-01 | `Program.cs` | `public partial class Program { }` placed inside try block before `app.Run()` causing CS1022/CS8803 | Moved `public partial class Program { }` after `finally` block |
| B-02 | `ProjectTasksController.cs` / `IssuesController.cs` | Duplicate `AddCommentRequest` class (CS0101) | Removed duplicate from `IssuesController.cs` (ProjectTasks version is superset) |
| B-03 | `AuditLogInterceptor.cs` | `channel.Writer` not on `IAuditLogChannel` interface (CS1061); `TryWrite` no overload with 1 arg (CS1501) | Changed `_channel` from `Channel<AuditLog>` to `IAuditLogChannel`; use `channel.TryWrite(entry)` |
| B-04 | `LeaveController.cs` | `LeaveRequestId` not on `LeaveRequest` entity (CS1061) | Changed to `LeaveId` (entity has `LeaveId`, DTO has `LeaveRequestId`) |
| B-05 | `LeaveController.cs` | `TotalDays` decimal→int conversion (CS0266) | Changed `LeaveRequestListItem.TotalDays` from `int` to `decimal` |
| B-06 | `EmployeeLoansController.cs` | `CreatedAt` not on `LoanRepayment` (CS0117) | Removed `CreatedAt = DateTime.UtcNow` (entity has `RepaidAt`) |
| B-07 | `PayrollController.cs` | `SalaryCurrency` not on `SalarySlip` (CS1061) | Changed to `s.Employee.SalaryCurrency` (property is on `Employee`) |
| B-08 | `AuditLogTests.cs` | Constructor mismatch after AuditLogInterceptor change | Added mock `IAuditLogChannel` to test constructor |
| B-09 | `LicenseMiddlewareTests.cs` | Constructor missing `ILicenseKeyCache` parameter | Added mock `ILicenseKeyCache` field + constructor calls |
| B-10 | `payroll/page.tsx` | `PaginatedResponse<SalarySlip>` not assignable to `SalarySlip[]` (TS2345) | Changed to `data.items ?? []` |

## Baseline results (after build fixes)

| Command | Result | Notes |
|---|---|---|
| Backend build (`dotnet build LaoHR.API.csproj -c Release`) | **PASS** (0 errors, 5 warnings) | Pre-existing OpenTelemetry NU1902 warnings + 1 CS8604 nullability |
| Test project build (`dotnet build LaoHR.Tests.csproj -c Release`) | **PASS** (0 errors, 12 warnings) | Pre-existing warnings |
| Backend tests (`dotnet test -c Release --no-build`) | **47 passed, 21 failed** (68 total) | Failures are integration tests using `WebApplicationFactory` — entry point issue from try/catch wrapping in Program.cs |
| Frontend typecheck (`npx tsc --noEmit`) | **PASS** (0 errors) | After B-10 fix |

## Pre-existing test failures analysis

The 21 test failures are all integration tests that use `CustomWebApplicationFactory` → `WebApplicationFactory<Program>`. The error: "The entry point exited without ever building an IHost" — this is because the `try { } catch { } finally { }` block in `Program.cs` wraps the entire host creation, and `WebApplicationFactory` expects the `Program` class to be the entry point. The `public partial class Program { }` declaration must be visible to `WebApplicationFactory`.

This is a known issue with Serilog try/catch wrapping + `WebApplicationFactory`. The fix is to ensure `public partial class Program { }` is declared OUTSIDE the try/catch block (which I already moved). The remaining failures may be from the `Testing` environment configuration or the factory's inability to find the entry point.

## Git status

- Branch: `master`
- Modified files: 14 (pre-existing uncommitted work from Phase 2-2C)
- Untracked files: 20+ (new controllers, Docker, CI, docs)
- Phase 3A changes: 10 build fixes (B-01 through B-10)