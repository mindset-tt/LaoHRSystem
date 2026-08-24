# 00 — Baseline (Phase 4B)

Re-baselined 2026-08-24 (not trusting prior summaries blindly).

| Area | Result |
|---|---|
| Backend build | PASS (0 warnings, 0 errors) |
| Backend tests | 173 PASS / 0 FAIL (before Phase 4B additions) |
| Frontend tests | 39 PASS / 0 FAIL (before Phase 4B additions) |
| Frontend typecheck | PASS |
| Frontend build | PASS |
| Frontend lint | 41 errors / 38 warnings (pre-existing) |
| CI/CD | NONE (removed in 4A.1) |
| Canonical DB | PostgreSQL 16 |
| Design-time secret | REMOVED (4A.1) |

## Finance/accounting state (before Phase 4B)
- **Exists**: Expense, ExpenseCategory, EmployeeLoan, LoanRepayment, Budget,
  CostCenter, ConversionRate, Supplier, NumberSequence.
- **Missing**: SupplierInvoice, Payment, BankAccount, Account (COA), JournalEntry,
  JournalLine, FiscalYear, FiscalPeriod, Customer, CustomerInvoice, Receipt.
- `Expense` lacks CostCenterId/ProjectId/DepartmentId (needed for GL posting).
- `IBudgetService` has no "post actual" method (deferred to Phase 4B).
- `IBackOfficeAccessService` finance checks are Admin-only (no finer-grained finance permissions).
