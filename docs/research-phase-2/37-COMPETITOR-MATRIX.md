# 37 — Competitor Matrix + 38 — Current System Gap Matrix + 39 — Feature Priority + 40 — Architecture Options

> Research date: 2026-08-21.

## 37 — Competitor gap matrix (full)

See `11-HRIS-GLOBAL-BENCHMARK.md` for the detailed matrix. Summary:

| Domain | LaoHR vs competitors |
|---|---|
| Employee core | Competitive |
| Attendance + biometric | Superior (ZKTeco) |
| Leave | Competitive |
| Payroll (Lao-compliant) | Superior (unique) |
| Recruitment | Gap (P3) |
| Onboarding | Gap (P2) |
| Performance | Gap (P2) |
| Expenses + Loans | Superior (loans unique) |
| PM layer | Unique (HRIS + PM combined) |
| Knowledge | Unique |
| Mobile | Gap (responsive only, P2 PWA) |
| Workflow | Gap (hardcoded, P1 Stateless) |
| Reporting | Partial (P1-P2) |
| Lao localization | Superior (unique) |

**LaoHR should NOT try to match every competitor feature.** Focus on Lao compliance + HR core + payroll + lightweight PM. Add talent management (recruitment, onboarding, performance) only when customer demand exists.

## 38 — Current system gap matrix (CRITICAL OUTPUT)

| Capability | Current LaoHR | Lao Requirement | Global Best Practice | Gap | Priority | Evidence |
|---|---|---|---|---|---|---|
| PIT brackets | Engine exists, seeded values unverified | Must match current MOF schedule | Configurable tax brackets | Verify seeded values | P0 | `04` |
| NSSF ceiling | NssfBase may be uncapped | Contribution ceiling may apply | Cap NssfBase | Add ceiling | P0 | `05` |
| NSSF rates (5.5%/6%) | ✅ matches | 5.5%/6.0% verified | — | None | — | `05` |
| Minimum wage | Not enforced in payroll | LAK 2,500,000 (Oct 2024) | Validate salary ≥ minimum | Add validation | P1 | `03` |
| Overtime day-type | Single OvertimePay | 1.5×/2×/3× (normal/rest/holiday) | Day-type OT entries | Add OvertimeEntry | P1 | `02` |
| Taxable/non-taxable allowances | Single Allowances | Some allowances may be non-taxable | Split allowances | Add allowance types | P1 | `04` |
| Personal allowance + dependants | Not modelled | Deductions reduce taxable income | Add deductions | P1 | `04` |
| Leave day-counts | Configurable, unverified | Must match Labour Law | Verify seeded LeavePolicy | P0 | `06` |
| Public holidays | Hardcoded + seed-defaults | Annual government decree | Annual import | P2 | `06` |
| Lunisolar holidays | IsRecurring (Gregorian) | Lunar calendar dates | Lunar calc or import | P2 | `06` |
| Foreign employee tracking | ❌ | Work permit/visa/expiry required | Add fields | P2 | `10`, `25` |
| Password hashing | SHA-256 | — | Argon2id (OWASP) | P0 | `27` |
| Global exception handler | ❌ | — | IExceptionHandler + ProblemDetails | P1 | `27` |
| PG migrations | EnsureCreated fallback | — | Migrate() with PG migrations | P0 | `29` |
| Backup/PITR | ❌ | — | WAL + pg_basebackup | P0 | `32` |
| Reverse proxy/TLS | ❌ | — | Caddy (auto-TLS) | P1 | `31` |
| Loan→payroll deduction | Manual | — | Auto-link | P1 | `23` |
| Project RBAC | Stored not enforced | — | Enforce roles | P1 | `27` |
| Frontend tests | ❌ | — | Vitest + RTL | P1 | `34` |
| PM/Finance/Knowledge tests | ❌ | — | Integration tests | P0 | `34` |
| Mock data (dashboard/docs) | Present | — | Remove | P1 | `23-IMPLEMENTATION-STATUS` |
| /403 page | Missing | — | Create | P1 | handoff |
| proxy.ts redirect | Commented out | — | Implement or confirm not needed | P1 | `27` (proxy.ts is correct name in Next 16) |
| CI branch | main/dev vs master | — | Align | P1 | handoff |

## 39 — Feature priority scoring model

Score each recommendation on (1=low, 5=high):

| Factor | Weight |
|---|---|
| Business Value | 3 |
| Compliance Value | 4 |
| Security Value | 4 |
| User Value | 2 |
| Frequency | 2 |
| Risk Reduction | 3 |
| Implementation Cost (inverse) | 2 |
| Operational Cost (inverse) | 1 |
| Architecture Impact (inverse) | 2 |
| Dependency Count (inverse) | 1 |

**Score = Σ(factor × weight).** Higher = higher priority.

## 40 — Architecture options (ADR candidates)

### ADR-1: Password hashing migration
- **Current**: SHA-256. **Option A**: Argon2id (OWASP #1). **Option B**: PBKDF2 (FIPS). **Option C**: bcrypt (legacy).
- **Recommendation**: Argon2id via `Konscious.Security.Cryptography.Argon2`. Rehash-on-login migration. **Decide now: YES (P0).**

### ADR-2: Approval engine
- **Current**: Hardcoded. **Option A**: Stateless + Hangfire. **Option B**: Elsa. **Option C**: Camunda.
- **Recommendation**: Stateless + Hangfire (low complexity, no infra). **Decide: P1.**

### ADR-3: Deployment
- **Current**: Docker Compose. **Option A**: Compose + Caddy (stay). **Option B**: Kubernetes. **Option C**: Cloud managed.
- **Recommendation**: Stay on Docker Compose + add Caddy reverse proxy. **Decide: P1 (add Caddy). No Kubernetes.**

### ADR-4: Search
- **Current**: Basic ILIKE filters. **Option A**: pg_trgm + ICU collation (PG only). **Option B**: Elasticsearch. **Option C**: MeiliSearch.
- **Recommendation**: pg_trgm + ICU on existing PostgreSQL. **Decide: P2. No Elasticsearch.**

### ADR-5: RAG/AI
- **Current**: None. **Option A**: pgvector on PG. **Option B**: External vector DB. **Option C**: None.
- **Recommendation**: None for now. If pursued, pgvector. **Decide: P3 (defer).**

### ADR-6: Multi-tenancy
- **Current**: Singleton CompanySetting. **Option A**: Stay single-org. **Option B**: Multi-entity. **Option C**: Multi-tenant SaaS.
- **Recommendation**: Stay single-org for now. Add multi-entity only if customer demand. **Decide: defer (P3).**

### ADR-7: Background jobs
- **Current**: BackgroundService (leave accrual, retention, audit writer). **Option A**: Stay BackgroundService. **Option B**: Hangfire. **Option C**: Quartz.NET.
- **Recommendation**: Stay BackgroundService for existing jobs. Add Hangfire only for approval reminders/escalation. **Decide: P2.**