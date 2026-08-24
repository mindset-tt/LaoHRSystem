# 12 — Warehouse

## Entity
`Warehouse`: Code, Name, NameLao, WorkLocationId, Address, ManagerEmployeeId, Status.

## Multiple warehouses
Supported. No assumption of a single office stockroom.

## Reuse
`WorkLocationId` links a warehouse to a branch/location (reuses existing
`WorkLocation`, no new `Branch` entity).

## Access
Inventory-gated (Admin/HR).
