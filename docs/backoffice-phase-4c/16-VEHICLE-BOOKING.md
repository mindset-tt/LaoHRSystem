# 16 — VEHICLE BOOKING

## Overlap invariant

Same as room booking: absolute overlap (excludes CANCELLED) is rejected.

## Availability

IN_MAINTENANCE / OUT_OF_SERVICE / DISPOSED vehicles cannot be booked.

## Concurrency

`BookingService.BookVehicleAsync` locks the vehicle row (`SELECT ... FOR UPDATE`)
inside a transaction. Proven on PostgreSQL 16
(`VehicleBookingRace_TwoConcurrentOverlappingBookings_OneSucceeds`).

## API

`/api/fleet/bookings` (list/create/cancel).

## Status

PASS.
