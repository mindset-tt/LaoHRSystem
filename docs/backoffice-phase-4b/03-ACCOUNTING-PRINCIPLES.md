# 03 — Accounting Principles

## Double-entry invariant (ABSOLUTE)
`SUM(DEBIT) == SUM(CREDIT)` for every POSTED journal entry. No tolerance except
explicitly documented decimal rounding.

## Journal line invariant
A line must not have Debit > 0 AND Credit > 0, and not both zero.

## Posted journal immutability
Posted journals cannot be edited/deleted. Correction = reversing journal + new
correct journal.

## Posting account
Journal lines must target accounts with `IsPostingAccount == true`. Header/
summary accounts reject lines.

## Period control
Posting to a CLOSED/LOCKED period is rejected.

## No fake accounting
No fabricated VAT/withholding/tax rates. No fake "Profit". Management reports are
distinct from statutory financial statements (no regulatory certification claimed).
