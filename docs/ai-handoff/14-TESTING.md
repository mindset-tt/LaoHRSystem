# 14 — Testing

> `VERIFIED` from `LaoHR.Tests/` structure + subagent.

## Test framework

xUnit 2.9.3 + `Microsoft.AspNetCore.Mvc.Testing` 10.0.1 (integration via `WebApplicationFactory` + InMemory or postgres) + Moq 4.20.72 + FluentAssertions 8.8.0 + Bogus 35.6.5 (fake data) + coverlet 6.0.4 (Cobertura coverage).

## Test inventory

### Unit tests (`Tests/Unit/`)
| Area | Files | Coverage |
|---|---|---|
| Controllers | `BankTransferControllerTests.cs` | Bank transfer file generation |
| Services | `LicenseServiceTests.cs` (in `Unit/Services/`) | License RSA verify/tamper detection |
| Models | (in `Unit/Models/` if exists) | — |
| Meta | `CoverageBoosterTests.cs`, `UnitTest1.cs` | Coverage boosters |

### Integration tests (`Tests/Integration/`)
| Area | Files | Coverage |
|---|---|---|
| Controllers | `AttendanceControllerTests`, `AuthControllerTests`, `DocumentsControllerTests`, `EmployeesControllerTests`, `HolidaysControllerTests`, `LeaveControllerTests`, `LicenseControllerTests`, `PayrollControllerTests`, `ReportsControllerTests`, `SettingsControllerTests` | HR-core controllers |
| Api | `GeneralApiTests.cs` (in `Integration/Api/`) | General/smoke |
| Services | (in `Integration/Services/`) | — |
| Data | (in `Integration/Data/`) | — |
| Helpers | `Tests/Helpers/` | Test helpers/factories |

## Test matrix

| Area | Unit | Integration | E2E | Current Confidence |
|---|---|---|---|---|
| Auth | ✅ | ✅ | ❌ | MEDIUM-HIGH |
| Employees | ❌ | ✅ | ❌ | MEDIUM |
| Attendance | ❌ | ✅ | ❌ | MEDIUM |
| Leave | ❌ | ✅ | ❌ | MEDIUM |
| Payroll | ❌ | ✅ | ❌ | MEDIUM |
| Reports | ❌ | ✅ | ❌ | MEDIUM |
| Holidays | ❌ | ✅ | ❌ | MEDIUM |
| Settings | ❌ | ✅ | ❌ | MEDIUM |
| Documents | ❌ | ✅ | ❌ | MEDIUM |
| Bank Transfer | ✅ | ❌ | ❌ | MEDIUM |
| License | ✅ | ✅ | ❌ | HIGH |
| Projects/Tasks | ❌ | ❌ | ❌ | NONE |
| Risks/Issues | ❌ | ❌ | ❌ | NONE |
| Resources | ❌ | ❌ | ❌ | NONE |
| Expenses/Loans | ❌ | ❌ | ❌ | NONE |
| Announcements/Knowledge | ❌ | ❌ | ❌ | NONE |
| Comments | ❌ | ❌ | ❌ | NONE |
| Refresh tokens | ❌ | ✅ (via Auth) | ❌ | LOW-MEDIUM |
| Retention job | ❌ | ❌ | ❌ | NONE |
| Audit interceptor | ❌ | ❌ | ❌ | NONE |
| Frontend | ❌ | ❌ | ❌ | NONE |

## Important functionality with NO tests

- **All PM/PL controllers** (Projects, ProjectTasks, Milestones, Risks, Issues, Resources) — `VERIFIED` no test files.
- **All Finance controllers** (Expenses, EmployeeLoans).
- **All Knowledge controllers** (Announcements, KnowledgeArticles, Comments).
- **RetentionService** background job.
- **AuditLogInterceptor / AuditLogWriter** fire-and-forget pipeline.
- **PayrollService** calculation engine (only controller-level integration tests).
- **Frontend** — zero tests (no Vitest/RTL/Playwright).

## Test configuration

- CI runs tests with `ASPNETCORE_ENVIRONMENT=Testing` (InMemory DB) against postgres service container.
- Coverage: `coverage.cobertura.xml` exists; `CoverageReport_Logic/` present. No coverage threshold gate in CI.

## Risks

- Newer feature layers (PM/Finance/Knowledge) have **no test coverage** — regressions invisible.
- Frontend has **no tests at all**.
- No E2E happy-path validation.