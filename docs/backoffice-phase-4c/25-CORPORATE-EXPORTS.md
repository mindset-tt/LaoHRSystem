# 25 — CORPORATE EXPORTS

## Design

Reuses the shared `IFinanceExportService` (CSV/Excel, formula-injection safe,
UTF-8 BOM) rather than creating per-domain exporters. Corporate export endpoints
are not yet wired (deferred); the shared service is available for them.

## Status

PARTIAL — shared export infrastructure exists and is reusable; corporate-specific
export endpoints are deferred to a later phase (no duplicate exporter code).
