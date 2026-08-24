# 23 — TRANSACTION / LOCK REVIEW

## Findings

- Nested-transaction bug (TX-001) fixed in Phase 4B.2: inner services reuse the
  existing transaction (`alreadyInTransaction` check).
- Row locks (`SELECT ... FOR UPDATE`) are narrow and short-lived: budget, payment
  (invoice), journal, room, vehicle, number sequence.
- Lock order is consistent (single-row locks; no multi-row lock ordering that
  would deadlock).
- No transaction wraps external work (HTTP/SMTP) — transactions are DB-only.

## Deadlock handling

No blind retry of non-idempotent operations. Concurrency is proven on PostgreSQL 16
(finance + room + vehicle races).

## Status

PASS.
