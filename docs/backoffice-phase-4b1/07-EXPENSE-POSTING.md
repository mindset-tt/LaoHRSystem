# 07 — Expense Posting

## Endpoint
`POST /api/expenses/{id}/post-to-gl` (CanPostAccounting).

## Reuse
Existing `Expense` domain (no ExpenseV2). `ExpenseCategory.AccountId` (new,
optional) maps a category to a GL expense account; falls back to
`DEFAULT_EXPENSE_ACCOUNT`.

## Posting
Debit expense account, Credit employee payable account (configured).

## Semantics
Employee reimbursement (Expense) is distinct from vendor obligation
(SupplierInvoice); both may feed accounting. Posting is explicit, not automatic
on approval.

## Idempotency
Unique SourceType + SourceId + PostingPurpose ("EXPENSE").

## Tests
`PostingServiceTests.PostExpense_CreatesBalancedJournal`.
