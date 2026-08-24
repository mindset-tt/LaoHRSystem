# 21 — INDEX REVIEW

## Inventory

Indexes are declared in `LaoHRDbContext.OnModelCreating` (and `PerformanceIndexes.Apply`
for provider-agnostic boot-time indexes). Composite indexes match actual
filter/order patterns (e.g. `RoomBooking(RoomId, StartAt, EndAt)` for overlap
queries; `VehicleBooking(VehicleId, StartAt, EndAt)`).

## No index bloat

Indexes are added only for known query patterns (list filters, overlap checks,
FK lookups, unique constraints). No blanket per-field indexing.

## Status

PASS.
