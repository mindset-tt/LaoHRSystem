# 14 — Back Office Reports

## Status
PARTIAL. The Command Center (KPIs) is implemented. Curated tabular reports
(Supplier Directory, PR/PO Status, Goods Receipt, Stock Balance/Movement, Asset
Register/Assignment, Budget Utilization, Contract Register, Service Request
Summary) are NOT yet implemented as dedicated report endpoints — they are
available via the existing list endpoints (each already supports pagination,
filter, sort, search).

## Export
CSV/Excel via existing export architecture (deferred). No new PDF engine.

## Export security
Authorization applies server-side; a user cannot obtain forbidden
supplier/budget data via export (list endpoints are already role-gated).
