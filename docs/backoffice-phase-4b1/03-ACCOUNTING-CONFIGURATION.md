# 03 — Accounting Configuration

## Service
`AccountingConfigurationService` — mandatory account mappings stored as
SystemSettings (key/value). Auto-posting must not guess GL accounts.

## Required mappings
- `AP_CONTROL_ACCOUNT`
- `DEFAULT_EXPENSE_ACCOUNT`
- `CASH_ACCOUNT`
- `EMPLOYEE_PAYABLE_ACCOUNT`

## Setup status
`ACCOUNTING_CONFIGURED` is true only when all mandatory mappings exist.

## Fail-closed
If a mapping is missing, posting throws a clear configuration error; no
incomplete journal is created.

## No hardcoded account IDs
No `AccountId = 1001` in services. All mappings are configurable.

## UI
`/finance/settings` shows configured/missing status (no fake tax values).
