# 17 — Notification Verification

## Reused
`NotificationService` (in-app). No new notification system.

## Events emitted (Phase 4A/4A.1)
- PR submitted → `APPROVAL_REQUESTED` to direct manager.
- PR approved/rejected → `APPROVAL_APPROVED` / `APPROVAL_REJECTED` to requester.

## Deferred (no spam)
- PO approved, Goods receipt, Asset assigned, Contract expiry, Invoice due,
  Payment approved, Budget threshold — not yet wired (would require additional
  scheduler/hosted-job work; contract reminders deferred).

## Rule
Avoid notification spam; only meaningful events.
