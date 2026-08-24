# 07 — Backend Analysis

> `VERIFIED` from `Program.cs` + subagent controller/service exploration.

## Framework

ASP.NET Core 10 (Web API), EF Core 10.0.1 (Npgsql runtime, InMemory tests). Entry point: `Backend/LaoHR.API/Program.cs` (~340 lines).

## Application entry point

`Program.cs` — top-level statements. Configures Serilog, DI, auth, rate limiting, CORS, health checks, OpenTelemetry, Swagger, QuestPDF, middleware pipeline, DB init. See `05-RUNTIME-AND-DATA-FLOW.md` for full startup sequence.

## API architecture

Layered: Controllers → Services (for complex domains) → `LaoHRDbContext` → PostgreSQL. Simple CRUD controllers query `DbContext` directly. Cross-cutting: middleware (license, auth, rate-limit), interceptor (audit), background services (leave accrual, retention, audit writer).

## Controllers (28 files)

| Controller | Route | Auth | LOC | Purpose | Notes |
|---|---|---|---|---|---|
| `AuthController` | `api/auth` | AllowAnonymous(login/refresh), Authorize(me/logout) | 190 | Login, me, refresh, logout, logout-all | Refresh rotation + replay detection |
| `EmployeesController` | `api/employees` | Authorize | 291 | Employee CRUD + photo upload + nested departments | ⚠️ Possible duplicate/ambiguous `{id}` routes for departments |
| `AttendanceController` | `api/attendance` | Authorize | 206 | List, today, clock-in, clock-out, manual | Hard-coded UTC+7 |
| `PayrollController` | `api/payroll` | Authorize(Admin,HR) | 408 | Periods, run, slips, approve, paid, pdf, calculate, export Excel | ClosedXML |
| `PayrollAdjustmentController` | `api/payrolladjustment` | Authorize | 73 | Adjustments CRUD (blocks on locked periods) | |
| `LeaveController` | `api/leave` | Authorize | 541 | Requests, balance, calendar, policies, export, admin accrual/carryover/init | Largest controller; mixed responsibilities |
| `HolidaysController` | `api/holidays` | Authorize | 163 | CRUD + seed-defaults | |
| `DocumentsController` | `api/documents` | Authorize | 92 | Employee docs (files in wwwroot/uploads) | Extension-only validation |
| `ReportsController` | `api/reports` | Authorize | 83 | NSSF PDF + ZIP package | iText + QuestPDF |
| `DashboardController` | `api/dashboard` | Authorize | 50 | Stats (4 scalars) | |
| `SettingsController` | `api/settings` | Authorize | 48 | System settings key-value | |
| `CompanySettingsController` | `api/company-settings` | GET anon, PUT Authorize | 33 | Company settings | ⚠️ GET anonymous |
| `AddressController` | `api/address` | Authorize | 37 | Provinces/districts/villages | Cached |
| `AuditLogsController` | `api/auditlogs` | Authorize(Admin) | 39 | List audit logs (paged) | |
| `BankTransferController` | `api/banktransfer` | Authorize | 44 | BCEL/LDB transfer files | |
| `ConversionRatesController` | `api/settings/conversion-rates` | Authorize | 158 | CRUD + historical + current | |
| `WorkScheduleController` | `api/settings` | Authorize | 116 | Work schedule + workdays/calculate | |
| `LicenseController` | `api/license` | AllowAnonymous | 82 | Status + activate | |
| `ProjectsController` | `api/projects` | Authorize | — | List, detail, create, update, members, activities | Paged |
| `ProjectTasksController` | `api/projects/{projectId}/projecttasks` | Authorize | — | Task CRUD, comments, assignees, my-tasks | |
| `MilestonesController` | `api/projects/{projectId}/milestones` | Authorize | — | Milestone CRUD | |
| `RisksController` | `api/projects/{projectId}/risks` | Authorize | — | Risk CRUD | |
| `IssuesController` | `api/projects/{projectId}/issues` | Authorize | — | Issue CRUD + comments | |
| `ResourcesController` | explicit full routes | Authorize | — | Resource allocation CRUD (project + global) | |
| `CommentsController` | `api/comments` | Authorize | — | Polymorphic comments (PROJECT/TASK/ISSUE/EXPENSE/LOAN/RISK/RESOURCE) | Soft delete |
| `ExpensesController` | `api/expenses` | Authorize | — | Expense CRUD + approve/reject/pay + categories | |
| `EmployeeLoansController` | `api/employeeloans` | Authorize | — | Loan CRUD + approve/reject/activate/cancel + repayments | |
| `AnnouncementsController` | `api/announcements` | Authorize | — | Announcement CRUD + mark-read | |
| `KnowledgeArticlesController` | `api/knowledgearticles` | Authorize | — | Article CRUD + categories (view count) | |

See `09-API-INVENTORY.md` for full endpoint detail.

## Services (15 files)

| Service | Interface | Purpose |
|---|---|---|
| `PayrollService` | (concrete) | Payroll engine: NSSF rates, tax brackets, currency conversion, progressive tax, overtime |
| `PayslipPdfService` | (concrete) | Bilingual payslip PDF (QuestPDF, Phetsarath OT font) |
| `BankTransferService` | `IBankTransferService` | BCEL (text H/D/T records) + LDB (CSV) transfer files |
| `NssfReportService` | (concrete) | NSSF social security report PDF (QuestPDF) |
| `PdfFormService` | (concrete) | LSSO Payment Form PDF fill (iText7) — has `// TODO: adjust positions` |
| `EmailService` | `IEmailService` | SMTP email (mocks/logs if host is `smtp.example.com`) |
| `CompanySettingsService` | `ICompanySettingsService` | Company settings CRUD |
| `AddressService` | `IAddressService` | Province/district/village lookup |
| `LeaveService` | `ILeaveService` | Balance init, monthly accrual, year-end carry-over |
| `WorkDayService` | `IWorkDayService` | Work-day calculations (schedule + holidays) |
| `RefreshTokenService` | `IRefreshTokenService` | Token rotation + revocation + replay detection |
| `LicenseKeyCache` | `ILicenseKeyCache` | In-memory license cache (5min positive, 15s negative) |
| `PasswordHasher` | (static) | SHA-256 hashing ⚠️ (not bcrypt/argon2) |
| `LaoNumberUtils` | (static) | Number → Lao Kip words (for PDF) |
| `SecurityRequirementsOperationFilter` | `IOperationFilter` | Swagger JWT security requirement filter |

## Middleware

- `LicenseMiddleware` — intercepts all requests except `/swagger`, `/api/auth`, `/api/license`, `/health`; verifies license from DB (cached 5min); returns HTTP 402 if invalid. Skipped in Testing. Runs before auth.

## Validators

- `LoginRequestValidator` (FluentValidation) — username (3-50), password (min 3). **Only validator** — other inputs validated inline in controllers.

## Background jobs / workers

| Job | Type | Schedule | Purpose |
|---|---|---|---|
| `LeaveScheduledJobsService` | `BackgroundService` | hourly check; monthly accrual on 1st; year-end carry-over on Jan 1 | Leave accrual + carry-over |
| `RetentionService` | `BackgroundService` | hourly check; daily at configured UTC hour | Prune audit logs (>365d) + revoked refresh tokens (>30d). **Disabled by default** (`Retention:Enabled=false`) |
| `AuditLogWriter` | `BackgroundService` | continuous (channel consumer) | Writes audit logs from `Channel<AuditLog>` in separate scope |

## Validation

FluentValidation registered via `AddValidatorsFromAssemblyContaining<Program>()`, but only `LoginRequestValidator` exists. Most validation is inline in controllers.

## Authentication / authorization

- JWT Bearer (HS256), 60-min tokens (configurable `Jwt:DurationInMinutes`).
- Default-deny: `FallbackPolicy = RequireAuthenticatedUser()`.
- Roles: Admin, HR, Employee (hard-coded strings).
- Rate limiting on login: 5/60s per IP.
- See `11-AUTH-AND-SECURITY.md`.

## Error handling

- Controllers return `IActionResult` with status codes. No global exception handler middleware confirmed. `INFERRED` default ASP.NET Core exception handling.
- `ApiClientError` on frontend.

## Logging

Serilog: JSON console (`RenderedCompactJsonFormatter`), optional rolling file (`Logs/laohr-.json`, 14-day, via `Serilog:WriteToFile`), request logging middleware (enriches Host/Scheme/UserAgent/ClientIP, never logs Authorization).

## Caching

`MemoryCache` registered. Used by `LicenseKeyCache` (5min/15s TTL) and `AddressController` (address lookups).

## Rate limiting

`AddRateLimiter` with `auth-login` policy: sliding window, 5 attempts / 60s per IP, 6 segments, 429 on rejection.

## File processing

- Documents: `wwwroot/uploads` (disk). Extension-only validation (PDF, images, Word).
- PDF: QuestPDF (payslips, NSSF report), iText7 (LSSO form fill).
- Excel: ClosedXML (payroll export).

## External integrations

- ZKTeco biometric devices via `LaoHR.Bridge.Service` (separate Windows Service) + `LaoHR.Bridge.Config` (WPF config app).
- SMTP email via `EmailService`.
- No third-party API integrations (no OAuth providers, no payment gateways, no cloud storage).