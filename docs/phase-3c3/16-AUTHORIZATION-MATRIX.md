# 16 — Authorization Matrix

| Area | API | Auth | Scope |
|---|---|---|---|
| Executive | `/api/analytics/executive` | Admin/HR | Org |
| HR | `/api/analytics/hr` | Admin/HR | Org |
| Manager | `/api/analytics/my-team` | Any | Self + direct reports |
| Attendance | `/api/analytics/attendance` | Any | Visible scope |
| Leave | `/api/analytics/leave` | Any | Visible scope |
| Payroll | `/api/analytics/payroll` | Admin/HR | Org (aggregate only) |
| Finance | `/api/analytics/finance` | Any | Visible scope |
| PM | `/api/analytics/pm` | Any | Org |
| Leave list/export | `/api/leave`, `/api/leave/export` | Any | Visible scope |
| Expense list/detail | `/api/expenses`, `/api/expenses/{id}` | Any | Visible scope |
| Loan list/detail | `/api/employeeLoans`, `/{id}` | Any | Visible scope |
| Documents | `/api/documents/employee/{id}` | Any | Visible scope |

## Tests
- `ReadPathIdorTests` (6) — cross-employee read denial.
- `AnalyticsAuthorizationTests` (5) — role-based dashboard visibility.
- `AnalyticsServiceTests` (5) — KPI correctness on known datasets.
