# 11 — SERVICE REQUEST ROUTING

## Design

- Employees see only their own requests; Admin/HR see all (controller-scoped).
- Assignment is recorded in `ServiceRequestHistory` (ASSIGNED) with from/to values.
- Comments reuse the polymorphic `EntityComment` (SERVICE_REQUEST added to the
  allow-list).

## Status

PASS.
