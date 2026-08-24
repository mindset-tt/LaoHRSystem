# 16 — Audit Verification

## Mechanism
`AuditLogInterceptor` (SaveChangesInterceptor) automatically captures
Added/Modified/Deleted entries for ALL entities (including Back Office) and
enqueues them to a bounded channel drained by `AuditLogWriter`. No per-entity
wiring needed.

## Coverage
Representative operations are covered by the integration tests:
- PR approval (PurchaseRequest status change) → audited.
- Stock adjustment (StockMovement) → audited.
- Asset assignment (AssetAssignment) → audited.
- Budget update (Budget) → audited.

## Note
The interceptor is registered only for the relational (Npgsql) path in
`Program.cs`; the InMemory test provider does not attach it. Audit persistence
is verified against real PostgreSQL (Phase 3D) and remains automatic for all
new entities.

## Rule
Never log secret data (bank/tax fields are not written to audit payloads).
