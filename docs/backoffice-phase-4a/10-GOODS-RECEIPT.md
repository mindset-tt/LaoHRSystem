# 10 — Goods Receipt

## Entities
- `GoodsReceipt`: ReceiptNumber, PurchaseOrderId, ReceivedByEmployeeId,
  ReceivedDate, WarehouseId, Notes, Status.
- `GoodsReceiptItem`: PurchaseOrderItemId, QuantityReceived, AcceptedQuantity,
  RejectedQuantity, Notes.

## Flow
1. Create receipt (DRAFT) against a PO.
2. Post receipt → stock movements recorded (RECEIPT) for stocked items + PO
   status updated (PARTIALLY_RECEIVED / RECEIVED).

## Partial receipt
Supported. PO is not marked RECEIVED until all lines are fully received.

## Invariants
- Cannot receive more than ordered (per line).
- Cannot receive against CANCELLED/CLOSED PO.
- Posting is idempotent (POSTED guard prevents double stock count).
- Stock movements are transactional (rollback on failure).

## Three-way match foundation
PO ↔ Goods Receipt ↔ Supplier Invoice relationships are clean; full AP posting
is deferred (Phase 4B).

## Numbering
`GRN-{yyyy}-{000000}` via `NumberSequenceService`.
