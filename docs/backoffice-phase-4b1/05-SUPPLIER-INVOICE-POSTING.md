# 05 — Supplier Invoice Posting

## Endpoint
`POST /api/supplier-invoices/{id}/post` (CanPostAccounting).

## Posting
Debit expense/account lines (per invoice line, or default expense account),
Credit AP control account. Uses configured mappings (no hardcoded accounts).

## Idempotency
Unique SourceType + SourceId + PostingPurpose ("INVOICE") prevents double-posting.

## State
APPROVED is distinct from POSTED. Approval does not automatically mean posting.

## Budget reconciliation
If the invoice is PO-backed and the PO links to a budget, `RecognizeActualAsync`
moves Committed → Actual.

## Tests
`PostingServiceTests`: balanced journal, double-post throws, unconfigured fails closed.
