# 12 — Phase 3B Handoff

# PHASE 3B HANDOFF

## Current Architecture
Next.js 16 → ASP.NET Core 10 → EF Core (Npgsql) → PostgreSQL 16. Docker Compose + GitHub Actions CI. Modular monolith.

## Work Completed in 3A

### Build fixes (pre-existing uncommitted code)
- Fixed `Program.cs` syntax error (try/catch/finally + public partial class Program placement)
- Fixed duplicate `AddCommentRequest` class (removed from IssuesController)
- Fixed `AuditLogInterceptor` constructor (IAuditLogChannel interface mismatch)
- Fixed `LeaveController` property name (`LeaveRequestId` → `LeaveId`)
- Fixed `LeaveRequestListItem.TotalDays` type (`int` → `decimal`)
- Fixed `EmployeeLoansController` (removed non-existent `CreatedAt` on `LoanRepayment`)
- Fixed `PayrollController` (SalaryCurrency from Employee, not SalarySlip)
- Fixed `payroll/page.tsx` type error (PaginatedResponse → items array)

### Correctness
- Employee routes: NOT AMBIGUOUS (Phase 1 audit was wrong — two `[HttpGet("{id}")]` on different controllers)
- CompanySettings: Added explicit `[Authorize]` (class-level) + `[Authorize(Roles="Admin,HR")]` on PUT
- CI branch alignment: Added `master` to CI triggers

### Security
- Password hashing: Migrated from SHA-256 to PBKDF2-HMAC-SHA256 (600k iterations, salted, constant-time)
- Migration strategy: rehash-on-login with `PasswordHashVersion` column (1=legacy, 2=PBKDF2)
- JWT fail-fast: Production/Staging throws if `Jwt:Key` missing
- Global exception handler: IExceptionHandler + ProblemDetails (RFC 7807)

### Database
- PIT brackets updated to Law No. 88/NA verified values (2.5M/5M/15M/25M/65M)
- `AppUser.PasswordHashVersion` column added
- `decimal.Parse` InvariantCulture fix in PayrollService (critical locale bug)

### Frontend
- Mock activity data removed from dashboard (replaced with empty state)
- /403 forbidden page created
- Frontend typecheck passes

### Tests
- Payroll tax test updated with verified brackets + rule IDs
- Audit log tests updated for fire-and-forget channel pattern
- Test constructors fixed for AuditLogInterceptor + LicenseMiddleware changes

## Security State
- Password hashing: PBKDF2-HMAC-SHA256 (OWASP acceptable, 600k iterations)
- JWT: fail-fast in Production, hardcoded fallback only in Dev/Test
- CompanySettings: Admin/HR only for writes, authenticated for reads
- Default-deny: preserved (FallbackPolicy)
- Global exception handler: ProblemDetails, no stack trace in prod

## Database State
- PIT brackets: Updated to Law 88/NA (VERIFIED, LOCKED)
- NSSF ceiling: 4,500,000 (VERIFIED via Notification 0824/NSSFO, LOCKED)
- NSSF rates: 6.0%/5.5% (VERIFIED, LOCKED)
- EnsureCreated: still used in Testing; Migrate() with fallback in non-Testing
- PG-compatible migrations: NOT YET CREATED (Phase 3B task)

## Test State
- 47 passed, 21 failed (68 total)
- 21 failures: ALL pre-existing WebApplicationFactory entry point issue (try/catch wrapping)
- 0 new failures from Phase 3A changes
- Payroll tax test: PASS (updated with verified brackets)
- Audit log tests: PASS (updated for channel pattern)

## Infrastructure State
- Docker compose: exists (postgres + api + web)
- CI: GitHub Actions (backend build+test, frontend typecheck+build, docker smoke)
- CI branches: aligned to include master
- Backup/DR: NOT YET IMPLEMENTED (Phase 3B task)
- TLS/reverse proxy: NOT YET IMPLEMENTED (Phase 3B task)

## UX State
- Dashboard mock activity: REMOVED (empty state)
- /403 page: CREATED
- proxy.ts: correct for Next.js 16 (not renamed)
- Frontend typecheck: PASS

## Compliance Architecture State
- PIT brackets: LOCKED (Law 88/NA, PwC VERIFIED)
- NSSF rates: LOCKED (PwC VERIFIED)
- NSSF ceiling: LOCKED (Notification 0824/NSSFO, PwC VERIFIED)
- OT multipliers: LOCKED (Labour Law Art. 48, AsianLII VERIFIED)
- Leave: LOCKED (Labour Law Art. 20/21/39/40, AsianLII VERIFIED)
- Severance: LOCKED (Labour Law Art. 29/33, AsianLII VERIFIED)
- Versioned rule engine: NOT YET IMPLEMENTED (Phase 3B task)

## Locked Lao Rules
32 of 38 compliance rules LOCKED with VERIFIED evidence. See `docs/research-phase-2c/18-COMPLIANCE-LOCK-STATUS.md`.

## Blocked Lao Rules
6 rules still PROFESSIONAL_CONFIRMATION_REQUIRED:
- OT hourly divisor (labour lawyer)
- Leave carry-over (labour lawyer)
- NSSF minimum floor (LSSO)
- NSSF exact contribution-base definition (LSSO)
- Bank salary file format (BCEL/JDB)
- Visa/stay permit categories (immigration lawyer)

## Outstanding Professional Confirmations
See `docs/research-phase-2c/15-PROFESSIONAL-CONFIRMATION-PACK.md` for full question sets.

## Technical Debt Remaining
- EnsureCreated → PG-compatible migrations (Phase 3B)
- WebApplicationFactory entry point issue (try/catch wrapping)
- Frontend mock data in employee edit + documents (Phase 3B)
- proxy.ts redirect logic (Phase 3B)
- No frontend tests (Phase 3B)
- No backup/DR (Phase 3B)
- No TLS/reverse proxy (Phase 3B)
- No versioned rule engine (Phase 3B)
- OpenTelemetry NU1902 vulnerability warnings (update packages)

## Known Bugs
- WebApplicationFactory integration tests fail (21 tests) — entry point issue from try/catch wrapping in Program.cs
- Pre-existing: `DbSeeder.SeedAddresses` fails on PostgreSQL (SQL Server syntax)

## Safe Next Features
- PM/Finance/Knowledge tests (no legal dependency)
- Frontend tests (Vitest + RTL)
- Backup/DR (WAL + PITR)
- TLS/reverse proxy (Caddy)
- Versioned rule engine architecture
- OvertimeEntry model (with configurable rates + LOCKED multipliers)
- Termination/severance model (with LOCKED formulas)

## Do Not Implement Yet
- NSSF contribution base exact definition (BLOCKED)
- OT hourly divisor (BLOCKED)
- Leave carry-over rules (BLOCKED)
- Bank salary file format (BLOCKED)
- Production payroll (PRODUCTION_PAYROLL_READY = NO)

## Recommended Next Workstream
1. Fix WebApplicationFactory entry point issue (so integration tests pass)
2. Create PG-compatible EF migrations (replace EnsureCreated)
3. Add backup/DR (WAL + PITR)
4. Add TLS/reverse proxy (Caddy)
5. Add frontend tests
6. Implement versioned rule engine
7. Add PM/Finance/Knowledge controller tests

## Validation Commands
```powershell
$env:Path = "$HOME\.dotnet;" + $env:Path
# Backend build
cd D:\LaoHRSystem\Backend\LaoHR.API; dotnet build LaoHR.API.csproj -c Release
# Test project build
cd D:\LaoHRSystem\Backend\LaoHR.Tests; dotnet build LaoHR.Tests.csproj -c Release
# Tests (Testing env = InMemory, no license mw)
$env:ASPNETCORE_ENVIRONMENT="Testing"
$env:Jwt__Key="ci-only-test-key-not-for-production-do-not-use-64+chars-long-string"
$env:Jwt__Issuer="LaoHRServer"
$env:Jwt__Audience="LaoHRClient"
dotnet test LaoHR.Tests.csproj -c Release --no-build --verbosity minimal
# Frontend typecheck
cd D:\LaoHRSystem\frontend; npx tsc --noEmit
```