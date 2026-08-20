# 11 — Implementation Plan

> Vertical slices. Each slice ships independently behind a flag if needed.

## Slice 0.1 — Schema + Authorization Hardening (Phase 0)

**Goal**: stop the bleeding.

1. Convert `EnsureCreated` to `Migrate`:
   * `Program.cs` → `db.Database.Migrate()`.
   * Move existing seed data from `DbSeeder` into a migration (`HasData`
     equivalent).
   * Delete `DbSeeder.Seed` from runtime (keep method for tests).
2. Add `[Authorize]` on every controller currently lacking it.
3. Move secrets to env vars; document in `appsettings.Development.json`.
4. Add `RateLimiter` on `/api/auth/login`.
5. Add `MemoryCache` for license key (5 min).
6. Add explicit indexes:
   * `Attendance(AttendanceDate)`
   * `SalarySlip(PeriodId)`
   * `LeaveRequest(StartDate, EndDate, Status)`
   * `Employee(IsActive, DepartmentId)`
   * `AuditLog(Timestamp)` + `(EntityName, Timestamp)`
7. Convert `AuditLogInterceptor` to fire-and-forget:
   * Collect audit entries on save, write them in a `BackgroundService` from
     a `Channel<AuditLog>`.
8. Lock CORS to `process.env.CORS_ALLOWED_ORIGINS` split.
9. Disable default password seeding outside `IsDevelopment`.

**Acceptance**
* `dotnet ef migrations script` produces a clean script.
* New indexes present via `\d+` in psql.
* `GET /api/employees` requires JWT.
* Login returns 429 after 5 failed attempts in 60 s.

## Slice 0.2 — Pagination + Projections (Phase 0)

**Goal**: stop list endpoints from being unbounded.

1. Add `PaginatedQuery` + `PaginatedResponse<T>` (already typed in frontend).
2. Update endpoints:
   * `EmployeesController.GetEmployees` → paged
   * `AttendanceController.GetAttendance` → cursor
   * `LeaveController.GetLeaveRequests` → paged
   * `PayrollController.GetSlips` → paged
   * `HolidaysController.GetHolidays` → paged by year
   * `AuditLogsController.GetLogs` → cursor
3. Introduce DTO projections (`EmployeeDto`, `LeaveRequestDto`) to avoid
   returning nav graphs.

**Acceptance**
* 10,000 employees load in < 1 s with paging=50.
* Network payload < 30 kB per page.

## Slice 1.1 — Design Tokens (Phase 1)

1. `globals.css` → `:root { --color-…, --space-…, --radius-… }`.
2. Add Tailwind v4 `@theme { … }` mirror so utilities work.
3. Replace raw colors in `Card`, `Button`, `Input`, `Modal`, `Sidebar`.
4. Add `ThemeProvider` (`light` / `dark` / `system`).
5. Wire `/settings` selects.

**Acceptance**
* Switching theme changes every page.
* `prefers-color-scheme` honored.

## Slice 1.2 — Primitives (Phase 1)

1. Build `<DataTable>` (sort, column visibility, sticky header, pagination,
   row virtualization).
2. Build `<Toast>` + `useToast()`.
3. Build `<EmptyState/>`, `<ErrorState/>`, `<PageHeader/>`, `<Breadcrumbs/>`,
   `<MobileNav/>`.
4. Replace per-page tables with `<DataTable>`.
5. Replace `alert()` calls with `useToast()`.

## Slice 1.3 — Forms + i18n (Phase 1)

1. Add `react-hook-form` + `zod`.
2. Build `<FormField/>` wrapper.
3. Move all hand-rolled `errors` states to Zod.
4. Catalog all hard-coded strings → i18n keys.
5. Use `Intl.NumberFormat('lo-LA', { style: 'currency', currency: 'LAK' })`
   in formatting helpers.

## Slice 2.1 — Project / Milestone (Phase 2)

1. Migration: `projects`, `milestones`, `project_members`, `tags`.
2. `ProjectsController`, `MilestonesController`, `ProjectMembersController`,
  `TagsController`.
3. `[Authorize]` policies: `project.read`, `project.create`, `project.update`,
  `project.delete`.
4. Frontend `/projects` portfolio + filters.
5. Frontend `/projects/[id]` overview + team + milestones.
6. `<PageHeader/>` + breadcrumbs everywhere.

## Slice 2.2 — Task (Phase 2)

1. Migration: `tasks`, `task_links`, `task_assignees`, `task_watchers`.
2. `TasksController` + `TaskLinksController`.
3. Frontend `<TaskViewEngine/>` (List/Board/Calendar/Timeline/Gantt) over a
   shared `useTaskQuery`.
4. Frontend `Cmd+K` palette + global search.
5. Notifications: in-app + email.

## Slice 2.3 — Activity + Comments (Phase 2)

1. `activity_events` table.
2. Outbox pattern: every task write appends an `activity_event`.
3. Background worker fans out notifications.
4. Comments on tasks / projects.
5. `@mentions` (parse body for `@username`, link to mention).

## Slice 3 — Risk / Issue / Resource (Phase 3)

1. Migration: `risks`, `issues`, `skills`, `employee_skills`,
   `timesheet_entries`.
2. Controllers.
3. Frontend risk matrix (SVG, ~50 LOC).
4. Capacity heatmap (SVG, week × employee).
5. Timesheet grid (week grid + day total).

## Slice 4 — Finance Extension (Phase 4)

1. Migration: `budgets`, `actual_costs`, `revenues`.
2. `BudgetsController`, `ActualCostsController`, `RevenuesController`.
3. `/projects/[id]/budget` view.
4. Portfolio dashboard widgets.

## Slice 5 — Knowledge + Collaboration (Phase 5)

1. Migration: `documents`, `wiki_pages`, `comments`, `mentions`.
2. Controllers.
3. Markdown editor (light: textarea + preview, ~200 LOC; defer rich editor).
4. Document upload + preview.

## Slice 6 — Operations (Phase 6)

1. Serilog + JSON sink.
2. OpenTelemetry (HTTP + EF).
3. `Dockerfile.api`, `compose.yml`, GitHub Actions.
4. Audit log retention: daily job moves rows > 90 d to cold table.
5. Refresh tokens persisted; revocation list.

## Slice 7 — Advanced (Phase 7, on demand)

* Saved views (personal + shared)
* Project templates
* Approval policy UI
* Calendar integration (Google/Outlook)
* Slack/Teams deep-link generator
* Webhooks

## Cross-cutting Test Plan

| Layer    | Tool                        | Coverage target |
| -------- | --------------------------- | --------------- |
| Domain   | xUnit                       | payroll, leave accrual, critical rules |
| API      | xUnit + WebApplicationFactory | every endpoint happy + sad paths |
| Perms    | xUnit matrix                | per role × endpoint |
| Perf     | k6 / Bombardier             | dashboard + list with 10k rows |
| Frontend | Vitest + RTL                | `lib/`, providers, primitives |
| E2E      | Playwright                  | login → clock-in → submit timesheet → see on dashboard |
| Visual   | Playwright snapshots        | light + dark tokens |

## Risk Register for the Plan

| Risk                                              | Mitigation                                        |
| ------------------------------------------------- | ------------------------------------------------- |
| Migration drift                                   | Run `dotnet ef migrations script` in CI           |
| Frontend bundle creep                             | `@next/bundle-analyzer` in CI                     |
| Background work overload                          | Backpressure via `Channel` + max parallelism      |
| Permissions regression                            | Permission matrix test in CI                      |
| DateTime confusion                                | All `DateTime` UTC; convert on display in `lib/datetime.ts`; forbid `DateTime.Now` outside the boundary |
| Hard-coded test data in frontend                  | Strip after each slice; add Vitest assertion that catches regressions |
