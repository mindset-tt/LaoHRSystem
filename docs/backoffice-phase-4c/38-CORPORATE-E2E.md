# 38 — CORPORATE E2E

End-to-end user flows for the major corporate domains.

## Contract

DRAFT → create (UI) → submit-approval (API) → approve (API) → ACTIVE →
renew (API, history preserved) → terminate (API, history preserved).

## Service Desk

Employee creates request (UI) → Admin/HR assigns (API) → status change (API,
history recorded) → comments (reused EntityComment) → resolve/close.

## Room

Employee books room (API, overlap rejected) → concurrent conflict rejected
(PG16) → cancellation frees slot.

## Fleet

Vehicle created (UI) → booking (API, overlap rejected) → trip start/complete
(odometer invariants) → odometer correction (audited).

## Travel

Employee creates request (UI) → submit (API, direct-manager approval) →
approve/reject (API, notification) → expense linked via `Expense.TravelRequestId`.

## Visitor

Pre-register (UI) → check-in (API, host notified) → check-out (API).

## Status

PASS (backend E2E proven by tests; UI covers list/create/action surfaces).
