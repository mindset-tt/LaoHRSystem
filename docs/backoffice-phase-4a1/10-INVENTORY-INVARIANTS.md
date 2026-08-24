# 10 — Inventory Invariants

- Stock movement quantity must be non-zero (positive).
- Stocked items require a warehouse.
- Transfers balance (out + in atomically, single transaction).
- Historical movements are immutable (append-only ledger).
- Adjustments require a reason.
- Negative stock rejected unless `ALLOW_NEGATIVE_STOCK=true`.
- Stock balance derives from the movement ledger (no mutable `Item.Quantity`).

## Transactionality
`InventoryService.TransferAsync` and `GoodsReceiptsController.Post` use a
transaction (relational providers) so a partial business state cannot persist.
