# 00 — BASELINE

Phase 4B.2 evidence baseline, captured before the evidence work began.

## Test baselines

| Suite | Count | Result |
|---|---|---|
| Backend standard (InMemory) | 209 | PASS |
| Frontend (Vitest) | 48 | PASS |
| Real PG16 concurrency | 4 | PASS (skipped when `LAOHR_TEST_CONNECTION` unset) |

## Environment

- .NET 10 SDK at `$HOME\.dotnet` (10.0.400).
- EF Core 10.0.11 + Npgsql.EntityFrameworkCore.PostgreSQL 10.0.3.
- PostgreSQL 16 (`postgres:16-alpine`) is the canonical production target.
- Next.js 16.1.1 / React 19 / TypeScript 5 / Tailwind v4.
- ClosedXML 0.105.1 for Excel export.

## Finance surface at baseline

- `IFinanceAccessService` — 20 capability methods, all collapsing to `IsFinance()` (Finance OR Admin).
- `IBackOfficeAccessService` — procurement/inventory/assets/contracts (Admin/HR); finance = Admin-only.
- Controllers: Accounts, BankAccounts, Budgets, Expenses, FinanceSettings, FiscalPeriods, Journals, Payments, SupplierInvoices.
- Only one finance export existed: `JournalsController.ExportTrialBalance` (CSV).
- Audit: `AuditLogInterceptor` (fire-and-forget channel) with **no secret redaction**.
- SoD: `SegregationOfDutiesService` (3 self-approval/post policies, conservative defaults enabled).

## Known gaps identified at baseline

1. `FinanceAccessService` capability-shaped but not capability-differentiated.
2. `BudgetsController` used `IBackOfficeAccessService` (Admin-only) — inconsistent with the rest of finance.
3. No audit secret exclusion (bank numbers, SWIFT, password hashes, tokens leaked into audit JSON).
4. No General Ledger endpoint; no AP aging / payment / invoice register exports; no Excel finance export.
5. No test coverage for `FinanceAccessService` role mapping or `ExportTrialBalance`.
