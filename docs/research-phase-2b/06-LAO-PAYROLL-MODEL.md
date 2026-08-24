# 06 — Lao Payroll Model + 07 — PIT Verification + 08 — NSSF Verification + 09 — Minimum Wage

> Research date: 2026-08-21. Sources: Laotian Times (citing MOF), Trading Economics (citing MOF/MOLSW), codebase inspection.
> ⚠️ NOT legal advice.

## 07 — PIT verification (CRITICAL — CORRECTED)

### Governing law (CORRECTED from Phase 2)

| Field | Value | Classification | Source quality | Freshness |
|---|---|---|---|---|
| **Law** | **Income Tax Law No. 67/NA** — approved by National Assembly June 2019, effective 17 February 2020 | D (Laotian Times citing MOF) | 3 | Current |
| **Replaced** | Tax Law No. 70/NA (2015) | D | 3 | Superseded |
| **Amendment proposed** | 24 June 2025 by Finance Minister — 15% minimum profit tax (OECD), reduced rates for listed companies, micro-enterprise taxation | D | 3 | Possibly Current |
| **Presidential decree** | 6 August 2025 — raised tax-free threshold from 1.3M → 2.5M | D (Laotian Times citing MOF) | 3 | Current (eff. June 2026) |
| **Issuing authority** | Ministry of Finance (ກະຊວງການເງິນ) | A | 5 | Current |
| **Lao term** | ພາສີເງິນເດືອນ (salary tax) / ພາສີລາຍໄດ້ບຸກຄົນ (PIT) | — | — | — |

### Current PIT brackets (HIGH confidence — CORRECTED)

| Bracket | Monthly taxable income (LAK) | Rate | Confidence |
|---|---|---|---|
| 1 | 0 – **2,500,000** | **0%** (tax-free) | HIGH |
| 2 | 2,500,001 – 5,000,000 | **5%** | HIGH |
| 3 | 5,000,001 – 15,000,000 | **10%** | HIGH |
| 4 | 15,000,001 – 25,000,000 | **15%** | HIGH |
| 5 | 25,000,001 – 65,000,000 | **20%** | HIGH |
| 6 | 65,000,001+ | **25%** | HIGH |

**Source**: Laotian Times (30 June 2026), citing Ministry of Finance. Presidential decree promulgated 6 August 2025, effective June 2026.
**Worked example** (from source): Person earning LAK 6M/month → no tax on first 2.5M, next 2.5M at 5%, remaining 1M at 10%.

### 🚨 CODEBASE MISMATCH (P0 CRITICAL)

**Codebase** (`LaoHRDbContext.cs:227-233`):
```
Bracket 1: 0 – 1,300,000 → 0%   ← OUTDATED (should be 2,500,000)
Bracket 2: 1,300,001 – 5,000,000 → 5%   ← OUTDATED lower bound (should be 2,500,001)
Bracket 3: 5,000,001 – 15,000,000 → 10%   ← CORRECT
Bracket 4: 15,000,001 – 25,000,000 → 15%   ← CORRECT
Bracket 5: 25,000,01 – 65,000,000 → 20%   ← CORRECT
Bracket 6: 65,000,001+ → 25%   ← CORRECT
```

**Action required**: Update brackets 1 and 2 to reflect the new 2,500,000 threshold. Brackets 3-6 are already correct. **This is a P0 compliance bug — the system undercharges tax for incomes between 1.3M and 2.5M.**

### What is taxable
- Salaries, wages, bonuses, allowances, and other employment income (VERIFIED from Laotian Times).
- Classification: D. Confidence: HIGH.

### What could NOT be verified (UNKNOWN)

| Item | Confidence |
|---|---|
| Personal allowance (separate from tax-free threshold) | UNKNOWN |
| Dependant deduction amount | UNKNOWN |
| Non-taxable allowance types (transport? meal?) | UNKNOWN |
| Foreign employee treatment | UNKNOWN |
| Monthly withholding deadline | UNKNOWN |
| Annual reconciliation deadline | UNKNOWN |
| Filing format | UNKNOWN |

### PIT change history

| Year | Instrument | Change | Effective |
|---|---|---|---|
| 2015 | Tax Law No. 70/NA | Baseline tax law | 2015 |
| 2019/2020 | Income Tax Law No. 67/NA | Replaced 70/NA; reformed rates | 17 Feb 2020 |
| 2025 | Presidential decree (6 Aug) | Tax-free threshold 1.3M → 2.5M | June 2026 |
| 2025 | Amendment proposed (24 Jun) | 15% min profit tax, reduced listed rates | Pending |

## 08 — NSSF verification

### Rates (CONFIRMED)

| Component | Rate | Classification | Source quality | Freshness |
|---|---|---|---|---|
| Employer | 6.0% | D (Trading Economics citing MOLSW) | 2 | Current |
| Employee | 5.5% | D | 2 | Current |
| Total | 11.5% | D (derived) | 2 | Current |
| Historical (pre-2016) | Both 4.5% | D | 2 | Old But Effective |

**Codebase**: `NSSF_EMPLOYEE_RATE=0.055`, `NSSF_EMPLOYER_RATE=0.060` — VERIFIED matches.

### Ceiling (UNRESOLVED — CRITICAL)

| Item | Value | Classification | Source quality | Freshness |
|---|---|---|---|---|
| Contribution ceiling | **LAK 4,500,000/month** (in codebase) | A (codebase only) | 5 (code) / 0 (external) | Unverified |
| External confirmation | **NONE found** | H (unknown) | — | Unknown |

> ⚠️ **The LAK 4,500,000 ceiling could NOT be verified from any accessible external source.** No Lao government source, LSSO website, or international publication confirmed this figure. It may be correct (the developer may have had access to LSSO documentation), but it MUST be confirmed with LSSO before production use.

### Law

| Field | Value | Classification |
|---|---|---|
| Law | Law on Social Security (ກົດໝາຍວ່າດ້ວຍປະກັນສັງຄົມ), 2013 | D (commonly cited) |
| ILO NATLEX | ISN 91506 (403 on fetch) | D |
| Law number | UNKNOWN — not confirmed from primary source | H |
| Administrator | LSSO (ອົງການປະກັນສັງຄົມ) | A |

### Coverage (from Laotian Times, Dec 2022)
Medical care, abortion/miscarriage, work-related accidents, occupational diseases, labor loss, disability, work-related illness, retirement, death, family support, unemployment. Classification: D. Confidence: MEDIUM.

### UNKNOWN items

| Item | Confidence |
|---|---|
| Contribution base definition (base only? +OT? +allowances?) | UNKNOWN |
| Minimum contribution base | UNKNOWN |
| Foreign worker treatment | UNKNOWN |
| Payment deadline | UNKNOWN |
| Reporting format | UNKNOWN |
| Sub-fund breakdown (pension/health/injury/maternity split) | UNKNOWN |

## 09 — Minimum wage

| Item | Value | Classification | Source quality | Freshness |
|---|---|---|---|---|
| Current monthly minimum | **LAK 2,500,000** | D (Laotian Times + WageIndicator) | 3 | Current |
| Effective date | **1 October 2024** | D | 3 | Current |
| Previous | LAK 1,600,000 | D | 3 | Superseded |
| Unskilled worker subsistence allowance | LAK 900,000 (additional) | D | 3 | Current |
| Decree number | UNKNOWN | H | — | Unknown |
| Issuing authority | MOLSW | A | 5 | Current |
| Scope | Private sector (production, business, service, household); excludes OT, welfare, benefits | D | 3 | Current |
| Under review (Mar 2026) | Up to LAK 4,100,000 proposed | D | 3 | Pending |

**LaoHR**: No minimum wage validation in payroll. GAP: P1 — add validation that `BaseSalary ≥ 2,500,000` (configurable).

### Historical changes

| Period | Amount (LAK/month) | Source |
|---|---|---|
| Pre-Oct 2024 | 1,600,000 | Laotian Times |
| From 1 Oct 2024 | 2,500,000 | Laotian Times + WageIndicator |
| Under review (Mar 2026) | Up to 4,100,000 proposed | Laotian Times (pending) |

## 06 — Payroll model (full flow)

```
1. BaseSalary (contract currency: LAK/USD/THB/CNY)
2. → Convert to LAK via ConversionRate
3. + OvertimePay (GAP: needs day-type differentiation 1.5×/2×/3×)
4. + Allowances (GAP: needs taxable/non-taxable split)
5. + Bonus
6. = GrossIncome
7. NSSF:
   a. NssfBase = min(GrossIncome, NSSF_CEILING) — ceiling = 4,500,000 (UNVERIFIED)
   b. NssfEmployeeDeduction = NssfBase × 5.5%
   c. NssfEmployerContribution = NssfBase × 6.0%
8. TaxableIncome = GrossIncome − NssfEmployeeDeduction − personal_allowance − dependant_deductions (GAP: deductions not modelled)
9. TaxDeduction = progressive PIT (brackets: 0/5/10/15/20/25% at 2.5M/5M/15M/25M/65M) — CORRECTED brackets
10. OtherDeductions = loan_installments (GAP: not auto-linked) + advances + adjustments
11. NetSalary = GrossIncome − NssfEmployeeDeduction − TaxDeduction − OtherDeductions
12. → Convert NetSalary back to contract currency if PaymentCurrency = ORIGINAL
```

**LaoHR's `PayrollService` follows this order** (VERIFIED). Gaps are in parameters (brackets need update, ceiling unverified, deductions missing) and overtime differentiation, not the calculation sequence.