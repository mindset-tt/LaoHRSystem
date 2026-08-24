# 13 — Expense Integration

## Reuse
Existing `Expense` domain is reused (no ExpenseV2). Employee Expense (reimbursement)
is distinct from SupplierInvoice (vendor obligation); both may feed accounting.

## Lifecycle (foundation)
Expense Approved → accounting posting eligible → payment/reimbursement → journal.
Not yet auto-posted (requires account mappings + explicit posting semantics).

## Gap
`Expense` lacks CostCenterId/ProjectId/DepartmentId. Adding these is a follow-up
to enable GL posting with dimensional tags.

## Access
Existing expense authorization preserved (self-service + Admin/HR approval).
