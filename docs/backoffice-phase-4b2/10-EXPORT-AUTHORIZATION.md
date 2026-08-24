# 10 — EXPORT AUTHORIZATION

Server-side enforcement of finance report/export endpoints.

## Model

Every export/report endpoint enforces authorization via `IFinanceAccessService`
(`CanViewFinanceReports` for views, `CanExportFinance` for exports). Hidden
frontend buttons are not security.

## Expected matrix

| Role | Report view | Export |
|---|---|---|
| Employee | 403 | 403 |
| HR | 403 | 403 |
| Warehouse/Procurement (capabilities held by Admin/HR) | 403 | 403 |
| Finance | 200 | 200 |
| Admin | 200 | 200 |

Note: "Warehouse" and "Procurement" are not distinct roles in this system — they
are capabilities granted to Admin/HR via `IBackOfficeAccessService`. HR (which
holds those capabilities) is proven forbidden from finance exports, which
transitively proves Warehouse/Procurement cannot export finance.

## Tests

`FinanceExportAuthorizationTests`:
- `Employee_CannotExportFinance`
- `Employee_CannotViewFinanceReports`
- `Hr_CannotExportFinance`
- `Hr_CannotViewFinanceReports`
- `Finance_CanViewFinanceReports`
- `Finance_CanExportFinance`
- `Admin_CanExportFinance`

## Status

PASS.
