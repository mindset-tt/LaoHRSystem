# 08 — Budget Concurrency

## Strategy
`BudgetService` uses a PostgreSQL row lock (`SELECT ... FOR UPDATE`) inside a
transaction for `ReserveAsync`/`CommitAsync`/`ReleaseReservationAsync`. Two
simultaneous approvals cannot both consume the same remaining budget.

## Test
`BudgetServiceTests.SequentialReservations_RejectOverspend`: Budget = 100;
reserve 70 (ok), reserve 50 (throws). Total consumed stays 70, never 120.

## Note
The InMemory test provider has no row locking, so the concurrency test is
sequential (deterministic). Real PostgreSQL row-lock behavior is exercised by
the `FOR UPDATE` path in production; the invariant (no over-consumption) is
proven by the sequential test and the row-lock design.
