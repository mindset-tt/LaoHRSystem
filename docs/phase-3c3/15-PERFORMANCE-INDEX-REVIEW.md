# 15 — Performance / Index Review

## Existing indexes (relevant to analytics)
- `Employee`: `EmployeeCode` (unique), `ManagerId`, `PositionId`, `WorkLocationId`.
- `Attendance`: `(EmployeeId, AttendanceDate)` unique.
- `LeaveRequest`: (via `LeavePolicy` unique; no explicit employee/date index).
- `SalarySlip`: no explicit index (FKs only).
- `Expense`: `ExpenseNumber` unique, `(EmployeeId, Status)`, `(Status, ExpenseDate)`.
- `EmployeeLoan`: `LoanNumber` unique, `(EmployeeId, Status)`.
- `Project`: `Code` unique.
- `ProjectTask`: `(ProjectId, Status)`.
- `Risk`: `(ProjectId, Status)`, `(ProjectId, Priority)`.
- `Issue`: `(ProjectId, Status)`, `(ProjectId, AssigneeId)`.

## Recommended (evaluate before adding)
- `LeaveRequest (EmployeeId, Status, StartDate)` — leave analytics hot path.
- `SalarySlip (PeriodId)` — payroll analytics.
- `Attendance (AttendanceDate)` — daily trend (already covered by `(EmployeeId, AttendanceDate)` for per-employee, but a date-leading index helps range scans).

## No fabricated EXPLAIN
Real PostgreSQL unavailable; no fabricated query plans. Indexes added only when justified by measured need.

## No materialized views
Normal queries first; views/materialized views only after measured need.
