# 09 — Procurement Integration

## Flow (one source of truth)
```
PR (DRAFT → PENDING_APPROVAL → APPROVED)
  → budget reservation (if BudgetId set)
  → PO (DRAFT → SENT)
  → Goods Receipt (DRAFT → POSTED)
  → Stock Movement (RECEIPT) for stocked items
  → Asset auto-generation for ASSET items
```

## Budget enforcement
- PR approval reserves against `BudgetId`.
- PO creation commits (converts reservation → commitment), inheriting the PR's
  budget if not overridden.

## Tests
`ProcurementFlowTests` (3 tests): partial receipt → full receipt (PO status +
stock total 10), receipt idempotency (double post rejected, stock stays 5),
asset auto-generation (3 laptops → 3 assets).
