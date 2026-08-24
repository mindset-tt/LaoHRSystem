# 07 — Approval Architecture + 12 — ADR Register + 13 — Source of Truth

> Phase 3C1 workstreams 3C1.9 + ADRs + source-of-truth.

## 07 — Approval architecture

### Model (3 entities)

**`ApprovalRequest`** — a reusable approval request:
- `RequestType` (LEAVE, EXPENSE, LOAN, ATTENDANCE_CORRECTION, OVERTIME)
- `EntityId` (polymorphic reference to the business entity)
- `RequesterEmployeeId`
- `Status` (DRAFT, PENDING, APPROVED, REJECTED, CANCELLED)
- `CurrentStepIndex`

**`ApprovalStep`** — a sequential step:
- `StepOrder`
- `ResolverType` (DIRECT_MANAGER, DEPARTMENT_MANAGER, ROLE, EMPLOYEE)
- `ApproverEmployeeId` (resolved server-side at creation)
- `Status` (PENDING, APPROVED, REJECTED, SKIPPED)

**`ApprovalAction`** — immutable history:
- `ActorEmployeeId`, `Action`, `Comment`, `ActedAt`

### Service (`IApprovalService` / `ApprovalService`)
- `CreateRequestAsync` — resolves approvers server-side, creates sequential steps
- `ApproveAsync` — validates actor is the resolved approver, advances step
- `RejectAsync` — validates actor, sets REJECTED
- `CancelAsync` — requester only

### Approver resolution (server-side, never trusted from client)
- `DIRECT_MANAGER` → requester's `ManagerId`
- `DEPARTMENT_MANAGER` → requester's department's `ManagerEmployeeId`
- `ROLE` → first active employee with the given AppUser role
- `EMPLOYEE` → explicit employee id

### Security
- Approver identity is resolved server-side; the client cannot submit `approvedBy`.
- An employee cannot approve their own request (the resolved approver is checked against the actor).
- Concurrency: transitions are validated atomically (status check + step check in one SaveChanges).

### Pilot
The approval engine is implemented and tested but **NOT yet wired to any existing domain** (Leave/Expense/Loan still use their bespoke approval logic). This is intentional — the pilot migration is deferred to Phase 3C2 to avoid breaking existing behavior.

## 12 — ADR register

### ADR-06: Department hierarchy model
**Decision**: Extend existing `Department` with `ParentDepartmentId` (self-referencing) + `ManagerEmployeeId` + `SortOrder`. No new entity.
**Rationale**: Avoid duplicate entities; hierarchy is a natural extension of the existing flat model.

### ADR-07: Employee manager model
**Decision**: Add `ManagerId` (self-referencing) to `Employee`. `Department.ManagerEmployeeId` (accountable head) is distinct from `Employee.ManagerId` (direct reporting manager).
**Rationale**: These are semantically different concepts; conflating them would break approval routing.

### ADR-08: Position semantics
**Decision**: `Position` is an organizational slot (title + department + job code), distinct from `Employee.JobTitle` (free text). Lightweight — no compensation bands.
**Rationale**: A slot-based position enables headcount/vacancy later without forcing full job architecture now.

### ADR-09: Approval engine architecture
**Decision**: Lightweight database-backed approval (ApprovalRequest/Step/Action) with server-side approver resolution. NOT a BPM engine (no Camunda/Temporal/Elsa).
**Rationale**: Sequential approvals are a small state space; a full BPM engine is overkill. Matches Phase 2 research (Stateless + Hangfire recommendation, but even simpler — plain DB state).

### ADR-10: Single company (no multi-tenant)
**Decision**: Remain single-company. `CompanySetting` singleton preserved. `int` IDs (not GUID) — future multi-company is additive.
**Rationale**: No customer demand for multi-tenant; adding tenant isolation now would add complexity without value.

## 13 — Source of truth

| Concern | Source of truth |
|---|---|
| Department hierarchy | `Department.ParentDepartmentId` (self-referencing FK) |
| Department manager | `Department.ManagerEmployeeId` |
| Employee manager | `Employee.ManagerId` (self-referencing FK) |
| Position assignment | `Employee.PositionId` → `Position` |
| Work location | `Employee.WorkLocationId` → `WorkLocation` |
| Approval state | `ApprovalRequest.Status` + `ApprovalStep.Status` |
| Approval history | `ApprovalAction` (immutable) |
| Cycle prevention | `OrganizationHierarchyService` (single reusable validator) |
| Approver resolution | `ApprovalService.ResolveApproverAsync` (server-side) |