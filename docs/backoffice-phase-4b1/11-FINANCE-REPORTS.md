# 11 — Finance Reports

## Implemented (via list endpoints)
Supplier Invoice Register, Payment Register, Chart of Accounts, Journal list,
Trial Balance, AP Aging.

## Trial balance export
`GET /api/journals/trial-balance/export` (CSV, UTF-8, finance-gated) with
AccountCode, AccountName, AccountType, Debit, Credit, Balance.

## Management vs statutory
These are management reports, NOT statutory financial statements (no regulatory
certification claimed).

## Filtering + pagination
List endpoints support status/date/supplier/account filters and server-side pagination.
