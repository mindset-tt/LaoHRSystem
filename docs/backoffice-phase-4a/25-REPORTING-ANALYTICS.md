# 25 — Reporting & Analytics

## Status
PARTIAL. Existing reporting/analytics architecture preserved. Back Office
reports are NOT yet implemented (deferred).

## Planned reports
Procurement Spend, Supplier Spend, PR Aging, PO Status, Receiving, Stock
Balance, Stock Movement, Low Stock, Asset Register, Asset Assignment, Asset
Maintenance, Budget vs Actual, Expense Summary, AP Aging, Contract Expiry,
Project Spend.

## Planned KPIs
`BO-PENDING-APPROVALS`, `PROC-OPEN-PR`, `PROC-OPEN-PO`, `PROC-PO-VALUE`,
`PROC-PENDING-RECEIPTS`, `INV-LOW-STOCK`, `INV-OUT-OF-STOCK`, `AST-TOTAL`,
`AST-ASSIGNED`, `AST-MAINTENANCE`, `FIN-BUDGET`, `FIN-COMMITTED`, `FIN-ACTUAL`,
`FIN-AVAILABLE`, `FIN-AP-OUTSTANDING`, `CONTRACT-EXPIRING`.

## Exports
CSV/Excel via existing export architecture; authorization server-side.

## Caution
No fake accounting KPIs from incomplete sources.
