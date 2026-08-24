# 27 — Performance Indexes

## Indexes added (Phase 4A)
- `NumberSequence` (Prefix, Year) unique.
- `Supplier` (SupplierCode) unique, (Status).
- `CostCenter` (Code) unique.
- `Budget` (FiscalYear, DepartmentId, Category), (FiscalYear, CostCenterId).
- `PurchaseRequest` (RequestNumber) unique, (Status, CreatedAt).
- `PurchaseRequestItem` (PurchaseRequestId).
- `PurchaseOrder` (PONumber) unique, (SupplierId, Status).
- `PurchaseOrderItem` (PurchaseOrderId).
- `GoodsReceipt` (ReceiptNumber) unique, (PurchaseOrderId).
- `GoodsReceiptItem` (GoodsReceiptId).
- `InventoryCategory` (Code) unique.
- `InventoryItem` (SKU) unique, (ItemType, IsActive).
- `Warehouse` (Code) unique.
- `StockMovement` (ItemId, WarehouseId, OccurredAt), (ReferenceType, ReferenceId).
- `Asset` (AssetCode) unique, (Status).
- `AssetAssignment` (AssetId, AssignedAt), (EmployeeId, ReturnedAt).
- `Contract` (ContractNumber) unique, (Status, EndDate).
- `ServiceRequestCategory` (Code) unique.
- `ServiceRequest` (RequestNumber) unique, (Status, CreatedAt), (AssignedEmployeeId, Status).

## Note
Single-host optimization. No excessive caching/background workers. PostgreSQL
indexes on FK + filter columns.
