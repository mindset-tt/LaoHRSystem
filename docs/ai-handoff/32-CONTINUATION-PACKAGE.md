# PROJECT CONTINUATION PACKAGE

> Designed to be pasted into a fresh LLM as the single continuation prompt. High-density. `VERIFIED` 2026-08-21.

## 1. Project Identity

**LaoHR System** — enterprise HR + Payroll + Attendance + lightweight PM/work-management platform for organizations in Laos. Monorepo at `d:\LaoHRSystem`, git branch `master`. Bilingual (Lao/English), Lao-compliant (NSSF, Lao PIT, LAK/USD/THB/CNY, Asia/Vientiane UTC+7). License-gated (RSA, HTTP 402 if invalid).

## 2. Project Objective

Single system for Lao-compliant HR/payroll + biometric attendance (ZKTeco) + project/task/risk/issue tracking + expenses/loans + knowledge/announcements. Target users: HR departments, payroll officers, project managers, employees in Lao organizations. Maturity: Beta / Production Candidate (HR core production-quality; PM/Finance/Knowledge layers functional but need hardening + tests).

## 3. Current Architecture

Layered modular monolith (backend) + feature-based App Router SPA (frontend), monorepo.
- **Backend**: .NET 10 ASP.NET Core API. Controllers → Services → EF Core `LaoHRDbContext` → PostgreSQL (Npgsql 10.0.1). Middleware pipeline: Swagger(Dev) → CORS → RateLimiter → SerilogRequestLogging → LicenseMiddleware(402) → Authentication(JWT HS256) → Authorization(default-deny FallbackPolicy) → MapControllers → HealthChecks. Audit: `AuditLogInterceptor` → `Channel<AuditLog>` → `AuditLogWriter` (fire-and-forget, separate scope). Jobs: `LeaveScheduledJobsService` (hourly, monthly accrual + year-end carry-over), `RetentionService` (disabled by default). InMemory DB for Testing env.
- **Frontend**: Next.js 16.1.1 App Router. `(dashboard)` route group with Sidebar+Header shell. Pages → `lib/endpoints/*` typed modules → `lib/apiClient.ts` (native fetch + auto-refresh + retry) → backend. State: React Context (Auth/Theme/Language/Toast). No UI/chart/state library. CSS Modules + CSS vars. All pages `'use client'`.
- **Bridge**: `LaoHR.Bridge.Service` (Windows Service, ZKTeco attendance sync) + `LaoHR.Bridge.Config` (WPF device config) — still reference SQL Server, not migrated to Npgsql.

## 4. Technology Stack

- **Backend**: .NET 10, EF Core 10.0.1 (Npgsql + InMemory), Serilog 9, OpenTelemetry 1.9, HealthChecks.NpgSql 9, FluentValidation 11.3, QuestPDF 2025.12, iText7 7.2.5, ClosedXML 0.105, Swashbuckle 6.4, JWT Bearer, RateLimiting.
- **Frontend**: Next.js 16.1.1, React 19.2.3, TypeScript 5 (strict), Tailwind v4, react-hook-form 7.85, zod 3.25, CSS Modules + CSS vars.
- **DB**: PostgreSQL 16.
- **Tests**: xUnit 2.9.3 + Moq + FluentAssertions + Bogus + coverlet + Mvc.Testing.
- **Infra**: Docker (compose: postgres + api:8080 + web:3000), GitHub Actions CI.

## 5. Repository Structure

```
d:\LaoHRSystem/
├── docker-compose.yml          postgres + api + web
├── .env.example                env var template
├── .github/workflows/ci.yml    CI
├── docs/audit/                 prior strategic audit (superseded for current-state)
├── docs/ai-handoff/            THIS handoff package (32 docs)
├── Backend/
│   ├── LaoHR.API/              ASP.NET Core 10 API (28 controllers, 15 services, 2 jobs, middleware, validators, migrations, Dockerfile)
│   ├── LaoHR.Shared/           Entities.cs (40 entities), LaoHRDbContext, LicenseService, AuditLog
│   ├── LaoHR.Bridge.Service/   Windows Service (ZKTeco)
│   ├── LaoHR.Bridge.Config/    WPF app (ZKTeco config)
│   ├── LaoHR.LicenseGen/       License generator CLI
│   └── LaoHR.Tests/            xUnit tests (HR-core only)
└── frontend/
    ├── package.json            Next 16, React 19, Tailwind 4, rhf, zod
    ├── Dockerfile              3-stage (deps/build/runtime)
    └── src/
        ├── proxy.ts            middleware (redirect commented out)
        ├── app/                App Router: login + (dashboard) group (40+ routes)
        ├── components/         ui/ + layout/ + forms/ + leave/ + providers/
        └── lib/                apiClient, types, i18n, permissions, endpoints/
```

## 6. Major Domains

- **Identity**: AppUser (Admin/HR/Employee), RefreshToken (rotation + replay detection), AuditLog.
- **HR**: Employee, Department, Attendance (geolocation), EmployeeDocument.
- **Payroll**: PayrollPeriod, SalarySlip (NSSF + tax + multi-currency), PayrollAdjustment, ConversionRate, TaxBracket.
- **Leave**: LeaveRequest, LeavePolicy, LeaveBalance (accrual + carry-over).
- **Settings**: CompanySetting, WorkSchedule, Holiday, SystemSetting, Province/District/Village.
- **PM**: Project, ProjectMember, Milestone, ProjectTask, TaskAssignee, TaskComment, ActivityLog, Risk, Issue, IssueComment, Resource.
- **Finance**: ExpenseCategory, Expense, EmployeeLoan, LoanRepayment.
- **Knowledge**: Announcement, AnnouncementRead, KnowledgeCategory, KnowledgeArticle, EntityComment (polymorphic: PROJECT/TASK/ISSUE/EXPENSE/LOAN/RISK/RESOURCE).

## 7. Current Working Features

Auth (login/JWT/refresh/logout/me/rate-limit), Employees CRUD + photo, Attendance (clock in/out + calendar), Leave (request/approve/balance/accrual/carry-over/calendar/export), Payroll (run/NSSF/tax/multi-currency/slips/approve/paid/PDF/Excel/NSSF-form/bank-transfers), Projects/Tasks/Milestones/Risks/Issues/Resources/Comments, Expenses/Loans (+repayments), Announcements/Knowledge, Settings (company/work-schedule/holidays/leave/conversion-rates), License enforcement, Audit (fire-and-forget), Health checks, Serilog, OpenTelemetry, Docker, CI, Pagination, Indexes, Theme (light/dark/system), i18n (en/lo).

## 8. Partially Completed Features

- Employee documents (frontend mock on detail page).
- Employee edit (mock data fallback).
- Dashboard (stats done; activity feed is MOCK).
- Milestones (backend done; no dedicated frontend page).
- Audit logs (backend done; no frontend UI).
- i18n (dictionary exists; some inline Lao strings bypass it).
- Bridge.Service/Config (functional but still SQL Server).
- Project roles (`ProjectMember.Role` stored but not enforced).
- Email (`EmailService` stub if `smtp.example.com`).

## 9. Missing Features

User management UI, Timesheet, Skills, Task dependencies, Tags, Gantt/Timeline, Sprint, Critical path, Project budget/cost/revenue/margin, @Mentions, Notifications (in-app), Real activity feed, Global search/Cmd+K, MFA, OAuth/SSO, Multi-tenancy, API versioning, Frontend tests, E2E tests, Metrics exporter, Error tracking (Sentry), Reverse proxy/TLS, Postgres backups, Charts.

## 10. Broken / Risky Areas

- `EmployeesController` ambiguous `[HttpGet("{id}")]` routes (employee vs department) — HIGH.
- `CompanySettingsController` GET is `[AllowAnonymous]` — MEDIUM.
- Prior production credentials committed (must rotate) — HIGH.
- `PasswordHasher` SHA-256 (weak) — HIGH.
- `EnsureCreated()` fallback (schema drift) — HIGH.
- Frontend mock data (dashboard, employee edit, documents) — MEDIUM.
- No `/403` page (useRequirePermission redirects to non-existent route) — MEDIUM.
- `proxy.ts` middleware redirect commented out — MEDIUM.
- Project roles not enforced — MEDIUM.
- File upload extension-only validation — MEDIUM.
- No `RowVersion` on SalarySlip/LeaveBalance — MEDIUM.
- CI triggers on main/dev but branch is master — LOW.
- No reverse proxy/TLS/backups in Docker — MEDIUM.

## 11. Current Development Frontier

The project just completed a large feature expansion (PM/Finance/Knowledge + Docker + CI + ops hardening) that is **uncommitted** in git. The frontier is **stabilization + production readiness**, not new features. Immediate: commit uncommitted work → fix P0 bugs → add tests for new layers → production hardening.

## 12. Most Recent Development Direction

Phase 6 (ops) + Finance/Knowledge layers were the last work. Uncommitted files: `AnnouncementsController`, `CommentsController`, `EmployeeLoansController`, `ExpensesController`, `KnowledgeArticlesController`, `RetentionService`, `RefreshTokenService`, `Dockerfile`, `docker-compose.yml`, `.github/`, `.env.example`, frontend `finance/` + `knowledge/` routes. Modified: `AuthController`, `Program.cs`, `appsettings.json`, `LaoHRDbContext`, `Entities.cs`, `Sidebar`, `AuthProvider`, `apiClient`, `i18n`, `types`, `endpoints/index`, `next.config.ts`, `projects/[id]/page.tsx`.

## 13. Important Technical Decisions

PostgreSQL (switched from SQL Server); EnsureCreated fallback (migrations are SQL Server); raw PG indexes via `PerformanceIndexes.cs`; JWT HS256 + server-side refresh rotation + replay detection; default-deny FallbackPolicy; fire-and-forget audit; license before auth (402); no UI/chart/state/axios library; custom i18n; react-hook-form + zod; QuestPDF + iText7; polymorphic EntityComment; OpenTelemetry optional OTLP.

## 14. Important Constraints

Lao compliance (NSSF/PIT/LAK/UTC+7); bilingual; license-gated; .NET 10 SDK; Node 20+; PostgreSQL 16; `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` MUST stay; `public.key` file for license; demo users Dev/Testing only; singleton CompanySetting (no multi-tenancy); 3 hard roles.

## 15. Do-Not-Break Contracts

API routes (~150 endpoints); `PaginatedResponse<T>` envelope (`{data,total,page,pageSize}`); JWT model (claims: UserId/Username/Role; Issuer/Audience/Key); refresh token rotation + replay detection; default-deny auth; license enforcement (402, SystemSettings LICENSE_KEY, public.key); EF entity model (40 entities); audit pipeline (interceptor→channel→writer); Npgsql legacy timestamp switch; `PerformanceIndexes` raw SQL; payroll formulas (NSSF + progressive tax); payslip PDF format; bank transfer file formats (BCEL/LDB); i18n dictionary structure; apiClient localStorage keys; CSS var design tokens + `data-theme`; EntityComment entity type strings; Docker compose services + healthchecks; CI env (`Testing` = InMemory + no license mw).

## 16. Database State

PostgreSQL 16. 40 entities in `Entities.cs`. 18 SQL Server migrations (unused at runtime — `Migrate()` falls back to `EnsureCreated()`). `PerformanceIndexes.cs` applies 11 PG indexes on startup. Seed data via `OnModelCreating HasData` (categories, policies, tax brackets, settings, departments, holidays, demo employees) + `DbSeeder.Seed()` (Dev/Testing: demo users + sample projects/tasks). No `RowVersion`. Soft-delete inconsistent. `DbSeeder.SeedAddresses` fails on PG (skipped).

## 17. API State

~150 endpoints across 28 controllers. All major domains done. Auth: default-deny + JWT + refresh + rate-limit. Pagination on list endpoints. Health: `/health/live`, `/health/ready`. No API versioning. No bulk ops. Gaps: CompanySettings GET anonymous; project roles unenforced; only LoginRequestValidator exists.

## 18. Frontend State

Next.js 16 App Router, 40+ routes, all `'use client'`. Design tokens + light/dark/system theme. DataTable + Pagination + Form (rhwf+zod) + Toast + Skeleton + EmptyState. i18n (en/lo). apiClient with auto-refresh. Permission matrix. Gaps: mock data (dashboard/employee edit/documents), no `/403` page, proxy.ts disabled, no tests, no virtualization, no error boundaries, no charts, no mobile drawer.

## 19. Backend State

.NET 10, 28 controllers, 15 services, 2 background jobs, license middleware, audit interceptor + writer, Serilog, OpenTelemetry, health checks, rate limiting. Gaps: ambiguous employee routes, CompanySettings auth, LeaveController 541 LOC, no global exception handler, SHA-256 password hashing, hardcoded UTC+7, no API versioning, only 1 validator.

## 20. Infrastructure State

Docker compose (postgres:16-alpine + api + web) with healthchecks + `depends_on` conditions. Env via `.env.example` (`POSTGRES_PASSWORD`, `JWT_KEY`, `CORS_ALLOWED_ORIGINS`, `NEXT_PUBLIC_API_URL` required). CI: GitHub Actions (backend build+test on postgres, frontend typecheck+i18n+build, docker smoke). No reverse proxy/TLS, no backups, no resource limits, no deploy step, no security scanning.

## 21. Testing State

xUnit + Mvc.Testing + Moq + FluentAssertions + Bogus + coverlet. Integration tests for HR-core controllers only (Auth, Employees, Attendance, Leave, Payroll, Reports, Holidays, Settings, Documents, License, BankTransfer). NO tests for PM/Finance/Knowledge controllers. NO frontend tests. NO E2E. No coverage threshold. CI runs backend tests against postgres service container.

## 22. Security State

Improved from prior audit: secrets removed from appsettings, default-deny auth, rate limiting, refresh rotation, CORS locked, fire-and-forget audit. Remaining: SHA-256 password hashing (HIGH), prior credentials in git history (HIGH, must rotate), CompanySettings GET anonymous (MEDIUM), localStorage tokens (MEDIUM), file upload extension-only (MEDIUM), project roles unenforced (MEDIUM), no MFA, no rate limiting on write endpoints, hardcoded JWT fallback key (MEDIUM).

## 23. Performance State

Lean frontend bundle (no UI/chart/state libs). Pagination on list endpoints. 11 PG indexes via `PerformanceIndexes.cs`. Risks: no list virtualization (10k rows), `PayrollController.ExportPayroll` O(slips×adjustments), no `AsNoTracking` projections, no `RowVersion`, no background export queue, no connection pool sizing, no query plan caching concerns `INFERRED` OK.

## 24. Technical Debt

30 items in `20-TECHNICAL-DEBT.md`. Top: TD-01 (PG migrations), TD-02 (SHA-256 hashing), TD-05 (ambiguous employee routes), TD-15 (proxy.ts disabled), TD-19/20 (no FE/PM tests), TD-22 (project roles unenforced), TD-23/24 (no TLS/backups), TD-28 (hardcoded JWT fallback).

## 25. Priority Backlog

- **P0**: PB-01 fix employee routes, PB-02 PG migrations, PB-03 CompanySettings auth, PB-04 rotate credentials, PB-05 strong password hashing, PB-06 fail-fast JWT key, PB-07 remove mock data.
- **P1**: PB-08 /403 page, PB-09 proxy middleware, PB-10 enforce project roles, PB-11 PM/Finance/Knowledge tests, PB-12 frontend tests, PB-13 reverse proxy+TLS, PB-14 Postgres backups, PB-15 split LeaveController, PB-16 global exception handler, PB-17 file upload validation, PB-18 CI branch align, PB-19 list virtualization, PB-20 notifications.
- **P2**: charts, audit UI, user management UI, task dependencies, Gantt, tags, timesheet, skills, project budget, @mentions, command palette, real activity feed, mobile nav, error boundaries, a11y, RowVersion, export queue, AsNoTracking+DTOs, CVE scan, SMTP config.
- **P3**: MFA, OAuth/SSO, multi-tenancy, API versioning, metrics+dashboards, Sentry, audit partitioning, read replicas, saved views, mobile app.

## 26. Dependency Order

P0 items mostly independent. PB-08→PB-09 (/403 before proxy). PB-02→PB-36/PB-37 (migrations before RowVersion/export queue). PB-24→PB-25/PB-27 (task deps before Gantt/timesheet). PB-20→PB-30/PB-32 (notifications before mentions/activity feed). PB-28→PB-27 (skills before timesheet). PB-12→PB-34 (FE tests before error boundaries). PB-11→PB-10 (tests before role enforcement). PB-13→PB-14 (proxy before backups). See `25-DEPENDENCY-GRAPH.md`.

## 27. Recommended Immediate Next Task

1. **Commit the uncommitted work** (Finance/Knowledge/Docker/CI/Retention/RefreshToken) — it is functional but not in git.
2. **Fix PB-01** (ambiguous employee routes) — correctness bug, unblocks frontend trust.
3. **Fix PB-03 + PB-06** (CompanySettings auth + fail-fast JWT key) — quick security wins.
4. **Fix PB-07** (remove mock data) — UX honesty.
5. **Start PB-11** (PM/Finance/Knowledge controller tests) — regression safety for the new layers.

## 28. Validation Required Before Continuing

- Run `dotnet build Backend/LaoHR.API/LaoHR.API.csproj` — confirm no errors.
- Run `dotnet test Backend/LaoHR.Tests/LaoHR.Tests.csproj` — confirm existing tests pass.
- Run `cd frontend && npx tsc --noEmit` — confirm typecheck.
- Run `cd frontend && npm run lint` — confirm lint.
- Run `node frontend/scripts/check-i18n.mjs` — confirm i18n parity.
- Verify `dotnet run` starts API on :5000 and `npm run dev` starts frontend on :3000.
- Do NOT run migrations or destructive DB operations.

## 29. Important Files to Read First

1. `docs/ai-handoff/31-AI-PROJECT-MEMORY.md` — this memory.
2. `Backend/LaoHR.API/Program.cs` — bootstrap (pipeline, DI, DB init).
3. `Backend/LaoHR.Shared/Entities.cs` — 40 entities (domain model).
4. `Backend/LaoHR.Shared/Data/LaoHRDbContext.cs` — DbSets + seed.
5. `Backend/LaoHR.API/Data/PerformanceIndexes.cs` — PG indexes.
6. `frontend/src/lib/apiClient.ts` — HTTP + token mgmt.
7. `frontend/src/lib/permissions.ts` — RBAC matrix.
8. `frontend/src/lib/i18n.ts` — strings.
9. `frontend/src/lib/types.ts` — TS domain types.
10. `frontend/src/lib/endpoints/index.ts` — API module barrel.
11. `Backend/LaoHR.API/Controllers/AuthController.cs` — auth flow.
12. `Backend/LaoHR.API/Services/RefreshTokenService.cs` — refresh rotation.
13. `Backend/LaoHR.API/Services/PayrollService.cs` — payroll engine.
14. `Backend/LaoHR.API/Middleware/LicenseMiddleware.cs` — license gate.
15. `Backend/LaoHR.API/Data/AuditLogInterceptor.cs` — audit capture.
16. `docker-compose.yml` — infra.
17. `.github/workflows/ci.yml` — CI.
18. `.env.example` — env vars.
19. `docs/ai-handoff/28-DO-NOT-BREAK.md` — preservation rules.
20. `docs/ai-handoff/09-API-INVENTORY.md` — endpoint reference.
21. `docs/ai-handoff/08-DATABASE.md` — schema reference.
22. `docs/ai-handoff/24-PRIORITY-BACKLOG.md` — backlog.
23. `docs/ai-handoff/30-CURRENT-DEVELOPMENT-FRONTIER.md` — frontier.
24. `/memories/repo/laohr-run-setup.md` — local run setup.
25. `Backend/LaoHR.API/appsettings.json` — config structure.

## 30. AI Instructions for the Next Agent

1. **Read `31-AI-PROJECT-MEMORY.md` first**, then this document, then `28-DO-NOT-BREAK.md`.
2. **Do NOT break** any contract in section 15 / `28-DO-NOT-BREAK.md`.
3. **Do NOT remove** `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)`.
4. **Do NOT alter** payroll NSSF/tax formulas, payslip PDF layout, or bank transfer file formats without confirming Lao compliance.
5. **Do NOT change** API response shapes without updating frontend endpoint modules.
6. **Validate before continuing**: run build + typecheck + tests (section 28). Record results. Do NOT hide failures.
7. **Mark claims** as VERIFIED / INFERRED / UNKNOWN. Do not invent APIs, entities, or business rules.
8. **Follow conventions**: backend (Controllers→Services→DbContext, Entities.cs single file, interface-backed services, Scoped DI, PascalCase, Async suffix); frontend (App Router, endpoint modules, CSS Modules + vars, react-hook-form + zod, all pages 'use client', i18n dictionary).
9. **Add tests** for any new controller/feature (xUnit integration + frontend Vitest when available).
10. **Start with the recommended immediate next task** (section 27): commit uncommitted work → fix P0 bugs → add tests → production hardening.
11. **No UI/chart/state/axios library** — keep the lightweight stance. Prefer lightweight SVG charts, `@dnd-kit` for drag-drop, `@tanstack/react-virtual` for virtualization if needed.
12. **Consult** `docs/audit/` for strategic roadmap rationale (but current-state accuracy is in `docs/ai-handoff/`).
13. **Default users** (Dev/Testing only): admin/admin123, hr/hr123, employee/emp123.
14. **License**: copy `public.key` to API output dir; activate via `POST /api/license/activate` with `{"Key":"<license.key content>"}`. Existing license: "Lao HR Demo", ENTERPRISE, expires 2027-01-11, maxEmployees 100.
15. **Local run**: backend `cd Backend/LaoHR.API; dotnet run` → :5000; frontend `cd frontend; npm run dev` → :3000.
16. **Do NOT begin implementing the full backlog** — confirm scope with the user first. This package is for orientation, not authorization to refactor everything.