# 25 — PostgreSQL Concurrency

## Strategy
PostgreSQL-safe transaction/concurrency semantics for:
- invoice balance (payment allocation)
- budget actual (row lock)
- journal posting (transition guard)
- period locking
- numbering (row lock)

## Row locks
`BudgetService`, `NumberSequenceService`, and `AccountsPayableService` use
`SELECT ... FOR UPDATE` (relational) inside transactions. InMemory (tests) skips
the transaction (no row lock).

## Races addressed
- Payment race: two simultaneous payments cannot overpay the same invoice
  (transaction + remaining-amount check).
- Journal post race: posting a DRAFT twice is transition-guarded (only DRAFT posts).
- Period close race: posting to a non-OPEN period is rejected.

## Real PostgreSQL
Concurrency tests are exercised against PostgreSQL 16 (not InMemory-only).
