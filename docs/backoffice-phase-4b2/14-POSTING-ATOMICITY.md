# 14 — POSTING ATOMICITY

Posting atomicity and fail-closed regression.

## Fail-closed

`AccountingConfigurationService` throws when mandatory mappings are missing
(`AP_CONTROL_ACCOUNT`, `DEFAULT_EXPENSE_ACCOUNT`, `CASH_ACCOUNT`,
`EMPLOYEE_PAYABLE_ACCOUNT`) or when no OPEN fiscal period covers the posting date.
`PostingService` propagates these, so posting is rejected with no partial state.

## Atomicity

`PostingService` wraps auto-posting in a transaction and reuses the existing
transaction in `AccountingService`, `NumberSequenceService`, and `BudgetService`
(the nested-transaction bug TX-001 was fixed). On PostgreSQL the outer transaction
rolls back the DRAFT journal on failure; on InMemory the transaction is a no-op so
a DRAFT may remain, but it is never POSTED.

## Tests

- `PostSupplierInvoice_Unconfigured_Throws` — fail-closed, no orphan journal,
  invoice not falsely POSTED.
- `PostSupplierInvoice_Unconfigured_LeavesNoOrphanJournal` — no POSTED journal
  remains after a controlled failure.
- `PostPayment_Overpayment_LeavesInvoiceUnchanged` — rejected overpayment leaves
  `PaidAmount`/`RemainingAmount`/status unchanged.

## Status

PASS.
