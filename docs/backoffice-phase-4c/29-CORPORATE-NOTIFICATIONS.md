# 29 — CORPORATE NOTIFICATIONS

## Implemented

- `TRAVEL_APPROVED` / `TRAVEL_REJECTED` — notify the travel requester.
- `VISITOR_ARRIVED` — notify the host on check-in.
- `CONTRACT_APPROVED` — notify the contract owner.

## Reused

`NotificationService` (single system). No spam loops; notifications are
event-driven on explicit actions.

## Status

PASS.
