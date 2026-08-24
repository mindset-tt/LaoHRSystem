# 07 — General Ledger

## Derivation
The GL is DERIVED from POSTED `JournalLine` records. No mutable GL balance table
as the sole source of truth.

## Account balance
`AccountingService.GetAccountBalanceAsync` = SUM(debit) − SUM(credit) over POSTED
lines for the account.

## Trial balance
`GetTrialBalanceAsync(fiscalPeriodId)` returns per-account Debit/Credit/Balance
from POSTED lines in the period. Total debits == total credits (double-entry).

## Query
Journals list supports status + fiscalPeriod filters with pagination.
