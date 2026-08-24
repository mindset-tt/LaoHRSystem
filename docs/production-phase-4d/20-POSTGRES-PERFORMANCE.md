# 20 — POSTGRES PERFORMANCE

## Index review

Indexes exist for the audited hot paths (added across phases):
- Finance: `Payment(PaymentNumber)`, `Payment(Status,PaymentDate)`, `SupplierInvoice(SupplierId,InvoiceNumber)`, `JournalEntry(SourceType,SourceId,PostingPurpose)` (filtered unique), etc.
- Corporate: `CorporateDocument(OwnerEntityType,OwnerEntityId)`, `CorporateDocument(ExpiryDate)`, `RoomBooking(RoomId,StartAt,EndAt)`, `VehicleBooking(VehicleId,StartAt,EndAt)`, `TravelRequest(EmployeeId)`, `TravelRequest(Status)`, `Visit(HostEmployeeId)`, `Visit(ExpectedAt)`, `WorkOrder(SourceType,SourceId)`, `WorkOrder(Status)`, `Vehicle(RegistrationNumber)` (unique), etc.
- `PerformanceIndexes.Apply` runs provider-agnostic `CREATE INDEX IF NOT EXISTS` on boot.

## Foreign keys

High-use FK columns are indexed (explicitly configured in `OnModelCreating`).

## Connection pool

Npgsql defaults (MaxPoolSize 100). No custom tuning applied (measure before tuning).

## Status

PASS (indexes present for known query patterns; no blind tuning).
