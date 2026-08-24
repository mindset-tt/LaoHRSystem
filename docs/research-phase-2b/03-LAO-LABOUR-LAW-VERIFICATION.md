# 03 — Lao Labour Law Verification + 04 — Working Hours & Overtime + 05 — Leave Verification

> Research date: 2026-08-21. Sources: ILO NATLEX (403), GlobaLex (VERIFIED), Lao Official Gazette (LIVE), Laotian Times, HR guides.
> ⚠️ NOT legal advice. Article numbers NOT verified from primary text.

## 03 — Labour law verification

| Field | Value | Classification | Source quality | Freshness |
|---|---|---|---|---|
| Primary statute | Labour Law, **Law No. 006/NOC, 27 Dec 2006** | D (secondary, widely cited) | 4 (ILO record exists) | Old But Effective |
| Amendment | Law No. 052/NOC, 26 Dec 2013 (commonly cited) | D | 3 | Possibly Current |
| Issuing authority | National Assembly (ສະພາແຫງຊາດ) | A (GlobaLex + Constitution) | 5 | Current |
| Implementing ministry | MOLSW (ກະຊວງແຮງງານ ແລະ ສະຫວັດດີການສັງຄົມ) | A | 5 | Current |
| Current version status | 2006 law as amended 2013 remains governing; newer amendment discussed but unconfirmed | G (unverified) | — | Unknown |
| Official Gazette | laoofficialgazette.gov.la — LIVE, hosts legislation PDFs | A | 5 | Current |
| MOLSW decree found | "ຂໍ້ຕົກລົງວ່າດ້ວຍ ການກຳນົດວຽກເບົາ ແລະ ວຽກທີ່ເປັນອັນຕະລາຍ" (light/hazardous work decree) | A (Official Gazette) | 5 | Current |

**Lao terms**: ກົດໝາຍແຮງງານ (kat mai haeng ngan) = Labour Law · ພະນັກງານ (phana kngan) = employee · ສັນຍາແຮງງານ (sanya haeng ngan) = employment contract · ນາຍຈ້າງ (nai chng) = employer · ລູກຈ້າງ (luk chng) = employee/worker.

**Employment contracts**: Written contracts required; categories: indefinite (permanent), fixed-term, seasonal/project-based. Classification: D (secondary). Confidence: HIGH (universally reported).

**Probation**: Commonly cited 30 days (ordinary), 60 days (technical). Classification: D. Confidence: LOW — article not verified.

**Termination/severance**: Notice periods + severance required by law; exact tiers/formula UNKNOWN. Classification: H. DFDL 2026 seminar agenda mentions "severance rules and calculations" — confirming the topic is regulated, but specifics not retrieved.

## 04 — Working hours & overtime

### Working hours

| Rule | Value | Classification | Confidence |
|---|---|---|---|
| Max daily hours | 8 hours/day | D | MEDIUM |
| Max weekly hours | 48 hours/week (6×8) | D | MEDIUM |
| Rest interval | Mandated; exact duration UNKNOWN | H | UNKNOWN |
| Weekly rest | ≥1 day/week (typically Sunday) | D | MEDIUM |
| Night work | ~22:00–05:00; premium applies | H | UNKNOWN |
| Hazardous work | Reduced hours (MOLSW decree exists on Official Gazette) | A (decree exists) | MEDIUM (decree confirmed, numbers not retrieved) |

**LaoHR**: `WorkSchedule.DailyWorkHours=8`, `StandardMonthlyHours=160` (20 days). VERIFIED consistent with 8h/day.

### Overtime

| Rule | Value | Classification | Confidence |
|---|---|---|---|
| Ordinary weekday OT | 1.5× normal hourly rate | D | MEDIUM |
| Night OT multiplier | Premium applies; exact UNKNOWN | H | UNKNOWN |
| Rest day/weekend OT | 2× (some: 2.5× or 3×) | E (CONFLICTING) | LOW |
| Public holiday OT | 3× | D | MEDIUM |
| Max OT/day | 3 hours | D | MEDIUM |
| Max OT/month | 45h (some: 48h) | E (CONFLICTING) | LOW |
| OT authorization | Employer/employee agreement | D | MEDIUM |
| Calculation base (hourly rate divisor) | UNKNOWN (÷26? ÷30? ÷working days?) | H | UNKNOWN |

**LaoHR**: Single `OvertimePay` field — no day-type differentiation. GAP: P1 (compliance). The `LeavePolicy`/`WorkSchedule` architecture is configurable (good), but the payroll engine needs `OvertimeEntry` (date, hours, type, rate).

**⚠️ CONFLICTING**: Rest-day OT (2× vs 2.5×/3×) and monthly OT cap (45h vs 48h) conflict across sources. **Do NOT hardcode until confirmed from law text.**

## 05 — Leave verification

| Leave type | Lao term | Entitlement | Classification | Confidence |
|---|---|---|---|---|
| Annual leave | ລາພັກປະຈຳປີ | 12–15 days/year after 1 year + public holidays | E (CONFLICTING) | LOW |
| Sick leave | ລາປ່ວຍ | ~30 days/year paid (longer via NSSF) | D | LOW |
| Maternity leave | ລາເກີດລູກ | 90–105 days, full pay (NSSF maternity) | E (CONFLICTING) | LOW |
| Paternity leave | ລາພັກພໍ່ | ~3 days | D | LOW (may not be statutory) |
| Marriage leave | ລາພັກແຕ່ງງານ | ~3 days | D | LOW (may not be statutory) |
| Bereavement leave | ລາພັກຍ້າຍສົບ | ~3 days | D | LOW (may not be statutory) |
| Unpaid leave | ລາພັກບໍ່ໄດ້ຮັບຄ່າຈ້າງ | By agreement | D | LOW |
| Carry-over | — | Rules exist; specifics UNKNOWN | H | UNKNOWN |
| Accrual | — | Monthly accrual; specifics UNKNOWN | H | UNKNOWN |
| Eligibility | — | Annual leave after 1 year; exact threshold UNKNOWN | D | MEDIUM |

**⚠️ CONFLICTING**: Annual leave (12 vs 15 days) and maternity (90 vs 105 days) conflict across HR guides. **Do NOT hardcode a single number until confirmed from law text.**

**LaoHR**: `LeavePolicy` with configurable `AnnualQuota`, `AccrualPerMonth`, `MaxCarryOver`, `RequiresAttachment`, `AllowHalfDay`. Architecture is good (configurable). The seeded values need verification against the law. GAP: P0 (verify seeded values).

### Law → Current implementation → Gap → Required change

| LAW | CURRENT | GAP | REQUIRED | Priority |
|---|---|---|---|---|
| Annual leave 12-15 days | Configurable `AnnualQuota` | Verify seeded value | Confirm + update seed | P0 |
| Maternity 90-105 days | MATERNITY type supported | Verify seeded days + funding | Confirm + update seed | P0 |
| Sick ~30 days | SICK type supported | Verify seeded days + pay rate | Confirm + update seed | P1 |
| Paternity/marriage/bereavement ~3 days | May not be seeded | Add if statutory | Confirm + seed | P2 |
| OT 1.5×/2×/3× | Single OvertimePay | No day-type | Add OvertimeEntry | P1 |
| OT cap 45h | Not enforced | Can exceed | Add validation | P2 |
| Probation 30/60 days | No field | Cannot track | Add ProbationEndDate | P2 |
| Severance | No model | Cannot calculate | Add termination model | P2 |
| Notice periods | No model | Cannot track | Add notice fields | P2 |