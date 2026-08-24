# 19 — Finance Authorization

## Service
`IFinanceAccessService` (Admin/Finance only). HR ≠ Finance; Procurement ≠ Accounting;
Warehouse ≠ Finance.

## Capabilities
FINANCE_VIEW/MANAGE, AP_VIEW/MANAGE/APPROVE, PAYMENT_VIEW/CREATE/APPROVE,
ACCOUNTING_VIEW/POST, COA_MANAGE, PERIOD_MANAGE, AR_VIEW/MANAGE,
BANK_ACCOUNT_VIEW/MANAGE.

## Mapping
All finance/accounting capabilities → Admin only (the platform's Finance role).
HR and Employee have no finance/accounting access.

## Tests
`FinanceAuthorizationTests` (7 tests): HR and Employee cannot list supplier
invoices, payments, bank accounts, accounts, or journals (403 Forbidden).
