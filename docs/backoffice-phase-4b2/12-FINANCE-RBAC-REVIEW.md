# 12 — FINANCE RBAC REVIEW

Review of all role checks in the finance/accounting surface.

## Role check inventory

| Location | Check | Classification |
|---|---|---|
| `FinanceAccessService` | `IsInRole("Finance") \|\| IsInRole("Admin")` | Finance capability bundle |
| `BackOfficeAccessService.CanViewFinance/CanManageFinance` | `IsAdmin()` only | Back-office finance (budgets) |
| `AuditLogsController` | `[Authorize(Roles = "Admin")]` | Admin-only audit |
| `PayrollController`, `AnalyticsController`, etc. | `[Authorize(Roles = "Admin,HR")]` | HR domain (not finance) |

## Findings

1. **`FinanceAccessService` is capability-shaped but not capability-differentiated** —
   all 20 methods return `IsFinance()`. This is acceptable for the current
   three-role + Finance model, but the interface is ready for finer-grained
   permissions later.

2. **Budget RBAC inconsistency (FIXED)** — `BudgetsController` used
   `IBackOfficeAccessService.CanViewFinance()` (Admin-only), so the `Finance`
   role could view AP/payments/journals/COA but NOT budgets. Switched to
   `IFinanceAccessService` so Finance + Admin both access budgets consistently.

## Domain separation

- **HR ≠ Finance**: HR has no finance capabilities (`FinanceAccessService` returns
  false for HR). Proven by `FinanceAuthorizationTests` and
  `FinanceExportAuthorizationTests`.
- **Procurement ≠ Accounting**: procurement is Admin/HR via `IBackOfficeAccessService`;
  accounting is Finance/Admin via `IFinanceAccessService`.
- **Warehouse ≠ Finance**: warehouse (inventory) is Admin/HR; finance is
  Finance/Admin.
- **System Admin ≠ operational Accountant**: Admin is a superset, but the
  `Finance` role grants accounting capabilities without global HR/system admin.

## Status

PASS — with the budget RBAC fix applied.
