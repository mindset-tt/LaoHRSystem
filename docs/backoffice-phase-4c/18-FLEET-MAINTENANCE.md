# 18 — FLEET MAINTENANCE

## Design

Vehicle maintenance uses the shared `WorkOrder` (SourceType = VEHICLE). No
separate vehicle-maintenance table.

## Cost

Maintenance cost links to Expense/PO/SupplierInvoice for financial posting; the
work order `Cost` is informational.

## Status

PASS.
