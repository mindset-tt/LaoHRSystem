# 03 — Modular Architecture

## Target
Modular monolith (NO microservices, NO Kafka/RabbitMQ/Redis cluster/K8s).

```
Frontend (Next.js)
   ↓
ASP.NET Core API (Controllers)
   ↓
Application/Domain Services (ApprovalService, InventoryService, NumberSequenceService, ...)
   ↓
Modules (HR, Finance, Procurement, Inventory, Assets, Projects, Corporate)
   ↓
EF Core (LaoHRDbContext)
   ↓
PostgreSQL 16
```

## Shared platform services (single source of truth)
- Identity / Auth (JWT + refresh)
- Authorization (`BackOfficeAccessService` + `DataScopeService`)
- Approvals (`ApprovalService`)
- Notifications (`NotificationService`)
- Audit (`AuditLogInterceptor` — automatic)
- Documents (existing `EmployeeDocument` infra)
- Numbering (`NumberSequenceService` — new, concurrency-safe)
- Reporting / Analytics (existing)

## Module boundaries
- Controllers do NOT manipulate unrelated module internals.
- New modules live in `LaoHR.Shared/BackOfficeEntities.cs` (entities) and
  `LaoHR.API/Controllers/*` (controllers) + `LaoHR.API/Services/*` (services).
- No isolated mini-apps; one DbContext, one auth, one approval engine.

## Naming
- Product brand: "Lao Back Office" (UI).
- Technical namespace: `LaoHR.*` retained (no mass rename — risk not justified).

## Data model governance (applied)
- decimal for all money (never float/double).
- Stock balance derived from movement ledger (no mutable Item.Quantity).
- PO lines preserve commercial snapshot.
- Asset assignment is append-only history.
- Soft-delete/status instead of physical delete for suppliers/contracts.
