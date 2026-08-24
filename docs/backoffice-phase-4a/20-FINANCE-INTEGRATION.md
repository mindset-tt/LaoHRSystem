# 20 — Finance Integration

## Existing
`Expense`, `EmployeeLoan`, `ExpenseCategory` (Phase 4). Reused, NOT recreated
(no ExpenseV2).

## Phase 4A additions
- `Budget` + `CostCenter` (lightweight budgeting).
- `NumberSequence` (concurrency-safe numbering for all finance docs).

## Connections
Expense → Department → CostCenter → Project → Budget → approval (where
appropriate). Budget "Actual" currently derives from PAID expenses.

## Deferred (Phase 4B)
- Accounts Payable (SupplierInvoice, Payment).
- Accounts Receivable (Customer, CustomerInvoice, Receipt).
- General Ledger (ChartOfAccount, JournalEntry, JournalLine, FiscalPeriod).
- Cash/Bank foundation.

## Accounting caution
No invented Lao VAT/tax/withholding/accounting standards. Configurable
foundations only. No fake accounting KPIs (Profit/Balance Sheet/Cash Flow) from
incomplete sources.
