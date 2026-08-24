# 07 — REPORT ENDPOINTS

Finance report endpoints added in Phase 4B.2, all under `FinanceReportsController`
(`/api/finance/reports`), all gated by `IFinanceAccessService`.

| Report | Endpoint | Filters | Auth |
|---|---|---|---|
| Supplier Invoice Register | `GET /api/finance/reports/supplier-invoices` | from, to, supplierId, status, departmentId, costCenterId, projectId | `CanViewFinanceReports` |
| AP Aging | `GET /api/finance/reports/ap-aging` | — | `CanViewFinanceReports` |
| Payment Register | `GET /api/finance/reports/payments` | from, to, status | `CanViewFinanceReports` |
| General Ledger | `GET /api/finance/reports/general-ledger` | fiscalPeriodId | `CanViewFinanceReports` |
| Trial Balance | `GET /api/finance/reports/trial-balance` | fiscalPeriodId | `CanViewFinanceReports` |
| Expense Summary | `GET /api/finance/reports/expense-summary` | from, to | `CanViewFinanceReports` |
| Budget Utilization | `GET /api/finance/reports/budget-utilization` | fiscalYear | `CanViewFinanceReports` |

## Semantics

- **General Ledger** is derived from POSTED journal lines (no mutable GL balance
  table as sole truth). `AccountingService.GetGeneralLedgerAsync` returns
  `GeneralLedgerRow` per posted line.
- **Trial Balance** reuses `AccountingService.GetTrialBalanceAsync`; total debit
  equals total credit for the selected period (double-entry invariant).
- **AP Aging** reuses `AccountsPayableService.GetAgingAsync` (buckets: Current,
  1–30, 31–60, 61–90, Over 90).
- **Expense Summary** reuses the existing `Expense` domain (no `ExpenseV2`).
- **Budget Utilization** exposes Approved/Reserved/Committed/Actual/Available
  per the established budget lifecycle.

## Status

PASS — all seven reports implemented with server-side authorization.
