# 13 — Stock Movement

## Entity
`StockMovement`: ItemId, WarehouseId, MovementType, Quantity, ReferenceType,
ReferenceId, OccurredAt, PerformedByEmployeeId, Notes.

## Movement types
`RECEIPT`, `ISSUE`, `TRANSFER_IN`, `TRANSFER_OUT`, `ADJUSTMENT_IN`,
`ADJUSTMENT_OUT`, `RETURN`.

## Invariants
- Quantity must be positive (non-zero).
- Stocked items require a warehouse.
- Historical movements are immutable (append-only ledger).
- Negative stock rejected unless `ALLOW_NEGATIVE_STOCK` setting is true.

## Access
Inventory-gated (Admin/HR). All movements audited automatically.
