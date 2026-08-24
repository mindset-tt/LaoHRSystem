# 38 — Phase 3 Readiness Gate

> Research date: 2026-08-21. Determines whether implementation is safe to begin.

## Readiness assessment

| Area | Required confidence | Current confidence | Ready? | Blocker |
|---|---|---|---|---|
| **PIT brackets** | VERIFIED/HIGH | **HIGH** (corrected: 2.5M/5M/15M/25M/65M) | ✅ YES | None — brackets confirmed by presidential decree via Laotian Times citing MOF. Codebase update can proceed. |
| **PIT deductions/allowances** | HIGH | **UNKNOWN** | ⚠️ PARTIAL | Personal allowance + dependant deductions unknown. Can implement brackets now; add deductions when confirmed. |
| **NSSF rates** | VERIFIED/HIGH | **HIGH** (6.0%/5.5%) | ✅ YES | None. |
| **NSSF ceiling** | VERIFIED/HIGH | **LOW** (4,500,000 in code, unverified) | ❌ **NO** | Must confirm with LSSO. System can proceed with configurable ceiling (already is); but value must be verified before production payroll. |
| **NSSF base definition** | HIGH | **UNKNOWN** | ⚠️ PARTIAL | What wages are included (base only? +OT? +allowances?) unknown. Assume gross; confirm. |
| **Overtime calculation** | VERIFIED/HIGH | **LOW/MEDIUM** (1.5×/3× medium; 2× conflicting; base divisor unknown) | ❌ **NO** | Multipliers conflicting; hourly divisor unknown. Must confirm with labour lawyer before implementing OvertimeEntry. |
| **Statutory leave** | VERIFIED/HIGH | **LOW** (12 vs 15 days; 90 vs 105 days — conflicting) | ❌ **NO** | Must confirm exact day-counts with labour lawyer. System can proceed with configurable LeavePolicy (already is); but seeded values must be verified. |
| **Minimum wage** | VERIFIED/HIGH | **HIGH** (LAK 2,500,000, 1 Oct 2024) | ✅ YES | None. |
| **Foreign workers** | HIGH | **UNKNOWN** | ⚠️ PARTIAL | Work permit/quota/visa rules unknown. Can add data fields; validation rules need confirmation. |
| **Data privacy** | HIGH | **MEDIUM** (no dedicated PDPL; ET Law 2012 exists) | ✅ YES | No dedicated law found. Apply best practices. Confirm with legal firm if concerned. |
| **Security architecture** | HIGH | **HIGH** (Argon2id, IExceptionHandler, etc. — all VERIFIED) | ✅ YES | None. |
| **PostgreSQL migration** | HIGH | **HIGH** (PG 18 docs confirmed best practices) | ✅ YES | None. |
| **Backup/DR** | HIGH | **HIGH** (WAL+PITR confirmed) | ✅ YES | None. |
| **Payroll rule versioning** | HIGH | **HIGH** (architectural pattern clear) | ✅ YES | None — but must implement before rate changes break historical runs. |

## Phase 3 readiness verdict

### **PARTIALLY READY**

**What CAN proceed immediately (P0 — no legal confirmation needed):**
- ✅ Update PIT brackets (1.3M→2.5M threshold) — confirmed by presidential decree
- ✅ Fix employee routes, CompanySettings auth, JWT fail-fast
- ✅ Argon2id password hashing — OWASP verified
- ✅ PG-compatible migrations — technical, no legal dependency
- ✅ WAL + PITR backups — technical
- ✅ PM/Finance/Knowledge tests — technical
- ✅ Credential rotation — security
- ✅ Remove mock data, /403 page, proxy.ts — UX
- ✅ Caddy reverse proxy + TLS — infrastructure
- ✅ Frontend tests — technical

**What CANNOT proceed without professional confirmation:**
- ❌ NSSF ceiling value — must confirm 4,500,000 with LSSO
- ❌ Overtime multipliers + hourly divisor — must confirm with labour lawyer
- ❌ Statutory leave day-counts — must confirm with labour lawyer
- ❌ PIT personal allowance + dependant deductions — must confirm with tax adviser
- ❌ Foreign worker quota/rules — must confirm with MOLSW/lawyer

**What can proceed with SAFE ASSUMPTIONS (configurable, flagged as unverified):**
- ⚠️ NSSF ceiling — keep 4,500,000 configurable (already is); flag as unverified; confirm before production payroll
- ⚠️ Leave quotas — keep configurable LeavePolicy (already is); confirm seeded values before production
- ⚠️ OT multipliers — implement OvertimeEntry with configurable rates; confirm defaults before production
- ⚠️ Foreign worker fields — add data fields without validation rules; add rules when confirmed

## Recommendation

### **PHASE 3 MAY BEGIN — with conditions:**

1. **Immediately start** P0 technical/security/UX work (no legal dependency).
2. **Update PIT brackets** in codebase (confirmed — presidential decree).
3. **Implement payroll rule versioning** (P1) so future rate changes don't break historical runs.
4. **Engage Lao professionals in parallel** to resolve:
   - NSSF ceiling (LSSO)
   - OT multipliers + divisor (labour lawyer)
   - Leave day-counts (labour lawyer)
   - PIT deductions (tax adviser)
   - Foreign worker rules (MOLSW/lawyer)
5. **Do NOT run production payroll** until all compliance parameters are professionally confirmed.
6. **Keep all compliance values configurable** (the architecture already supports this) so they can be updated without code changes once confirmed.

### Legal formula gate

| Formula | Ready to implement? | Condition |
|---|---|---|
| PIT progressive calculation | ✅ YES (engine + brackets confirmed) | Update bracket thresholds; add deductions when confirmed |
| NSSF contribution calculation | ✅ YES (rates confirmed) | Verify ceiling value with LSSO before production |
| Overtime calculation | ⚠️ PARTIAL (multipliers unconfirmed) | Implement OvertimeEntry with configurable rates; confirm defaults |
| Leave accrual | ✅ YES (engine is configurable) | Verify seeded quotas with lawyer |
| Minimum wage validation | ✅ YES (amount confirmed) | — |

## Source traceability

Every payroll formula should be traceable:
```
System Rule (TaxBracket seed)
  → Requirement HR-LAO-001 (PIT threshold 2.5M)
    → Claim CL-01 (PIT brackets corrected)
      → Evidence E-23, E-24 (Laotian Times citing MOF, presidential decree 6 Aug 2025)
        → Law: Income Tax Law No. 67/NA + Presidential Decree (6 Aug 2025)
```

## Final statement

**Phase 3 is PARTIALLY READY.** Technical, security, and UX work can begin immediately. Payroll compliance parameters (NSSF ceiling, OT multipliers, leave day-counts, PIT deductions) require professional confirmation before production use. The system architecture is configurable — all compliance values can be updated without code changes once confirmed. **Do NOT run production payroll until all compliance parameters are verified by qualified Lao professionals.**