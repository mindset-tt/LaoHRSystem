# 12 — Page Inventory

> Every frontend route, mapped to its current behavior, data, and known
> problems.

## Login

| Route | Purpose | User | Data | Problems | Action |
| ----- | ------- | ---- | ---- | -------- | ------ |
| `/login` | Sign in | Anonymous | POST `/api/auth/login` | No "forgot password", no language selector on the page, no show-password toggle | Add forgot link + language chip + show-password |

## Dashboard

| Route | Purpose | User | Data | Problems | Action |
| ----- | ------- | ---- | ---- | -------- | ------ |
| `/` | Stats + quick actions + activity | All | GET `/api/dashboard/stats` | Activity is mock data; one-size-fits-all view | Replace mock with `ActivityEvent`; add role-aware widgets; time-series mini charts |

## Employees

| Route | Purpose | User | Data | Problems | Action |
| ----- | ------- | ---- | ---- | -------- | ------ |
| `/employees` | List + search + filter | HR/Admin | GET `/api/employees` + `/api/departments` | No paging; falls back to mock on error; no column visibility; no sort | Add `<DataTable>`; remove mock fallback; add URL-state filters |
| `/employees/new` | Create | HR/Admin | GET `/api/departments` | Departments also fall back to mock on error; no address picker integration | Wire real address picker; remove mock fallback |
| `/employees/[id]` | Detail (tabs) | HR/Admin (self for Employee) | GET `/api/employees/:id` | Documents tab never fetches `/api/documents/employee/:id`; no edit-in-place | Wire documents; add edit-in-place for personal info |
| `/employees/[id]/edit` | Edit | HR/Admin | same as above | Same mock fallback | Same |

## Attendance

| Route | Purpose | User | Data | Problems | Action |
| ----- | ------- | ---- | ---- | -------- | ------ |
| `/attendance` | Calendar/list + clock-in/out | All | GET `/api/attendance`, `/api/attendance/today` | Native geolocation requires HTTPS; no filter by employee for HR; timezone hard-coded | Add HR filter; centralize TZ helper; warn when not on HTTPS/localhost |

## Leave

| Route | Purpose | User | Data | Problems | Action |
| ----- | ------- | ---- | ---- | -------- | ------ |
| `/leave` | Tabs: my / approvals / calendar | All | GET `/api/leave`, `/api/leave/balance`, `/api/leave/calendar` | Hard-coded English relative time; `alert()` on export error; no attachment preview | Use `formatRelativeTime`; replace `alert`; add attachment preview |

## Payroll

| Route | Purpose | User | Data | Problems | Action |
| ----- | ------- | ---- | ---- | -------- | ------ |
| `/payroll` | Periods + slips + exports | HR/Admin | GET `/api/payroll/periods`, `/api/payroll/periods/{id}/slips`, GET `/api/payroll/periods/{id}/export` | Slip detail missing; no diff vs previous period; large exports block | Add slip detail page; add period diff; stream large exports |

## Reports

| Route | Purpose | User | Data | Problems | Action |
| ----- | ------- | ---- | ---- | -------- | ------ |
| `/reports` | NSSF download | HR/Admin | GET `/api/reports/nssf/{id}`, GET `/api/reports/nssf/zip/{id}` | Only NSSF; no project reports; uses `alert()` on failure | Add project status report; add NSSF history; replace `alert` with toast |

## Settings

| Route | Purpose | User | Data | Problems | Action |
| ----- | ------- | ---- | ---- | -------- | ------ |
| `/settings` | Profile + (fake) theme/lang | All | — | Theme/language disabled | Wire to `ThemeProvider`/`LanguageProvider`; save to user pref |
| `/settings/company` | Company info + address | HR/Admin | GET/PUT `/api/company-settings`, `/api/address/*` | Address chain not validated client-side | Add validation; preview address |
| `/settings/work-schedule` | Work days + hours + Saturday | HR/Admin | GET/PUT `/api/settings/work-schedule` | No Saturday preview | Add preview |
| `/settings/holidays` | Holiday CRUD | HR/Admin | GET/PUT `/api/holidays` | No recurring-vs-fixed year view | Add year view |
| `/settings/leave` | Leave policy CRUD | HR/Admin | GET/PUT `/api/leave/policies` | No quota projection | Add projection per employee |
| `/settings/currency-rates` | Rate CRUD | HR/Admin | GET/PUT `/api/settings/conversion-rates` | No rate history chart | Add sparkline |

## Planned (Phase 2+)

| Route | Purpose | User | Notes |
| ----- | ------- | ---- | ----- |
| `/my` | Personal home | All | Widgets: today's tasks, timesheet, leave balance, approvals, notifications |
| `/my/tasks` | Tasks assigned to me | All | Across projects |
| `/my/timesheet` | Week grid | All | Submit for approval |
| `/my/leave` | Leave balance + request | All | Replaces `/leave` "my-leave" tab |
| `/my/attendance` | Personal attendance + clock | All | Replaces `/attendance` for employees |
| `/my/approvals` | Approvals inbox | HR/PM/Admin | Replaces `/leave` "approvals" tab |
| `/my/notifications` | Notification history | All | |
| `/projects` | Portfolio | PM/HR/Admin | Filters + saved views |
| `/projects/new` | Create project | PM/HR/Admin | |
| `/projects/[id]` | Workspace overview | PM/HR/Admin (member read) | Tabs |
| `/projects/[id]/list` | Task list | as above | `<DataTable>` |
| `/projects/[id]/board` | Kanban | as above | `@dnd-kit` |
| `/projects/[id]/calendar` | Calendar | as above | |
| `/projects/[id]/timeline` | Timeline | as above | |
| `/projects/[id]/gantt` | Gantt | as above | Lightweight CSS grid |
| `/projects/[id]/milestones` | Milestones | as above | |
| `/projects/[id]/team` | Members | as above | |
| `/projects/[id]/risks` | Risk register | as above | |
| `/projects/[id]/issues` | Issues | as above | |
| `/projects/[id]/budget` | Budget vs actual | as above | |
| `/projects/[id]/wiki` | Project wiki | as above | |
| `/projects/[id]/documents` | Project docs | as above | |
| `/projects/[id]/activity` | Activity feed | as above | |
| `/projects/[id]/settings` | Project settings | PM/Admin | |
| `/people/skills` | Skill matrix | HR/PM | |
| `/people/capacity` | Capacity heatmap | HR/PM | |
| `/people/utilization` | Utilization trends | HR/PM | |
| `/time/overtime` | Overtime register | HR | |
| `/finance/budgets` | Cross-project budgets | HR/Finance | |
| `/finance/nssf` | NSSF history | HR/Finance | Replaces `/reports` for finance |
| `/knowledge/documents` | All docs | All | |
| `/knowledge/wiki` | Wiki landing | All | |
| `/knowledge/wiki/[id]` | Wiki page | All | |
| `/admin/users` | User management | Admin | |
| `/admin/roles` | Roles | Admin | |
| `/admin/permissions` | Permissions | Admin | |
| `/admin/workflows` | Approval policies | Admin | |
| `/admin/audit` | Audit viewer | Admin | |
| `/admin/integrations` | Future | Admin | placeholder |
