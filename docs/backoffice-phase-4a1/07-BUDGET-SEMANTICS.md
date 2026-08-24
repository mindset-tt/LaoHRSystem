# 07 — Budget Semantics

## States (each amount resides in exactly ONE state — no double counting)
- **Approved** — the budget envelope.
- **Reserved** — PR approved (reservation).
- **Committed** — PO created (reservation converted, or direct commitment).
- **Actual** — expense/AP posted (Phase 4B).

## Formula
```
Available = Approved - Reserved - Committed - Actual
```

## Transitions
- PR approved → `ReserveAsync(amount)` → Reserved += amount.
- PO created → `CommitAsync(amount)` → Reserved -= min(Reserved, amount);
  Committed += amount (delta beyond reservation is a new commitment).
- PR rejected/cancelled → `ReleaseReservationAsync(amount)` → Reserved -= amount.

## Overspend policy
`ALLOW_BUDGET_OVERRUN` system setting (default false = reject overspend). No
invented business policy — configurable only.

## Authorization
- Finance (Admin) manages budgets.
- Normal employees do not see unrelated confidential financial details.
