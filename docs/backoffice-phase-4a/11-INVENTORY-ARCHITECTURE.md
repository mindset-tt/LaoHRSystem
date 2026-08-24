# 11 — Inventory Architecture

## Entities
- `InventoryItem`: SKU, Name, NameLao, Description, CategoryId, UnitOfMeasure,
  ItemType, TrackInventory, ReorderLevel, IsActive.
- `InventoryCategory`: Code, Name, NameLao, ParentCategoryId, SortOrder, IsActive.

## Item types
`STOCK`, `CONSUMABLE`, `SERVICE`, `ASSET` — routes purchase flow correctly.

## Stock balance = derived from ledger
No mutable `Item.Quantity`. `InventoryService.GetBalanceAsync` sums the
`StockMovement` ledger (ins − outs).

## Costing
NOT implemented (no FIFO/LIFO/weighted). Quantity correctness first; costing is
a separate accounting enhancement.

## Reorder
Basic low-stock indicator (current ≤ reorder level). No predictive AI.

## Access
Inventory-gated (`CanViewInventory`/`CanManageInventory` = Admin/HR).
