# 21 — Remaining Gaps

## Deferred (Phase 4C)
- Tax engine, VAT/withholding statutory filing (LEGAL_CONFIRMATION_REQUIRED).
- Bank API / bank reconciliation automation.
- Inventory costing, asset depreciation.
- Full AR customer billing UI.
- Excel (.xlsx) export for other finance reports; general ledger export with running balance.
- Match override endpoint (reason/user/timestamp).
- Finance notifications (invoice/payment approval events).
- Cash/bank balance derived from posted journal activity (not OpeningBalance).

## Unchanged production blockers
- PRODUCTION_PAYROLL_READY = NO (6 Lao compliance blockers).
- PRODUCTION_DEPLOYMENT_READY = NO (TLS, frontend 3 HIGH vulns, observability, load test, CD).

## Accounting statutory
ACCOUNTING_STATUTORY_READY = NO (no verified Lao tax/accounting rules).
