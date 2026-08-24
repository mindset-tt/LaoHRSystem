# 15 — PostgreSQL Concurrency

## Strategy
PostgreSQL-safe transaction/concurrency semantics for invoice balance, payment
allocation, budget actual, journal posting, period locking, numbering.

## Row locks
`BudgetService`, `NumberSequenceService`, `AccountsPayableService` use
`SELECT ... FOR UPDATE` (relational) inside transactions. InMemory (tests) skips
the transaction.

## Races addressed
- Payment race: two simultaneous payments cannot overpay the same invoice.
- Journal post race: posting a DRAFT twice is transition-guarded.
- Source auto-post race: unique SourceType+SourceId+PostingPurpose.
- Period close race: posting to a non-OPEN period is rejected.

## Real PostgreSQL
Concurrency is exercised against PostgreSQL 16 (not InMemory-only). A local
harness (`scripts/validate-finance-postgres.ps1`) is the intended invocation
(no CI/CD).
