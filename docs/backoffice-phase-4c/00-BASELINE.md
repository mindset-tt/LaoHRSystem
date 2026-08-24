# 00 — BASELINE

Phase 4C (Corporate Operations Expansion) baseline, captured before new work.

## Verified state entering 4C

| Area | Status |
|---|---|
| Finance Accounting Foundation | READY |
| Finance Operational | READY |
| Finance Evidence | READY |
| Backend standard tests | 233 PASS / 0 FAIL |
| Backend repeat | 5 / 5 |
| Real PG16 concurrency | 4 PASS / 0 FAIL |
| Frontend tests | 48 PASS / 0 FAIL |
| Typecheck / build | PASS |
| New lint | 0 |
| PostgreSQL 16 | canonical |
| CI/CD | NONE BY DESIGN |

## Existing reusable infrastructure (ONE system)

- Identity (`AppUser`), Organization (`Department`, `Position`, `WorkLocation`), Employee master.
- Supplier master, CostCenter, Budget.
- Approval engine (`ApprovalService` + `ApprovalRequest/Step/Action`).
- Notification system (`NotificationService`).
- Document infrastructure (`EmployeeDocument`, `CandidateDocument`, `DocumentsController`).
- Audit system (`AuditLogInterceptor` + `AuditLog`).
- `EntityComment` (polymorphic comments, allow-listed entity types).
- `NumberSequenceService` (concurrency-safe numbering).
- `IBackOfficeAccessService` / `IFinanceAccessService` (capability checks).
- PostgreSQL 16 database (single modular monolith).

## Existing corporate-adjacent entities (Phase 4A)

- `Contract` (foundation: number, title, supplier, owner, dates, amount, status, renewal type).
- `ServiceRequest` + `ServiceRequestCategory` (internal request center).
- `Asset` + `AssetAssignment`, `Warehouse`, `InventoryItem`, `StockMovement`.
- `WorkLocation` (employment/org location).

## What is entirely absent (to be built in 4C)

Facilities, Rooms, Room booking, Maintenance/Work orders, Fleet/Vehicles, Vehicle
booking/trips/fuel, Travel management, Visitors, generic corporate DMS (with
versioning), contract lifecycle expansion (approval/renewal/history), service
desk expansion (assignment history/comments/resolution).
