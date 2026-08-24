# 22 — VISITOR MANAGEMENT

## Design

`Visitor` (minimal personal data: name, company, phone, email, notes — no
ID/passport storage by default) and `Visit` (visitor, host, facility, purpose,
expected/check-in/check-out, status).

## Status

EXPECTED, CHECKED_IN, CHECKED_OUT, CANCELLED, NO_SHOW.

## Privacy

Ordinary employees cannot browse the visitor register; only Admin/HR (reception/
front desk) can. Host is notified on check-in via `NotificationService`.

## API

`/api/visitors` (list/create), `/api/visitors/visits` (list/create/check-in/check-out).

## Status

PASS.
