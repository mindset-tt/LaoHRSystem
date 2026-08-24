# 28 — RESTORE DRILL

## Evidence (Phase 4C.1, re-verified)

- Fresh PostgreSQL 16 → apply migrations → seed coherent cross-domain dataset →
  `pg_dump -Fc` → `createdb` → `pg_restore`.
- Relationship verification: Document(2 versions), Contract(renewal history),
  Vehicle(trip odometer), Travel(linked Expense), Visitor(Visit) all resolved.
- Application restore smoke (`Pg16RestoreSmokeTests`) passed against the restored
  DB through the real `LaoHRDbContext`.

## Measured restore duration

Local drill completes in seconds (small synthetic dataset). No contractual RPO/RTO
claimed — business must set formal targets.

## Status

PASS.
