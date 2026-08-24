# 06 — DOCUMENT EXPIRY

## Design

`CorporateDocument.ExpiryDate` is optional. Meaningful for insurance, vehicle
registration, licenses, supplier certificates, contract attachments, employee
certifications. Not forced for every document.

## Query

Documents can be filtered by `ExpiryDate` (indexed). Expiry notifications reuse
`NotificationService` with configurable lead time (30/14/7 days) — implemented as
a query surface, not a spammy background loop.

## Status

PASS (expiry field + index + query surface; notification lead-time configurable).
