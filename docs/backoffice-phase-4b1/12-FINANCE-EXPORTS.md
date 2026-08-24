# 12 — Finance Exports

## Implemented
Trial balance CSV export (UTF-8, finance-gated). Reuses existing export
architecture (ClosedXML available for future Excel).

## Authorization
`CanExportFinance` (Finance/Admin). No export bypasses financial permissions.

## Audit
Sensitive financial bulk export is audited (via `AuditLogInterceptor`).

## Deferred
Excel (.xlsx) export for other finance reports; general ledger export with
running balance.
