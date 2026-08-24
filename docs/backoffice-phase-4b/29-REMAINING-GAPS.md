# 29 — Remaining Gaps

## Deferred (Phase 4C)
- Full tax engine, VAT/withholding statutory filing (LEGAL_CONFIRMATION_REQUIRED).
- Bank API / bank reconciliation automation.
- Inventory costing, asset depreciation.
- Full AR customer billing UI (model + APIs done).
- Dedicated finance report endpoints + CSV/Excel export.
- Match override endpoint (reason/user/timestamp).
- Expense → GL auto-posting (needs CostCenterId/ProjectId/DepartmentId on Expense).
- Supplier invoice → journal auto-posting (needs account mappings).
- Payment → journal auto-posting (needs account mappings).
- Segregation-of-duties enforcement (creator ≠ approver) as configurable policy.
- Bank account masked representation for non-Finance users.

## Unchanged production blockers
- PRODUCTION_PAYROLL_READY = NO (6 Lao compliance blockers).
- PRODUCTION_DEPLOYMENT_READY = NO (TLS, frontend 3 HIGH vulns, observability, load test, CD).

## Accounting statutory
ACCOUNTING_STATUTORY_READY = NO (no verified Lao tax/accounting rules).
