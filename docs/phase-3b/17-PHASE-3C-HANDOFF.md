# 17 — Phase 3C Handoff

# LAOHR PHASE 3C HANDOFF

## 1. Project State
LaoHR is a .NET 10 + Next.js 16 + PostgreSQL 16 HR/payroll/PM platform for Lao PDR. Foundation is now stable: builds pass, tests pass (0 failures), PostgreSQL migrations generated, versioned compliance architecture in place.

## 2. Phase 3B Changes
- **Integration test recovery**: Fixed Serilog `CreateBootstrapLogger` → `CreateLogger` (root cause of 21 failures). 21 → 0 failures.
- **Stale test fixes**: pagination envelope, authentication, employee-user linking.
- **PostgreSQL migrations**: Removed 18 SQL Server migrations, generated fresh `InitialCreatePostgres` baseline (PostgreSQL types).
- **Production startup**: Removed `EnsureCreated` fallback; `Migrate()` only in non-Testing.
- **Versioned compliance architecture**: `ComplianceRule` + `PayrollRuleSnapshot` entities, `ComplianceRuleService`, seeded 28 VERIFIED rules.
- **Frontend mock removal**: employee edit fallback, EmployeeForm departments fallback, employee detail documents mock.
- **/403 page**: fixed `<a>` → `<Link>`.
- **New tests**: 10 added (compliance rule service, password hashing).

## 3. Backend Health
- Build: PASS (0 errors, 5 pre-existing warnings)
- Tests: 78 passed, 0 failed

## 4. Test Health
- 78 tests, 0 failures
- New: ComplianceRuleServiceTests (4), PasswordHasherTests (6)

## 5. Database Health
- PostgreSQL baseline migration generated (PostgreSQL types verified)
- `PasswordHashVersion` column present
- `ComplianceRules` + `PayrollRuleSnapshots` tables present

## 6. Migration State
- Fresh `InitialCreatePostgres` baseline migration
- `LaoHRDbContextFactory` design-time factory
- Production uses `Migrate()` only
- Live migration validation: NOT RUN (Docker/PostgreSQL unavailable)

## 7. Security State
- PBKDF2-HMAC-SHA256 password hashing (600k iterations, salted, constant-time)
- Rehash-on-login migration (PasswordHashVersion 1→2)
- JWT fail-fast in Production
- CompanySettings Admin/HR authorization
- Global exception handler (ProblemDetails)
- Default-deny authorization preserved

## 8. Infrastructure State
- Docker compose exists (postgres + api + web)
- CI aligned to master branch
- Backup/DR: NOT IMPLEMENTED (Docker unavailable)
- TLS/reverse proxy: NOT IMPLEMENTED

## 9. Frontend State
- Typecheck: PASS
- Production build: PASS
- Lint: 40 pre-existing errors (React Compiler set-state-in-effect + no-explicit-any) — large refactor deferred
- Mock data: removed from dashboard, employee edit, EmployeeForm, employee detail documents
- /403 page: created

## 10. Compliance Architecture
- `ComplianceRule` entity (effective-dated, versioned, source metadata)
- `ComplianceRuleService` (effective-date lookup, BLOCKED exclusion)
- 28 VERIFIED rules seeded
- BLOCKED rules NOT seeded

## 11. Payroll Reproducibility
- `PayrollRuleSnapshot` entity added
- `SalarySlip` already stores computed values
- Effective-date lookup tested (superseded rule returns correct historical value)

## 12. Locked Lao Rules
28 VERIFIED rules (PIT brackets, NSSF rates/ceiling, minimum wage, OT multipliers, leave minimums, severance, foreign quota).

## 13. Blocked Lao Rules
- OT hourly divisor
- Leave carry-over
- NSSF minimum floor
- NSSF exact contribution-base definition
- Bank salary file format
- Visa/stay permit categories

## 14. Production Readiness
- PRODUCTION_PAYROLL_READY = NO (blocked rules remain)
- Backup/DR: NOT COMPLETE
- TLS: NOT COMPLETE

## 15. Remaining Technical Debt
- Live migration/backup/restore validation (Docker unavailable)
- Frontend lint (40 pre-existing errors)
- Frontend tests (not configured)
- OpenTelemetry NU1902 vulnerability warnings
- `DbSeeder.SeedAddresses` fails on PostgreSQL (SQL Server syntax)

## 16. Remaining Bugs
- None known (0 test failures)

## 17. Safe Product Expansion Areas
- Organization hierarchy (ManagerId, department hierarchy)
- Approval engine (Stateless)
- ESS/MSS
- Notifications
- Reporting/dashboards
- PM UX (Kanban/Gantt/resources)

## 18. Product Areas Still Blocked
- Production payroll (legal blockers)
- Bank integration (file format unconfirmed)

## 19. Recommended Phase 3C Priorities
1. Organization hierarchy (ManagerId + department hierarchy) — foundation for approvals/ESS/MSS
2. Approval engine (Stateless)
3. ESS/MSS
4. Notifications
5. Reporting/dashboards

## 20. Validation Commands
```powershell
$env:Path = "$HOME\.dotnet;" + $env:Path
# Backend build
cd D:\LaoHRSystem\Backend\LaoHR.API; dotnet build LaoHR.API.csproj -c Release
# Tests
cd D:\LaoHRSystem\Backend\LaoHR.Tests
$env:ASPNETCORE_ENVIRONMENT="Testing"
$env:Jwt__Key="ci-only-test-key-not-for-production-do-not-use-64+chars-long-string"
$env:Jwt__Issuer="LaoHRServer"
$env:Jwt__Audience="LaoHRClient"
dotnet test LaoHR.Tests.csproj -c Release --no-build --verbosity minimal
# Frontend
cd D:\LaoHRSystem\frontend; npx tsc --noEmit; npm run build
```

## 21. Do-Not-Break Rules
See `docs/ai-handoff/28-DO-NOT-BREAK.md`. Key: API routes, PaginatedResponse envelope, JWT model, refresh rotation, default-deny auth, license enforcement, EF entity model, audit pipeline, Npgsql legacy timestamp switch, payroll formulas, payslip PDF, bank transfer formats, i18n structure, apiClient localStorage keys, CSS tokens, EntityComment types, Docker compose services, CI env conventions.