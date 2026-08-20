# 01 — Current System Map

> Audit performed on `D:/LaoHRSystem` (master branch, latest commit `6e22539`).
> All findings are evidence-based, citing file paths and line numbers.

## 1. Product Reality Check

LaoHR is **not yet a work-management platform**. Despite the master prompt's framing
(Jira / Linear / ClickUp / MS Project comparisons), the codebase today is an
**HR + Payroll + Attendance system** with a thin, partially-implemented operations
shell around it.

**There is no `Project`, `Task`, `Milestone`, `Risk`, `Issue`, `Timesheet`, `Sprint`,
`Backlog`, `Epic`, or `Wiki` model in the database.** The schema is exclusively HR
record-keeping:

| Domain      | Entities present                                                       | Entities missing (vs Jira/Linear/PPM)                |
| ----------- | ---------------------------------------------------------------------- | ---------------------------------------------------- |
| Identity    | `AppUser`                                                              | Role/permission rows, scopes, orgs                   |
| Org chart   | `Department`                                                           | Position, reporting line, employee-grade             |
| People      | `Employee`, `EmployeeDocument`                                         | Skills, assignments, contracts as first-class        |
| Time        | `Attendance`                                                           | Timesheet, overtime, time-off vs attendance          |
| Leave       | `LeaveRequest`, `LeavePolicy`, `LeaveBalance`                          | Holiday calendar integrated; nothing else missing here |
| Money       | `PayrollPeriod`, `SalarySlip`, `PayrollAdjustment`, `ConversionRate`   | Budget, cost, revenue, project linkage, profitability |
| Settings    | `SystemSetting`, `CompanySetting`, `WorkSchedule`, `Holiday`, `TaxBracket` | Workflow policies, approval policies               |
| Address     | `Province`, `District`, `Village`                                      | —                                                    |
| Compliance  | `AuditLog`                                                             | Fine-grained RBAC tables, retention policy           |
| Operations  | **NONE**                                                               | Project, Task, Milestone, Risk, Issue, Comment, Activity |

This means the entire product pivot implied by the prompt — *"turn this into a
lightweight, fast, scalable work-management platform"* — is **a new product
layer on top of an existing HR core**, not a refactor.

## 2. Solution Architecture (current)

```
D:/LaoHRSystem/
├── README.md
├── Backend/
│   ├── LaoHR.API/                  ASP.NET Core 10 web API
│   │   ├── Controllers/            17 controllers, ~3,600 LOC
│   │   ├── Services/               13 services, ~2,900 LOC
│   │   ├── Data/                   AuditLogInterceptor, DbSeeder
│   │   ├── Jobs/                   LeaveScheduledJobsService (BackgroundService)
│   │   ├── Middleware/             LicenseMiddleware
│   │   ├── Migrations/             18 EF migrations
│   │   ├── Validators/             LoginRequestValidator
│   │   ├── Program.cs              bootstrap
│   │   ├── appsettings.json
│   │   └── LaoHR.API.csproj
│   ├── LaoHR.Shared/               Cross-project library
│   │   ├── AuditLog.cs
│   │   ├── Entities.cs             22 entities, ~860 LOC
│   │   ├── Class1.cs               (empty placeholder)
│   │   ├── Data/LaoHRDbContext.cs  EF Core context + seed
│   │   ├── Models/LicenseData.cs
│   │   ├── Models/PayrollAdjustment.cs
│   │   └── Services/LicenseService.cs
│   ├── LaoHR.Bridge.Config/        WPF desktop app for ZKTeco fingerprint device config
│   ├── LaoHR.Bridge.Service/       Windows service that pushes attendance from ZKTeco
│   ├── LaoHR.LicenseGen/           Offline license-key generator CLI
│   └── LaoHR.Tests/                xUnit integration + helper tests
└── frontend/
    ├── src/app/
    │   ├── login/
    │   └── (dashboard)/            group route — sidebar layout
    │       ├── layout.tsx
    │       ├── page.tsx            dashboard
    │       ├── employees/          list, [id] detail/edit, new
    │       ├── attendance/
    │       ├── leave/
    │       ├── payroll/
    │       ├── reports/
    │       └── settings/           page + 5 subpages
    ├── src/components/
    │   ├── ui/                     Card, Button, Input, Select, Modal, Skeleton,
    │   │                           MaskedField, ConfirmationModal, LanguageSelector
    │   ├── layout/                 Sidebar, Header
    │   ├── forms/                  EmployeeForm, LeaveRequestForm, NewPeriodModal,
    │   │                           AdjustmentModal
    │   ├── leave/LeaveCalendar.tsx
    │   └── providers/              AuthProvider, LanguageProvider
    ├── src/lib/
    │   ├── apiClient.ts            token mgmt + refresh + retry
    │   ├── datetime.ts             Asia/Vientiane helpers
    │   ├── i18n.ts                 en/lo dictionary (one file)
    │   ├── permissions.ts          client RBAC matrix
    │   ├── types.ts                TS types mirroring Entities.cs
    │   └── endpoints/              9 endpoint modules
    ├── src/proxy.ts                Next.js middleware (proxy to backend)
    ├── next.config.ts              turbopack + react-compiler
    ├── package.json                next 16, react 19, tailwindcss 4 — *no UI lib*
    └── tsconfig.json
```

## 3. Tech Stack Snapshot

| Layer    | Technology                                                |
| -------- | --------------------------------------------------------- |
| API      | .NET 10, ASP.NET Core, EF Core (Npgsql + InMemory)        |
| DB       | PostgreSQL (prod), InMemory (tests)                        |
| Auth     | JWT bearer (`HS256`, 8-hour tokens, no refresh table)      |
| Frontend | Next.js 16 (App Router, Turbopack, react-compiler), React 19, Tailwind v4 CSS modules |
| i18n     | Hand-rolled `en` / `lo` dictionary in `lib/i18n.ts`       |
| Charts   | **NONE** in dependencies                                  |
| State    | React Context (Auth, Language) — no Redux/Zustand          |
| Tests    | xUnit + `Microsoft.AspNetCore.Mvc.Testing`                 |
| Hardware | ZKTeco fingerprint devices via `LaoHR.Bridge.*` desktop apps |
| License  | Custom LicenseService + key stored in `SystemSettings`     |

The frontend is deliberately UI-library-free (only Next + React + Tailwind). That is
a good foundation for **lightweight** but means every component is hand-rolled —
including icons, tables, and forms.

## 4. Backend Inventory

### 4.1 Controllers (`Backend/LaoHR.API/Controllers/`)

| Controller                  | LOC   | Endpoints (excerpt)                                           | Notes                                                          |
| --------------------------- | ----: | ------------------------------------------------------------- | -------------------------------------------------------------- |
| `AddressController`         |    37 | GET provinces, districts, villages                            | Cached reads, no auth attribute                                |
| `AttendanceController`      |   206 | GET list, today, POST clock-in/-out, manual entry             | Hard-coded Lao TZ = UTC+7 inside handlers                      |
| `AuditLogsController`       |    39 | GET logs (limit, entity)                                      | Admin-only; no pagination metadata                             |
| `AuthController`            |   190 | POST login, refresh, GET me                                   | Seeds admin/hr demo users on every login call                  |
| `BankTransferController`    |    44 | GET BCEL/LDB txt files                                        | No auth                                                        |
| `CompanySettingsController` |    33 | GET, PUT settings                                             | No role guard                                                  |
| `ConversionRatesController` |   158 | CRUD + historical                                             | Good use of effective/expiry dates                             |
| `DashboardController`       |    50 | GET stats                                                     | 4 scalar counts; not extensible                                |
| `DocumentsController`       |    92 | GET/POST/DELETE employee documents                            | Files on disk under `wwwroot/uploads`                          |
| `EmployeesController`       |   291 | CRUD + photo upload; **+ nested `DepartmentsController`**     | Two controllers in one file                                    |
| `HolidaysController`        |   163 | CRUD + seed-defaults                                          | Soft delete via `IsActive`                                     |
| `LeaveController`           |   541 | requests, balance, calendar, policies, export, attachments   | Largest controller; mixed responsibilities                     |
| `LicenseController`         |    82 | status, activate                                              | Stores key in SystemSettings                                   |
| `PayrollAdjustmentController` |   73 | GET/POST/DELETE                                               | Blocks edits on LOCKED periods                                 |
| `PayrollController`         |   408 | periods, run, slips, approve, paid, pdf, calculate, export Excel | ClosedXML; complex dynamic column logic                   |
| `ReportsController`         |    83 | NSSF report, ZIP package                                      | Uses PdfFormService + NssfReportService                         |
| `SettingsController`        |    48 | GET/PUT system settings                                       | Key-value bag                                                  |
| `WorkScheduleController`    |   116 | GET/PUT schedule, workdays/calculate                          | Singleton pattern                                              |

**Observations**

* No `[Authorize]` on `AddressController`, `BankTransferController`,
  `CompanySettingsController`, `LicenseController`, `ReportsController`,
  `HolidaysController` `POST/PUT/DELETE`. This is a **security gap**.
* `AuthController.SeedDefaultAdminAsync` is called on **every login attempt**
  (`AuthController.cs:33`). It does an `AnyAsync` check, but it still queries
  the DB and creates a scope on each call.
* `LeaveController.cs` is 541 lines and bundles 4 logical endpoints
  (requests, balance, calendar, attachments). It should be split.
* `DashboardController` returns 4 hard-coded scalars. No time-series, no
  filters, no role-aware data.

### 4.2 Services (`Backend/LaoHR.API/Services/`)

| Service                  | LOC  | Purpose                                                  |
| ------------------------ | ---: | -------------------------------------------------------- |
| `AddressService`         |   45 | Province/District/Village lookups                        |
| `BankTransferService`    |   93 | BCEL/LDB txt file generation                             |
| `CompanySettingsService` |   60 | Get/Update singleton                                     |
| `EmailService`           |   66 | SMTP client (placeholders)                               |
| `LaoNumberUtils`         |   67 | Lao-script number formatting                             |
| `LeaveService`           |  186 | Yearly init, monthly accrual, year-end carry-over        |
| `NssfReportService`      |  190 | NSSF monthly report PDF                                  |
| `PasswordHasher`         |   19 | PBKDF2/SHA wrapper (19 lines — verify contents)          |
| `PayrollService`         |  522 | Salary calc, NSSF + tax brackets, conversion rates      |
| `PayslipPdfService`      |  374 | iText 7/QuestPDF payslip                                 |
| `PdfFormService`         |  148 | Fill PDF form for NSSF                                   |
| `SecurityRequirementsOperationFilter` | 39 | Swagger JWT plumbing                  |
| `WorkDayService`         |  242 | Working-day calculations, holidays                       |

### 4.3 Background work

`LeaveScheduledJobsService` (BackgroundService) polls every hour to run
monthly accrual (1st of month) and year-end carry-over (Jan 1). Polling is a
heavyweight pattern — a cron-style `IScheduler` (Quartz/Hangfire) or simply
re-checking on-demand would be cheaper. The job creates its own scope per run
which is correct, but it never persists execution history.

### 4.4 Middleware

* `LicenseMiddleware` runs on every request (bypassing only `/swagger`,
  `/api/auth`, `/api/license`). It synchronously does a `FindAsync("LICENSE_KEY")`
  on every request — should be cached for at least 30 s.

### 4.5 Migrations

18 migrations. No destructive operations visible, but the migrations folder
contains SQL-Server-flavored EF migrations while `Program.cs` configures
**Npgsql** with `EnsureCreated` (the migrations are never applied — see Risks).
This is the most serious latent bug in the codebase.

## 5. Data Model — Full Entity Inventory

(All entities live in `Backend/LaoHR.Shared/Entities.cs`.)

| Entity              | Key field                 | Notable fields                                                | Indexes                              |
| ------------------- | ------------------------- | ------------------------------------------------------------- | ------------------------------------ |
| `Department`        | `DepartmentId`            | bilingual name, code, IsActive                                | —                                    |
| `Employee`          | `EmployeeId`              | bilingual name, NSSF/Tax IDs, SalaryCurrency, ProfilePath     | `EmployeeCode` unique                |
| `Attendance`        | `AttendanceId`            | ClockIn/Out, geolocation, method, work hours                  | `(EmployeeId, AttendanceDate)` unique|
| `PayrollPeriod`     | `PeriodId`                | Year, Month, Status                                           | `(Year, Month)` unique               |
| `SalarySlip`        | `SlipId`                  | base/OT/allowances/bonus, NSSF, tax, net, dual-currency       | —                                    |
| `LeaveRequest`      | `LeaveId`                 | Type, dates, half-day, attachment path, status                | —                                    |
| `LeavePolicy`       | `LeavePolicyId`           | Quota, carry-over, accrual/mo, attachment rule                | `LeaveType` unique                   |
| `LeaveBalance`      | `LeaveBalanceId`          | Per employee/type/year                                        | `(EmployeeId, LeaveType, Year)` unique |
| `TaxBracket`        | `BracketId`               | Min/MaxIncome, TaxRate, SortOrder                             | —                                    |
| `SystemSetting`     | `SettingKey`              | key-value bag for rates & flags                               | PK on key                            |
| `Holiday`           | `HolidayId`               | Date, Name (En/Lao), IsRecurring                              | `Date` unique                        |
| `EmployeeDocument`  | `DocumentId`              | Type, FileName, FilePath                                      | —                                    |
| `Province`          | `PrId`                    | Bilingual                                                     | —                                    |
| `District`          | `DiId`                    | Bilingual, FK Province                                        | —                                    |
| `Village`           | `VillId`                  | Bilingual, FK District                                        | —                                    |
| `CompanySetting`    | `Id`                      | Address FKs, banking, contact                                 | —                                    |
| `WorkSchedule`      | `WorkScheduleId`          | Day toggles, Saturday config, late threshold                  | —                                    |
| `ConversionRate`    | `ConversionRateId`        | From/To, EffectiveDate, ExpiryDate, Rate                      | —                                    |
| `PayrollAdjustment` | `AdjustmentId`            | Employee/Period, Type (EARNING/DEDUCTION/BONUS), IsTaxable    | —                                    |
| `AppUser`           | `UserId`                  | Username, PasswordHash, Role, EmployeeId (FK optional)        | —                                    |
| `AuditLog`          | `Id` (Guid)               | UserId, EntityName, Action, KeyValues JSON, Old/New JSON      | —                                    |

### 5.1 What's *not* there but the prompt assumes

* **Project / Task / Milestone / Dependency / Risk / Issue / Comment / Activity**
* **Skill / Position / Grade / Reporting line**
* **Timesheet** (actual hours per day per task per employee)
* **Budget / Cost / Revenue / Margin**
* **Sprint / Backlog / Epic**
* **Approval workflow** beyond manual `ApprovedById` columns on `LeaveRequest`
* **Notification** storage
* **Tag / Label**
* **Saved view**
* **Organization / Tenant boundary** (multi-company is implicit via singleton
  `CompanySetting`, not via a row)

## 6. Frontend Inventory

### 6.1 Routes

| Route                                | Page file                            | Purpose                                  | Primary data                          |
| ------------------------------------ | ------------------------------------ | ---------------------------------------- | ------------------------------------- |
| `/login`                             | `app/login/page.tsx`                 | Login form                               | —                                     |
| `/`                                  | `app/(dashboard)/page.tsx`           | Stats overview                           | `/api/dashboard/stats`                |
| `/employees`                         | `app/(dashboard)/employees/page.tsx` | List + search + filter                   | `/api/employees`, `/api/departments`  |
| `/employees/new`                     | `…/employees/new/page.tsx`           | Create employee                          | `/api/departments`                    |
| `/employees/[id]`                    | `…/employees/[id]/page.tsx`          | Detail (tabs: personal/employment/docs)  | `/api/employees/:id`                  |
| `/employees/[id]/edit`               | `…/employees/[id]/edit/page.tsx`     | Edit employee                            | `/api/employees/:id`                  |
| `/attendance`                        | `…/attendance/page.tsx`              | Calendar + list, clock-in/out            | `/api/attendance`, `/api/attendance/today` |
| `/leave`                             | `…/leave/page.tsx`                   | Tabs: my-leave, approvals, calendar      | `/api/leave`, `/api/leave/balance`, `/api/leave/calendar` |
| `/payroll`                           | `…/payroll/page.tsx`                 | Periods, slips, adjustments, exports     | `/api/payroll/periods`, `/api/payroll/periods/{id}/slips`, `/api/PayrollAdjustment` |
| `/reports`                           | `…/reports/page.tsx`                 | NSSF report + ZIP package download       | `/api/payroll/periods`, `/api/reports/nssf/zip/{id}` |
| `/settings`                          | `…/settings/page.tsx`                | Profile + theme/language (disabled)      | —                                     |
| `/settings/company`                  | `…/settings/company/page.tsx`        | Company info, banking, address           | `/api/company-settings`, `/api/address/*` |
| `/settings/work-schedule`            | `…/settings/work-schedule/page.tsx`  | Work days + hours + Saturday config      | `/api/settings/work-schedule`, `/api/settings/workdays/calculate` |
| `/settings/holidays`                 | `…/settings/holidays/page.tsx`       | Holiday CRUD                             | `/api/holidays`                       |
| `/settings/leave`                    | `…/settings/leave/page.tsx`          | Leave policy CRUD                        | `/api/leave/policies`, `/api/leave/types` |
| `/settings/currency-rates`           | `…/settings/currency-rates/page.tsx` | Conversion rate CRUD                     | `/api/settings/conversion-rates`      |

### 6.2 Components

`components/ui/`:
* `Button`, `Card`, `Input`, `Modal`, `Select`, `ConfirmationModal`, `Skeleton`,
  `MaskedField`, `LanguageSelector` — all hand-rolled, all CSS-Module-based.

`components/layout/`:
* `Sidebar` (collapsible, permission-filtered), `Header`.

`components/forms/`:
* `EmployeeForm` (~280 LOC, reused for new + edit),
  `LeaveRequestForm`, `NewPeriodModal`, `AdjustmentModal`.

`components/leave/`:
* `LeaveCalendar` (single component used by leave page).

`components/providers/`:
* `AuthProvider` (token mgmt + 401 retry + refresh),
  `LanguageProvider`.

### 6.3 Endpoints (`src/lib/endpoints/`)

9 modules: `adjustments.ts`, `attendance.ts`, `auth.ts`, `company.ts`,
`conversionRates.ts`, `employees.ts`, `leave.ts`, `payroll.ts`, `reports.ts`,
`schedule.ts`. Each is a thin wrapper around `apiClient`.

### 6.4 i18n

`lib/i18n.ts` is a single dictionary with `en` and `lo` keys. Some pages bypass
the dictionary and hard-code Lao strings inline (`Sidebar.tsx:65-83`,
`SettingsPage` mostly).

### 6.5 Theming

`globals.css` defines light-mode variables. **Dark mode is not implemented**.
The settings page exposes a `<select>` for theme that is hard-coded `disabled`.

## 7. Strengths

* Clean, single-tenant HR core with bilingual support, correct NSSF/PIT
  calculation, and good test coverage of the payroll engine.
* Sensible auth design: short-lived JWT + cookie-backed refresh + 401 retry
  (`apiClient.ts:84-108`).
* Hand-rolled UI primitives are small and tree-shakeable — perfectly consistent
  with the lightweight mandate.
* Proper EF Core interceptor (`AuditLogInterceptor`) for auto-audit.
* Background leave scheduler decoupled from request path.
* Time-zone handling is centralized in `lib/datetime.ts` (Asia/Vientiane).

## 8. Weaknesses (top 20)

1. **Schema is HR-only.** No Project/Task/Milestone/Risk/Issue/Comment exists,
   so the "work-management platform" goal is a net-new product layer.
2. **Migrations vs `EnsureCreated` mismatch.** `Program.cs:161` calls
   `db.Database.EnsureCreated()` against Npgsql even though migrations folder
   targets SQL Server. New schema changes will not be tracked and the model
   snapshot will drift.
3. **Authorization gaps.** `Address`, `BankTransfer`, `CompanySettings`,
   `License`, `Reports`, and most of `Holidays` controllers lack
   `[Authorize]`.
4. **No pagination anywhere.** `GetEmployees`, `GetAttendance` (Take(100)),
   `GetLeaveRequests`, `GetSlips` return unbounded lists — will fail at scale.
5. **`AuthController.SeedDefaultAdminAsync` runs on every login attempt.**
6. **No dark mode** despite the UI exposing a (disabled) theme selector.
7. **No tests for the new Project/Task code paths** because they don't exist.
8. **`AuditLogInterceptor` writes JSON blobs on every change**, including
   low-value updates (e.g., `UpdatedAt` ticks). At scale this becomes the
   largest table.
9. **`LicenseMiddleware` does a DB hit per request** instead of caching.
10. **Hard-coded timezone `UTC+7` literal** appears in `AttendanceController`
    twice instead of using `TimeZoneInfo.Utc` + `LAO_TIMEZONE`.
11. **i18n is half-done.** Many UI strings hard-coded, no plural forms, no
    currency/number formatting.
12. **Dashboard returns 4 hard-coded scalars.** No time-series, no filtering,
    no role-aware view.
13. **`Department` is referenced inconsistently** — sometimes by id, sometimes
    by nav. `EmployeesController` has its own nested `DepartmentsController`
    class.
14. **`LeaveController` is 541 LOC** — should be split into requests, balances,
    calendar, attachments.
15. **`PayrollController.ExportPayroll`** uses ClosedXML to dynamically
    assemble columns — works, but the loop runs `O(employees × adjustments)`
    server-side with no streaming.
16. **No command palette / global search**, despite being the obvious answer
    to navigation growth.
17. **No Kanban / Board / Gantt / Timeline views** — no `Task` model.
18. **`appsettings.json` contains production DB credentials** committed to the
    repo.
19. **No CORS lock-down.** CORS allows any origin with credentials.
20. **No telemetry / health checks** beyond default `/health`.

## 9. Duplicated Capabilities

* **Two payroll calculators** — `PayrollController.Calculate` uses legacy
  fields, while `PayrollService` reads modern settings.
* **Two controllers in one file** (`EmployeesController` +
  `DepartmentsController` in `EmployeesController.cs`).
* **Two settings surfaces** — `SettingsController` (key/value) and
  `CompanySettingsController` (singleton row). The former is generic but
  used for things like NSSF rates, the latter is for company info.
* **Two ways to compute work days** — `WorkSchedule.IsWorkDay` (instance)
  and `WorkDayService.GetWorkDayBreakdownAsync` (service).
* **Two timezone definitions** — `Asia/Vientiane` string in `lib/datetime.ts`
  and a `CreateCustomTimeZone("LaoTime", …)` literal in `AttendanceController`.

## 10. Placeholder / Stubbed Code

* `EmailService.SendAsync` body is a TODO (`Services/EmailService.cs`).
* `LaoHR.Shared/Class1.cs` is an empty class — leftover from the project
  template.
* `frontend/src/lib/permissions.ts:46` — Employee role has `payroll.export`
  which is not enforced by the backend.
* Dashboard "Recent Activity" cards (`page.tsx:128-143`) are mock data, not
  fetched from any API.
* Settings page (`settings/page.tsx:117-137`) has disabled selects for theme
  and language.
* `AuditLogInterceptor:90-92` adds audit entries to the same `SaveChanges`
  transaction — if the audit write fails, the user's change is rolled back.
  This is a hidden coupling.

## 11. Operational Reality

* PostgreSQL connection string is committed: `appsettings.json:10`.
* The Bridge apps are **WPF / Windows Service** — they assume Windows-only
  ZKTeco hardware. Fine for on-premise, hostile for cloud/SaaS.
* `LaoHR.LicenseGen` is a CLI tool for offline license issuance. Acceptable
  for on-prem distribution.
* The product has no Dockerfile, no `docker-compose.yml`, no CI workflow
  file — deployment is manual.

## 12. Map → Prompt Categories

| Prompt category                | Status                                                    |
| ------------------------------ | --------------------------------------------------------- |
| Organization / People          | **Partial** (`Employee`, `Department`)                    |
| Projects / Planning / Tasks    | **Missing** (no schema, no UI)                            |
| Finance / Cost / Revenue       | **Cost side only** (payroll)                              |
| HR / Attendance / Leave        | **Complete**                                              |
| Knowledge / Documents          | **Partial** (employee docs only; no wiki)                 |
| Administration / RBAC          | **Partial** (3 hard-coded roles; no scoping)              |
| Multi-tenancy                  | **Not designed for** (singleton `CompanySetting`)         |
| Audit                          | **Implemented but no retention policy**                   |
| Notifications                  | **Missing**                                               |
| Comments / Activity feed       | **Missing**                                               |
| Search / Command palette       | **Missing**                                               |
| Dashboard per role             | **Single dashboard** for all                              |
