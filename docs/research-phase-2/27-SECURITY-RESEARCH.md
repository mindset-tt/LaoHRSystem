# 27 — Security Research + 28 — Data Privacy + 29 — Database & Scalability + 30 — Performance + 31 — DevOps + 32 — Backup/DR + 33 — Observability + 34 — Test Strategy

> Research date: 2026-08-21. Sources: OWASP (VERIFIED), Microsoft Learn (VERIFIED), PostgreSQL 18 docs (VERIFIED), W3C WCAG 2.2 (VERIFIED).

## 27 — Security research

### Password hashing (VERIFIED — OWASP)

| Current | Weakness | Industry recommendation | Recommended path |
|---|---|---|---|
| SHA-256 (no salt/work factor) | Fast hash → brute-forceable if DB leaks | **Argon2id** (OWASP #1 preference) | Migrate via rehash-on-login: verify SHA-256 → rehash with Argon2id → store with `hash_algo` flag |

**OWASP parameters**: Argon2id m=19456 (19MiB), t=2, p=1 (min).
**.NET library**: `Konscious.Security.Cryptography.Argon2` or `Geralt` (not built into Identity).
**ASP.NET Core Identity `PasswordHasher`**: PBKDF2 HMAC-SHA256, 100,000 iterations (below OWASP's 600,000 — use Argon2id instead).
**Migration**: No lockout — on next login, verify old hash, rehash with Argon2id, update flag. **VERIFIED strategy.**

### Other security

| Topic | Current | Recommendation | Priority |
|---|---|---|---|
| JWT | HS256, 60min, env key | OK; consider RS256 if external validators needed | P3 |
| Refresh tokens | ✅ rotation + replay detection | Good | — |
| MFA | ❌ | TOTP (Google Authenticator) | P2 |
| SSO/OIDC | ❌ | Add if enterprise customers demand | P3 |
| RBAC | 3 hard roles | Add policy-based `[Authorize(Policy=...)]` | P2 |
| Project RBAC | Stored not enforced | Enforce ProjectMember.Role | P1 |
| Rate limiting | Login only | Add global limiter + per-endpoint | P2 |
| CORS | Locked (prod) | Good | — |
| CSP headers | ❌ | Add Content-Security-Policy | P2 |
| Global exception handler | ❌ | `IExceptionHandler` + `AddProblemDetails()` (.NET 8+, VERIFIED) | P1 |
| Secrets | Env vars (cleaned) | Good; rotate historical | P0 |
| File upload | Extension-only | Magic-byte validation | P1 |
| CSRF | N/A (JWT in header) | — | — |

## 28 — Data privacy (Lao PDR)

| Item | Status | Confidence |
|---|---|---|
| Lao data protection law | **UNKNOWN** — no confirmed comprehensive PDPR equivalent found | UNKNOWN |
| Employee personal data | Stored in plaintext (no at-rest encryption) | VERIFIED |
| Salary data | Plaintext | VERIFIED |
| Government IDs (NSSF, Tax) | Plaintext | VERIFIED |
| Medical data (sick leave) | Attachment path stored | VERIFIED |
| Biometric data | ZKTeco fingerprints on device (not in HR DB) — VERIFIED good | VERIFIED |
| Access control | RBAC + default-deny | VERIFIED |
| Retention | `RetentionService` (disabled) | VERIFIED |
| Consent | ❌ | UNKNOWN if required |
| Breach handling | ❌ | UNKNOWN |
| Cross-border transfer | ❌ | UNKNOWN |

**Classification**: Lao data protection law status = **UNKNOWN — requires legal research.** Apply best practices regardless: encrypt PII at rest, mask in non-prod, audit access, retention policy. **P2.**

## 29 — Database & scalability

### PostgreSQL production (VERIFIED — PG 18 docs)

| Topic | Recommendation | Priority | Confidence |
|---|---|---|---|
| Migrations | Generate PG-compatible migrations; replace EnsureCreated | P0 | VERIFIED |
| Backup | WAL archiving + `pg_basebackup` for PITR (pg_dump alone insufficient) | P0 | VERIFIED |
| Audit partitioning | `PARTITION BY RANGE (created_at)` monthly + `pg_partman` | P2 | VERIFIED |
| Connection pooling | Set `Maximum Pool Size` explicitly; consider PgBouncer for multi-instance | P2 | MEDIUM |
| Pagination | Cursor/keyset for large tables (offset is O(n)) | P1 | HIGH |
| pg_trgm | GIN index for fuzzy search (test Lao quality) | P2 | VERIFIED/MEDIUM |
| ICU collation | `lo_LA` for Lao sorting | P2 | HIGH |
| JSONB | Already used (audit changes) | — | — |
| Read replicas | Only at 50k+ scale | P3 | — |
| Materialized views | Monthly summaries for dashboards | P3 | — |

### Scale profiles

| Scale | Employees | Architecture change needed? |
|---|---|---|
| 50 | ~50 | No — current architecture sufficient |
| 500 | ~500 | No — add pagination (done) + indexes (done) |
| 5,000 | ~5k | Cursor pagination + AsNoTracking projections + audit partitioning + background export queue |
| 50,000 | ~50k | Read replicas + PgBouncer + partitioning + materialized views + dedicated reporting DB |

**Verdict**: Current architecture (single Postgres + single API + single Next.js) is sufficient up to ~5,000 employees with the P1/P2 improvements. No Kubernetes/microservices needed. **Keep lightweight.**

## 30 — Performance targets (proposed — label as proposals)

| Metric | Target | Notes |
|---|---|---|
| Page interaction (INP) | < 200ms | |
| API p95 (auth) | < 200ms | |
| API p95 (list 1k rows, paged) | < 300ms | |
| Dashboard query | < 500ms | |
| Search response | < 500ms | |
| Payroll batch (1k employees) | < 30s | |
| Report generation (PDF) | < 10s | |
| Memory per API container | < 512MB | |
| Memory per Next.js node | < 400MB | |
| DB connections steady | < 30 | |

> These are proposals, not committed SLAs. Validate against real workloads.

## 31 — DevOps

| Option | LaoHR fit | Confidence |
|---|---|---|
| Docker Compose (current) | ✅ Best for 50–500 employees, single-server | VERIFIED |
| Single VM | ✅ Simplest on-prem | HIGH |
| Kubernetes | ❌ Overkill until 50k+ or multi-region | HIGH |
| Managed containers | ⚠️ Only if cloud-hosted | MEDIUM |
| On-premises | ✅ Likely Lao market reality (data sovereignty) | MEDIUM |

**Verdict**: Docker Compose + single VM is the right deployment for LaoHR's target scale. Add reverse proxy (Caddy — auto-TLS, simplest) + backups. Do NOT adopt Kubernetes. **Keep lightweight.**

## 32 — Backup & DR (VERIFIED — PG 18 docs)

| Item | Recommendation | Priority |
|---|---|---|
| WAL archiving | Enable `archive_mode=on` + `archive_command` | P0 |
| Base backup | `pg_basebackup` weekly | P0 |
| PITR | Restore to any point in time | P0 |
| pg_dump | Periodic logical backup (DR + version migration) | P1 |
| Restore testing | Monthly restore test | P1 |
| RPO | 1 hour (WAL archive frequency) | P0 |
| RTO | 4 hours (restore from backup) | P0 |
| Encryption | Encrypt backups at rest | P1 |
| Off-site | Copy backups to separate location | P1 |
| Document backups | `wwwroot/uploads` — include in backup plan | P1 |

## 33 — Observability

| Current | Gap | Recommendation | Priority |
|---|---|---|---|
| Serilog JSON console | — | Good | — |
| Optional file sink | — | Good | — |
| OpenTelemetry traces | OTLP optional | Configure collector (Tempo/Jaeger) in prod | P2 |
| Health checks | ✅ live + ready | Good | — |
| Metrics | ❌ | Add Prometheus exporter (OTel metrics) | P3 |
| Dashboards | ❌ | Grafana (Loki for logs + Tempo for traces + Prometheus for metrics) | P3 |
| Alerts | ❌ | Alert on health check failures, error rate spikes | P2 |
| PII redaction | ⚠️ | Serilog destructuring + filter salary/ID fields | P1 |
| Correlation IDs | ⚠️ | Add `TraceId` to all logs (OTel does this) | P2 |

**Verdict**: Current observability is a good baseline. P1 — PII redaction + alert on health. P2 — OTLP collector + correlation IDs. P3 — full Grafana stack. Do NOT over-instrument for 50–500 employee scale.

## 34 — Test strategy

### Ideal testing pyramid

| Layer | Framework | LaoHR current | Priority |
|---|---|---|---|
| Unit (domain logic) | xUnit | ✅ (partial) | P1 (PayrollService) |
| Repository | xUnit + TestableDbContext | ❌ | P2 |
| API integration | xUnit + Mvc.Testing + postgres | ✅ (HR core only) | P0 (PM/Finance/Knowledge) |
| Database | xUnit + postgres test container | ✅ (CI service) | P1 |
| Contract | — | ❌ | P3 |
| Frontend component | Vitest + Testing Library | ❌ | P1 |
| E2E | Playwright | ❌ | P2 |
| Accessibility | axe-core | ❌ | P2 |
| Visual regression | — | ❌ | P3 |
| Security | OWASP ZAP / manual | ❌ | P2 |
| Performance | k6 / NBomber | ❌ | P3 |
| Migration | xUnit + test DB | ❌ | P1 (when migrations added) |
| Backup/restore | Manual/scripted | ❌ | P1 |

### Lao-specific test data

| Data type | Recommendation | Priority |
|---|---|---|
| Lao names (first/last) | Generate with Bogus + Lao name pool | P1 |
| Lao addresses (province/district/village) | Use real admin structure (not random) | P1 |
| LAK salaries | Realistic ranges (min wage 2.5M → executive 50M+) | P1 |
| Lao phone numbers | +856 20XX XXXX | P1 |
| Lao holidays | Annual official list | P1 |
| Mixed Lao/Latin names | Test sorting + search | P1 |
| Foreign employees | Test visa/work permit fields | P2 |