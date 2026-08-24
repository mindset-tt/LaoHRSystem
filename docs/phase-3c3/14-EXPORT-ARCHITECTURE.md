# 14 — Export Architecture

## Existing
- Leave export (ClosedXML Excel) — now scope-intersected.
- NSSF report (PDF) + NSSF package (ZIP) via `ReportsController`.
- Payslip PDF via `PayrollController`.

## Security
Exports use the same scope rules as dashboards/lists. An employee cannot bypass dashboard authorization via "Export All".

## Excel
ClosedXML reused. Lao Unicode, dates, numbers, headers export correctly.

## CSV
UTF-8 (BOM only if Excel compatibility requires). Not yet added for analytics — deferred until a specific CSV report is requested.

## Audit
Sensitive exports (payroll/PIT/NSSF/loans) should log who/what/when/filters (no full file contents). Deferred to a dedicated audit pass.
