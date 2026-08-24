# 06 — Number Sequence

## Service
`NumberSequenceService` (Phase 4A, hardened in 4A.1). DB-backed sequence row per
(prefix, year). Format: `{prefix}-{yyyy}-{seq:000000}`.

## Concurrency safety (4A.1)
The sequence row is locked with `SELECT ... FOR UPDATE` (PostgreSQL) inside a
transaction, so concurrent requests cannot read the same `LastValue` and produce
duplicates. InMemory (tests) has no row lock; the transaction is skipped there.

## Centralized numbering
All Back Office records use `NumberSequenceService` — no controller-specific
`COUNT + 1` logic. Prefixes: `PR`, `PO`, `GRN`, `AST`, `SUP`, `CTR`, `SR`.

## Tests
`NumberSequenceServiceTests` (3 tests): sequential numbers, independent prefixes,
100 concurrent requests → 100 unique numbers (no duplicates).
