# 22 — Remaining Gaps

## Deferred (Phase 4B)
- Accounts Payable (SupplierInvoice, Payment), Accounts Receivable, General
  Ledger (CoA, Journal, FiscalPeriod), Cash/Bank, Financial statements.
- Asset maintenance, depreciation engine.
- Contract reminders (expiry/renewal notifications).
- Platform DMS (supplier/contract/invoice documents).
- Dedicated Back Office report endpoints + CSV/Excel export.
- Global search, supplier portal, multi-company, OCR/AI/RAG.

## Known follow-ups
- Budget "Actual" is not yet populated (no AP/expense posting in Phase 4A.1).
- Contract expiry reminders not scheduled (no scheduler platform).
- `LaoHRDbContextFactory` still uses a placeholder connection string (no real
  secret, but `database update` requires `--connection` or `LAOHR_EF_CONNECTION_STRING`).

## Unchanged production blockers
- PRODUCTION_PAYROLL_READY = NO (6 Lao compliance blockers).
- PRODUCTION_DEPLOYMENT_READY = NO (TLS, frontend 3 HIGH vulns, observability,
  load test, CD).
