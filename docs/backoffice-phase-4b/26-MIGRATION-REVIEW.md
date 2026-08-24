# 26 — Migration Review

## Migration
`AddFinanceAccountingFoundation` — purely ADDITIVE (0 AlterColumn, 0 DropColumn,
15 CreateTable, 40 CreateIndex; DropTable only in `Down()`).

## New tables (15)
Accounts, FiscalYears, FiscalPeriods, JournalEntries, JournalLines,
SupplierInvoices, SupplierInvoiceLines, Payments, PaymentAllocations, BankAccounts,
Customers, CustomerInvoices, CustomerInvoiceLines, Receipts, ReceiptAllocations.

## Discipline
- Append-only history (no rebase).
- `NPGSQL_LEGACY_TIMESTAMP` unset during `migrations add` (0 AlterColumn).
- Financial history is especially sensitive — no destructive operations in `Up()`.

## Validation
Fresh migration on PostgreSQL 16: PASS (118 tables = 103 + 15).
