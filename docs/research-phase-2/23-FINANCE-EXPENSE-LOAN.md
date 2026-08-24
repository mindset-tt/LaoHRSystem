# 23 — Finance (Expense/Loan) + 24 — Knowledge + 25 — Documents + 26 — Reporting/BI

> Research date: 2026-08-21.

## Finance features (23)

| Feature | LaoHR | Priority | Notes |
|---|:--:|:--:|---|
| Expense claims | ✅ | — | Approve/reject/pay workflow |
| Expense categories | ✅ (seeded) | — | |
| Advances | ⚠️ (loans cover this) | — | |
| Reimbursement | ✅ (expense) | — | |
| Per diem | ❌ | P3 | |
| Travel/mileage | ❌ | P3 | |
| Payroll deductions | ⚠️ (manual OtherDeductions) | P1 | Auto-link loan installments |
| Employee loans | ✅ | — | Full lifecycle |
| Installments | ✅ | — | LoanRepayment |
| Settlement | ❌ | P3 | |
| Accounting export | ❌ | P2 | Journal export |
| Cost center allocation | ❌ | P2 | |
| Project cost allocation | ❌ | P2 | |

**Recommendation**: P1 — auto-link loan installments to payroll deductions. P2 — add cost center + accounting export (journal entries for payroll/expenses/loans).

## Knowledge management (24)

| Feature | LaoHR | Priority |
|---|:--:|:--:|
| Categories | ✅ (seeded) | — |
| Articles | ✅ | — |
| Search | ✅ (basic filter) | P2 (full-text) |
| Versioning | ❌ | P3 |
| Draft/review/publish workflow | ❌ | P3 |
| Expiry | ❌ | P3 |
| Audience targeting | ⚠️ (announcements have audience) | P2 |
| Acknowledgement | ❌ | P2 |
| Attachments | ❌ | P2 |
| Analytics (views) | ✅ (view count) | — |
| Permissions | ✅ (Authorize) | — |
| Comments | ✅ (EntityComment) | — |

**Recommendation**: P2 — add article attachments + acknowledgement tracking + audience targeting. P3 — versioning + draft workflow. Keep simple.

## Document management (25)

| Feature | LaoHR | Priority |
|---|:--:|:--:|
| Upload/download | ✅ | — |
| Metadata (type) | ✅ (DocumentType enum) | — |
| Versioning | ❌ | P3 |
| Expiry tracking | ❌ | P2 (visa, work permit, contract) |
| Approvals | ❌ | P3 |
| Retention policy | ❌ | P3 |
| Legal hold | ❌ | P3 |
| Encryption at rest | ❌ | P2 |
| Virus scanning | ❌ | P2 |
| Preview | ❌ | P3 |
| Search | ❌ | P3 |
| Permissions | ✅ (Authorize) | — |
| Audit | ✅ (interceptor) | — |

**Recommendation**: P2 — add `ExpiryDate` to `EmployeeDocument` (visa/work permit/contract expiry alerts). P2 — store documents outside `wwwroot/uploads` (encrypted blob storage or at least outside web root). P2 — file upload magic-byte validation. P3 — versioning, preview, retention.

## Reporting & BI (26)

### Statutory reports (Lao PDR)

| Report | LaoHR | Priority | Notes |
|---|:--:|:--:|---|
| NSSF monthly report | ✅ | — | PDF + ZIP |
| NSSF payment form (LSSO) | ✅ | — | iText form fill |
| PIT monthly withholding | ❌ | P1 | Monthly tax remittance |
| PIT annual reconciliation | ❌ | P2 | Fiscal year (Oct–Sep) |
| Headcount report | ❌ | P2 | |
| Payroll summary | ✅ (Excel export) | — | |
| Bank transfer files | ✅ | — | BCEL/LDB |

### Management reports

| Report | LaoHR | Priority |
|---|:--:|:--:|
| Headcount trend | ❌ | P2 |
| Turnover | ❌ | P2 |
| Attendance summary | ❌ | P2 |
| Overtime report | ❌ | P2 |
| Leave report | ✅ (Excel export) | — |
| Payroll cost | ❌ | P2 |
| Tax liability | ❌ | P1 |
| Social security liability | ❌ | P1 |
| Loan outstanding | ❌ | P2 |
| Expense summary | ❌ | P2 |
| Project workload | ❌ | P2 |
| Risk report | ❌ | P3 |

### Dashboards (49)

| Dashboard | LaoHR | Priority |
|---|:--:|:--:|
| Employee | ⚠️ (stats only) | P2 |
| Manager | ❌ | P2 |
| HR | ⚠️ (4 scalars) | P1 (extensible) |
| Payroll | ❌ | P2 |
| Finance | ❌ | P2 |
| PM | ❌ | P2 |
| Executive | ❌ | P2 |
| System Admin | ❌ | P3 |

### Data visualization (50)

| Chart | Priority | Notes |
|---|---|---|
| Headcount trend (line) | P2 | |
| Turnover (line) | P2 | |
| Payroll cost (bar/line) | P2 | |
| Department distribution (donut) | P2 | |
| Absence trend (line) | P2 | |
| Overtime (bar) | P2 | |
| Project health (status cards) | P2 | |
| Risk heatmap (SVG matrix) | P2 | |
| Resource utilization (bar) | P3 | |

**Recommendation**: Build lightweight SVG charts (no chart library — keep lightweight). P1 — PIT/NSSF liability reports + HR dashboard. P2 — management reports + role dashboards + SVG charts.