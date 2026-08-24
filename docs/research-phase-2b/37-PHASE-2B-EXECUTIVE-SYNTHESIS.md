# 37 — Phase 2B Executive Synthesis

> Research date: 2026-08-21. Final synthesis of Phase 2B evidence hardening.

## What was confirmed

| Finding | Confidence | Evidence |
|---|---|---|
| Minimum wage LAK 2,500,000 (1 Oct 2024) | HIGH | Laotian Times + WageIndicator |
| NSSF rates: employer 6.0%, employee 5.5% | HIGH | Trading Economics + codebase |
| PIT top rate 25% | HIGH | Trading Economics + Laotian Times |
| Labour Law No. 006/NOC (2006) | HIGH | GlobaLex + ILO record |
| Timezone UTC+07:00 (Asia/Vientiane) | VERIFIED | Wikipedia |
| LAK: att obsolete, whole-kip | VERIFIED | Wikipedia |
| Fiscal year 1 Oct – 30 Sep | VERIFIED | Wikipedia |
| proxy.ts is correct in Next.js 16 | VERIFIED | Next.js docs |
| Argon2id required (SHA-256 unsuitable) | VERIFIED | OWASP |
| IExceptionHandler + ProblemDetails in .NET 8+ | VERIFIED | Microsoft Learn |
| PostgreSQL WAL + PITR for production | VERIFIED | PG 18 docs |
| Stateless + Hangfire for approvals | HIGH | GitHub + hangfire.io |
| Lao Official Gazette is live | VERIFIED | Direct access |

## What was corrected

| Previous (Phase 2) | Corrected (Phase 2B) | Impact |
|---|---|---|
| PIT brackets at 1.3M/2M/8.5M/12.5M/18.5M | **2.5M/5M/15M/25M/65M** (presidential decree 6 Aug 2025) | **P0: codebase seed is OUTDATED** |
| PIT law = "Tax Administration 2015" | **Income Tax Law No. 67/NA** (2020) | Update documentation |
| Constitution Article 38 = privacy | Article 38 = asylum; **Article 29** = inviolability | Update privacy research |
| NSSF ceiling = UNKNOWN (not in system) | **4,500,000 in codebase** (but unverified externally) | P0: verify with LSSO |

## What remains unresolved

| Item | Confidence | Impact | Who can resolve |
|---|---|---|---|
| NSSF ceiling (4,500,000 unverified) | LOW | HIGH (payroll) | LSSO |
| Annual leave days (12 vs 15) | LOW (conflicting) | HIGH (leave) | Lao labour lawyer |
| Maternity leave days (90 vs 105) | LOW (conflicting) | HIGH (leave) | Lao labour lawyer |
| OT rest-day multiplier (2× vs 2.5×/3×) | LOW (conflicting) | HIGH (payroll) | Lao labour lawyer |
| OT monthly cap (45h vs 48h) | LOW (conflicting) | MEDIUM | Lao labour lawyer |
| OT calculation base (hourly divisor) | UNKNOWN | HIGH (payroll) | Lao labour lawyer |
| PIT personal allowance + dependant deductions | UNKNOWN | HIGH (payroll) | Lao tax adviser |
| PIT foreign employee treatment | UNKNOWN | MEDIUM | Lao tax adviser |
| PIT/NSSF filing deadlines | UNKNOWN | MEDIUM | Tax adviser / LSSO |
| Probation period (30/60 days) | LOW | MEDIUM | Lao labour lawyer |
| Notice periods + severance formula | UNKNOWN | MEDIUM | Lao labour lawyer |
| Foreign worker quota % | UNKNOWN | MEDIUM | MOLSW / lawyer |
| Dedicated data protection law | MEDIUM (likely none) | MEDIUM | Lao legal firm |
| Bank salary batch file formats | UNKNOWN | MEDIUM | BCEL/JDB corporate banking |

## Architecture recommendations (unchanged from Phase 2)

- Keep Docker Compose + single PostgreSQL (sufficient to ~5,000 employees / 300 concurrent with optimizations).
- No Kubernetes, Redis, Elasticsearch, BPM, microservices.
- Add Caddy reverse proxy + TLS.
- Argon2id password hashing (P0).
- PG-compatible migrations (P0).
- WAL + PITR backups (P0).
- Stateless + Hangfire for approvals (P1).
- pg_trgm + ICU collation for Lao search (P2).

## Roadmap changes from Phase 2

| Item | Old priority | New priority | Why |
|---|---|---|---|
| Update PIT brackets (1.3M→2.5M) | P0 (verify) | **P0 (CONFIRMED + UPDATE CODE)** | New evidence: decree confirmed; code is outdated |
| NSSF ceiling verification | P0 (if exists) | **P0 (CONFIRMED exists in code, verify value)** | New evidence: 4,500,000 is in codebase |
| PIT law citation | — | Update docs | Corrected to Law No. 67/NA |
| Payroll rule versioning | P2 | **P1** | PIT threshold change proves rates change; need versioned rules |
| Historical payroll reproducibility | P3 | **P1** | Without rule snapshots, historical runs break when rates update |

## What the next implementation phase should contain

**Phase 3A (Correctness & Security) — UPDATED**:
1. **🚨 UPDATE PIT brackets** in `LaoHRDbContext.cs`: bracket 1 threshold 1,300,000→2,500,000; bracket 2 lower bound 1,300,001→2,500,001. (P0, confirmed)
2. Fix `EmployeesController` ambiguous routes (P0).
3. Add `[Authorize]` to CompanySettingsController GET (P0).
4. Argon2id password hashing (P0).
5. PG-compatible migrations (P0).
6. WAL + PITR backups (P0).
7. Credential rotation (P0).
8. PM/Finance/Knowledge tests (P0).
9. **Verify NSSF ceiling** (4,500,000) with LSSO (P0 — requires human).
10. **Verify seeded LeavePolicy values** against law (P0 — requires human).

**Phase 3B-3J**: As per Phase 2 roadmap, with payroll rule versioning elevated to P1.