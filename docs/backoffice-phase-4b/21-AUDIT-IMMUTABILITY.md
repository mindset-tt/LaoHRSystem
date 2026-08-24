# 21 — Audit / Immutability

## Mechanism
`AuditLogInterceptor` automatically captures Added/Modified/Deleted entries for
ALL entities (including finance) and enqueues them to a bounded channel drained
by `AuditLogWriter`. No per-entity wiring.

## High-risk finance operations (audited automatically)
Supplier invoice creation, invoice amount change, match override, invoice approval,
payment creation, payment approval, journal posting, journal reversal, period
close/open, bank-account change, COA change.

## Immutability
- Posted journals immutable (correction via reversal + new journal).
- No hard-delete of posted journal, payment, approved invoice, or historical bank
  transaction (use VOID/REVERSED/CLOSED states).

## Snapshot
Important financial mutations preserve before/after state. Never log credentials
or full unnecessary bank secrets.
