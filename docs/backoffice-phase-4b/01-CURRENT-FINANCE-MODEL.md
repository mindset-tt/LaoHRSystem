# 01 — Current Finance Model

## Existing concepts (reused, NOT duplicated)
| Concept | Entity | API | Service | Frontend | Reuse |
|---|---|---|---|---|---|
| Expense | `Expense` | `ExpensesController` | none (controller-direct) | `/finance/expenses` | Reused as-is |
| ExpenseCategory | `ExpenseCategory` | inline in ExpensesController | none | — | Reused |
| EmployeeLoan | `EmployeeLoan` | `EmployeeLoansController` | none | `/finance/loans` | Reused |
| LoanRepayment | `LoanRepayment` | inline | none | — | Reused |
| Budget | `Budget` | `BudgetsController` | `BudgetService` | `/budgets` | Reused + extended |
| CostCenter | `CostCenter` | inline in BudgetsController | none | — | Reused |
| ConversionRate | `ConversionRate` | `ConversionRatesController` | none | `/settings/currency-rates` | Reused |
| Supplier | `Supplier` | `SuppliersController` | none | `/procurement/suppliers` | Reused |

## Missing (added in Phase 4B)
SupplierInvoice, SupplierInvoiceLine, Payment, PaymentAllocation, BankAccount,
Account (COA), FiscalYear, FiscalPeriod, JournalEntry, JournalLine, Customer,
CustomerInvoice, CustomerInvoiceLine, Receipt, ReceiptAllocation.

## Conflicting semantics resolved
- `Expense` (employee reimbursement) vs `SupplierInvoice` (vendor obligation) —
  kept separate; both may feed accounting.
- `Budget.FiscalYear` (int) vs new `FiscalYear` entity — the new entity is the
  canonical fiscal calendar; `Budget.FiscalYear` remains a plain year tag.
