# 00 — Current Platform Baseline (Phase 4A)

## Purpose
Re-baseline of the repository before the Back Office expansion. This documents
what actually exists (verified against source), not what was claimed.

## Verified state (re-baselined 2026-08-24)

| Area | State |
|---|---|
| Backend build | PASS (0 warnings, 0 errors) |
| Backend tests | 148 PASS / 0 FAIL (before Phase 4A additions) |
| Frontend typecheck | PASS |
| Frontend tests | 33 PASS / 0 FAIL |
| Frontend build | PASS |
| Frontend lint | 79 problems (41 errors, 38 warnings) — pre-existing |
| Real PostgreSQL | Native PostgreSQL 18.6 on 127.0.0.1:5432 (Docker daemon NOT running) |
| Migrations | 6 migrations, 84 tables (before Phase 4A) |

## Existing platform capabilities (preserved)
- **Identity/Auth**: JWT + refresh rotation, `AppUser` (roles: Admin, HR, Employee).
- **HR**: Employee, Department (hierarchy), Position, WorkLocation, Attendance, Leave, Payroll (NSSF/PIT), ESS/MSS.
- **Approvals**: `ApprovalService` (polymorphic, server-side approver resolution).
- **Notifications**: `NotificationService` (in-app).
- **Documents**: `EmployeeDocument` + `DocumentsController`.
- **Finance (partial)**: `Expense`, `EmployeeLoan`, `ExpenseCategory`.
- **Projects**: Project, Task, Milestone, Risk, Issue, Resource, Kanban/Gantt/Portfolio.
- **Recruitment/Onboarding**, **Performance/Talent/Learning**.
- **Audit**: `AuditLogInterceptor` (automatic for any entity).
- **Analytics/Reporting**, **Serilog**, **OpenTelemetry**, **Health checks**.

## Key architectural facts
- Single `LaoHRDbContext` (83 DbSets before Phase 4A) in `LaoHR.Shared`.
- All entities in `LaoHR.Shared/Entities.cs` (namespace `LaoHR.Shared.Models`).
- Default-deny authorization via `FallbackPolicy = RequireAuthenticatedUser()`.
- 3 hard roles (string column on `AppUser.Role`); no permission table.
- Numbering is inline per-controller (NOT concurrency-safe) — replaced in Phase 4A.

## What did NOT exist (before Phase 4A)
Supplier, Budget, PurchaseRequest, PurchaseOrder, GoodsReceipt, InventoryItem,
Warehouse, StockMovement, Asset, Contract, ServiceRequest, CostCenter,
ChartOfAccount, JournalEntry, SupplierInvoice, Payment, NumberSequence.
