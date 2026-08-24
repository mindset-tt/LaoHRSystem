# 11 — Asset Auto-Creation

## Behavior
When a Goods Receipt line references an `InventoryItem` with `ItemType == ASSET`,
posting the receipt creates one fixed asset per accepted unit (e.g. 3 laptops →
3 asset records, each with its own code + assignment history).

## Idempotency
`GenerateAssetsAsync` counts existing assets for the receipt line and only
creates the delta, so re-posting (or a retry) cannot duplicate assets.

## Traceability
Each generated asset preserves: `PurchaseOrderItemId`, `GoodsReceiptItemId`,
`PurchaseDate` (PO order date), `AcquisitionCost` (PO line unit price), `Currency`.

## Test
`ProcurementFlowTests.AssetItemReceipt_GeneratesAssets_Idempotently` (3 assets).
