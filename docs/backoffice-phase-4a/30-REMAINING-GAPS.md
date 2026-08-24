# 30 — Remaining Gaps

## Deferred (not in Phase 4A)
- Full Accounts Payable / Receivable, General Ledger, Financial Statements.
- Asset Maintenance, Depreciation engine.
- Advanced Budget planning (monthly/quarterly allocation, hard budget enforcement).
- Contract reminders (expiry/renewal notifications).
- Platform DMS (supplier/contract/invoice documents).
- Back Office "Command Center" dashboard + role-specific dashboards.
- Back Office reports + KPIs.
- Global search (PostgreSQL-based).
- Supplier portal, Customer billing, Multi-company, Multi-tenant, EDI, Bank integration.
- OCR, AI, RAG, microservices, CI/CD, cloud deployment.

## Known follow-ups
- Budget consumption is not yet a hard block on PR/PO creation.
- Asset auto-generation from Goods Receipt (ItemType=ASSET) is foundation-only.
- `LaoHRDbContextFactory` still has hardcoded `Password=laohr` (design-time only).

## Not production-ready (unchanged from Phase 3D)
- PRODUCTION_PAYROLL_READY = NO (6 Lao compliance blockers).
- PRODUCTION_DEPLOYMENT_READY = NO (TLS, frontend 3 HIGH vulns, observability, load test, CD).
