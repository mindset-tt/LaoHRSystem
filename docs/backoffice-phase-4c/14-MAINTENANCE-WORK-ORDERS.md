# 14 — MAINTENANCE WORK ORDERS

## Design

A single polymorphic `WorkOrder` with `SourceType` + `SourceId`
(FACILITY, ROOM, ASSET, VEHICLE, SERVICE_REQUEST). No separate per-domain
maintenance tables.

## Model

`WorkOrder` (number, source, category, title, description, priority, status,
assigned employee, supplier, scheduled/completed, cost/currency).

## Finance

`Cost` is informational; authoritative financial posting links to Expense/PO/
SupplierInvoice (no duplicate accounting).

## API

`/api/work-orders` (list/create/update).

## Status

PASS.
