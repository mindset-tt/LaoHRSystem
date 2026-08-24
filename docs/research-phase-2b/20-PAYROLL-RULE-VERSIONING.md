# 20 — Payroll Rule Versioning + 21 — Historical Reproducibility + 22 — Law vs Company Policy + 23 — Hardcode vs Configure Matrix

> Research date: 2026-08-21. READ-ONLY — no implementation.

## 20 — Payroll rule versioning

### Problem
Legal rates change (PIT threshold changed 1.3M→2.5M in Aug 2025; NSSF rates changed 4.5%→6%/5.5% in 2016; minimum wage changes periodically). A production payroll system must:
1. Apply the correct rate for the period being calculated.
2. Reproduce historical calculations using the rates that were in effect at the time.
3. Audit which rule/version was used.

### Proposed model (research only — do NOT implement)

```
ComplianceRule
├── RuleId
├── RuleType (PIT_BRACKET / NSSF_RATE / NSSF_CEILING / MINIMUM_WAGE / OT_MULTIPLIER / LEAVE_QUOTA)
├── Jurisdiction (LAO_PDR)
├── EffectiveFrom (date)
├── EffectiveTo (date, nullable = current)
├── Version (int)
├── Source (law number, decree number, article)
├── SourceUrl
├── Parameters (JSONB — bracket thresholds, rates, ceilings, etc.)
├── VerifiedDate
├── VerifiedBy
```

### Versioning needs by rule type

| Rule | Versioning needed? | Current LaoHR approach | Recommended |
|---|---|---|---|
| PIT brackets | YES (changed 2025) | `TaxBracket` seeded (static) | Versioned DB table with EffectiveFrom/To |
| NSSF rates | YES (changed 2016) | `SystemSetting` (editable, no history) | Versioned DB table |
| NSSF ceiling | YES (if it changes) | `SystemSetting` (static) | Versioned DB table |
| Minimum wage | YES (changed Oct 2024) | Not stored | Versioned DB table |
| OT multipliers | YES (if law changes) | Not stored (single OvertimePay) | Versioned DB table |
| Leave quotas | YES (company policy + law) | `LeavePolicy` (editable, no history) | Add EffectiveFrom/To to LeavePolicy |
| Holiday calendar | YES (annual) | `Holiday` (per-year entries) | Good — annual entries already |

### PIT change history (example of why versioning matters)

| Period | Tax-free threshold | Law |
|---|---|---|
| Pre-2020 | 1,300,000 | Tax Law No. 70/NA (2015) |
| 2020-Jun 2026 | 1,300,000 | Income Tax Law No. 67/NA (2020) |
| June 2026+ | **2,500,000** | Presidential decree (6 Aug 2025) |

A payroll run for March 2026 must use 1,300,000 threshold; a run for July 2026 must use 2,500,000.

## 21 — Historical payroll reproducibility

### Principle
A production payroll system should be able to reproduce any past payroll run with the same result, even if rates have since changed.

### Required snapshot data (per payroll run)

| Data | Current LaoHR | Gap |
|---|---|---|
| Tax brackets (as of run date) | `TaxBracket` (current only) | No snapshot — historical runs would use current brackets |
| NSSF rates | `SystemSetting` (current only) | No snapshot |
| NSSF ceiling | `SystemSetting` (current only) | No snapshot |
| Exchange rates | `ConversionRate` (historical with effective/expiry) | ✅ GOOD — has history |
| Employee salary | `SalarySlip.BaseSalary` (stored per slip) | ✅ GOOD |
| Adjustments | `PayrollAdjustment` (stored) | ✅ GOOD |
| Work schedule | `WorkSchedule` (current only) | No snapshot — but rarely changes |
| Rule version reference | ❌ | No link to which rule version was used |

### Recommendation (research only)
- P1: Add `RuleVersionId` or `RuleSnapshot` to `SalarySlip` (JSONB of tax brackets + NSSF params used).
- P2: Implement `ComplianceRule` versioned table.
- P3: Full payroll run snapshot (immutable input + rule version + output).

## 22 — Law vs company policy

| Rule type | Legal floor | Company improvement | LaoHR support |
|---|---|---|---|
| Annual leave | 12-15 days (law) | Company may grant more | `LeavePolicy.AnnualQuota` (configurable) — ✅ supports both |
| Sick leave | ~30 days (law) | Company may extend | `LeavePolicy` — ✅ |
| Maternity leave | 90-105 days (law) | Company may top up pay | `LeavePolicy` — ✅ |
| OT multipliers | 1.5×/2×/3× (law) | Company may pay more | Not modelled (single OvertimePay) — GAP |
| Minimum wage | 2,500,000 (law) | Company pays above | `Employee.BaseSalary` (free) — ✅ |
| Work hours | 8h/day, 48h/week (law) | Company may reduce | `WorkSchedule` (configurable) — ✅ |
| Notice period | Statutory minimum | Company may require more | Not modelled — GAP (P2) |
| Severance | Statutory formula | Company may pay more | Not modelled — GAP (P2) |

**Principle**: The system should support BOTH the legal floor AND company-defined improvements. The current `LeavePolicy`/`WorkSchedule`/`SystemSetting` architecture already supports this (configurable). The gap is in OT (no configurable multipliers) and termination (no model).

## 23 — Hardcode vs configure matrix

| Rule | Hardcode? | Configure? | Versioned DB? | Annual calendar? | Company policy? | User setting? | Recommendation |
|---|---|---|---|---|---|---|---|
| PIT brackets | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | Versioned DB (ComplianceRule) |
| PIT rates | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | Versioned DB |
| NSSF rates | ❌ | ⚠️ (current SystemSetting) | ✅ | ❌ | ❌ | ❌ | Versioned DB (with history) |
| NSSF ceiling | ❌ | ⚠️ (current SystemSetting) | ✅ | ❌ | ❌ | ❌ | Versioned DB |
| Minimum wage | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | Versioned DB (for validation) |
| OT multipliers | ❌ | ✅ | ✅ (if law changes) | ❌ | ✅ (company can pay more) | ❌ | Configurable + versioned |
| OT max hours | ❌ | ✅ | ✅ | ❌ | ✅ | ❌ | Configurable |
| Leave quotas | ❌ | ✅ (LeavePolicy) | ⚠️ (add EffectiveFrom/To) | ❌ | ✅ | ❌ | Configurable + versioned |
| Leave types | ⚠️ (enum) | ✅ | ❌ | ❌ | ✅ | ❌ | Configurable (current is good) |
| Public holidays | ❌ | ❌ | ❌ | ✅ | ✅ | ❌ | Annual calendar (current Holiday model is good) |
| Work schedule | ❌ | ✅ (WorkSchedule) | ❌ | ❌ | ✅ | ❌ | Configurable (current is good) |
| Work hours | ❌ | ✅ | ❌ | ❌ | ✅ | ❌ | Configurable (current is good) |
| Tax personal allowance | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | Versioned DB |
| Dependant deduction | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | Versioned DB |
| Probation period | ⚠️ (30/60 days default) | ✅ | ❌ | ❌ | ✅ | ❌ | Configurable with legal default |
| Notice periods | ❌ | ✅ | ❌ | ❌ | ✅ | ❌ | Configurable |
| Severance formula | ⚠️ (if statutory) | ✅ | ❌ | ❌ | ✅ | ❌ | Configurable with legal default |
| Exchange rates | ❌ | ✅ (ConversionRate) | ✅ (has history) | ❌ | ❌ | ❌ | Versioned DB (current is good) |
| Company info | ❌ | ✅ (CompanySetting) | ❌ | ❌ | ✅ | ❌ | Configurable (current is good) |
| Email SMTP | ❌ | ✅ (env vars) | ❌ | ❌ | ❌ | ❌ | Env config (current is good) |
| JWT key | ❌ | ✅ (env vars) | ❌ | ❌ | ❌ | ❌ | Env config (current is good) |

### Key insight
The most critical gap is **PIT brackets and NSSF parameters need versioned DB storage** (not just `SystemSetting` which has no history). The current `TaxBracket` table has the right structure but no `EffectiveFrom/To` fields. Adding date effectiveness to `TaxBracket` + `SystemSetting` (or a new `ComplianceRule` table) is the recommended approach. **P1.**