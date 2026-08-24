# 09 — Purchase Order

## Entities
- `PurchaseOrder`: PONumber, SupplierId, RequestId, Currency, OrderDate,
  ExpectedDate, PaymentTerms, Status, Subtotal, Tax, Total.
- `PurchaseOrderItem`: Description, ItemId, Quantity, Unit, UnitPrice, TaxRate,
  LineTotal, Notes.

## Commercial snapshot
PO lines preserve description/quantity/price/tax/unit at order time, even if the
item master changes later.

## Lifecycle
`DRAFT` → `SENT` → `PARTIALLY_RECEIVED` → `RECEIVED` → `CLOSED`; `CANCELLED`.

## Invariants
- Cannot create PO from a rejected/non-approved request.
- Cannot PO from a BLOCKED supplier.
- Cannot receive against a CANCELLED/CLOSED PO.
- Cannot cancel a RECEIVED/CLOSED/PARTIALLY_RECEIVED PO.

## Numbering
`PO-{yyyy}-{000000}` via `NumberSequenceService`.
