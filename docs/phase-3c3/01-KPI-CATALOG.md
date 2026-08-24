# 01 — KPI Catalog

Every KPI is defined before visualization. Stable IDs, business meaning, source, date field, scope, and aggregation.

| KPI ID | Name | Lao Name | Definition | Formula | Source | Date Field | Scope | Aggregation |
|---|---|---|---|---|---|---|---|---|
| HR-HEADCOUNT-ACTIVE | Active Headcount | ຈຳນວນພະນັກງານທີ່ເຄື່ອນໄຫວ | Employees with `IsActive == true` | COUNT | Employee | — (as-of now) | Org (privileged) / self+reports (manager) | Count |
| HR-HEADCOUNT-INACTIVE | Inactive | ພະນັກງານບໍ່ເຄື່ອນໄຫວ | Employees with `IsActive == false` | COUNT | Employee | — | HR/Admin | Count |
| HR-NEW-HIRES | New Hires (YTD) | ພະນັກງານໃໝ່ | Employees hired since Jan 1 of current year | COUNT | Employee | HireDate | HR/Admin | Count |
| HR-LEAVE-PENDING | Pending Leave | ການລາພັກທີ່ລໍຖ້າ | Leave requests with `Status == PENDING` | COUNT | LeaveRequest | — | HR/Admin | Count |
| HR-LEAVE-TODAY | On Leave Today | ລາພັກມື້ນີ້ | Approved leave overlapping today | COUNT | LeaveRequest | StartDate/EndDate | Org | Count |
| ATT-PRESENT-TODAY | Present Today | ມາເຮັດວຽກມື້ນີ້ | Attendance `PRESENT` today | COUNT | Attendance | AttendanceDate | Org | Count |
| ATT-LATE | Late | ມາຊ້າ | Attendance with `IsLate == true` | COUNT | Attendance | AttendanceDate | Scoped | Count |
| ATT-EARLY-LEAVE | Early Leave | ອອກກ່ອນເວລາ | Attendance with `IsEarlyLeave == true` | COUNT | Attendance | AttendanceDate | Scoped | Count |
| ATT-MISSING-CLOCKOUT | Missing Clock Out | ຂາດການບັນທຶກອອກ | ClockIn set, ClockOut null | COUNT | Attendance | AttendanceDate | Scoped | Count |
| LEAVE-APPROVED | Approved Leave | ການລາພັກທີ່ອະນຸມັດ | Leave `APPROVED` in range | COUNT | LeaveRequest | StartDate | Scoped | Count |
| LEAVE-REJECTED | Rejected Leave | ການລາພັກທີ່ປະຕິເສດ | Leave `REJECTED` in range | COUNT | LeaveRequest | StartDate | Scoped | Count |
| PAY-GROSS-TOTAL | Gross Payroll | ເງິນເດືອນລວມ | Sum of `GrossIncome` | SUM | SalarySlip | PayrollPeriod | HR/Admin | Sum |
| PAY-NET-TOTAL | Net Payroll | ເງິນເດືອນສຸດທິ | Sum of `NetSalary` | SUM | SalarySlip | PayrollPeriod | HR/Admin | Sum |
| PAY-NSSF-EMPLOYEE | NSSF Employee | NSSF ພະນັກງານ | Sum of `NssfEmployeeDeduction` | SUM | SalarySlip | PayrollPeriod | HR/Admin | Sum |
| PAY-NSSF-EMPLOYER | NSSF Employer | NSSF ນາຍຈ້າງ | Sum of `NssfEmployerContribution` | SUM | SalarySlip | PayrollPeriod | HR/Admin | Sum |
| PAY-PIT | PIT | ພາສີລາຍໄດ້ | Sum of `TaxDeduction` | SUM | SalarySlip | PayrollPeriod | HR/Admin | Sum |
| FIN-EXPENSE-SUBMITTED | Submitted Expenses | ລາຍຈ່າຍທີ່ສົ່ງ | Sum `AmountLak` where `SUBMITTED` | SUM | Expense | ExpenseDate | Scoped | Sum |
| FIN-EXPENSE-APPROVED | Approved Expenses | ລາຍຈ່າຍທີ່ອະນຸມັດ | Sum `AmountLak` where `APPROVED`/`PAID` | SUM | Expense | ExpenseDate | Scoped | Sum |
| FIN-LOAN-OUTSTANDING | Outstanding Principal | ເງິນກູ້ຄົງຄ້າງ | Sum `PrincipalLak` where `ACTIVE`/`APPROVED` | SUM | EmployeeLoan | — | Scoped | Sum |
| FIN-LOAN-ACTIVE | Active Loans | ເງິນກູ້ທີ່ເຄື່ອນໄຫວ | Loans with `Status == ACTIVE` | COUNT | EmployeeLoan | — | Scoped | Count |
| PM-PROJECT-ACTIVE | Active Projects | ໂຄງການທີ່ເຄື່ອນໄຫວ | Projects `ACTIVE` | COUNT | Project | — | Org | Count |
| PM-PROJECT-COMPLETED | Completed Projects | ໂຄງການສຳເລັດ | Projects `COMPLETED` | COUNT | Project | — | Org | Count |
| PM-PROJECT-AT-RISK | Projects At Risk | ໂຄງການທີ່ມີຄວາມສ່ຽງ | Open risks with HIGH/CRITICAL priority | COUNT | Risk | — | Org | Count |
| PM-TASK-OPEN | Open Tasks | ວຽກທີ່ເປີດ | Tasks not DONE/CANCELLED | COUNT | ProjectTask | — | Org | Count |
| PM-TASK-OVERDUE | Overdue Tasks | ວຽກທີ່ເກີນກຳນົດ | Tasks past DueDate, not done | COUNT | ProjectTask | DueDate | Org | Count |
| PM-RISK-OPEN | Open Risks | ຄວາມສ່ຽງທີ່ເປີດ | Risks `OPEN` | COUNT | Risk | — | Org | Count |
| PM-RISK-CRITICAL | Critical Risks | ຄວາມສ່ຽງຮ້າຍແຮງ | Open risks `CRITICAL` | COUNT | Risk | — | Org | Count |
| PM-ISSUE-OPEN | Open Issues | ບັນຫາທີ່ເປີດ | Issues not DONE/CANCELLED | COUNT | Issue | — | Org | Count |
| MSS-DIRECT-REPORTS | Direct Reports | ຜູ້ໃຕ້ບັງຄັບບັນຊາ | Active employees with `ManagerId == self` | COUNT | Employee | — | Manager | Count |
| MSS-PRESENT-TODAY | Present Today | ມາເຮັດວຽກມື້ນີ້ | Direct reports present today | COUNT | Attendance | AttendanceDate | Manager | Count |
| MSS-ON-LEAVE-TODAY | On Leave Today | ລາພັກມື້ນີ້ | Direct reports on leave today | COUNT | LeaveRequest | StartDate/EndDate | Manager | Count |
| MSS-PENDING-APPROVALS | Pending Approvals | ການອະນຸມັດທີ່ລໍຖ້າ | Approval steps assigned to self, PENDING | COUNT | ApprovalStep | — | Manager | Count |

## Ownership
- HR: headcount, new hires, leave, attendance
- Payroll: PAY-* (gross/net/NSSF/PIT)
- Finance: FIN-* (expense/loan)
- PM: PM-* (project/task/risk/issue)
- Executive: cross-domain summary

## Null / no-data semantics
- `0` = a real zero count/sum.
- Empty breakdown list = no data (rendered as "No data", distinct from 0).
- `Not permitted` = 403 (role/scope), distinct from empty.

## Comparison
- Delta shown only where mathematically meaningful (e.g. period-over-period). Zero-division → no delta (not `Infinity%`).
