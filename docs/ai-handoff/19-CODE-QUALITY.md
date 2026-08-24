# 19 — Code Quality

> `VERIFIED` from grep searches + subagent exploration.

## Conventions observed

### Backend
- **Layered**: Controllers (thin) → Services (complex domains) → DbContext → PostgreSQL. Simple CRUD skips services.
- **Entity single file**: `LaoHR.Shared/Entities.cs` holds all 40 entity classes (~1800+ lines). DbContext in separate file.
- **DTOs**: mostly entities returned directly or anonymous projections — no dedicated DTO library. `INFERRED`.
- **Services**: interface-backed where complex (`ILeaveService`, `IRefreshTokenService`, `IBankTransferService`, etc.); concrete for PDF/payroll (`PayrollService`, `PayslipPdfService`).
- **DI lifetimes**: Scoped for DbContext + most services; Singleton for `LicenseKeyCache`, `AuditLogChannel`; Hosted for background services.
- **Audit**: EF interceptor + channel + background writer (fire-and-forget).
- **Validation**: FluentValidation registered but only `LoginRequestValidator` exists; inline validation elsewhere.
- **Naming**: PascalCase, `XxxController`, `XxxService`, `XxxId` PKs.
- **Async**: `Async` suffix on async methods.

### Frontend
- **App Router** with `(dashboard)` route group.
- **Endpoint modules**: typed wrappers in `lib/endpoints/` per domain.
- **Components**: CSS Modules (`*.module.css`), hand-built primitives in `components/ui/`.
- **Forms**: react-hook-form + zod via `<Form>` primitive (except `EmployeeForm` — manual useState).
- **State**: React Context (Auth, Theme, Language, Toast) — no external state lib.
- **i18n**: custom dictionary in `lib/i18n.ts` (`en`/`lo`); some inline Lao strings (inconsistent).
- **All pages `'use client'`** — no server components doing data fetching.

## TODO / FIXME / placeholder signals

`VERIFIED` via grep:
- `Backend/LaoHR.API/Services/PdfFormService.cs:103` — `// TODO: User to adjust positions (X, Y)` (LSSO form fill).
- `Backend/LaoHR.API/Controllers/IssuesController.cs:273` + `ProjectTasksController.cs:278` — `Status = request.Status ?? "TODO"` (this is a status value, not a TODO marker).
- `Backend/LaoHR.Tests/Integration/Api/GeneralApiTests.cs:89` — comment `// Maybe 404 if not implemented, or 200.`
- `Backend/LaoHR.API/Data/DbSeeder.cs:430-431` — seed tasks with `Status = "TODO"` (status value).

**No `FIXME`, `HACK`, `XXX`, `TEMP`, `coming soon`, `WIP` markers found in backend code.** `VERIFIED`.

## Frontend mock/placeholder signals

`VERIFIED` via grep:
- `employees/[id]/edit/page.tsx:38` — `// Fallback to mock data for demo`.
- `employees/[id]/page.tsx:59` — `// Mock upload` (documents).
- `(dashboard)/page.tsx:130-141` — dashboard activity feed uses mock entries (`t.dashboardPage.activity.mock.*`).

## Dead / unused code

| Item | Confidence | Evidence |
|---|---|---|
| `LaoHR.Shared/Class1.cs` | HIGH | Empty placeholder class |
| `LaoHR.Tests/UnitTest1.cs` | HIGH | Coverage booster placeholder |
| `frontend/public/*` default SVGs | HIGH | Not app-specific (logo is inline SVG) |
| `frontend/src/app/(dashboard)/finance/page.module.css` | MEDIUM | Orphaned CSS (no `finance/page.tsx`) |
| `LaoHR.API/Database/AddressinLao/` SQL | MEDIUM | SQL Server syntax — fails on PostgreSQL (gracefully skipped) |
| `LaoHR.API/Database/LaoHR_Schema.sql` | MEDIUM | Reference schema (not executed at runtime) |
| Bridge.Service / Bridge.Config SqlServer refs | HIGH | Not migrated to Npgsql; may be unused if Bridge apps not run |

## Code quality observations

- **`LeaveController` is 541 LOC** bundling requests, balance, calendar, policies, export, admin — should be split. `VERIFIED`.
- **`EmployeesController` has ambiguous routes** — `GetEmployee` and `GetDepartments` both use `[HttpGet("{id}")]`. `VERIFIED` (potential routing bug).
- **`DashboardController` returns 4 hard-coded scalars** — not extensible. `VERIFIED`.
- **No global exception handler** middleware confirmed. `INFERRED`.
- **`PasswordHasher` uses SHA-256** — not a password-hashing KDF. `VERIFIED`.
- **No API versioning**. `VERIFIED`.
- **Inline Lao strings** in `settings/page.tsx` bypass i18n dictionary. `VERIFIED`.
- **`proxy.ts`** named `proxy` not `middleware`; redirect logic commented out. `VERIFIED`.
- **No Roslyn analyzers** configured in backend. `INFERRED`.
- **Frontend lint**: `eslint-config-next` configured; `npm run lint` exists but not in CI. `VERIFIED`.