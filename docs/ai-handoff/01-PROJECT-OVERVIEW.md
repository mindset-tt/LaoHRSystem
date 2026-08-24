# 01 — Project Overview

## Project name

**LaoHR System** (Lao HR System) — repository root `d:\LaoHRSystem`, git branch `master`.

## Project type

`VERIFIED` — Enterprise HR + Payroll + Attendance management system for organizations in Laos, extended with a lightweight Project/Work-management layer (PM/PL). It is a **monorepo** containing a .NET backend, a Next.js frontend, hardware-bridge desktop apps, and a license generator.

## Business objective

`VERIFIED` (from `README.md` + domain code):

- **Target users**: HR departments, payroll officers, project managers, and employees in Lao organizations.
- **Primary workflows**:
  - Employee lifecycle (CRUD, documents, profiles).
  - Attendance via biometric ZKTeco devices + manual clock-in/out with geolocation.
  - Leave requests with approval workflow + accrual + carry-over.
  - Payroll: calculation (NSSF + Lao progressive PIT), multi-currency (LAK/USD/THB/CNY), payslip PDF, Excel export, bank-transfer files (BCEL/LDB), NSSF form fill.
  - Project management: projects, milestones, tasks, Kanban board, risks, issues, resource allocation, activity log, comments.
  - Finance: expense claims (approve/reject/pay), employee loans/advances with repayment tracking.
  - Knowledge base: articles + announcements with read-tracking.
  - Admin: company settings, work schedule, holidays, leave policies, currency conversion rates, audit logs, license management.
- **Business problem**: a single system to manage Lao-compliant HR/payroll plus lightweight project/work tracking, bilingual (Lao/English), with biometric attendance.
- **Important domain concepts**: NSSF (social security), Lao PIT (progressive income tax), LAK (Lao Kip) currency, Asia/Vientiane timezone (UTC+7), ZKTeco fingerprint devices, license-key enforcement, `Asia/Vientiane` date/time throughout.

## Current maturity

**Beta / Production Candidate (HR core)** — `INFERRED` from evidence:

- HR core (employees, attendance, payroll, leave, reports) is feature-complete and appears production-quality: `VERIFIED` (controllers + services fully implemented, payroll engine with NSSF/tax/multi-currency, PDF/Excel export).
- PM/PL layer (projects, tasks, risks, issues, resources) is implemented end-to-end (backend + frontend) but newer and less battle-tested: `VERIFIED` code exists, `INFERRED` maturity.
- Finance layer (expenses, loans) and Knowledge layer (articles, announcements, comments) are implemented end-to-end: `VERIFIED`.
- Infrastructure (Docker, CI, health checks, Serilog, OpenTelemetry, rate limiting, refresh tokens) is in place: `VERIFIED`.
- Remaining blockers to production are operational (migration strategy, no frontend tests, proxy disabled, dashboard mock data) rather than feature gaps — see `23-IMPLEMENTATION-STATUS.md`.

Classification: **Beta** (HR core near production; PM/Finance/Knowledge layers functional but need hardening + test coverage).