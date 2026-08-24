# 06 — Payment Posting

## Endpoint
`POST /api/payments/{id}/post-to-gl` (CanPostAccounting).

## Posting
Debit AP control account, Credit bank/cash GL account (from the payment's
BankAccount.GLAccountId, or the configured CASH_ACCOUNT).

## Bank account GL mapping
Each BankAccount optionally references a GL account; posting requires it (or the
cash fallback). No hardcoded bank account.

## Idempotency
Unique SourceType + SourceId + PostingPurpose ("PAYMENT") prevents duplicate journals.

## Atomicity
Allocation + remaining validation + journal + paid amount + status are atomic.

## Tests
`AccountsPayableServiceTests` (payment allocation) + `PostingServiceTests`.
