# 08 — Budget Actual Reconciliation

## Semantics (no double count)
- Reserved → PR approved.
- Committed → PO created.
- Actual → posted invoice/expense.

## Transition
`BudgetService.RecognizeActualAsync(amount)` moves Committed → Actual (reduces
commitment, increases actual). `ReleaseCommitmentAsync` releases commitment on
PO cancellation.

## Example
Budget 1000 → PR 600 (Reserved 600) → PO (Committed 600) → Invoice 400 →
Committed 200, Actual 400. Available = 1000 - 0 - 200 - 400 = 400.

## Partial + multi-invoice
PO 1000 → Invoice A 400 + Invoice B 600 → Committed 0, Actual 1000 (no double count).

## Invoice void
If an invoice was recognized as actual then voided, correction uses reversal
semantics (not blind decrement).

## Budget ≠ Accounting
Budget management is separate from the General Ledger.

## Tests
`BudgetServiceTests.RecognizeActual_MovesCommitmentToActual_NoDoubleCount` and
`ReleaseCommitment_FreesBudget`.
