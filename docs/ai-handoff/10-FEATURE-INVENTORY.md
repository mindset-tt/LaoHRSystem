# 10 — Feature Inventory

> Statuses: DONE / MOSTLY_DONE / PARTIAL / SCAFFOLDED / PLACEHOLDER / BROKEN / NOT_STARTED / UNKNOWN.
> `VERIFIED` from controllers + frontend pages.

## A. Identity & Auth

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Login (username/password) | ✅ | ✅ | AppUser | ✅ | DONE |
| JWT access tokens (60min) | ✅ | ✅ | — | ✅ | DONE |
| Refresh token rotation + replay detection | ✅ | ✅ | RefreshToken | ✅ | DONE |
| Logout / logout-all | ✅ | ✅ | ✅ | — | DONE |
| `/me` profile | ✅ | ✅ | — | ✅ | DONE |
| Rate limiting (login) | — | ✅ | — | — | DONE |
| RBAC (Admin/HR/Employee) | ✅ | ✅ (default-deny + Roles) | AppUser.Role | ✅ | DONE |
| User management UI | ❌ | ❌ | — | — | NOT_STARTED |
| Password policy (complexity) | — | ❌ (SHA-256 only) | — | — | PARTIAL |

## B. Employees

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Employee CRUD | ✅ | ✅ | Employee | ✅ | DONE |
| Photo upload | ✅ | ✅ | — | — | DONE |
| Employee documents | ✅ (mock on detail) | ✅ | EmployeeDocument | ✅ | MOSTLY_DONE (mock UI) |
| Departments CRUD | ✅ | ✅ (nested in Employees) | Department | — | MOSTLY_DONE (ambiguous route) |
| Skills | ❌ | ❌ | — | — | NOT_STARTED |
| Reporting line | ❌ | ❌ | — | — | NOT_STARTED |

## C. Attendance

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Clock in/out (geolocation) | ✅ | ✅ | Attendance | ✅ | DONE |
| Manual entry | ✅ | ✅ | ✅ | — | DONE |
| Calendar view | ✅ | ✅ | ✅ | — | DONE |
| Today's attendance | ✅ | ✅ | ✅ | — | DONE |
| List (paged) | ✅ | ✅ | ✅ | ✅ | DONE |
| ZKTeco biometric sync | — | ✅ (Bridge.Service) | ✅ | — | MOSTLY_DONE (Bridge still SqlServer) |
| Overtime tracking | ❌ (via adjustments) | ❌ | — | — | NOT_STARTED |

## D. Leave

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Request + attachment | ✅ | ✅ | LeaveRequest | ✅ | DONE |
| Approve/reject workflow | ✅ | ✅ | ✅ | — | DONE |
| Leave balance + accrual | ✅ | ✅ (LeaveService + jobs) | LeaveBalance | — | DONE |
| Year-end carry-over | — | ✅ (job) | ✅ | — | DONE |
| Half-day leave | ✅ | ✅ | ✅ | — | DONE |
| Leave policies config | ✅ | ✅ | LeavePolicy | — | DONE |
| Calendar view | ✅ | ✅ | ✅ | — | DONE |
| Excel export | ✅ | ✅ | — | — | DONE |
| Admin manual accrual/init | — | ✅ | ✅ | — | DONE |

## E. Payroll

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Payroll periods | ✅ | ✅ | PayrollPeriod | ✅ | DONE |
| Run calculation (NSSF + PIT) | ✅ | ✅ (PayrollService) | SalarySlip | ✅ | DONE |
| Multi-currency (LAK/USD/THB/CNY) | ✅ | ✅ | ✅ | — | DONE |
| Salary slips view | ✅ | ✅ | ✅ | ✅ | DONE |
| Approve / mark paid | ✅ | ✅ | ✅ | — | DONE |
| Payslip PDF | ✅ | ✅ (QuestPDF) | — | — | DONE |
| Excel export | ✅ | ✅ (ClosedXML) | — | — | DONE |
| Dynamic adjustments | ✅ | ✅ | PayrollAdjustment | — | DONE |
| NSSF form + report PDF | ✅ | ✅ (iText + QuestPDF) | — | ✅ | DONE |
| Bank transfer files (BCEL/LDB) | ✅ | ✅ | — | ✅ | DONE |

## F. Projects / PM

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Project CRUD | ✅ | ✅ | Project | — | DONE |
| Project members | ✅ | ✅ | ProjectMember | — | DONE |
| Milestones | — | ✅ | Milestone | — | MOSTLY_DONE (no dedicated FE page) |
| Tasks CRUD | ✅ | ✅ | ProjectTask | — | DONE |
| Task assignees | ✅ | ✅ | TaskAssignee | — | DONE |
| Task comments | ✅ | ✅ | TaskComment | — | DONE |
| Kanban board | ✅ | — (client) | ✅ | — | DONE |
| My tasks | ✅ | ✅ | ✅ | — | DONE |
| Activity log | ✅ | ✅ | ActivityLog | — | DONE |
| Risks | ✅ | ✅ | Risk | — | DONE |
| Issues + comments | ✅ | ✅ | Issue/IssueComment | — | DONE |
| Resource allocation | ✅ | ✅ | Resource | — | DONE |
| Task dependencies (FS/SS/FF/SF) | ❌ | ❌ | — | — | NOT_STARTED |
| Gantt / Timeline | ❌ | — | ✅ (dates) | — | NOT_STARTED |
| Tags/Labels | ❌ | ❌ | — | — | NOT_STARTED |
| Timesheet | ❌ | ❌ | — | — | NOT_STARTED |
| Sprint/Iteration | ❌ | ❌ | — | — | NOT_STARTED |
| Critical path | ❌ | ❌ | — | — | NOT_STARTED |

## G. Finance

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Expense claims + workflow | ✅ | ✅ | Expense | — | DONE |
| Expense categories | ✅ | ✅ | ExpenseCategory (seeded) | — | DONE |
| Employee loans + repayments | ✅ | ✅ | EmployeeLoan/LoanRepayment | — | DONE |
| Loan approve/reject/activate/cancel | ✅ | ✅ | ✅ | — | DONE |
| Project budget | ❌ | ❌ | — | — | NOT_STARTED |
| Actual cost / revenue / margin | ❌ | ❌ | — | — | NOT_STARTED |

## H. Knowledge & Collaboration

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Knowledge articles | ✅ | ✅ | KnowledgeArticle | — | DONE |
| Knowledge categories | ✅ | ✅ | KnowledgeCategory (seeded) | — | DONE |
| Announcements + read-tracking | ✅ | ✅ | Announcement/AnnouncementRead | — | DONE |
| Comments (polymorphic) | ✅ | ✅ | EntityComment | — | DONE |
| @Mentions | ❌ | ❌ | — | — | NOT_STARTED |
| Notifications (in-app/email) | ❌ | EmailService stub | — | — | PARTIAL |
| Activity feed (dashboard) | MOCK | ❌ | ActivityLog (project) | — | PLACEHOLDER (mock data) |
| Global search / Cmd+K | ❌ | ❌ | — | — | NOT_STARTED |

## I. Settings & Admin

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Company settings + address | ✅ | ✅ | CompanySetting | — | DONE |
| Work schedule config | ✅ | ✅ | WorkSchedule | ✅ | DONE |
| Holidays CRUD + seed | ✅ | ✅ | Holiday | ✅ | DONE |
| Leave policies | ✅ | ✅ | LeavePolicy | — | DONE |
| Conversion rates + history | ✅ | ✅ | ConversionRate | — | DONE |
| Audit logs (API) | ❌ (no UI) | ✅ | AuditLog | — | MOSTLY_DONE (no FE page) |
| License activation | — | ✅ | SystemSetting | ✅ | DONE |
| Theme (light/dark/system) | ✅ | — | — | — | DONE |
| Language (en/lo) | ✅ | — | — | — | MOSTLY_DONE (inline strings) |

## J. Platform & Ops

| Feature | UI | Backend | DB | Tests | Status |
|---|---|---|---|---|---|
| Pagination (server-side) | ✅ | ✅ | — | — | DONE |
| Indexes (hot paths) | — | ✅ (PerformanceIndexes) | ✅ | — | DONE |
| Health checks | — | ✅ | — | — | DONE |
| Serilog structured logging | — | ✅ | — | — | DONE |
| OpenTelemetry tracing | — | ✅ | — | — | DONE |
| Rate limiting | — | ✅ | — | — | DONE |
| Fire-and-forget audit | — | ✅ | — | — | DONE |
| Refresh token server-side | ✅ | ✅ | ✅ | — | DONE |
| Docker (api + web + postgres) | — | ✅ | — | — | DONE |
| CI (GitHub Actions) | — | ✅ | — | — | DONE |
| Retention pruning | — | ✅ (disabled by default) | — | — | DONE |
| Secrets in env vars | — | ✅ (appsettings cleaned) | — | — | DONE |
| Frontend tests | ❌ | — | — | — | NOT_STARTED |
| API versioning | — | ❌ | — | — | NOT_STARTED |
| Global exception handler | — | ❌ (unconfirmed) | — | — | UNKNOWN |
| Error tracking (Sentry) | — | ❌ | — | — | NOT_STARTED |