# 26 — Database Migration

## Migration
`20260824020934_AddBackOfficeFoundation` — purely ADDITIVE (0 AlterColumn,
19 CreateTable). No rebase of the existing 6-migration chain.

## New tables (19)
NumberSequences, Suppliers, CostCenters, Budgets, PurchaseRequests,
PurchaseRequestItems, PurchaseOrders, PurchaseOrderItems, GoodsReceipts,
GoodsReceiptItems, InventoryCategories, InventoryItems, Warehouses,
StockMovements, Assets, AssetAssignments, Contracts, ServiceRequestCategories,
ServiceRequests.

## Timestamp convention
Existing chain uses `timestamp with time zone` (Npgsql default). The new
migration matches it. The legacy timestamp switch (`Npgsql.EnableLegacyTimestampBehavior`)
is applied ONLY for `database update` (via `NPGSQL_LEGACY_TIMESTAMP=1` env var),
NOT for `migrations add` — otherwise EF scaffolds non-additive AlterColumn ops.

## Validation
- Fresh migration against real PostgreSQL 18.6: PASS (103 tables = 84 + 19).
- Seed data (ServiceRequestCategories): PASS (6 rows).

## Rule
All future migrations are ADDITIVE. No baseline regeneration.
