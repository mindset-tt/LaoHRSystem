# 01 — Finance RBAC

## Change
`FinanceAccessService` refactored from Admin-only to capability-based. A new
"Finance" role grants finance/accounting access; "Admin" remains a superset.

## Roles
- Admin → full system + finance access (superset).
- Finance → finance/accounting capabilities only (NOT HR/employee admin).
- HR → no finance access (HR ≠ Finance).
- Employee → no finance access.

## Capabilities
FINANCE_VIEW/MANAGE, AP_VIEW/CREATE/APPROVE, PAYMENT_VIEW/CREATE/APPROVE,
ACCOUNTING_VIEW/POST, COA_VIEW/MANAGE, FISCAL_PERIOD_MANAGE, AR_VIEW/MANAGE,
BANK_VIEW/MANAGE, FINANCE_REPORT_VIEW, FINANCE_EXPORT.

## System Admin ≠ Accountant
Accountants use the "Finance" role; they are not required to be global Admin.

## Tests
`FinanceAuthorizationTests` proves: Finance role can list supplier invoices (200),
Finance role cannot create employees (403), HR/Employee cannot access finance surfaces.
