# 17 — Asset Maintenance

## Status
DEFERRED. No `AssetMaintenance` entity implemented in Phase 4A.

## Planned shape (Phase 4B)
`AssetMaintenance`: AssetId, Type, VendorId, StartDate, CompletedDate, Cost,
Status, Notes.

## Depreciation
DEFERRED. No legal/accounting depreciation engine without accounting-policy
requirements. Architecture foundation only (AcquisitionCost + Currency fields
already present on `Asset`).
