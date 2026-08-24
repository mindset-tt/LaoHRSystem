# 30 — PG16 CONCURRENCY

## Harness

`scripts/validate-corporate-postgres.ps1` starts a disposable `postgres:16-alpine`
container, applies migrations, runs the real-relational concurrency tests, and
destroys the container.

## Tests (6 total)

Finance (4): Payment race, Journal race, Auto-post race, Period-close race.
Corporate (2): Room booking race, Vehicle booking race.

## Result

6 / 6 PASS.

## Booking concurrency mechanism

`BookingService` locks the room/vehicle row (`SELECT ... FOR UPDATE`) inside a
transaction, so two simultaneous overlapping bookings serialize and only one
succeeds. Proven on PostgreSQL 16 (not InMemory).
