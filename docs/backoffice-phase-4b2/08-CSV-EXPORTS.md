# 08 — CSV EXPORTS

CSV export closure for finance reports, via a shared `IFinanceExportService`.

## Endpoints

| Report | Endpoint |
|---|---|
| Supplier Invoice Register | `GET /api/finance/reports/supplier-invoices/export` |
| AP Aging | `GET /api/finance/reports/ap-aging/export` |
| Payment Register | `GET /api/finance/reports/payments/export` |
| General Ledger | `GET /api/finance/reports/general-ledger/export` |
| Trial Balance | `GET /api/finance/reports/trial-balance/export` |
| Expense Summary | `GET /api/finance/reports/expense-summary/export` |
| Budget Utilization | `GET /api/finance/reports/budget-utilization/export` |

All gated by `CanExportFinance()`.

## Formula-injection protection

`FinanceExportService.SanitizeCell` prefixes a single quote to any cell whose
text begins with `=`, `+`, `-`, `@`, tab, or CR, so it is treated as literal text
rather than a spreadsheet formula.

## UTF-8 / Lao

CSV is emitted as UTF-8 **with BOM** so Lao text (supplier names, account names,
journal descriptions) opens correctly in Excel.

## Tests

`FinanceExportServiceTests`:
- `BuildCsv_PrefixesFormulaInjectionCharacters`
- `BuildCsv_EmitsUtf8Bom_ForLaoText`
- `BuildCsv_EscapesCommasAndQuotes`

## Status

PASS.
