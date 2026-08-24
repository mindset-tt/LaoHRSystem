# 12 — FACILITIES

## Semantics

- `WorkLocation` = employment/organization location.
- `Warehouse` = stock location.
- `Facility` = physical operational site/building/office.
- `Room` = bookable/maintainable space inside a facility.

## Model

`Facility` (code, name, type, manager, address, status) and `Room` (facility,
code, name, type, capacity, floor, bookable).

## Authorization

Broad read (Admin/HR/Finance); narrow manage (Admin/HR).

## API

`/api/facilities` (list/create), `/api/facilities/rooms` (list/create).

## Status

PASS.
