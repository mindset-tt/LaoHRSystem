# 02 — Segregation of Duties

## Service
`SegregationOfDutiesService` — lightweight, configurable policy (NOT a generic
rules engine). Server-side enforcement (frontend hiding is not sufficient).

## Policies (configurable via SystemSettings, conservative PRODUCT defaults)
- `SOD_PREVENT_INVOICE_SELF_APPROVAL` (default true)
- `SOD_PREVENT_PAYMENT_SELF_APPROVAL` (default true)
- `SOD_PREVENT_JOURNAL_SELF_POST` (default true)

## Enforcement
- Invoice approval → `EnforceInvoiceApprovalAsync` (creator ≠ approver).
- Payment approval → `EnforcePaymentApprovalAsync` (creator ≠ approver).
- Journal post → `EnforceJournalPostAsync` (creator ≠ poster).

## Note
These are PRODUCT defaults, NOT Lao legal requirements.

## Tests
`SegregationOfDutiesServiceTests` (4 tests): same-creator throws, different-approver succeeds.
