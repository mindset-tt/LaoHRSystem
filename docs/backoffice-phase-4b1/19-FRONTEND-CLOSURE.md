# 19 — Frontend Closure

## Routes
`/finance/dashboard`, `/finance/invoices`, `/finance/payments`, `/finance/accounts`,
`/finance/journals`, `/finance/settings` (new).

## Finance settings page
Shows accounting configuration status (configured/missing control accounts).

## Posted journal UI
Read-only (no Edit button) — enforced by backend immutability.

## Bank masking UI
Uses server-provided masked value.

## i18n
Lao + English keys added for finance settings.

## Tests
`financeAccounting.test.ts` extended (48 total frontend tests).
