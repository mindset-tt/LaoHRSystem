# 02 — NSSF Final Research + 03 — Leave Final Research + 04 — Overtime Final Research + 05 — PIT Final Research

> Research date: 2026-08-21. Sources: PwC Worldwide Tax Summaries (quality 5/5, VERIFIED), AsianLII Lao Labour Law Law No. 06/NA (quality 5/5, VERIFIED), Laotian Times.

## 02 — NSSF final research

### Rates (VERIFIED — PwC + codebase)

| Component | Rate | Source | Quality | Confidence |
|---|---|---|---|---|
| Employer | 6.0% of gross remuneration | PwC + codebase | 5 | VERIFIED |
| Employee | 5.5% of gross remuneration | PwC + codebase | 5 | VERIFIED |
| Total | 11.5% | Derived | 5 | VERIFIED |

### Ceiling (VERIFIED — PwC citing MOLSW notification)

| Item | Value | Source | Quality | Confidence |
|---|---|---|---|---|
| Contribution ceiling | **LAK 4,500,000/month** | PwC citing **Notification No. 0824/NSSFO** from MOLSW | 5 | **VERIFIED** |
| Codebase match | `NSSF_CEILING_BASE=4500000` | `LaoHRDbContext.cs:236` | 5 (code) | **VERIFIED — codebase is correct** |

> **Phase 2B gap CLOSED**: The NSSF ceiling of LAK 4,500,000 is now VERIFIED from PwC Worldwide Tax Summaries, citing Notification No. 0824/NSSFO from the Ministry of Labour and Social Welfare.

### Contribution base

| Item | Value | Source | Confidence |
|---|---|---|---|
| Base definition | "Gross remuneration" (PwC wording) | PwC | HIGH — "gross remuneration" likely means all employment income |
| NSSF employee deductible from PIT | YES | PwC Deductions page | VERIFIED |
| Minimum contribution floor | UNKNOWN | Not found | UNRESOLVED — PROFESSIONAL_CONFIRMATION_REQUIRED |

### Payment + reporting deadlines (VERIFIED)

| Obligation | Deadline | Source | Quality | Confidence |
|---|---|---|---|---|
| NSSF monthly payment | **20th of following month** | PwC | 5 | VERIFIED |
| NSSF monthly return | Monthly (same as payment) | PwC | 5 | VERIFIED |

### Foreign workers (VERIFIED)

| Rule | Value | Source | Confidence |
|---|---|---|---|
| NSSF for expatriates | Required if working >12 months in Laos | PwC | VERIFIED |
| Bilateral exemptions | UNKNOWN | Not found | UNRESOLVED |

### Coverage (VERIFIED)

Medical care, maternity, sickness, work injury, occupational disease, disability, old-age (pension), death, family support, unemployment.

### Pension rules (VERIFIED — Laotian Times 2025-06-23)

| Rule | Value | Confidence |
|---|---|---|
| Full pension contribution years | 25 years | MEDIUM (quality 3) |
| Retirement age (men) | 57–60 | MEDIUM |
| Retirement age (women) | 55–60 | MEDIUM |
| Reduced pension (23-24 years) | 1% deduction/year below 25 | MEDIUM |
| Lump-sum (non-qualifying) | 85% × final salary × 1.5 months × contribution years | MEDIUM |

### NSSF compliance lock

| Rule | Value | Status |
|---|---|---|
| Employer rate 6.0% | VERIFIED (PwC + code) | **LOCKED** |
| Employee rate 5.5% | VERIFIED (PwC + code) | **LOCKED** |
| Ceiling LAK 4,500,000 | VERIFIED (PwC citing Notification 0824/NSSFO) | **LOCKED** |
| Payment deadline 20th | VERIFIED (PwC) | **LOCKED** |
| Employee contribution deductible from PIT | VERIFIED (PwC) | **LOCKED** |
| Foreign workers >12 months | VERIFIED (PwC) | **LOCKED** |
| Contribution base = gross remuneration | HIGH (PwC wording) | **TEMPORARY** (confirm exact definition) |
| Minimum contribution floor | UNKNOWN | **BLOCKED** (PROFESSIONAL_CONFIRMATION_REQUIRED) |

---

## 03 — Leave final research (ALL CONFLICTS RESOLVED)

Source: **Lao Labour Law, Law No. 06/NA, 27 December 2006** — full English text on AsianLII. Quality 5/5. VERIFIED.

### Annual leave (Article 21 — VERIFIED)

| Rule | Value | Article | Confidence |
|---|---|---|---|
| Standard annual leave | **15 days/year**, full pay | Art. 21 | VERIFIED |
| Hazardous work annual leave | **18 days/year** | Art. 21 | VERIFIED |
| Eligibility | After **1 full year** of service | Art. 21 | VERIFIED |
| Exclusions | Weekly rest + public holidays NOT counted | Art. 21 | VERIFIED |

> **Phase 2B conflict RESOLVED**: 15 days (not 12). The 12-day figure was from outdated HR guides.

### Sick leave (Article 20 — VERIFIED)

| Rule | Value | Article | Confidence |
|---|---|---|---|
| Sick leave (monthly-paid) | **30 days/year**, full pay | Art. 20 | VERIFIED |
| Medical certificate | Required | Art. 20 | VERIFIED |
| Daily/hourly workers | Via social security after 90+ days work | Art. 20 | VERIFIED |

### Maternity leave (Article 39 — VERIFIED)

| Rule | Value | Article | Confidence |
|---|---|---|---|
| Minimum maternity leave | **At least 90 days** | Art. 39 | VERIFIED |
| Post-birth minimum | **At least 42 days** after birth | Art. 39 | VERIFIED |
| Pay | **Full pay** at normal salary | Art. 39 | VERIFIED |
| Who pays | Employer OR social security fund | Art. 39 | VERIFIED |
| Extended illness after birth | +30 days at 50% pay (doctor certified) | Art. 39 | VERIFIED |
| Childcare time | 1 hour/day for 1 year | Art. 39 | VERIFIED |
| Miscarriage | Leave per doctor, normal pay | Art. 39 | VERIFIED |

> **Phase 2B conflict RESOLVED**: 90 days (not 105). The 105-day figure was from the Civil Servants Law (different legislation) or adding the 30-day extended illness provision.

### Maternity allowance (Article 40 — VERIFIED)

| Rule | Value | Article | Confidence |
|---|---|---|---|
| Maternity allowance | At least **60% of minimum wages** | Art. 40 | VERIFIED |
| Twins | Additional **50%** of maternity allowance | Art. 40 | VERIFIED |

### Other leave types

| Leave type | Statutory? | Source | Confidence |
|---|---|---|---|
| Paternity leave | **NOT in Labour Law (Art. 1-77)** | Labour Law text | VERIFIED (absence) |
| Marriage leave | **NOT in Labour Law** | Labour Law text | VERIFIED (absence) |
| Bereavement leave | **NOT in Labour Law** | Labour Law text | VERIFIED (absence) |
| Unpaid leave | By agreement (company policy) | — | MEDIUM |

> Paternity/marriage/bereavement are **not statutory for private sector** under the 2006 Labour Law. They may exist in implementing regulations or be company policy. The Civil Servants Law (amended 2025) includes these for civil servants only.

### Carry-over rules

| Item | Status | Confidence |
|---|---|---|
| Annual leave carry-over | **NOT specified in law text** | UNRESOLVED — PROFESSIONAL_CONFIRMATION_REQUIRED |
| Leave encashment | NOT specified | UNRESOLVED |

### Leave compliance lock

| Rule | Value | Status |
|---|---|---|
| Annual leave 15 days (18 hazardous) | VERIFIED (Art. 21) | **LOCKED** |
| Sick leave 30 days full pay | VERIFIED (Art. 20) | **LOCKED** |
| Maternity ≥90 days full pay | VERIFIED (Art. 39) | **LOCKED** |
| Maternity allowance ≥60% min wage | VERIFIED (Art. 40) | **LOCKED** |
| Maternity twins +50% | VERIFIED (Art. 40) | **LOCKED** |
| Paternity/marriage/bereavement | NOT STATUTORY (private sector) | **NOT_APPLICABLE** (company policy) |
| Carry-over rules | UNRESOLVED | **PROFESSIONAL_CONFIRMATION_REQUIRED** |

---

## 04 — Overtime final research (ALL CONFLICTS RESOLVED)

Source: **Labour Law Article 48** + **Article 18**. VERIFIED from primary law text.

### OT multipliers (Article 48 — VERIFIED)

| OT type | Multiplier | Article | Confidence |
|---|---|---|---|
| Ordinary weekday OT (daytime) | **1.5× (150%)** | Art. 48 | VERIFIED |
| Night work OT on regular working day | **2× (200%)** | Art. 48 | VERIFIED |
| Rest day/holiday OT (daytime) | **2.5× (250%)** | Art. 48 | VERIFIED |
| Rest day/holiday OT (night) | **3× (300%)** | Art. 48 | VERIFIED |

> **Phase 2B conflict RESOLVED**: All four rates (1.5/2/2.5/3) are correct — they apply to different scenarios (day vs night × weekday vs rest day). The "2× vs 2.5× vs 3×" confusion was because different sources cited different scenarios.

### Night shift bonus (Article 48 — VERIFIED)

| Rule | Value | Article | Confidence |
|---|---|---|---|
| Night shift additional pay | At least **15%** of regular hourly rate, per hour 22:00–05:00 | Art. 48 | VERIFIED |

### OT caps (Article 18 — VERIFIED)

| Rule | Value | Article | Confidence |
|---|---|---|---|
| Max OT per day | **3 hours** | Art. 18 | VERIFIED |
| Max OT per month | **45 hours** | Art. 18 | VERIFIED |
| OT > 45h/month | Requires labour admin authorization + trade union approval | Art. 18 | VERIFIED |
| Continuous daily OT | Prohibited (except emergencies) | Art. 18 | VERIFIED |
| OT consent | Trade union/worker reps + employees must consent | Art. 18 | VERIFIED |

> **Phase 2B conflict RESOLVED**: 45 hours/month (not 48h — the 48h was the weekly normal working hours limit, Art. 16).

### Hourly rate divisor

| Item | Status | Confidence |
|---|---|---|
| Divisor formula | Law says "hourly wages of a regular working day" (Art. 48) but does NOT specify the divisor | UNRESOLVED — PROFESSIONAL_CONFIRMATION_REQUIRED |
| Safe assumption | Monthly salary ÷ 26 days ÷ 8 hours (common practice) | MEDIUM (unverified) |

### OT compliance lock

| Rule | Value | Status |
|---|---|---|
| Weekday day OT 1.5× | VERIFIED (Art. 48) | **LOCKED** |
| Weekday night OT 2× | VERIFIED (Art. 48) | **LOCKED** |
| Rest day day OT 2.5× | VERIFIED (Art. 48) | **LOCKED** |
| Rest day night OT 3× | VERIFIED (Art. 48) | **LOCKED** |
| Night bonus 15% | VERIFIED (Art. 48) | **LOCKED** |
| Max OT/day 3h | VERIFIED (Art. 18) | **LOCKED** |
| Max OT/month 45h | VERIFIED (Art. 18) | **LOCKED** |
| Hourly divisor | UNRESOLVED | **PROFESSIONAL_CONFIRMATION_REQUIRED** |

---

## 05 — PIT final research

Source: **PwC Worldwide Tax Summaries — Lao PDR** (quality 5/5, VERIFIED, last reviewed 07 Aug 2026).

### Governing law (CORRECTED from Phase 2B)

| Field | Value | Confidence |
|---|---|---|
| Current law | **Amended Income Tax Law No. 88/NA, dated 25 June 2025** | VERIFIED (PwC) |
| Published in Official Gazette | 19 June 2026 | VERIFIED (PwC) |
| Effective | 3 July 2026 | VERIFIED (PwC) |
| Previous law | Income Tax Law No. 67/NA (2020) | VERIFIED |
| Original | Tax Law No. 70/NA (2015) | VERIFIED |

> **Phase 2B correction**: Phase 2B cited "presidential decree 6 Aug 2025" — the actual implementing legislation is **Law No. 88/NA (25 June 2025)**, published 19 June 2026, effective 3 July 2026.

### PIT brackets (VERIFIED — PwC)

**Monthly (LAK):**

| From | To | Rate |
|---|---|---|
| 0 | 2,500,000 | 0% |
| 2,500,000 | 5,000,000 | 5% |
| 5,000,000 | 15,000,000 | 10% |
| 15,000,000 | 25,000,000 | 15% |
| 25,000,000 | 65,000,000 | 20% |
| 65,000,000+ | — | 25% |

**Annual (LAK):**

| From | To | Rate |
|---|---|---|
| 0 | 30,000,000 | 0% |
| 30,000,000 | 60,000,000 | 5% |
| 60,000,000 | 180,000,000 | 10% |
| 180,000,000 | 300,000,000 | 15% |
| 300,000,000 | 780,000,000 | 20% |
| 780,000,000+ | — | 25% |

Confidence: **VERIFIED** (PwC, quality 5/5).

### Personal allowance (VERIFIED)

| Item | Value | Source | Confidence |
|---|---|---|---|
| Tax-free threshold | LAK 2,500,000/month (30M/year) | PwC | VERIFIED |
| Separate personal allowance? | **NO** — the 2.5M tax-free band IS the personal allowance | PwC | VERIFIED |

### Dependant deduction (VERIFIED)

| Item | Value | Source | Confidence |
|---|---|---|---|
| Per dependent (spouse, parent, children) | **LAK 5,000,000/year** | PwC Deductions page | VERIFIED |
| Max dependents | **3 persons** | PwC | VERIFIED |
| Max annual deduction | **LAK 15,000,000/year** | PwC | VERIFIED |

### NSSF deductibility (VERIFIED)

| Item | Value | Source | Confidence |
|---|---|---|---|
| NSSF employee contribution deductible from taxable income | **YES** | PwC | VERIFIED |
| NSSF employer contribution | 6% of gross | PwC | VERIFIED |
| NSSF employee contribution | 5.5% of gross | PwC | VERIFIED |
| NSSF ceiling | LAK 4.5M (Notification 0824/NSSFO) | PwC | VERIFIED |

### Taxable vs non-taxable (VERIFIED)

| Item | Taxable? | Source | Confidence |
|---|---|---|---|
| Base salary | YES | PwC | VERIFIED |
| Allowances | YES (all salary and non-cash benefits in kind taxable) | PwC | VERIFIED |
| Overtime pay | **Exempt for employees with base salary < LAK 3M/month** | PwC | VERIFIED |
| Overtime pay (base ≥ LAK 3M) | Taxable | PwC | VERIFIED |
| Bonus | YES (same progressive rates) | PwC + Laotian Times | VERIFIED |
| NSSF employee contribution | Deductible | PwC | VERIFIED |
| Charitable contributions | NOT deductible | PwC | VERIFIED |

### Foreign employee treatment (VERIFIED)

| Item | Value | Source | Confidence |
|---|---|---|---|
| PIT brackets for foreigners | **Same as Lao nationals** | PwC | VERIFIED |
| Surcharge for foreigners | **None** | PwC | VERIFIED |
| Expatriates >183 days with foreign-source income | Obligated to pay PIT | PwC | VERIFIED |
| Residence definition | Art. 3, Items 11-12 of Law 88/NA | PwC | VERIFIED |

### Withholding + filing deadlines (VERIFIED)

| Obligation | Deadline | Source | Confidence |
|---|---|---|---|
| Monthly PIT withholding + payment | **20th of following month** | PwC | VERIFIED |
| Annual PIT return | **31 March** of following year | PwC | VERIFIED |
| Additional payment (if assessed) | 15 working days from notice | PwC | VERIFIED |
| Refund of excess | 10 working days | PwC | VERIFIED |
| Statute of limitations | 3 accounting years | PwC | VERIFIED |

### Penalties (VERIFIED)

| Violation | Penalty | Source |
|---|---|---|
| Late filing/payment | **0.1% per day** of delay | PwC |
| Under-reporting (1st) | 30% | PwC |
| Under-reporting (2nd) | 60% | PwC |
| Under-reporting (3rd) | 100% + closure + publication | PwC |

### PIT compliance lock

| Rule | Value | Status |
|---|---|---|
| Brackets 0/5/10/15/20/25% at 2.5M/5M/15M/25M/65M | VERIFIED (PwC) | **LOCKED** |
| Tax-free threshold = 2.5M (personal allowance) | VERIFIED (PwC) | **LOCKED** |
| Dependant deduction 5M/dependent, max 15M/year | VERIFIED (PwC) | **LOCKED** |
| NSSF employee deductible from PIT | VERIFIED (PwC) | **LOCKED** |
| OT exempt for base < 3M/month | VERIFIED (PwC) | **LOCKED** |
| Bonus = same progressive rates | VERIFIED (PwC) | **LOCKED** |
| Foreign workers same brackets | VERIFIED (PwC) | **LOCKED** |
| Monthly withholding due 20th | VERIFIED (PwC) | **LOCKED** |
| Annual return due 31 March | VERIFIED (PwC) | **LOCKED** |
| Late penalty 0.1%/day | VERIFIED (PwC) | **LOCKED** |
| Law = No. 88/NA (25 June 2025, eff. 3 July 2026) | VERIFIED (PwC) | **LOCKED** |