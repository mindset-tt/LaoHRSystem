# 31 — AI Project Memory

> High-density memory document. Read this first. `VERIFIED` from code inspection 2026-08-21.

## Project
**LaoHR System** — enterprise HR + Payroll + Attendance + lightweight PM/work-management for Lao organizations. Monorepo: .NET 10 backend + Next.js 16 frontend + ZKTeco bridge apps + license generator. Branch `master`.

## Purpose
Single system for Lao-compliant HR/payroll (NSSF, Lao PIT, LAK/USD/THB/CNY, Asia/Vientiane UTC+7) + biometric attendance + project/task/risk/issue tracking + expenses/loans + knowledge/announcements. Bilingual (Lao/English). License-gated.

## Architecture
Layered modular monolith (backend) + feature-based App Router SPA (frontend).
- Backend: Controllers → Services → EF Core `LaoHRDbContext` → PostgreSQL (Npgsql). Cross-cutting: LicenseMiddleware (before auth, 402) → Authentication (JWT HS256) → Authorization (default-deny FallbackPolicy) → RateLimiter → Serilog. Audit: `AuditLogInterceptor` → `Channel<AuditLog>` → `AuditLogWriter` (fire-and-forget). Jobs: `LeaveScheduledJobsService` (accrual/carry-over), `RetentionService` (disabled by default).
- Frontend: App Router `(dashboard)` group → endpoint modules → `apiClient` (fetch + auto-refresh) → backend. State: React Context (Auth/Theme/Language/Toast). No UI/chart/state library.

## Stack
- Backend: .NET 10, ASP.NET Core, EF Core 10.0.1 (Npgsql + InMemory), Serilog 9, OpenTelemetry 1.9, HealthChecks.NpgSql 9, FluentValidation 11.3, QuestPDF 2025.12, iText7 7.2.5, ClosedXML 0.105, Swashbuckle 6.4, JWT Bearer.
- Frontend: Next.js 16.1.1, React 19.2.3, TypeScript 5 (strict), Tailwind v4, react-hook-form 7.85, zod 3.25, CSS Modules + CSS vars (no tailwind.config.js).
- DB: PostgreSQL 16. Tests: xUnit 2.9.3 + Moq + FluentAssertions + Bogus + coverlet + Mvc.Testing.
- Infra: Docker (compose: postgres + api:8080 + web:3000), GitHub Actions CI.

## Current state
- **HR core**: production-quality (employees, attendance, payroll, leave, reports, settings).
- **PM layer**: done (projects, tasks, milestones, risks, issues, resources, comments, activity log).
- **Finance layer**: done (expenses, loans + repayments).
- **Knowledge layer**: done (articles, announcements, comments).
- **Ops**: Docker + CI + health + Serilog + OpenTelemetry + rate limiting + refresh tokens + fire-and-forget audit + retention.
- **Maturity**: Beta / Production Candidate (HR core); newer layers need hardening + tests.
- **Builds/runs**: YES locally (backend :5000, frontend :3000, PostgreSQL external).

## Completed work
~150 endpoints across 28 controllers. 40 entities. Pagination, indexes, default-deny auth, refresh rotation, license enforcement, bilingual i18n, light/dark theme, DataTable, Toast, Form (rhwf+zod). Docker + CI.

## Incomplete work
- **Uncommitted**: Finance/Knowledge/Docker/CI/Retention/RefreshToken (git status `??`/`M`) — not yet committed.
- **No tests**: PM/Finance/Knowledge controllers + entire frontend.
- **Mock data**: dashboard activity feed, employee edit fallback, employee documents.
- **Project roles**: stored but not enforced.
- **Bugs**: ambiguous employee routes, CompanySettings GET anonymous, no `/403` page, proxy.ts disabled.

## Critical constraints
- Npgsql `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` MUST stay before CreateBuilder.
- EF migrations are SQL Server-flavored; runtime uses Npgsql; `Migrate()` falls back to `EnsureCreated()`.
- `PerformanceIndexes.cs` applies raw PG indexes on startup (compensates for migration mismatch).
- License: `public.key` file in API working dir; `SystemSettings` LICENSE_KEY; HTTP 402 if invalid.
- Lao compliance: NSSF + progressive PIT + LAK + UTC+7 — do not alter payroll formulas without confirming compliance.
- Default-deny auth: `FallbackPolicy = RequireAuthenticatedUser()`.
- `PaginatedResponse<T>` envelope: `{ data, total, page, pageSize }`.
- No UI/chart/state/axios library (deliberate lightweight).
- Demo users (admin/admin123, hr/hr123, employee/emp123) seeded Dev/Testing only.

## Current blockers
- Production: no TLS/reverse proxy, no Postgres backups, prior credentials must be rotated, SHA-256 password hashing.
- Development: ambiguous employee routes, no FE tests, PM/Finance/Knowledge test gap, mock data.

## Next priority
1. Commit uncommitted Finance/Knowledge/Docker/CI/Retention/RefreshToken work.
2. Fix P0 bugs (employee routes, CompanySettings auth, `/403` page, mock data removal).
3. Add tests for PM/Finance/Knowledge controllers + frontend.
4. Production hardening (TLS, backups, credential rotation, password hashing, CI branch align).

## Development conventions
- Backend: Controllers (thin) → Services (complex) → DbContext. Entities in one file (`Entities.cs`). Interface-backed services (`IXxxService`) + concrete (`PayrollService` etc.). Scoped DI (Singleton: LicenseKeyCache, AuditLogChannel; Hosted: jobs). PascalCase, `XxxId` PKs, `Async` suffix.
- Frontend: App Router `(dashboard)` group. Endpoint modules in `lib/endpoints/`. CSS Modules + CSS vars. react-hook-form + zod via `<Form>` primitive (except `EmployeeForm` — manual useState). All pages `'use client'`. i18n dictionary in `lib/i18n.ts`.
- Testing: xUnit + Mvc.Testing (InMemory or postgres). Integration tests per controller.

## Testing rules
- CI: backend `dotnet test` (postgres service); frontend `tsc --noEmit` + i18n parity + standalone build; docker smoke.
- No coverage threshold gate.
- Add tests for any new controller/feature.

## Do-not-break rules
See `28-DO-NOT-BREAK.md`. Key: API routes, PaginatedResponse envelope, JWT model, refresh rotation, default-deny auth, license enforcement, EF entity model, audit pipeline, Npgsql legacy timestamp switch, payroll formulas, payslip PDF format, bank transfer formats, i18n structure, apiClient localStorage keys, CSS var tokens, EntityComment entity types, Docker compose services, CI env conventions.