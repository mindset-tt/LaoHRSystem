# 13 — ROOM BOOKING

## Overlap invariant

Absolute overlap (excludes CANCELLED):
`existing.StartAt < requested.EndAt AND existing.EndAt > requested.StartAt`.

Adjacent bookings (A: 09:00–10:00, B: 10:00–11:00) do NOT overlap.

## Capacity

If `ParticipantCount` is supplied and exceeds `Room.Capacity`, the booking is
rejected.

## Concurrency

`BookingService.BookRoomAsync` locks the room row (`SELECT ... FOR UPDATE`) inside
a transaction, so two simultaneous overlapping bookings serialize and only one
succeeds. Proven on PostgreSQL 16 (`RoomBookingRace_TwoConcurrentOverlappingBookings_OneSucceeds`).

## API

`/api/facilities/bookings` (list/create/cancel).

## Status

PASS.
