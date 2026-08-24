# 14 — Budget Actual Integration

## Semantics (no double count)
- Reserved → PR approved.
- Committed → PO created.
- Actual → posted invoice/expense.

## Transition (Phase 4B)
`BudgetService.RecognizeActualAsync(amount)` moves the amount from Committed →
Actual (reduces commitment, increases actual), so the amount resides in exactly
one state. `ReleaseCommitmentAsync` releases commitment on PO cancellation.

## Partial invoice
PO 100 → Invoice 40 → remaining commitment 60, actual 40 (per documented semantics).

## Tests
`BudgetServiceTests.RecognizeActual_MovesCommitmentToActual_NoDoubleCount` and
`ReleaseCommitment_FreesBudget`.
