# 02 — PAYMENT RACE (CONC-001)

## Symptom

Two concurrent payments against the same supplier invoice could both read
`RemainingAmount = 100` and both pass the overpayment check, over-allocating the
invoice.

## Root cause

`AccountsPayableService.PostPaymentAsync` read the invoice with a plain
`FirstOrDefaultAsync` (no row lock), so two transactions could both observe the
same remaining amount before either committed.

## Risk

Financial over-allocation: an invoice could be paid more than its total, and
`PaidAmount`/`RemainingAmount` would be corrupted.

## Fix

`LockInvoiceAsync` issues `SELECT ... FOR UPDATE` on PostgreSQL (raw SQL) inside
the payment transaction, serializing concurrent payments on the same invoice.
On non-relational (InMemory) it falls back to a plain read.

## Test

`PaymentRace_TwoConcurrentPayments_DoNotOverpay` — invoice total 100; two
concurrent payments of 80 each. Exactly one succeeds; final state
`PaidAmount == 80`, `RemainingAmount == 20`, status `PARTIALLY_PAID`.

## Regression protection

- Real PG16 test (runs in the harness).
- InMemory `PostPayment_Overpayment_LeavesInvoiceUnchanged` (atomicity).
