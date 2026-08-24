# 15 — FLEET

## Design

`Vehicle` (code, registration, make/model/year/VIN, type, fuel, status, location,
current odometer) with optional `AssetId` link to the canonical `Asset` (no
duplicate acquisition values).

## Status

AVAILABLE, RESERVED, IN_USE, IN_MAINTENANCE, OUT_OF_SERVICE, DISPOSED.

## Authorization

View (Admin/HR/Finance); manage (Admin/HR); book (any employee).

## API

`/api/fleet/vehicles` (list/create).

## Status

PASS.
