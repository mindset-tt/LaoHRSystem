# 19 — TRAVEL

## Design

`TravelRequest` (number, employee, department/project, purpose, destination,
departure/return dates, estimated cost, status). Optional `TravelSegment` and
`TravelAccommodation` records (not a booking engine).

## Invariant

`ReturnDate >= DepartureDate`.

## Status

DRAFT, PENDING_APPROVAL, APPROVED, BOOKED, IN_PROGRESS, COMPLETED, CANCELLED, SETTLED.

## API

`/api/travel` (list/get/create/submit/approve/reject).

## Status

PASS.
