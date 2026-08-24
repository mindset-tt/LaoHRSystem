# 09 — EXCEL EXPORTS

Excel (XLSX) export via ClosedXML 0.105.1, through `IFinanceExportService.BuildExcel`.

## Endpoints

| Report | Endpoint | Format |
|---|---|---|
| General Ledger | `GET /api/finance/reports/general-ledger/export?format=xlsx` | XLSX |
| Trial Balance | `GET /api/finance/reports/trial-balance/export?format=xlsx` | XLSX |

(CSV remains the default; `format=xlsx` selects Excel.)

## Quality

- Bold header row.
- `AdjustToContents()` column widths.
- `FreezeRows(1)` header freeze.
- Decimal cells formatted `#,##0.00`; date cells `yyyy-mm-dd`.

## No formula dependency

Accounting values are computed server-side; exports carry authoritative values
and never rely on spreadsheet formulas for balances.

## Tests

`FinanceExportServiceTests.BuildExcel_ProducesValidXlsx` (asserts PK ZIP
signature).

## Status

PASS — critical exports (General Ledger, Trial Balance) implemented; AP Aging
Excel is optional and not yet added (documented as PARTIAL for that one report).
