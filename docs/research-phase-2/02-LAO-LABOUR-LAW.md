# 02 — Lao Labour Law (ກົດໝາຍແຮງງານ)

> Research date: 2026-08-21. Sources: ILO NATLEX, Wikipedia, Trading Economics (citing Lao Ministry of Finance / MOLSW), WageIndicator.
> ⚠️ **LEGAL DISCLAIMER**: This research supports software requirements. It is NOT legal advice. Items marked UNKNOWN require confirmation from a qualified Lao legal/labour professional.

## Governing law

| Field | Value | Confidence |
|---|---|---|
| **Primary statute** | Labour Law of Lao PDR, **Law No. 006/NOC, dated 27 December 2006** (ກົດໝາຍແຮງງານ ສະບັບເລກທີ 006/NOC ລົງວັນທີ 27 ທັນວາ 2006) | HIGH (law exists, widely cited) |
| **Amendments** | Notable amendment in 2013; further implementing decrees/ordinances have followed | MEDIUM (exact amendment dates unconfirmed) |
| **Issuing authority** | National Assembly of Lao PDR | VERIFIED |
| **Implementing ministry** | Ministry of Labour and Social Welfare (MOLSW / ກະຊວງແຮງງານ ແລະ ສະຫວັດດີການສັງຄົມ) | VERIFIED |
| **ILO NATLEX** | ISN 92707 (record exists; page did not render in automated fetch) | MEDIUM |

> **Lao term**: ກົດໝາຍແຮງງານ (kat mai haeng ngan) = Labour Law. ພະນັກງານ (phana kngan) = employee/official. ສັນຍາແຮງງານ (sanya haeng ngan) = employment contract.

## Employment

| Rule | Detail | Confidence | Evidence |
|---|---|---|---|
| Employment relationships | Written contracts required; categories: indefinite (permanent), fixed-term, seasonal/project-based | HIGH | Commonly reported across HR guides |
| Employee classifications | ພະນັກງານລັດ (state employee), ພະນັກງານເອກະຊົນ (private employee) | HIGH | Lao terminology |
| Probation | Commonly cited as up to **30 days** for ordinary workers (up to 60 days for technical/specialised roles) | MEDIUM/UNKNOWN — confirm exact article | HR guides |
| Contract types | Permanent, fixed-term, seasonal | HIGH | |
| Termination | Notice periods + severance based on years of service required by law | UNKNOWN — exact formula requires confirmation | |
| Severance | Required; formula based on years of service | UNKNOWN — exact formula requires confirmation | |
| Disciplinary procedures | Graduated: warning → suspension → dismissal | UNKNOWN — specifics require confirmation | |

## Working hours

| Rule | Detail | Confidence | Evidence |
|---|---|---|---|
| Normal daily hours | **8 hours/day** | MEDIUM | Commonly reported; confirm in Labour Law article |
| Normal weekly hours | **48 hours/week** (6 days × 8 hours) | MEDIUM | Commonly reported |
| Rest period | At least **1 rest day per week** (typically Sunday) | MEDIUM | |
| Night work | Generally defined as 22:00–05:00; premium pay applies | UNKNOWN — exact definition/rate requires confirmation | |
| Weekly limit | 48 hours/week standard | MEDIUM | |

**LaoHR current implementation**: `WorkSchedule` entity — Mon–Sun flags, `DailyWorkHours=8`, `StandardMonthlyHours=160` (= 20 days × 8h). Saturday config (NONE/FULL/HALF). `LateThresholdMinutes=15`.
- **GAP**: LaoHR's `StandardMonthlyHours=160` assumes 20 workdays/month (Mon–Fri). The law allows up to 48h/week (6×8). If Saturday work is configured, monthly hours should be higher (23–26 days). `WorkSchedule.GetWorkDaysPerMonth()` already handles this (HALF=23, FULL=26). **VERIFIED consistent.**
- **GAP**: Night work definition and premium not modelled. `UNKNOWN` whether needed for target organizations.

## Overtime

| Rule | Detail | Confidence | Evidence |
|---|---|---|---|
| Normal working day OT | **1.5× normal hourly rate** | MEDIUM | Consistently reported across HR guides |
| Weekend/rest day work | **2× normal hourly rate** (some sources: 2.5× or 3× depending on compensatory rest) | MEDIUM | |
| Official public holidays | **3× normal hourly rate** | MEDIUM | |
| OT limit/day | Commonly cited: **max 3 hours/day** | MEDIUM | |
| OT limit/month | Commonly cited: **max 45 hours/month** (some: 48h) | MEDIUM | |
| OT authorization | Requires employer/employee agreement | MEDIUM | |

**LaoHR current implementation**: `SalarySlip.OvertimePay` is a single computed field; `PayrollService` calculates overtime. `WorkSchedule` has no night-work multiplier. Overtime is treated as a single amount, not differentiated by day type (normal/rest/holiday).
- **GAP**: LaoHR does NOT differentiate overtime by day type (1.5×/2×/3×). If the law requires this, the payroll engine needs a `OvertimeEntry` model (date, hours, type: normal/rest/holiday/night) → rate → amount. **P1 compliance gap — requires legal confirmation of exact multipliers.**
- **GAP**: OT monthly cap (45h) not enforced in the system. `P2` — validation/reporting.

## Termination & severance

| Rule | Confidence |
|---|---|
| Notice periods based on service length | UNKNOWN — requires confirmation |
| Severance based on years of service | UNKNOWN — requires confirmation |
| Disciplinary process: warning → suspension → dismissal | UNKNOWN — requires confirmation |

**LaoHR current**: Employee has `IsActive` flag (soft delete); no termination/severance model.
- **GAP**: No termination reason, notice date, severance calculation, exit checklist. `P2` — future HR capability.

## LaoHR gap summary (employment)

| LAW | CURRENT IMPLEMENTATION | GAP | REQUIRED FUTURE CHANGE | Priority |
|---|---|---|---|---|
| Probation period (30/60 days) | No probation field on Employee | Cannot track probation end | Add `ProbationEndDate`, `EmploymentType` (permanent/fixed-term/seasonal/probation) | P2 |
| Contract types | No `ContractType` field | Cannot distinguish contract types | Add `EmploymentType` enum | P2 |
| Termination/severance | `IsActive` flag only | No termination tracking, severance calc | Add termination model + severance engine | P2 |
| Disciplinary process | None | No disciplinary workflow | Add disciplinary case tracking | P3 |
| Night work premium | Not modelled | Cannot calculate night OT | Add night-work rate to payroll | P2 (if applicable) |
| OT day-type differentiation | Single `OvertimePay` | No 1.5×/2×/3× distinction | Add `OvertimeEntry` model | P1 (compliance) |
| OT monthly cap | Not enforced | Can exceed 45h/month | Add validation/reporting | P2 |