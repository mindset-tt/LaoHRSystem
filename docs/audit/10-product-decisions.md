# 10 — Product Decisions

> Every existing module is placed in exactly one of: **KEEP / IMPROVE / MERGE
> / REMOVE / ADD**. Anything not on this list has not been forgotten — it is
> implied by a parent item.

## KEEP

* `AuthController` (login, refresh, me) — but seed removed.
* `EmployeesController` — but with pagination, projection, and authorization
  defaults.
* `PayrollController` (periods, run, slips, approve, paid, pdf, export).
* `LeaveController` (split, but conceptually kept).
* `AttendanceController` (clock-in/out + manual entry).
* `HolidaysController`, `WorkScheduleController`, `ConversionRatesController`,
  `SettingsController`, `CompanySettingsController`.
* `BankTransferController` (BCEL/LDB).
* `ReportsController` (NSSF + ZIP).
* `DashboardController` — extended, not replaced.
* `AuditLogsController` — UI added.
* `LicenseController`.
* Frontend: `AuthProvider`, `LanguageProvider`, `apiClient`, `datetime`.
* Frontend: `Button`, `Card`, `Input`, `Modal`, `Select`, `Skeleton`,
  `ConfirmationModal`, `MaskedField`, `LanguageSelector`.
* Frontend: `EmployeeForm` (becomes the basis for project forms too).
* `db/Address*` tables (already a useful reference dataset).
* `PayrollAdjustment` (drives project cost allocation later).
* ZKTeco bridge (`LaoHR.Bridge.*`).

## IMPROVE

* `EmployeesController` — pagination, projections, photo upload cleanup.
* `LeaveController` — split into Requests / Balances / Calendar / Policies.
* `DashboardController` — role-aware widgets, time-series.
* `AuditLogInterceptor` — fire-and-forget + retention.
* `LicenseMiddleware` — cache.
* `LeaveScheduledJobsService` — cron-style + execution log.
* `apiClient.ts` — already good; minor: typed errors, retry budgets.
* `Sidebar.tsx` — group, persist collapsed, mobile drawer.
* `/settings` page — make theme/language functional.
* `Settings/*` subpages — better validation + previews.
* `Reports` page — chart-based summary cards.
* `EmployeeForm` — full address picker, dependent list.
* `LeaveRequestForm` — better half-day UX, attachment preview.
* `LeaveCalendar` — extend to month/week/day.
* All frontend pages — consistent `<PageHeader/>`, `<EmptyState/>`,
  `<Toast/>`.

## MERGE

* `SettingsController` (generic key/value) and `CompanySettingsController`
  (singleton) → one `SettingsController` with two endpoints
  (`/api/settings/system`, `/api/settings/company`). Drop duplicate DTO.
* `EmployeesController` + nested `DepartmentsController` → split into two
  controllers in their own files. They are different aggregates.
* `WorkSchedule.IsWorkDay` (instance) + `WorkDayService.GetWorkDayBreakdownAsync`
  → keep both, but have `WorkSchedule.IsWorkDay` call `WorkDayService`.
* `HolidaysController` + `HolidaysController.SeedDefaults` + seeder in
  `DbSeeder` → keep the controller, drop the duplicate seed logic from
  `DbSeeder`. One source for default holidays.
* `Sidebar` permissions + `lib/permissions.ts` + `[Authorize(Roles=...)]` on
  controllers → centralize in `AuthorizationPolicies.cs`.

## REMOVE

* `LaoHR.Shared/Class1.cs` — empty leftover.
* `frontend` mock-data fallbacks (`employees/page.tsx:49-58`,
  `EmployeeForm.tsx` department fallback). They mask errors.
* Hard-coded "Recent Activity" items on the dashboard (`page.tsx:128-143`)
  once real activity feed exists.
* `LeaveController` Excel export (it duplicates `PayrollController`).
* `frontend` `alert()` in `reports/page.tsx`.
* Default passwords from production seeding (move to dev-only seeder).
* Duplicate `EmployeeId = 1` fallback in `AttendanceController.GetCurrentEmployeeId`
  (line 164) — throw, don't silently guess.
* `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` in
  `Program.cs:15` — replace with proper `timestamp with time zone` columns.

## ADD

> All "add" items are documented in `09-roadmap.md`. Highlights below.

* **Aggregates**: `Project`, `Milestone`, `Task`, `TaskLink`, `Tag`,
  `ProjectMember`, `Risk`, `Issue`, `ChangeRequest`, `Skill`,
  `EmployeeSkill`, `TimesheetEntry`, `Budget`, `ActualCost`, `Revenue`,
  `Document`, `WikiPage`, `Comment`, `Mention`, `Notification`,
  `ActivityEvent`, `SavedView`, `ApprovalRequest`, `ApprovalPolicy`,
  `Organization`.
* **API**: pagination envelopes, projection DTOs, `[Authorize]` defaults,
  policy-based authorization, caching, rate limiting, OpenTelemetry,
  background export queue, file storage abstraction, refresh-token
  persistence.
* **Frontend**: design tokens, `<ThemeProvider>`, `<DataTable>`, `<Toast>`,
  `<EmptyState>`, `<PageHeader>`, `<MobileNav>`, `<CommandPalette>`,
  `<Charts>` (Bar/Line/Donut/Sparkline), view engine (List/Board/Calendar/
  Timeline/Gantt over Task), role-aware dashboard widgets, activity feed
  widget, notification center.
* **Cross-cutting**: `/health`, Docker, GitHub Actions, audit log
  retention + archival, saved filters, breadcrumbs, page titles, mobile
  drawer.
* **Tests**: Vitest + RTL for frontend, expand xUnit integration tests,
  add a `Permission` test that proves cross-role enforcement, perf tests
  for the dashboard with 10k employees seeded.

## Why we do NOT clone Jira/Linear/ClickUp

| Capability      | Why we don't copy                        |
| --------------- | ---------------------------------------- |
| Custom workflows| Most teams need 4–6 statuses max         |
| Automation engine| A few server-side rules cover 95% of needs |
| Marketplace     | Out of scope for an internal product    |
| Issue types     | One task + tag taxonomy is enough        |
| Customer portal | Not a CRM                                |
| Goals / OKRs    | Outside the product's mandate            |
| Roadmaps Gantt  | Project list + status badges suffice for v1 |

## Decision Principles

* Lightweight before featureful.
* Scale before scope.
* One model = one source of truth.
* Standard primitives before page-specific markup.
* No silent fallbacks to mock data.
* No silent auth defaults (default-deny).
* Build only after audit confirms the gap.
