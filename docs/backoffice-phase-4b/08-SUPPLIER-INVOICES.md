# 08 — Supplier Invoices

## Entities
- `SupplierInvoice`: InvoiceNumber, SupplierId, PurchaseOrderId, InvoiceDate,
  DueDate, Currency, Subtotal, TaxAmount, TotalAmount, PaidAmount, RemainingAmount,
  Status, MatchStatus, CostCenterId, ProjectId, DepartmentId.
- `SupplierInvoiceLine`: PurchaseOrderItemId, InventoryItemId, Description,
  Quantity, UnitPrice, Subtotal, TaxAmount, AccountId, CostCenterId, ProjectId.

## PO-backed vs non-PO
Both supported. Non-PO invoices do not bypass controls silently (still require
approval; match status = NOT_APPLICABLE).

## Status
DRAFT, PENDING_MATCH, MATCH_EXCEPTION, PENDING_APPROVAL, APPROVED,
PARTIALLY_PAID, PAID, VOID.

## Duplicate prevention
Unique (SupplierId, InvoiceNumber) with whitespace normalization. Invoice numbers
are NOT assumed globally unique across suppliers.

## Immutability
Approved/posted invoices are not freely rewritten; correction via void/reversal.

## Access
`CanViewAp`/`CanManageAp`/`CanApproveAp` (Admin/Finance only).
