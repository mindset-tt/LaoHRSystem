# 15 — BUDGET RECONCILIATION

Budget full-lifecycle reconciliation regression.

## Lifecycle semantics

Each amount resides in exactly ONE state (no double counting):
- Reserved → PR approved
- Committed → PO created (reservation converted)
- Actual → expense/AP posted (commitment moved to actual)

`Available = Approved - Reserved - Committed - Actual`.

## Full-lifecycle test

`FullLifecycle_ReserveCommitActual_ReconcilesCorrectly`:

| Step | Reserved | Committed | Actual | Available |
|---|---|---|---|---|
| Budget = 1000 | 0 | 0 | 0 | 1000 |
| PR = 600 | 600 | 0 | 0 | 400 |
| PO = 600 | 0 | 600 | 0 | 400 |
| Invoice A = 400 | 0 | 200 | 400 | 400 |
| Invoice B = 200 | 0 | 0 | 600 | 400 |

## PO cancellation

`ReleaseCommitmentAsync` releases unused commitment (tested by
`ReleaseCommitment_FreesBudget`).

## Reversal / void

Posted journals are immutable; correction is via reversal + new journal
(`AccountingService.ReverseAsync`). Invoices that already affected accounting are
not erased — they are voided only when unpaid.

## Status

PASS.
