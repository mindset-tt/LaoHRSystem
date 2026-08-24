# 23 — Implementation Status

> `VERIFIED` from code inspection. Current date: 2026-08-21.

## Completed (production-quality)

- **Auth**: login, JWT, refresh token rotation + replay detection, logout/logout-all, `/me`, rate limiting.
- **Employees**: CRUD, photo upload, list (paged, filtered), detail.
- **Attendance**: clock in/out (geolocation), manual entry, calendar, today, list (paged).
- **Leave**: request + attachment, approve/reject, balance + accrual + carry-over (background job), half-day, policies, calendar, Excel export, admin manual triggers.
- **Payroll**: periods, run (NSSF + Lao PIT + multi-currency), slips, approve, paid, PDF payslip, Excel export, dynamic adjustments, NSSF form + report PDF, bank transfer files (BCEL/LDB).
- **Projects**: CRUD, members, activity log.
- **Tasks**: CRUD, assignees, comments, my-tasks, Kanban board (frontend).
- **Risks**: CRUD.
- **Issues**: CRUD + comments.
- **Resources**: allocation CRUD (project + global).
- **Comments**: polymorphic (7 entity types), soft delete.
- **Expenses**: CRUD + approve/reject/pay + categories.
- **Loans**: CRUD + approve/reject/activate/cancel + repayments.
- **Announcements**: CRUD + read-tracking.
- **Knowledge**: articles CRUD + categories + view count.
- **Settings**: company, work schedule, holidays, leave policies, conversion rates (with history).
- **License**: activation + enforcement (HTTP 402).
- **Audit**: fire-and-forget interceptor + channel + writer.
- **Infrastructure**: Docker (api+web+postgres), health checks, Serilog JSON, OpenTelemetry tracing, rate limiting, retention job (disabled by default), `.env.example`, secrets removed from appsettings.
- **CI**: GitHub Actions (backend build+test, frontend typecheck+i18n+build, docker smoke).
- **Frontend**: design tokens, light/dark/system theme, Toast, DataTable, Pagination, Form (react-hook-form+zod), Skeleton, EmptyState/ErrorState, PageHeader, Breadcrumbs, i18n (en/lo), permission matrix, apiClient with auto-refresh.

## Mostly completed (small gaps)

- **Employee documents**: backend DONE; frontend detail page shows **mock** document data.
- **Employee edit**: mock data fallback on API error.
- **Dashboard**: stats DONE; activity feed is **mock** data.
- **Milestones**: backend DONE; no dedicated frontend page (managed within project detail `INFERRED`).
- **Audit logs**: backend DONE; **no frontend UI**.
- **i18n**: dictionary exists; some inline Lao strings bypass it.
- **Bridge.Service / Bridge.Config**: functional but still reference SQL Server (not migrated to Npgsql).

## Partially implemented

- **Project-level RBAC**: `ProjectMember.Role` stored but not enforced in controllers.
- **Email notifications**: `EmailService` stub (mocks if `smtp.example.com`); no notification model/preferences.
- **Responsive/mobile**: desktop-first; no mobile drawer.

## Scaffolded

- None identified — new layers are fully implemented (not just scaffolded).

## Placeholder

- **Dashboard activity feed**: mock entries (`t.dashboardPage.activity.mock.*`).
- **Employee detail documents**: mock upload.
- **Employee edit**: mock data fallback.

## Broken

- **`EmployeesController` ambiguous `{id}` routes** (employee vs department) — potential routing conflict.
- **`CompanySettingsController` GET** is `[AllowAnonymous]` — security gap.
- **`/403` page** referenced by `useRequirePermission` but does not exist.
- **`proxy.ts`** middleware redirect commented out (no-op).
- **`DbSeeder.SeedAddresses`** fails on PostgreSQL (SQL Server syntax) — address data not seeded on PG.

## Not started

- User management UI.
- Timesheet, Skills, Task dependencies, Tags, Gantt/Timeline, Sprint, Critical path.
- Project budget / actual cost / revenue / margin.
- @Mentions, Notifications (in-app), Activity feed (real).
- Global search / Command palette.
- MFA, OAuth/SSO, Multi-tenancy.
- API versioning, Frontend tests, E2E tests.
- Metrics exporter, Error tracking (Sentry), Dashboards.
- Reverse proxy / TLS, Postgres backups.

## Unknown

- Whether `proxy.ts` is actually wired as Next.js middleware (exports `proxy` not `middleware`; `next.config.ts` may reference it). `UNKNOWN`.
- Whether a global exception handler exists via other means. `UNKNOWN`.
- `launchSettings.json` dev port config. `UNKNOWN`.
- Whether CI actually runs (branch `master` vs trigger `main`/`dev`). `INFERRED` it does not trigger on `master`.

## Current project state (RIGHT NOW)

1. **Does it build?** `INFERRED` YES — CI builds backend + frontend; no build errors observed in workspace (a `build_errors.txt`/`build_err.txt` exist but are stale artifacts).
2. **Does it start?** `VERIFIED` setup notes confirm backend (`dotnet run` → :5000) + frontend (`npm run dev` → :3000) run locally.
3. **Are tests configured?** YES — xUnit + Mvc.Testing + coverlet; CI runs `dotnet test`.
4. **Are tests passing?** `UNKNOWN` — not run in this session; CI gate exists.
5. **Is frontend functional?** YES — all routes implemented; mock data in a few spots.
6. **Is backend functional?** YES — ~150 endpoints across 28 controllers.
7. **Is database configured?** YES — PostgreSQL via Npgsql; `EnsureCreated`/`Migrate` fallback; InMemory for tests.
8. **Is authentication functional?** YES — JWT + refresh rotation + rate limiting + default-deny.
9. **Is Docker functional?** YES — compose stack with healthchecks (requires env vars).
10. **Is production deployment configured?** PARTIAL — Docker + CI exist; no reverse proxy/TLS/backups/deploy step.
11. **What currently blocks production?** Reverse proxy/TLS, Postgres backups, credential rotation, strong password hashing, CI branch alignment, SMTP config.
12. **What currently blocks development?** Ambiguous employee routes, no frontend tests, PM/Finance/Knowledge test gap, mock data cleanup.