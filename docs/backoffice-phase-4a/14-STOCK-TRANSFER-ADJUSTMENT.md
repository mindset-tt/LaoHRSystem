# 14 — Stock Transfer & Adjustment

## Transfer
Warehouse A → Warehouse B. Atomic (single transaction): `TRANSFER_OUT` + `TRANSFER_IN`.
No inventory-out without a corresponding destination-in.

## Adjustment
Requires a reason, user, and timestamp. `ADJUSTMENT_IN` (positive) or
`ADJUSTMENT_OUT` (negative). Audited automatically.

## Invariants
- Source and destination warehouses must differ (transfer).
- Transfer quantity must be positive.
- Adjustment requires a non-empty reason.
- Negative stock rejected unless configured.

## Access
`CanAdjustInventory` (Admin/HR) for adjustments; `CanManageInventory` for transfers.
