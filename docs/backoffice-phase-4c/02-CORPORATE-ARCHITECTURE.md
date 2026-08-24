# 02 — CORPORATE ARCHITECTURE

Phase 4C architecture: a modular monolith extending the existing Back Office.

## Principles

- ONE identity, ONE organization structure, ONE employee/supplier master.
- ONE approval engine (`ApprovalService`), ONE notification system, ONE audit trail.
- ONE document foundation (polymorphic `CorporateDocument` + `DocumentVersion`).
- ONE PostgreSQL 16 database. No microservices, no message broker, no Redis, no Kubernetes.
- No duplicate master data; no `V2` entities.

## New entities (Phase 4C)

| Entity | Purpose |
|---|---|
| `CorporateDocument` | Polymorphic corporate document (owner entity type + id) |
| `DocumentVersion` | Immutable file version (monotonic) |
| `ContractHistory` | Append-only contract renewal/amendment/termination |
| `ServiceRequestHistory` | Append-only service request changes |
| `Facility` | Physical operational site |
| `Room` | Bookable space inside a facility |
| `RoomBooking` | Room reservation (concurrency-safe) |
| `WorkOrder` | Polymorphic maintenance work order |
| `Vehicle` | Fleet vehicle (optional Asset link) |
| `VehicleBooking` | Vehicle reservation (concurrency-safe) |
| `VehicleTrip` | Trip with odometer readings |
| `FuelLog` | Optional fuel log |
| `TravelRequest` | Business travel request |
| `TravelSegment` | Travel segment |
| `TravelAccommodation` | Accommodation record |
| `TravelAdvance` | Travel advance (distinct from loan) |
| `Visitor` | Visitor (minimal personal data) |
| `Visit` | Visit (pre-registration → check-in → check-out) |

## Extended existing entities

- `Contract` — added `ProjectId`, `CostCenterId`, `AutoRenew`, `NoticePeriodDays`.
- `Expense` — added `TravelRequestId` (travel expense integration).

## Services

- `ICorporateOperationsAccessService` — capability-based authorization.
- `IDocumentService` — versioning.
- `IBookingService` — room/vehicle booking with overlap + concurrency safety.
- `IContractLifecycleService` — renewal/termination with history.
- `IFleetService` — trips + odometer invariants.

## Controllers

`CorporateDocumentsController`, `FacilitiesController`, `FleetController`,
`TravelController`, `VisitorsController`, `WorkOrdersController`; extended
`ContractsController` and `ServiceRequestsController`.
