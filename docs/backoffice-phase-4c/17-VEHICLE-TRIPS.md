# 17 — VEHICLE TRIPS

## Design

`VehicleTrip` (vehicle, driver, start/end time, start/end odometer, destination,
purpose, optional booking link).

## Odometer invariants

- `EndOdometer >= StartOdometer`.
- A completed trip must not decrease the vehicle's current odometer.
- Corrections require an explicit admin action with a reason (audited).

## API

`/api/fleet/trips` (list), `/api/fleet/trips/start`, `/api/fleet/trips/{id}/complete`,
`/api/fleet/vehicles/{id}/correct-odometer`.

## Tests

`FleetServiceTests` — start sets IN_USE, complete updates odometer, end-below-start
rejected, regression rejected, correction writes audit.

## Status

PASS.
