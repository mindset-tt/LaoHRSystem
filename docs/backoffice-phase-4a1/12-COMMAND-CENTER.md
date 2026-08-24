# 12 — Command Center

## Objective
Answer "what requires my attention today?" — role-scoped, no fake values.

## Endpoint
`GET /api/backoffice/command-center` → `BackOfficeKpiDto`.

## Metrics (backed by current data only)
- Self-service (all roles): MyOpenPurchaseRequests, MyOpenServiceRequests.
- Privileged (Admin/HR): PendingApprovals, OpenPurchaseRequests,
  PendingPurchaseRequests, OpenPurchaseOrders, PendingReceipts, LowStockItems,
  OutOfStockItems, AssetsInMaintenance, ContractsExpiring, OpenServiceRequests.

## Role scoping
- Employee: my requests/approvals.
- Manager: team requests/approvals (via existing approval inbox).
- Procurement/Warehouse/Finance/Asset Admin/Executive: aggregate metrics.

## No salary data
No payroll/salary/loan/performance data is exposed here.

## Frontend
`/backoffice` route (role-sensitive cards). No fake fallback records.
