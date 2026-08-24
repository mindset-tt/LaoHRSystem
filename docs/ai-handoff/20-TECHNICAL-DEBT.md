# 20 — Technical Debt Register

> Do NOT implement. Report only. Severity: CRITICAL / HIGH / MEDIUM / LOW.

| ID | Area | Problem | Severity | Evidence | Suggested Future Action |
|---|---|---|---|---|---|
| TD-01 | Database | EF migrations are SQL Server-flavored; runtime uses Npgsql; `EnsureCreated()` fallback creates schema without migration history | HIGH | `Program.cs:344-355`, `Migrations/` | Generate PostgreSQL-compatible migrations (or rescaffold from current model); commit; switch to `Migrate()` only |
| TD-02 | Auth | `PasswordHasher` uses SHA-256 (no salt/work factor) — not a password-hashing KDF | HIGH | `Services/PasswordHasher.cs` | Replace with PBKDF2/Argon2/bcrypt with per-user salt |
| TD-03 | Frontend | Tokens in `localStorage` (XSS-vulnerable) | MEDIUM | `lib/apiClient.ts` | Consider httpOnly cookie + CSRF token, or keep JWT but mitigate XSS |
| TD-04 | Backend | `LeaveController` is 541 LOC, bundles 4+ concerns | MEDIUM | `Controllers/LeaveController.cs` | Split into LeaveRequests/Balances/Calendar/Policies controllers |
| TD-05 | Backend | `EmployeesController` has ambiguous `[HttpGet("{id}")]` routes (employee vs department) | HIGH | `Controllers/EmployeesController.cs` | Fix route conflict; separate DepartmentsController |
| TD-06 | Backend | `CompanySettingsController` GET is `[AllowAnonymous]` | MEDIUM | `Controllers/CompanySettingsController.cs` | Add `[Authorize]` |
| TD-07 | Backend | No global exception handler middleware | MEDIUM | `Program.cs` | Add `UseExceptionHandler` with problem-details response |
| TD-08 | Backend | Hardcoded UTC+7 in `AttendanceController` + `datetime.ts`; no central timezone constant | MEDIUM | `AttendanceController.cs`, `lib/datetime.ts` | Centralize `TimeZoneInfo` "Asia/Vientiane"; use Npgsql `timestamp with time zone` |
| TD-09 | Database | No `RowVersion` / optimistic concurrency on `SalarySlip`, `LeaveBalance` | MEDIUM | `Entities.cs` | Add `[Timestamp]`/`RowVersion` column on critical tables |
| TD-10 | Database | Soft-delete inconsistency (`IsActive` vs `DeletedAt` vs hard-delete) | LOW | `Entities.cs` | Standardize on one pattern |
| TD-11 | Backend | Only `LoginRequestValidator` exists; other validation inline | LOW | `Validators/` | Add validators for key write endpoints |
| TD-12 | Backend | No API versioning | LOW | — | Add `Asp.Versioning.Mvc` when breaking changes expected |
| TD-13 | Frontend | `EmployeeForm` uses manual `useState` (inconsistent with react-hook-form elsewhere) | LOW | `components/forms/EmployeeForm.tsx` | Migrate to react-hook-form + zod |
| TD-14 | Frontend | Inline Lao strings in `settings/page.tsx` bypass i18n dictionary | LOW | `settings/page.tsx` | Move to `lib/i18n.ts` |
| TD-15 | Frontend | `proxy.ts` middleware redirect commented out (no-op) | MEDIUM | `src/proxy.ts` | Implement or remove; wire as `middleware.ts` |
| TD-16 | Frontend | No `/403` page despite `useRequirePermission` redirect | MEDIUM | `useRequirePermission` → `/403` | Create `/403` page |
| TD-17 | Frontend | No list virtualization (10k rows render all) | MEDIUM | DataTable component | Add `@tanstack/react-virtual` for large lists |
| TD-18 | Frontend | No error boundaries | LOW | — | Add `<ErrorBoundary>` at route level |
| TD-19 | Frontend | No frontend tests | HIGH | — | Add Vitest + RTL + Playwright |
| TD-20 | Testing | PM/Finance/Knowledge controllers have zero tests | HIGH | `Tests/` | Add integration tests for all new controllers |
| TD-21 | Backend | `DashboardController` returns 4 hard-coded scalars (not extensible) | LOW | `DashboardController.cs` | Refactor to widget-based/composable |
| TD-22 | Backend | Project-member roles stored but not enforced in controllers | MEDIUM | `ProjectsController`, `ProjectMember.Role` | Add policy-based authorization per project role |
| TD-23 | Infrastructure | No reverse proxy / TLS termination in Docker | MEDIUM | `docker-compose.yml` | Add nginx/Caddy in front of api+web |
| TD-24 | Infrastructure | No Postgres backup config | MEDIUM | `docker-compose.yml` | Add backup volume + `pg_dump` cron |
| TD-25 | Backend | `DbSeeder.SeedAddresses` runs SQL Server SQL (fails on PG, skipped) | LOW | `Data/DbSeeder.cs` | Port address seed SQL to PostgreSQL |
| TD-26 | Backend | Bridge.Service + Bridge.Config still reference SQL Server | LOW | `.csproj` | Migrate to Npgsql if still used |
| TD-27 | Frontend | Base URL mismatch: apiClient default `localhost:5000` vs Docker `localhost:8080` | LOW | `apiClient.ts`, `Dockerfile` | Align; rely on `NEXT_PUBLIC_API_URL` |
| TD-28 | Backend | Hardcoded JWT fallback key in `Program.cs` | MEDIUM | `Program.cs` | Fail fast if `Jwt:Key` empty in Production |
| TD-29 | Observability | No metrics exporter; OTLP traces optional/no collector | LOW | `Program.cs` | Add metrics + ship to collector in prod |
| TD-30 | CI | CI triggers on `main`/`dev` but repo branch is `master` | LOW | `.github/workflows/ci.yml` | Align branch names |