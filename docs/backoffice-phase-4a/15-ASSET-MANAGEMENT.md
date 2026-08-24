# 15 — Asset Management

## Entity
`Asset`: AssetCode, Name, CategoryId, SerialNumber, PurchaseOrderItemId,
PurchaseDate, AcquisitionCost, Currency, WorkLocationId, CustodianEmployeeId, Status.

## Status
`AVAILABLE`, `ASSIGNED`, `IN_MAINTENANCE`, `LOST`, `DAMAGED`, `RETIRED`, `DISPOSED`.

## Source
- Manual registration (Phase 4A).
- Purchase → Goods Receipt → Asset creation (for ItemType=ASSET) — foundation
  via `PurchaseOrderItemId` link; full auto-generation deferred.

## Invariants
- Disposed/retired asset cannot be reassigned.
- Disposal requires no active assignment.

## Numbering
`AST-{yyyy}-{000000}` via `NumberSequenceService`.

## Access
Asset-gated (Admin/HR).
