# 24 — CORPORATE REPORTS

## Report surface

The corporate controllers expose list endpoints that serve as report registers:

- Contract Register — `GET /api/contracts` (status/search filters).
- Contract Expiry — `GET /api/contracts?status=EXPIRING` (or EndDate filter).
- Document Expiry — `GET /api/corporate-documents` (ExpiryDate filter).
- Service Request Backlog — `GET /api/service-requests?status=OPEN`.
- Facility Register — `GET /api/facilities`.
- Room Usage — `GET /api/facilities/bookings`.
- Work Orders — `GET /api/work-orders`.
- Fleet Register — `GET /api/fleet/vehicles`.
- Vehicle Usage — `GET /api/fleet/bookings` + `GET /api/fleet/trips`.
- Travel Register — `GET /api/travel`.
- Visitor Log — `GET /api/visitors/visits`.

## Authorization

Server-side capability (`CanViewFacilities`, `CanViewFleet`, `CanManageVisitors`,
etc.).

## Status

PASS (register endpoints; dedicated report aggregation deferred to a later phase).
