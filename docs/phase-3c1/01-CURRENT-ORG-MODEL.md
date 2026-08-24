# 01 — Current Organization Model + 02 — Target Organization Model

> Phase 3C1 workstreams 3C1.1/3C1.2.

## 01 — Current organization model (audit)

| Entity | Existing fields | Relationships | Reuse/Extend |
|---|---|---|---|
| `Department` | DepartmentId, DepartmentName, DepartmentNameEn, DepartmentCode, IsActive, CreatedAt | → Employee (1:N) | **EXTEND** (add hierarchy + manager) |
| `Employee` | EmployeeId, EmployeeCode, LaoName, EnglishName, ..., DepartmentId, JobTitle (free text), HireDate, BaseSalary, ... | → Department (N:1) | **EXTEND** (add ManagerId, PositionId, WorkLocationId) |
| `Position` | **DOES NOT EXIST** | — | **CREATE** (new entity) |
| `WorkLocation`/`Branch` | **DOES NOT EXIST** | — | **CREATE** (new entity) |
| `CompanySetting` | Singleton (company info) | — | Reuse (single-company) |
| `AppUser` | UserId, Username, Role, EmployeeId? | → Employee (0..1) | Reuse |

### Key findings
- `Department` is **flat** (no `ParentDepartmentId`, no `ManagerEmployeeId`).
- `Employee.JobTitle` is **free text** (not a Position entity).
- **No** `ManagerId` on Employee — no reporting lines.
- **No** Position or WorkLocation entities.
- `CompanySetting` is a singleton → single-company model (no multi-tenant).

## 02 — Target organization model

### Decision (ADR): Single company, multi-branch, hierarchical departments

LaoHR remains **single-company** (no multi-tenant SaaS). The `CompanySetting` singleton is preserved. Added:
- **Department hierarchy** (`ParentDepartmentId`, self-referencing)
- **Department manager** (`ManagerEmployeeId`, accountable head)
- **Employee manager** (`ManagerId`, direct reporting line)
- **Position** (organizational slot, distinct from free-text JobTitle)
- **WorkLocation** (branch/location with Lao address)

### Conceptual structure

```
Company (CompanySetting singleton)
    ↓
WorkLocation (branch/location)
    ↓
Department (hierarchical)
    ↓
Position (slot)
    ↓
Employee (with ManagerId reporting line)
```

### Multi-company decision
- **Single company** for now (matches `CompanySetting` singleton).
- IDs are `int` (not GUID) — future multi-company would require adding a `CompanyId` column, which is additive and non-blocking.
- No tenant isolation added now (deferred until customer demand).

### Relationship model

```
AppUser 0..1 → Employee (optional link)
Employee 0..1 → AppUser (optional)
Employee N:1 → Department
Employee N:1 → Manager (self-referencing, ManagerId)
Employee N:1 → Position
Employee N:1 → WorkLocation
Department N:1 → ParentDepartment (self-referencing)
Department N:1 → Manager (ManagerEmployeeId)
Position N:1 → Department
WorkLocation N:1 → Province/District/Village
```