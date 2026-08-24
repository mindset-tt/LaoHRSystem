# 29 — Last Known Good State

> Checkpoint for the next AI. `VERIFIED` from code inspection + repo memory.

## Working functionality

The following are confirmed working (run locally per repo memory; CI builds pass `INFERRED`):

### Backend (`.NET 10` API on `http://localhost:5000`)
- Auth: login (admin/admin123, hr/hr123, employee/emp123 in Dev/Testing), JWT + refresh rotation, logout, `/me`.
- Employees: CRUD, photo, list (paged).
- Attendance: clock in/out, list, today, calendar.
- Leave: request/approve/reject, balance, accrual (job), calendar, export.
- Payroll: periods, run, slips, approve, paid, PDF, Excel, NSSF, bank transfers.
- Projects/Tasks/Risks/Issues/Resources/Comments: full CRUD.
- Expenses/Loans: full CRUD + workflows + repayments.
- Announcements/Knowledge: full CRUD + read-tracking.
- Settings: company, work schedule, holidays, leave policies, conversion rates.
- License: status + activate (HTTP 402 enforcement).
- Health: `/health/live`, `/health/ready`.
- Audit: fire-and-forget interceptor.
- Background: leave accrual job (retention disabled by default).

### Frontend (Next.js on `http://localhost:3000`)
- Login + dashboard + all route groups render.
- Theme (light/dark/system), language (en/lo), Toast.
- DataTable + Pagination + Form (react-hook-form+zod).
- apiClient auto-refresh.

### Infrastructure
- Docker compose (postgres + api + web) with healthchecks — requires env vars (`POSTGRES_PASSWORD`, `JWT_KEY`, `CORS_ALLOWED_ORIGINS`, `NEXT_PUBLIC_API_URL`).
- CI (GitHub Actions) — backend build+test, frontend typecheck+i18n+build, docker smoke.

## Relevant modules

- `Backend/LaoHR.API/Program.cs` — bootstrap (single source of truth for pipeline).
- `Backend/LaoHR.Shared/Entities.cs` — 40 entities.
- `Backend/LaoHR.Shared/Data/LaoHRDbContext.cs` — DbSets + seed.
- `Backend/LaoHR.API/Data/PerformanceIndexes.cs` — PG indexes.
- `frontend/src/lib/apiClient.ts` — HTTP + token.
- `frontend/src/lib/i18n.ts` — strings.
- `frontend/src/lib/permissions.ts` — RBAC.

## Tests

- xUnit integration tests for HR-core controllers (Auth, Employees, Attendance, Leave, Payroll, Reports, Holidays, Settings, Documents, License, BankTransfer).
- No tests for PM/Finance/Knowledge controllers or frontend.
- CI runs tests against postgres service container.

## Architectural constraints

- Default-deny auth; JWT HS256; refresh rotation.
- Npgsql + legacy timestamp switch (must not remove).
- EnsureCreated/Migrate fallback (migrations are SQL Server).
- Fire-and-forget audit (channel + writer).
- License middleware before auth.
- PaginatedResponse<T> envelope.
- No UI/chart/state library (lightweight).

## Known blockers (non-blocking for dev, blocking for prod)

- Ambiguous employee routes (BR-01).
- CompanySettings GET anonymous (BR-02).
- Prior credentials must be rotated (BR-03).
- SHA-256 password hashing (BR-04).
- No reverse proxy/TLS/backups (BR-19/20).
- CI branch mismatch (BR-21).
- Mock data in frontend (BR-06/07/08).
- No `/403` page (BR-10).
- proxy.ts disabled (BR-09).

## Unfinished work

- Uncommitted Finance/Knowledge/Docker/CI/Retention/RefreshToken work (git status shows `??` + `M` files).
- PM/Finance/Knowledge layers need tests.
- Project roles not enforced.
- Frontend mock data cleanup.
- Production hardening (TLS, backups, credential rotation, password hashing).