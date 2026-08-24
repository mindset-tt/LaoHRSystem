# 01 — CORPORATE OPERATIONS GAP

Gap matrix for Phase 4C. "Existing" = code already present; "Reusable" = existing
infrastructure to build on; "Missing" = must be created.

| Domain | Existing | Reusable | Missing | Priority |
|---|---|---|---|---|
| Documents | `EmployeeDocument`, `CandidateDocument`, `DocumentsController` (employee-scoped) | `DocumentsController` storage pattern, `AuditLogInterceptor` | Generic corporate DMS (owner-entity polymorphic), versioning, classification, expiry | HIGH |
| Contracts | `Contract` foundation + `ContractsController` | `NumberSequenceService`, `ApprovalService`, `NotificationService`, DMS | Lifecycle states, approval, renewal history, expiry notifications | HIGH |
| Service Desk | `ServiceRequest` + `ServiceRequestCategory` + controller | `EntityComment`, `NotificationService`, `NumberSequenceService` | Assignment history, resolution, comments wiring, analytics | HIGH |
| Facilities | — | `WorkLocation` (distinct semantics), `AddressController` | `Facility` master | MEDIUM |
| Rooms | — | `Facility` | `Room`, `RoomBooking` (concurrency-safe) | MEDIUM |
| Maintenance | — | `Asset`, `Supplier`, `ServiceRequest` | `WorkOrder` | MEDIUM |
| Fleet | — | `Asset` (vehicle-as-asset), `NumberSequenceService` | `Vehicle`, `VehicleBooking`, `VehicleTrip`, `FuelLog` | MEDIUM |
| Travel | — | `Expense`, `ApprovalService`, `NotificationService`, DMS | `TravelRequest`, segments, accommodation, advance | MEDIUM |
| Visitors | — | `Facility`, `NotificationService` | `Visitor`, `Visit` | LOW |
| Admin Ops | — | `ServiceRequest`, `Inventory`, `Assets` | (reuse only) | LOW |
| Calendar/Bookings | — | `RoomBooking`, `VehicleBooking`, `TravelRequest` | Aggregator read model | LOW |

## Key decisions

1. **No duplicate masters** — reuse `Supplier`, `Employee`, `Department`, `Project`,
   `CostCenter`, `Asset`, `Expense`, `WorkLocation`.
2. **DMS is polymorphic** — one `CorporateDocument` with `OwnerEntityType` +
   `OwnerEntityId`, not per-domain document tables.
3. **Vehicles reference `Asset`** (optional) — no duplicate acquisition values.
4. **Travel advance ≠ EmployeeLoan** — separate `TravelAdvance` linked to
   `TravelRequest`, settled against `Expense` (no invented accounting).
5. **Work orders are polymorphic** — `SourceType` + `SourceId` (FACILITY/ROOM/ASSET/VEHICLE/SERVICE_REQUEST).
6. **Booking concurrency** — real PG16 tests for room + vehicle overlap races.
