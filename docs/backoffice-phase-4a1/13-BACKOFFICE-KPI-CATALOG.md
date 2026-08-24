# 13 — Back Office KPI Catalog

Stable KPI identifiers (adapted to actual semantics; no fake accounting KPIs):

| KPI | Source |
|---|---|
| PROC-PR-OPEN | PurchaseRequests status=APPROVED |
| PROC-PR-PENDING | PurchaseRequests status=PENDING_APPROVAL |
| PROC-PO-OPEN | PurchaseOrders status=SENT/PARTIALLY_RECEIVED |
| PROC-PO-PENDING-RECEIPT | PurchaseOrders status=SENT |
| PROC-SPEND-COMMITTED | Budget.CommittedAmount |
| INV-LOW-STOCK | items with on-hand ≤ reorder level |
| INV-OUT-OF-STOCK | items with on-hand ≤ 0 |
| INV-RECEIPTS | StockMovements type=RECEIPT |
| AST-TOTAL / AST-ASSIGNED / AST-AVAILABLE | Assets by status |
| FIN-BUDGET-APPROVED / RESERVED / COMMITTED / AVAILABLE | Budget fields |
| CONTRACT-EXPIRING | Contracts ACTIVE with EndDate ≤ now+30d |
| OPS-REQUEST-OPEN | ServiceRequests OPEN/IN_PROGRESS |

## No inventory value
`Inventory Value` is NOT implemented (no costing method). Quantity ≠ financial
valuation.
