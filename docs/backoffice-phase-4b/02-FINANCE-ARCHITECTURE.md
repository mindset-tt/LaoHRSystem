# 02 — Finance Architecture

## Chain
```
BUDGET → PR → PO → RECEIPT → INVENTORY/ASSET
   → SUPPLIER INVOICE → 3-WAY MATCH → AP → PAYMENT
   → JOURNAL → GENERAL LEDGER → REPORT
```

## Services
- `IFinanceAccessService` — finance/accounting authorization (Admin only).
- `IAccountingService` — journal posting, reversal, account balance, trial balance.
- `IAccountsPayableService` — 3-way match, payment allocation, AP aging.
- `IBudgetService` (extended) — reserve/commit/release + recognize actual.

## Principle
Operational event ≠ accounting entry unless posting semantics are defined.
No automatic journal generation until account mappings exist (fail with a clear
configuration error).

## No duplicate finance domain
Existing Expense/Loan/Budget/CostCenter/Supplier are reused. No ExpenseV2,
FinanceExpense, or second finance application.
