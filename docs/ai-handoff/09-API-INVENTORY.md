# 09 — API Inventory

> `VERIFIED` from subagent controller exploration. Auth column: `[Authorize]` = any authenticated; `[Authorize(Roles)]` = role-restricted; `[AllowAnonymous]` = public.

## Auth

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| POST | `/api/auth/login` | AllowAnonymous (rate-limited) | Login → JWT + refresh | COMPLETE |
| GET | `/api/auth/me` | Authorize | Current user info | COMPLETE |
| POST | `/api/auth/refresh` | AllowAnonymous | Rotate refresh token | COMPLETE |
| POST | `/api/auth/logout` | Authorize | Revoke single refresh token | COMPLETE |
| POST | `/api/auth/logout-all` | Authorize | Revoke all refresh tokens | COMPLETE |

## Employees

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/employees` | Authorize | List (paged: isActive, departmentId, search) | COMPLETE |
| GET | `/api/employees/{id}` | Authorize | Get by ID (with Department) | COMPLETE |
| POST | `/api/employees` | Authorize | Create | COMPLETE |
| PUT | `/api/employees/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/employees/{id}` | Authorize | Delete | COMPLETE |
| POST | `/api/employees/{id}/photo` | Authorize | Upload photo | COMPLETE |
| GET | `/api/employees/{id}` (depts) | Authorize | ⚠️ Departments (possible ambiguous route) | PARTIAL |
| POST | `/api/employees` (dept) | Authorize | Create department | COMPLETE |

## Attendance

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/attendance` | Authorize | List (paged: date, startDate, endDate, employeeId) | COMPLETE |
| GET | `/api/attendance/today` | Authorize | Today's attendance (UTC+7) | COMPLETE |
| POST | `/api/attendance/clock-in` | Authorize | Clock in (geolocation) | COMPLETE |
| POST | `/api/attendance/clock-out` | Authorize | Clock out (geolocation) | COMPLETE |
| POST | `/api/attendance` | Authorize | Manual entry | COMPLETE |

## Payroll

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/payroll/periods` | Admin,HR | List periods | COMPLETE |
| POST | `/api/payroll/periods` | Admin,HR | Create period | COMPLETE |
| POST | `/api/payroll/periods/{periodId}/run` | Admin,HR | Run payroll | COMPLETE |
| GET | `/api/payroll/periods/{periodId}/slips` | Admin,HR | List slips (paged) | COMPLETE |
| GET | `/api/payroll/slips/{slipId}` | Admin,HR | Get slip | COMPLETE |
| POST | `/api/payroll/slips/{slipId}/approve` | Admin,HR | Approve slip | COMPLETE |
| POST | `/api/payroll/slips/{slipId}/paid` | Admin,HR | Mark paid | COMPLETE |
| GET | `/api/payroll/slips/{slipId}/pdf` | Admin,HR | Download payslip PDF | COMPLETE |
| POST | `/api/payroll/calculate` | Admin,HR | Calculate preview | COMPLETE |
| GET | `/api/payroll/periods/{periodId}/export` | Admin,HR | Export Excel | COMPLETE |

## Payroll Adjustments

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/payrolladjustment` | Authorize | List (periodId, employeeId) | COMPLETE |
| POST | `/api/payrolladjustment` | Authorize | Create (blocks locked periods) | COMPLETE |
| DELETE | `/api/payrolladjustment/{id}` | Authorize | Delete (blocks locked) | COMPLETE |

## Leave

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/leave` | Authorize | List requests (paged: status, employeeId, year, month) | COMPLETE |
| GET | `/api/leave/balance` | Authorize | Balance (employeeId?, year) | COMPLETE |
| GET | `/api/leave/calendar` | Authorize | Calendar data (year/month) | COMPLETE |
| POST | `/api/leave` | Authorize | Create request (multipart + attachment) | COMPLETE |
| POST | `/api/leave/{id}/approve` | Authorize | Approve | COMPLETE |
| POST | `/api/leave/{id}/reject` | Authorize | Reject | COMPLETE |
| GET | `/api/leave/policies` | Authorize | List policies | COMPLETE |
| PUT | `/api/leave/policies/{id}` | Authorize | Update policy | COMPLETE |
| GET | `/api/leave/export` | Authorize | Export Excel | COMPLETE |
| POST | `/api/leave/admin/run-accrual` | Authorize | Manual accrual | COMPLETE |
| POST | `/api/leave/admin/run-carryover` | Authorize | Manual carry-over | COMPLETE |
| POST | `/api/leave/admin/initialize-balances` | Authorize | Init year balances | COMPLETE |

## Holidays

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/holidays` | Authorize | List (paged: year, recurring) | COMPLETE |
| GET | `/api/holidays/{id}` | Authorize | Get | COMPLETE |
| POST | `/api/holidays` | Authorize | Create | COMPLETE |
| PUT | `/api/holidays/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/holidays/{id}` | Authorize | Delete | COMPLETE |
| POST | `/api/holidays/seed-defaults` | Authorize | Seed Lao holidays | COMPLETE |

## Documents

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/documents/employee/{employeeId}` | Authorize | List employee docs | COMPLETE |
| POST | `/api/documents` | Authorize | Upload (multipart) | COMPLETE |
| DELETE | `/api/documents/{id}` | Authorize | Delete (file + DB) | COMPLETE |

## Reports

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/reports/nssf/{periodId}` | Authorize | NSSF form PDF | COMPLETE |
| GET | `/api/reports/nssf/zip/{periodId}` | Authorize | NSSF ZIP (form + report) | COMPLETE |

## Dashboard

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/dashboard/stats` | Authorize | 4 scalar counts | COMPLETE (not extensible) |

## Settings / Company / Address / Conversion / WorkSchedule

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/settings` | Authorize | System settings dict | COMPLETE |
| PUT | `/api/settings/{key}` | Authorize | Upsert setting | COMPLETE |
| GET | `/api/company-settings` | **Anonymous** ⚠️ | Company settings | COMPLETE (auth gap) |
| PUT | `/api/company-settings` | Authorize | Update | COMPLETE |
| GET | `/api/address/provinces` | Authorize | Provinces | COMPLETE |
| GET | `/api/address/districts/{provinceId}` | Authorize | Districts | COMPLETE |
| GET | `/api/address/villages/{districtId}` | Authorize | Villages | COMPLETE |
| GET | `/api/settings/conversion-rates` | Authorize | List rates | COMPLETE |
| GET | `/api/settings/conversion-rates/current` | Authorize | Current rates | COMPLETE |
| GET | `/api/settings/conversion-rates/{from}/to/{to}` | Authorize | Pair rate | COMPLETE |
| POST | `/api/settings/conversion-rates` | Authorize | Create (expires old) | COMPLETE |
| PUT | `/api/settings/conversion-rates/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/settings/conversion-rates/{id}` | Authorize | Delete | COMPLETE |
| GET | `/api/settings/work-schedule` | Authorize | Work schedule | COMPLETE |
| PUT | `/api/settings/work-schedule` | Authorize | Update | COMPLETE |
| GET | `/api/settings/workdays/calculate` | Authorize | Calc work days | COMPLETE |

## Audit / BankTransfer / License

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/auditlogs` | Admin | List (paged: entity) | COMPLETE |
| GET | `/api/banktransfer/bcel/{periodId}` | Authorize | BCEL file | COMPLETE |
| GET | `/api/banktransfer/ldb/{periodId}` | Authorize | LDB file | COMPLETE |
| GET | `/api/license/status` | AllowAnonymous | License status | COMPLETE |
| POST | `/api/license/activate` | AllowAnonymous | Activate key | COMPLETE |

## Projects

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/projects` | Authorize | List (paged: status, priority, search, mineOnly) | COMPLETE |
| GET | `/api/projects/{id}` | Authorize | Detail (members, stats) | COMPLETE |
| POST | `/api/projects` | Authorize | Create | COMPLETE |
| PUT | `/api/projects/{id}` | Authorize | Update | COMPLETE |
| POST | `/api/projects/{id}/members` | Authorize | Add member | COMPLETE |
| DELETE | `/api/projects/{id}/members/{employeeId}` | Authorize | Remove member | COMPLETE |
| GET | `/api/projects/{id}/activities` | Authorize | Activity log (paged) | COMPLETE |

## Project Tasks

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/projects/{projectId}/projecttasks` | Authorize | List (paged: status, priority, milestoneId, assignedToMe, search) | COMPLETE |
| GET | `/api/projects/{projectId}/projecttasks/{taskId}` | Authorize | Detail | COMPLETE |
| POST | `/api/projects/{projectId}/projecttasks` | Authorize | Create | COMPLETE |
| PUT | `/api/projects/{projectId}/projecttasks/{taskId}` | Authorize | Update | COMPLETE |
| POST | `/api/projects/{projectId}/projecttasks/{taskId}/comments` | Authorize | Add comment | COMPLETE |
| PUT | `/api/projects/{projectId}/projecttasks/{taskId}/assignees` | Authorize | Set assignees | COMPLETE |
| GET | `/api/my-tasks` | Authorize | My tasks across projects (paged) | COMPLETE |

## Milestones

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/projects/{projectId}/milestones` | Authorize | List (with task counts) | COMPLETE |
| POST | `/api/projects/{projectId}/milestones` | Authorize | Create | COMPLETE |
| GET/PUT/DELETE | `/api/projects/{projectId}/milestones/{id}` | Authorize | CRUD | COMPLETE |

## Risks

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/projects/{projectId}/risks` | Authorize | List (paged) | COMPLETE |
| GET | `/api/projects/{projectId}/risks/{id}` | Authorize | Detail | COMPLETE |
| POST | `/api/projects/{projectId}/risks` | Authorize | Create | COMPLETE |
| PUT | `/api/projects/{projectId}/risks/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/projects/{projectId}/risks/{id}` | Authorize | Delete | COMPLETE |

## Issues

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/projects/{projectId}/issues` | Authorize | List (paged) | COMPLETE |
| GET | `/api/projects/{projectId}/issues/{id}` | Authorize | Detail (with comments) | COMPLETE |
| POST | `/api/projects/{projectId}/issues` | Authorize | Create | COMPLETE |
| PUT | `/api/projects/{projectId}/issues/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/projects/{projectId}/issues/{id}` | Authorize | Delete | COMPLETE |
| POST | `/api/projects/{projectId}/issues/{id}/comments` | Authorize | Add comment | COMPLETE |

## Resources

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/resources` | Authorize | All resources (projectId, employeeId, role, activeOn) | COMPLETE |
| GET | `/api/projects/{projectId}/resources` | Authorize | Project resources | COMPLETE |
| GET | `/api/projects/{projectId}/resources/{id}` | Authorize | Detail | COMPLETE |
| POST | `/api/projects/{projectId}/resources` | Authorize | Create allocation | COMPLETE |
| PUT | `/api/projects/{projectId}/resources/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/projects/{projectId}/resources/{id}` | Authorize | Delete | COMPLETE |

## Comments (polymorphic)

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/comments/{entityType}/{entityId}` | Authorize | Thread (PROJECT/TASK/ISSUE/EXPENSE/LOAN/RISK/RESOURCE) | COMPLETE |
| GET | `/api/comments/{entityType}/{entityId}/summary` | Authorize | Count summary | COMPLETE |
| POST | `/api/comments` | Authorize | Add (one-level threading) | COMPLETE |
| PUT | `/api/comments/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/comments/{id}` | Authorize | Soft delete | COMPLETE |

## Expenses

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/expenses/categories` | Authorize | Categories | COMPLETE |
| GET | `/api/expenses` | Authorize | List (status, employeeId, categoryId, search) | COMPLETE |
| GET | `/api/expenses/{id}` | Authorize | Detail | COMPLETE |
| POST | `/api/expenses` | Authorize | Create claim | COMPLETE |
| PUT | `/api/expenses/{id}` | Authorize | Update | COMPLETE |
| POST | `/api/expenses/{id}/approve` | Authorize | Approve | COMPLETE |
| POST | `/api/expenses/{id}/reject` | Authorize | Reject | COMPLETE |
| POST | `/api/expenses/{id}/pay` | Authorize | Mark paid | COMPLETE |
| DELETE | `/api/expenses/{id}` | Authorize | Delete | COMPLETE |

## Employee Loans

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/employeeloans` | Authorize | List (status, employeeId, search) | COMPLETE |
| GET | `/api/employeeloans/{id}` | Authorize | Detail (with repayments) | COMPLETE |
| POST | `/api/employeeloans` | Authorize | Create | COMPLETE |
| PUT | `/api/employeeloans/{id}` | Authorize | Update | COMPLETE |
| POST | `/api/employeeloans/{id}/approve` | Authorize | Approve | COMPLETE |
| POST | `/api/employeeloans/{id}/reject` | Authorize | Reject | COMPLETE |
| POST | `/api/employeeloans/{id}/activate` | Authorize | Activate (installment plan) | COMPLETE |
| POST | `/api/employeeloans/{id}/cancel` | Authorize | Cancel | COMPLETE |
| GET | `/api/employeeloans/{id}/repayments` | Authorize | List repayments | COMPLETE |
| POST | `/api/employeeloans/{id}/repayments` | Authorize | Record repayment | COMPLETE |
| DELETE | `/api/employeeloans/{id}` | Authorize | Delete | COMPLETE |

## Announcements

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/announcements` | Authorize | List (severity, audience, search, unreadOnly) | COMPLETE |
| GET | `/api/announcements/{id}` | Authorize | Detail (with IsRead) | COMPLETE |
| POST | `/api/announcements` | Authorize | Create | COMPLETE |
| PUT | `/api/announcements/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/announcements/{id}` | Authorize | Delete | COMPLETE |
| POST | `/api/announcements/{id}/read` | Authorize | Mark read | COMPLETE |

## Knowledge Articles

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/api/knowledgearticles/categories` | Authorize | Categories (with counts) | COMPLETE |
| GET | `/api/knowledgearticles` | Authorize | List (categoryId, status, search) | COMPLETE |
| GET | `/api/knowledgearticles/{id}` | Authorize | Detail (increments views) | COMPLETE |
| POST | `/api/knowledgearticles` | Authorize | Create | COMPLETE |
| PUT | `/api/knowledgearticles/{id}` | Authorize | Update | COMPLETE |
| DELETE | `/api/knowledgearticles/{id}` | Authorize | Delete | COMPLETE |

## Health

| Method | Endpoint | Auth | Purpose | Status |
|---|---|---|---|---|
| GET | `/health/live` | AllowAnonymous | Liveness | COMPLETE |
| GET | `/health/ready` | AllowAnonymous | Readiness (postgres) | COMPLETE |

## API state summary

~150 endpoints across 28 controllers. All major domains implemented end-to-end. Auth gaps: `CompanySettingsController` GET is anonymous (should be `[Authorize]`). Most controllers use `[Authorize]` (default-deny fallback covers the rest). No API versioning. No bulk operations.