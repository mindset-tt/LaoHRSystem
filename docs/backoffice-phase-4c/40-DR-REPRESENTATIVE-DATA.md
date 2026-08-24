# 40 — DR REPRESENTATIVE DATA

Phase 4C.1 strengthens DR evidence with a coherent synthetic dataset.

## Dataset (`scripts/seed-corporate-dr.sql`)

A single coherent chain: Employee → Supplier → CorporateDocument (2 versions) →
Contract (+ renewal history) → ServiceRequest (+ history) → Facility → Room →
RoomBooking → WorkOrder → Asset → Vehicle → VehicleBooking → VehicleTrip →
TravelRequest → linked Expense → Visitor → Visit.

## Backup / restore

- `pg_dump -Fc` → `createdb` → `pg_restore` on PostgreSQL 16.
- Backup: PASS. Restore: PASS.

## Relationship verification (restored DB)

- Document `DR-DOC-001` → 2 versions, CurrentVersion 2.
- Contract `DR-CTR-001` → RENEWAL history (900 → 1000).
- Vehicle `DR-REG-001` → trip odometer 1000 → 1050.
- Travel `DR-TRV-001` → linked Expense `DR-EXP-001`.
- Visitor `DR Visitor` → Visit EXPECTED.

## Status

PASS.
