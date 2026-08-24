# 39 — CORPORATE EXPORT CLOSURE

Phase 4C.1 closes the previously-PARTIAL export gap.

## Endpoints (`/api/corporate/reports`)

| Report | CSV | Excel |
|---|---|---|
| Contract Register | ✓ | ✓ |
| Document Expiry | ✓ | — |
| Service Request Backlog | ✓ | ✓ |
| Fleet Register | ✓ | ✓ |
| Travel Register | ✓ | ✓ |
| Visitor Log | ✓ | — |

## Reuse

All exports use the shared `IFinanceExportService` (formula-injection safe,
UTF-8 BOM). No duplicate CSV/Excel implementation.

## Authorization + audit

- Server-side `CanViewCorporateReports` (Admin/HR/Finance).
- Sensitive bulk exports (contracts, visitors, travel) write a persisted
  `CORPORATE_EXPORT` audit record.

## Tests

`CorporateAuthorizationTests` — Employee forbidden from exports; HR allowed.

## Status

PASS.
