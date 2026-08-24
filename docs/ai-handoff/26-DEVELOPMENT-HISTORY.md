# 26 — Development History

> `VERIFIED` from git status (branch `master`, extensive uncommitted new work), migration dates, file structure, audit doc phases.
> Git CLI not available on PATH (`git` not recognized), so full commit history could not be retrieved. History is inferred from migration dates + file structure + audit phase references.

## Inferred timeline

### Phase 1 — HR core (2026-01-10 to 2026-01-19)
- 18 EF migrations created (SQL Server-flavored) covering: initial schema, holidays, employee dependents, salary currency, exchange rates, audit logs, ZKTeco toggle, system settings, company structure, leave enhancement, work schedule, Saturday work config, conversion rates, salary slip currency, bonus, payroll adjustments, AppUser.
- HR domain fully built: employees, attendance, payroll (NSSF + tax + multi-currency), leave, holidays, reports, documents, settings.

### Phase 2 — Auth + infrastructure hardening
- `AppUser` + JWT auth added (migration 20260119020017).
- Audit log interceptor added.
- License system built (`LaoHR.LicenseGen`, `LicenseService`, `LicenseMiddleware`).

### Phase 3 — PostgreSQL migration
- Switched from SQL Server to Npgsql (per repo memory).
- `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)`.
- `EnsureCreated()` used (SQL Server migrations incompatible with PG).
- `PerformanceIndexes.cs` created to apply PG indexes via raw SQL.

### Phase 4 — Frontend build
- Next.js 16 + React 19 + Tailwind v4 frontend built.
- All HR pages + login + dashboard + settings.
- Custom design system (CSS vars + CSS Modules), no UI library.
- i18n (en/lo), apiClient with token refresh, permission matrix.

### Phase 5 — PM/PL + Finance + Knowledge layers (current uncommitted work)
From git status, the following are **uncommitted** (new `??` files):
- `AnnouncementsController`, `CommentsController`, `EmployeeLoansController`, `ExpensesController`, `KnowledgeArticlesController` — new controllers.
- `RetentionService` — new background job.
- `RefreshTokenService` — server-side refresh tokens.
- `Dockerfile` (api + web), `docker-compose.yml`, `.dockerignore` — Docker.
- `.github/workflows/` — CI.
- `.env.example` — env template.
- Frontend `finance/`, `knowledge/` route groups — new pages.
- Modified: `AuthController`, `Program.cs`, `appsettings.json`, `LaoHRDbContext`, `Entities.cs`, `Sidebar`, `AuthProvider`, `apiClient`, `i18n`, `types`, `endpoints/index`, `next.config.ts`, `projects/[id]/page.tsx`.

Also already tracked (Phase 2 PM entities in `Entities.cs`): Project, ProjectMember, Milestone, ProjectTask, TaskAssignee, TaskComment, ActivityLog, Risk, Issue, IssueComment, Resource. Plus ProjectsController, ProjectTasksController, MilestonesController, RisksController, IssuesController, ResourcesController (committed earlier).

### Phase 6 — Ops hardening (per docker-compose/CI comments)
- Phase 6e: docker-compose (postgres + api + web).
- Phase 6f: CI (GitHub Actions).
- Serilog, OpenTelemetry, health checks, rate limiting, fire-and-forget audit, retention service.

## Recent development direction

The most recent work (uncommitted) adds:
1. **Finance layer** (expenses, loans) — backend + frontend.
2. **Knowledge layer** (articles, announcements, comments) — backend + frontend.
3. **Polymorphic comments** (`CommentsController`, `EntityComment`).
4. **Retention service** (audit + refresh token pruning).
5. **Docker + CI** infrastructure.
6. **Refresh token server-side** (rotation + replay detection).

## Abandoned / legacy

- SQL Server migrations (18 files) — superseded by Npgsql + EnsureCreated.
- `DbSeeder.SeedAddresses` (SQL Server SQL) — fails on PG, gracefully skipped.
- Bridge.Service / Bridge.Config — still SQL Server; possibly abandoned or pending migration.
- `docs/audit/` — prior strategic audit, partially superseded by current implementation.

## Likely current priorities

Based on the frontier (see `30-CURRENT-DEVELOPMENT-FRONTIER.md`):
- Committing/stabilizing the uncommitted Finance/Knowledge/Docker/CI work.
- Hardening the new layers (tests, role enforcement, mock data removal).
- Production readiness (TLS, backups, credential rotation).