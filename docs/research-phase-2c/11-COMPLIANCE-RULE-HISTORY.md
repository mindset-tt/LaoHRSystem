# 11 — Compliance Rule History + 12 — Compliance Rule Specification + 13 — Legal Floor vs Company Policy + 14 — Configuration Classification

> Research date: 2026-08-21.

## 11 — Compliance rule history

| Rule | Version | EffectiveFrom | EffectiveTo | Legal instrument | Source | Verification |
|---|---|---|---|---|---|---|
| PIT tax-free threshold | 1.3M | ~2015 | June 2026 | Tax Law No. 70/NA (2015) | PwC | VERIFIED |
| PIT tax-free threshold | **2.5M** | **July 2026** | current | **Amended Income Tax Law No. 88/NA** (25 Jun 2025) | PwC | VERIFIED |
| PIT rates (0/5/10/15/20/25%) | — | 2020 | current | Income Tax Law No. 67/NA (2020) → amended 88/NA (2025) | PwC | VERIFIED |
| PIT bracket boundaries | 1.3M/2M/8.5M/12.5M/18.5M | ~2015 | June 2026 | Tax Law 70/NA | Phase 2 (unverified) | OUTDATED |
| PIT bracket boundaries | **2.5M/5M/15M/25M/65M** | **July 2026** | current | Law 88/NA | PwC | VERIFIED |
| PIT dependant deduction | **5M/dependent, max 15M/yr** | current | current | Law 88/NA | PwC | VERIFIED |
| NSSF employer rate | 4.5% | pre-2016 | 2016 | Social Security regulations | Trading Economics | VERIFIED (historical) |
| NSSF employer rate | **6.0%** | 2016 | current | Social Security Law/regulations | PwC | VERIFIED |
| NSSF employee rate | 4.5% | pre-2016 | 2016 | — | Trading Economics | VERIFIED (historical) |
| NSSF employee rate | **5.5%** | 2016 | current | — | PwC | VERIFIED |
| NSSF ceiling | **4,500,000** | current | current | **Notification No. 0824/NSSFO** (MOLSW) | PwC | VERIFIED |
| Minimum wage | 1,600,000 | pre-Oct 2024 | Oct 2024 | MOLSW decree | Laotian Times | VERIFIED |
| Minimum wage | **2,500,000** | 1 Oct 2024 | current | MOLSW decree (number unknown) | Laotian Times + WageIndicator | VERIFIED |
| Minimum wage | ~4,100,000 (proposed) | pending | — | Under review (Mar 2026) | Laotian Times | POSSIBLY_CURRENT |
| OT weekday day | 1.5× | 2006 | current | Labour Law Art. 48 (06/NA) | AsianLII | VERIFIED |
| OT weekday night | 2× | 2006 | current | Art. 48 | AsianLII | VERIFIED |
| OT rest day day | 2.5× | 2006 | current | Art. 48 | AsianLII | VERIFIED |
| OT rest day night | 3× | 2006 | current | Art. 48 | AsianLII | VERIFIED |
| OT night bonus | 15% | 2006 | current | Art. 48 | AsianLII | VERIFIED |
| OT max/day | 3h | 2006 | current | Art. 18 | AsianLII | VERIFIED |
| OT max/month | 45h | 2006 | current | Art. 18 | AsianLII | VERIFIED |
| Annual leave | 15 days (18 hazardous) | 2006 | current | Art. 21 | AsianLII | VERIFIED |
| Sick leave | 30 days full pay | 2006 | current | Art. 20 | AsianLII | VERIFIED |
| Maternity leave | ≥90 days full pay | 2006 | current | Art. 39 | AsianLII | VERIFIED |
| Maternity allowance | ≥60% min wage | 2006 | current | Art. 40 | AsianLII | VERIFIED |
| Probation | 30/60 days | 2006 | current | Art. 27 | AsianLII | VERIFIED |
| Notice (indefinite) | 30/45 days | 2006 | current | Art. 28 | AsianLII | VERIFIED |
| Severance (dismissal) | 10%/15% | 2006 | current | Art. 29 | AsianLII | VERIFIED |
| Severance (unjustified) | 15%/20% | 2006 | current | Art. 33 | AsianLII | VERIFIED |
| Foreign worker quota | 10%/20% | 2006 | current | Art. 25 | AsianLII | VERIFIED |
| Working hours | 8h/day, 48h/week | 2006 | current | Art. 16 | AsianLII | VERIFIED |
| Weekly rest | ≥1 day | 2006 | current | Art. 19 | AsianLII | VERIFIED |
| PIT payment deadline | 20th | current | current | Law 88/NA | PwC | VERIFIED |
| NSSF payment deadline | 20th | current | current | — | PwC | VERIFIED |
| Annual PIT return | 31 March | current | current | — | PwC | VERIFIED |

> ⚠️ **Note**: The Labour Law is from 2006 (Law No. 06/NA). A 2013 amendment (052/NOC) is commonly cited but the AsianLII database only has the 2006 version. A Lao labour lawyer should confirm whether amendments have changed any values.

## 12 — Compliance rule specification (normalized)

| RuleId | Domain | Name | EffectiveFrom | Value | Unit | Source | Article | Verification |
|---|---|---|---|---|---|---|---|---|
| LAO-PIT-2026-BRACKET-01 | Tax | PIT bracket 1 (tax-free) | Jul 2026 | 0-2,500,000 | LAK/month, 0% | Law 88/NA | — | LOCKED |
| LAO-PIT-2026-BRACKET-02 | Tax | PIT bracket 2 | Jul 2026 | 2.5M-5M | LAK/month, 5% | Law 88/NA | — | LOCKED |
| LAO-PIT-2026-BRACKET-03 | Tax | PIT bracket 3 | Jul 2026 | 5M-15M | LAK/month, 10% | Law 88/NA | — | LOCKED |
| LAO-PIT-2026-BRACKET-04 | Tax | PIT bracket 4 | Jul 2026 | 15M-25M | LAK/month, 15% | Law 88/NA | — | LOCKED |
| LAO-PIT-2026-BRACKET-05 | Tax | PIT bracket 5 | Jul 2026 | 25M-65M | LAK/month, 20% | Law 88/NA | — | LOCKED |
| LAO-PIT-2026-BRACKET-06 | Tax | PIT bracket 6 | Jul 2026 | 65M+ | LAK/month, 25% | Law 88/NA | — | LOCKED |
| LAO-PIT-DEPENDENT | Tax | Dependant deduction | current | 5,000,000 | LAK/year/dependent, max 3 | Law 88/NA | — | LOCKED |
| LAO-PIT-OT-EXEMPT | Tax | OT exempt for low earners | current | base < 3M | LAK/month threshold | PwC | — | LOCKED |
| LAO-PIT-WITHHOLD-DEADLINE | Tax | Monthly withholding due | current | 20th | day of following month | PwC | — | LOCKED |
| LAO-PIT-ANNUAL-DEADLINE | Tax | Annual return due | current | 31 March | day | PwC | — | LOCKED |
| LAO-PIT-PENALTY | Tax | Late penalty | current | 0.1% | per day of delay | PwC | — | LOCKED |
| LAO-NSSF-EMPLOYER-RATE | Social Security | Employer rate | 2016 | 6.0% | of gross remuneration | PwC | — | LOCKED |
| LAO-NSSF-EMPLOYEE-RATE | Social Security | Employee rate | 2016 | 5.5% | of gross remuneration | PwC | — | LOCKED |
| LAO-NSSF-CEILING | Social Security | Contribution ceiling | current | 4,500,000 | LAK/month | Notification 0824/NSSFO | — | LOCKED |
| LAO-NSSF-DEADLINE | Social Security | Payment due | current | 20th | day of following month | PwC | — | LOCKED |
| LAO-NSSF-FOREIGN | Social Security | Foreign workers | current | >12 months | must register | PwC | — | LOCKED |
| LAO-MIN-WAGE | Labour | Minimum wage | Oct 2024 | 2,500,000 | LAK/month | MOLSW decree | — | LOCKED |
| LAO-MIN-WAGE-UNSKILLED | Labour | Unskilled subsistence | Oct 2024 | +900,000 | LAK/month | MOLSW | — | LOCKED |
| LAO-OT-WEEKDAY-DAY | Labour | OT weekday day | 2006 | 1.5× | multiplier | Law 06/NA | Art. 48 | LOCKED |
| LAO-OT-WEEKDAY-NIGHT | Labour | OT weekday night | 2006 | 2× | multiplier | Law 06/NA | Art. 48 | LOCKED |
| LAO-OT-RESTDAY-DAY | Labour | OT rest day day | 2006 | 2.5× | multiplier | Law 06/NA | Art. 48 | LOCKED |
| LAO-OT-RESTDAY-NIGHT | Labour | OT rest day night | 2006 | 3× | multiplier | Law 06/NA | Art. 48 | LOCKED |
| LAO-OT-NIGHT-BONUS | Labour | Night bonus | 2006 | 15% | of hourly rate, 22:00-05:00 | Law 06/NA | Art. 48 | LOCKED |
| LAO-OT-MAX-DAY | Labour | Max OT/day | 2006 | 3 | hours | Law 06/NA | Art. 18 | LOCKED |
| LAO-OT-MAX-MONTH | Labour | Max OT/month | 2006 | 45 | hours | Law 06/NA | Art. 18 | LOCKED |
| LAO-LEAVE-ANNUAL | Leave | Annual leave | 2006 | 15 | days/year, full pay | Law 06/NA | Art. 21 | LOCKED |
| LAO-LEAVE-ANNUAL-HAZARD | Leave | Annual leave (hazardous) | 2006 | 18 | days/year | Law 06/NA | Art. 21 | LOCKED |
| LAO-LEAVE-SICK | Leave | Sick leave | 2006 | 30 | days/year, full pay | Law 06/NA | Art. 20 | LOCKED |
| LAO-LEAVE-MATERNITY | Leave | Maternity leave | 2006 | ≥90 | days, full pay | Law 06/NA | Art. 39 | LOCKED |
| LAO-LEAVE-MATERNITY-ALLOW | Leave | Maternity allowance | 2006 | ≥60% | of min wage | Law 06/NA | Art. 40 | LOCKED |
| LAO-PROBATION | Labour | Probation | 2006 | 30/60 | days (physical/skilled) | Law 06/NA | Art. 27 | LOCKED |
| LAO-NOTICE-INDEFINITE | Labour | Notice (indefinite) | 2006 | 30/45 | days (physical/skilled) | Law 06/NA | Art. 28 | LOCKED |
| LAO-SEVERANCE-DISMISS | Labour | Severance (dismissal) | 2006 | 10%/15% | of monthly salary per month worked (<3yr/>3yr) | Law 06/NA | Art. 29 | LOCKED |
| LAO-SEVERANCE-UNJUST | Labour | Severance (unjustified) | 2006 | 15%/20% | of monthly salary per month worked (<3yr/>3yr) | Law 06/NA | Art. 33 | LOCKED |
| LAO-FOREIGN-QUOTA | Labour | Foreign worker quota | 2006 | 10%/20% | of workforce (physical/intellectual) | Law 06/NA | Art. 25 | LOCKED |
| LAO-WORK-HOURS | Labour | Working hours | 2006 | 8/48 | hours/day, hours/week | Law 06/NA | Art. 16 | LOCKED |
| LAO-OT-DIVISOR | Labour | OT hourly divisor | — | UNKNOWN | — | — | — | PROFESSIONAL_CONFIRMATION_REQUIRED |
| LAO-LEAVE-CARRYOVER | Leave | Carry-over rules | — | UNKNOWN | — | — | — | PROFESSIONAL_CONFIRMATION_REQUIRED |

## 13 — Legal floor vs company policy

| Rule | Statutory minimum (LOCKED) | Company improvement (configurable) | LaoHR support |
|---|---|---|---|
| Annual leave | 15 days (Art. 21) | Company may grant more (e.g., 18) | `LeavePolicy.AnnualQuota` ✅ |
| Sick leave | 30 days full pay (Art. 20) | Company may extend | `LeavePolicy` ✅ |
| Maternity | ≥90 days full pay (Art. 39) | Company may extend/ top up | `LeavePolicy` ✅ |
| OT multipliers | 1.5/2/2.5/3× (Art. 48) | Company may pay more | NOT MODELLED — needs OvertimeEntry |
| Minimum wage | 2,500,000 (Oct 2024) | Company pays above | `Employee.BaseSalary` ✅ |
| Work hours | 8h/day, 48h/week (Art. 16) | Company may reduce | `WorkSchedule` ✅ |
| Notice period | 30/45 days (Art. 28) | Company may require more | NOT MODELLED — needs termination fields |
| Severance | 10%/15% or 15%/20% (Art. 29/33) | Company may pay more | NOT MODELLED — needs severance engine |
| Probation | 30/60 days (Art. 27) | — | NOT MODELLED — needs ProbationEndDate |
| Paternity/marriage/bereavement | NOT STATUTORY | Company policy | `LeavePolicy` can support (if seeded) |

## 14 — Configuration classification

| Rule | CODE | CONFIGURATION | VERSIONED_DB | ANNUAL_CALENDAR | COMPANY_POLICY | EXTERNAL_REF | Recommendation |
|---|---|---|---|---|---|---|---|
| PIT brackets | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | VERSIONED_DB (with EffectiveFrom/To) |
| PIT dependant deduction | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | VERSIONED_DB |
| NSSF rates | ❌ | ⚠️ (current) | ✅ | ❌ | ❌ | ❌ | VERSIONED_DB (with history) |
| NSSF ceiling | ❌ | ⚠️ (current) | ✅ | ❌ | ❌ | ❌ | VERSIONED_DB |
| NSSF deadline (20th) | ⚠️ (rarely changes) | ✅ | ❌ | ❌ | ❌ | ❌ | CONFIGURATION (with legal default) |
| Minimum wage | ❌ | ❌ | ✅ | ❌ | ❌ | ❌ | VERSIONED_DB (for validation) |
| OT multipliers | ❌ | ✅ | ✅ | ❌ | ✅ (can pay more) | ❌ | CONFIGURE + VERSIONED (legal floor + company uplift) |
| OT caps | ❌ | ✅ | ✅ | ❌ | ✅ | ❌ | CONFIGURE |
| OT divisor | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ | CONFIGURE (once confirmed) |
| Leave quotas | ❌ | ✅ (LeavePolicy) | ⚠️ (add EffectiveFrom/To) | ❌ | ✅ | ❌ | CONFIGURE + VERSIONED |
| Leave types | ⚠️ (enum) | ✅ | ❌ | ❌ | ✅ | ❌ | CONFIGURE |
| Public holidays | ❌ | ❌ | ❌ | ✅ | ✅ | ❌ | ANNUAL_CALENDAR |
| Work schedule | ❌ | ✅ | ❌ | ❌ | ✅ | ❌ | CONFIGURE |
| Probation | ⚠️ (30/60 default) | ✅ | ❌ | ❌ | ✅ | ❌ | CONFIGURE (with legal default) |
| Notice periods | ❌ | ✅ | ❌ | ❌ | ✅ | ❌ | CONFIGURE |
| Severance | ⚠️ (formula) | ✅ | ❌ | ❌ | ✅ | ❌ | CONFIGURE (with legal formula) |
| Foreign worker quota | ⚠️ (10%/20%) | ✅ | ❌ | ❌ | ❌ | ❌ | CONFIGURE (with legal default) |
| Exchange rates | ❌ | ✅ | ✅ (has history) | ❌ | ❌ | ❌ | VERSIONED_DB (current is good) |
| Bank transfer format | ❌ | ✅ | ❌ | ❌ | ❌ | ✅ | EXTERNAL_REF (bank-specific) |