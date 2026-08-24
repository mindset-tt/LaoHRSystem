# 24 — Technical Claim Revalidation + 25 — Security Claim Revalidation + 26 — Scale Workload Research

> Research date: 2026-08-21. Sources: Next.js 16 docs (VERIFIED), Microsoft Learn (VERIFIED), PostgreSQL 18 docs (VERIFIED), OWASP (VERIFIED).

## 24 — Technical claim revalidation

| Claim (Phase 2) | Evidence | Assumptions | When true | When false | Confidence |
|---|---|---|---|---|---|
| Docker Compose sufficient to ~5,000 employees | Single Postgres + single API + single Next.js | Low concurrency, <100 concurrent users, <1M attendance rows/year | 50-500 employees, <50 concurrent | 5,000+ with 300+ concurrent, 12M attendance rows | HIGH (with workload caveats) |
| No Redis required | MemoryCache sufficient | Single instance, low cache invalidation frequency | Single instance | Multi-instance deployment needing shared cache | HIGH |
| No Elasticsearch required | pg_trgm + ILIKE sufficient | <50k employees, Lao search quality acceptable | <10k records per searchable table | Full-text search across millions of docs | HIGH (for current scale) |
| No Kubernetes required | Docker Compose is simpler | Single server, no multi-region | Single-server deployment | Multi-region, auto-scaling, 50k+ | HIGH |
| Stateless + Hangfire right for approvals | Small state space, per-entity | HR approvals are simple state machines | Leave/expense/loan approvals | Complex BPMN workflows with timers | HIGH (VERIFIED) |
| SVG charts sufficient | No chart library needed | Simple charts (line/bar/donut), <100 data points | Dashboards with basic charts | Complex interactive financial charts | HIGH |
| PWA preferable to native | Responsive web + service worker | Employees need mobile attendance/leave, not heavy mobile | ESS (leave request, payslip view, clock-in) | Offline biometric, push notifications, deep hardware integration | HIGH |

### Framework revalidation

| Framework | Version (Phase 2) | Current docs (Phase 2B) | Status |
|---|---|---|---|
| Next.js | 16.1.1 | v16.3.1 docs (2026-08-04) — proxy.ts confirmed correct | VERIFIED |
| React | 19.2.3 | — | Current |
| .NET | 10 | aspnetcore-10.0 docs (updated 2026-07-22) — IExceptionHandler confirmed | VERIFIED |
| EF Core | 10.0.1 | — | Current |
| Npgsql | 10.0.1 | — | Current |
| PostgreSQL | 16 | PG 18 docs (2026-08-13) — WAL/PITR/pg_trgm/partitioning confirmed | VERIFIED |
| OpenTelemetry | 1.9.0 | — | Current |
| Serilog | 9.0.0 | — | Current |
| Tailwind CSS | v4 | — | Current |

## 25 — Security claim revalidation

| Claim (Phase 2) | Evidence | Confidence | Status |
|---|---|---|---|
| Argon2id is OWASP #1 for password hashing | OWASP Password Storage Cheat Sheet (© 2026) — VERIFIED | VERIFIED | CONFIRMED |
| SHA-256 is unsuitable for passwords | OWASP: "Fast hashing algorithms such as SHA-256 are not suitable" | VERIFIED | CONFIRMED |
| Rehash-on-login migration strategy works | OWASP documents layered hash upgrade method | VERIFIED | CONFIRMED |
| IExceptionHandler + ProblemDetails built into .NET 8+ | Microsoft Learn (aspnetcore-10.0, 2026-07-22) — VERIFIED | VERIFIED | CONFIRMED |
| ASP.NET Core Identity PasswordHasher uses PBKDF2 100k iterations | Microsoft Learn (2026-05-15) — below OWASP's 600k recommendation | VERIFIED | CONFIRMED (use Argon2id instead) |
| Refresh token rotation + replay detection is best practice | Industry standard, OWASP-aligned | HIGH | CONFIRMED |
| Default-deny FallbackPolicy is correct | ASP.NET Core best practice | HIGH | CONFIRMED |

### Argon2id library comparison (not prematurely deciding)

| Library | .NET support | License | Maintenance | Notes |
|---|---|---|---|---|
| Konscious.Security.Cryptography.Argon2 | ✅ .NET | MIT | Active | Popular for Argon2 in .NET |
| Geralt | ✅ .NET | MIT | Active | Includes Argon2id + other crypto |
| BCrypt.Net (bcrypt) | ✅ .NET | BSD | Active | bcrypt only (OWASP #3, legacy) |
| ASP.NET Identity PasswordHasher | Built-in | MIT | Microsoft | PBKDF2 only (below OWASP recommendation) |

**Recommendation**: Evaluate Konscious or Geralt for Argon2id. Do NOT use built-in PasswordHasher (PBKDF2 100k is below OWASP). **P0.** Decision deferred to implementation phase.

## 26 — Scale workload research

### Workload models (not employee-count assumptions)

| Profile | Employees | Concurrent users | Clock-in/min | Payroll batch | Dashboard users | Report gen (concurrent) | DB size (est.) |
|---|---|---|---|---|---|---|---|
| Small | 50 | 5 | 5 | 50 slips | 2 | 1 | <1 GB |
| Medium | 500 | 50 | 50 | 500 slips | 10 | 2 | ~5 GB |
| Large | 5,000 | 300 | 100 | 5,000 slips | 20 | 5 | ~50 GB |
| Very Large | 20,000 | 1,000 | 300 | 20,000 slips | 50 | 10 | ~200 GB |

### Architecture adequacy by workload

| Profile | Docker Compose (single server) | Add | When to change |
|---|---|---|---|
| Small (50) | ✅ More than sufficient | Nothing | — |
| Medium (500) | ✅ Sufficient | Cursor pagination + AsNoTracking | — |
| Large (5,000) | ⚠️ Likely sufficient with tuning | Partitioning + export queue + PgBouncer (optional) | If p95 > 500ms or payroll > 60s |
| Very Large (20,000) | ❌ Likely insufficient on single server | Read replica + dedicated reporting DB + multi-instance + PgBouncer | When concurrent users > 500 or DB > 100GB |

### Key workload-dependent thresholds

| Metric | Small | Medium | Large | Very Large |
|---|---|---|---|---|
| Attendance rows/year | 12k | 120k | 1.2M | 4.8M |
| SalarySlips/year | 600 | 6k | 60k | 240k |
| AuditLog rows/year | 50k | 500k | 5M | 20M |
| Payroll run time (target) | <5s | <15s | <60s | <300s |
| API p95 (list, paged) | <100ms | <200ms | <300ms | <500ms |

### Verdict
- **Docker Compose + single PostgreSQL is sufficient up to Large (5,000 employees / 300 concurrent)** with P1/P2 optimizations (cursor pagination, AsNoTracking, audit partitioning, export queue).
- **Very Large (20,000) requires architectural change**: read replica, dedicated reporting DB, multi-instance API + PgBouncer, possibly partitioning by tenant/branch.
- **Kubernetes is NOT justified** for any profile below Very Large. At Very Large, consider managed containers or VM cluster, not necessarily Kubernetes.