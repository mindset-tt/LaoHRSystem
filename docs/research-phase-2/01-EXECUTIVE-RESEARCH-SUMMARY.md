# 01 — Executive Research Summary

> Phase 2 deep research. 2026-08-21. READ-ONLY — no code modified.

## What LaoHR already does well

- **HR core + payroll + attendance + leave** — production-quality with NSSF, Lao PIT, multi-currency, biometric ZKTeco.
- **Lao localization** — bilingual (en/lo), Asia/Vientiane timezone, LAK currency, Lao admin divisions, Phetsarath font for PDF.
- **PM/Finance/Knowledge layers** — functionally complete (projects, tasks, risks, issues, expenses, loans, articles, announcements, comments).
- **Ops** — Docker, CI, health checks, Serilog, OpenTelemetry, rate limiting, refresh token rotation, fire-and-forget audit, retention.
- **Lightweight architecture** — no UI/chart/state library; hand-built components; modular monolith. This is a strength, not a weakness.

## What is legally/commercially necessary in Lao PDR (VERIFIED)

| Requirement | Value | Confidence |
|---|---|---|
| NSSF employer contribution | 6.0% | VERIFIED |
| NSSF employee contribution | 5.5% | VERIFIED |
| PIT top marginal rate | 25% | VERIFIED |
| Minimum wage | LAK 2,500,000/month (1 Oct 2024) | VERIFIED |
| Timezone | UTC+07:00 (Asia/Vientiane), no DST | VERIFIED |
| Fiscal year | 1 Oct – 30 Sep | VERIFIED |
| LAK currency | Whole-kip convention (att obsolete) | VERIFIED |
| Bilingual | Lao + English | VERIFIED |
| Employer tax withholding | Monthly salary tax at source | HIGH |

## What current implementation may NOT match Lao requirements

| Area | Issue | Confidence | Priority |
|---|---|---|---|
| PIT brackets | Seeded values unverified against current MOF schedule | UNKNOWN | P0 |
| NSSF ceiling | May not be applied (if ceiling exists) | UNKNOWN | P0 |
| Leave day-counts | Seeded LeavePolicy values unverified | MEDIUM | P0 |
| Overtime | Single OvertimePay — no 1.5×/2×/3× day-type differentiation | MEDIUM | P1 |
| Allowances | Single field — no taxable/non-taxable split | UNKNOWN | P1 |
| Deductions | Personal allowance + dependant deductions not modelled | UNKNOWN | P1 |
| Public holidays | Hardcoded; no annual decree import; no lunisolar support | MEDIUM | P2 |
| Foreign employees | No visa/work permit/expiry tracking | MEDIUM | P2 |

> **All payroll tax/NSSF/leave parameters MUST be confirmed by a qualified Lao professional before production use.**

## What is required before production

1. **P0 Security**: Argon2id password hashing (SHA-256 is unsuitable — OWASP VERIFIED), credential rotation, fail-fast JWT key.
2. **P0 Correctness**: Fix ambiguous employee routes, CompanySettings auth, PG migrations (replace EnsureCreated).
3. **P0 Data safety**: PostgreSQL WAL + PITR backups (pg_dump alone insufficient — VERIFIED).
4. **P0 Compliance verification**: Confirm PIT brackets, NSSF ceiling, leave day-counts against Lao law.
5. **P0 Tests**: Integration tests for PM/Finance/Knowledge controllers (zero coverage).
6. **P1 Infra**: Reverse proxy (Caddy) + TLS, CI branch alignment, SMTP config.
7. **P1 UX**: Remove mock data, create /403 page, global exception handler.

## What is important but can wait

- Approval engine (Stateless + Hangfire) — P1.
- ManagerId + department hierarchy + MSS — P1.
- OvertimeEntry + allowance split + deductions — P1.
- Timesheet — P1.
- Charts + role dashboards — P2.
- Onboarding/offboarding + performance — P2.
- Task dependencies + Gantt — P2.
- Foreign employee tracking — P2.
- NFC normalization + ICU collation — P2.
- Mobile nav + PWA — P2.

## What features should NOT be added

| Feature | Reason |
|---|---|
| Kubernetes | Overkill for 50–5k employees |
| Elasticsearch | pg_trgm + PostgreSQL sufficient |
| Redis | MemoryCache sufficient |
| RabbitMQ/Kafka | No messaging need |
| Camunda/Elsa BPM | Stateless library sufficient |
| Microservices | Modular monolith is right |
| Chart library (recharts) | SVG charts sufficient |
| State library (Redux/Zustand) | Context sufficient |
| Native mobile app | PWA first |
| AI/LLM/RAG in production | Lao model quality unvalidated |
| Multi-tenancy SaaS | No customer demand |
| Recruitment/ATS (now) | Major module; no demand yet |
| Critical path / Sprint | Over-engineering |

## What architectural changes are justified

| Change | Priority | Justification |
|---|---|---|
| Argon2id password hashing | P0 | OWASP VERIFIED |
| PG-compatible migrations | P0 | Schema drift risk |
| WAL + PITR backups | P0 | Production data safety |
| Caddy reverse proxy + TLS | P1 | HTTPS in production |
| IExceptionHandler + ProblemDetails | P1 | RFC 7807 errors (VERIFIED .NET 8+) |
| Stateless + Hangfire | P1/P2 | Lightweight approval engine |
| pg_trgm + ICU collation | P2 | Lao search/sort |
| Audit partitioning | P2 | Unbounded growth |

## What architectural changes are unnecessary

- Kubernetes, microservices, Redis, Elasticsearch, BPM engine, message broker — all rejected. Current stack (single Postgres + single API + single Next.js + Docker Compose) is sufficient up to ~5,000 employees. **Keep lightweight.**

## What UX improvements matter most

1. Remove mock data (honesty) — P1.
2. /403 page + proxy redirect — P1.
3. Mobile nav drawer — P2.
4. Error boundaries — P2.
5. WCAG 2.2 AA (focus, target size, accessible auth) — P2.
6. Charts + role dashboards — P2.
7. Command palette — P2.

## What the next implementation phase should contain

**Phase 3A (Correctness & Security)**: Fix employee routes, CompanySettings auth, Argon2id, PG migrations, credential rotation, JWT fail-fast, WAL+PITR backups, PM/Finance/Knowledge tests, verify Lao payroll parameters.

**Phase 3B (Production Readiness)**: Caddy+TLS, CI branch align, exception handler, mock data removal, /403 page, proxy.ts, file upload validation, SMTP, frontend tests.

**Phase 3C (Lao Compliance)**: OvertimeEntry, allowance split, deductions, loan→payroll link, minimum wage validation, PIT/NSSF reports, address seed port, foreign employee fields, NFC + ICU.

**Phase 3D (UX/ESS/MSS)**: ManagerId, dept hierarchy, audit UI, user management, charts, dashboards, mobile nav, error boundaries, accessibility.

**Phase 3E-3J**: Approval engine, onboarding, performance, timesheet, Gantt, reporting, notifications, accounting export — incrementally based on demand.

**Recommended first task**: Fix `EmployeesController` ambiguous `{id}` routes (P0 correctness bug, low risk, unblocks employee work).

See `48-PHASE-3-IMPLEMENTATION-INPUT.md` for the complete specification.