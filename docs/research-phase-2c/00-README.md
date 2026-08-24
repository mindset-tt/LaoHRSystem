# 00 — Phase 2C README

> Critical Lao Compliance Closure & Production Rule Freeze. Research date: 2026-08-21. READ-ONLY — no code modified.

## Purpose

Phase 2C closes the remaining critical Lao statutory compliance gaps from Phase 2B, using primary-source evidence where available, and producing a final compliance lock status + professional confirmation pack for items that remain publicly unavailable.

## Major breakthroughs in Phase 2C

1. **Complete Lao Labour Law text found** (Law No. 06/NA, 27 Dec 2006) on AsianLII — primary source (quality 5/5). Resolves ALL leave and overtime conflicts.
2. **PwC Worldwide Tax Summaries for Laos** — quality 5/5, VERIFIED. Resolves NSSF ceiling, PIT deductions, foreign employee treatment, filing deadlines.
3. **NSSF ceiling CONFIRMED**: LAK 4,500,000 (Notification No. 0824/NSSFO from MOLSW). VERIFIED.
4. **NSSF payment deadline**: 20th of following month. VERIFIED.
5. **PIT dependant deduction**: LAK 5M per dependent (max 3, LAK 15M/year). VERIFIED.
6. **OT pay exempt from PIT** for employees with base salary < LAK 3M/month. VERIFIED.
7. **Foreign workers: same PIT brackets, no surcharge.** VERIFIED.
8. **PIT withholding due 20th of following month; annual return by 31 March.** VERIFIED.
9. **Amended Income Tax Law No. 88/NA** (25 June 2025) — not just a "presidential decree". VERIFIED.
10. **OT multipliers RESOLVED**: 1.5× (weekday day), 2× (weekday night), 2.5× (rest day day), 3× (rest day night) — Article 48. VERIFIED.
11. **Annual leave RESOLVED**: 15 days/year (18 for hazardous). Article 21. VERIFIED.
12. **Maternity leave RESOLVED**: at least 90 days, full pay. Article 39. VERIFIED.
13. **Severance RESOLVED**: 10%/15% (dismissal), 15%/20% (unjustified). Articles 29, 33. VERIFIED.
14. **Foreign worker quota RESOLVED**: 10% (physical), 20% (intellectual). Article 25. VERIFIED.

## What remains unresolved

- NSSF contribution base definition (base only? +OT? +allowances?)
- NSSF minimum contribution floor
- OT hourly rate divisor (monthly ÷ what?)
- Annual leave carry-over rules
- Paternity/marriage/bereavement leave (not in private-sector Labour Law — may be in regulations)
- Bank salary batch file specifications (not publicly documented)
- Specific visa/stay permit categories for employment
- Biometric data regulations
- Employee data retention requirements
- Labour Law amendments since 2006 (2013 amendment commonly cited but not verified)

## Document index

| # | Document | Domain |
|---|---|---|
| 00 | `00-README.md` | This file |
| 01 | `01-UNRESOLVED-CRITICAL-REGISTER.md` | All unresolved items at start + status |
| 02 | `02-NSSF-FINAL-RESEARCH.md` | NSSF rates, ceiling, base, deadlines |
| 03 | `03-LEAVE-FINAL-RESEARCH.md` | All leave types with article numbers |
| 04 | `04-OVERTIME-FINAL-RESEARCH.md` | OT multipliers, caps, divisor |
| 05 | `05-PIT-FINAL-RESEARCH.md` | PIT brackets, deductions, deadlines |
| 06 | `06-TERMINATION-SEVERANCE.md` | Probation, notice, severance formulas |
| 07 | `07-FOREIGN-WORKERS.md` | Quota, work permit, NSSF, tax |
| 08 | `08-DATA-PRIVACY-BIOMETRICS.md` | Privacy law, e-commerce, data localization |
| 09 | `09-BANK-PAYROLL.md` | Bank payroll batch research |
| 10 | `10-GOVERNMENT-REPORTING.md` | PIT/NSSF/labour reporting obligations |
| 11 | `11-COMPLIANCE-RULE-HISTORY.md` | Historical versioning of all rules |
| 12 | `12-COMPLIANCE-RULE-SPECIFICATION.md` | Normalized rule specification table |
| 13 | `13-LEGAL-FLOOR-VS-COMPANY-POLICY.md` | Statutory minimum vs company improvement |
| 14 | `14-CONFIGURATION-CLASSIFICATION.md` | Hardcode vs configure vs versioned DB |
| 15 | `15-PROFESSIONAL-CONFIRMATION-PACK.md` | Questions for professionals |
| 16 | `16-LAO-BILINGUAL-PROFESSIONAL-QUESTIONS.md` | Bilingual questions (EN + Lao) |
| 17 | `17-EVIDENCE-PACK.md` | Evidence pack for professional confirmation |
| 18 | `18-COMPLIANCE-LOCK-STATUS.md` | LOCKED vs TEMPORARY vs BLOCKED per rule |
| 19 | `19-DO-NOT-IMPLEMENT-YET.md` | What must not freeze business logic yet |
| 20 | `20-TECHNICAL-VS-COMPLIANCE-WORKSTREAMS.md` | Track A/B/C separation |
| 21 | `21-PRODUCTION-PAYROLL-READINESS.md` | Production payroll gate |
| 22 | `22-PHASE-3-FINAL-READINESS.md` | Final readiness by area |

## Sources found in Phase 2C

| Source | URL | Quality | Status |
|---|---|---|---|
| **AsianLII — Lao Labour Law (Law No. 06/NA, 2006)** | asianlii.org/la/legis/laws/lol2007123/ | 5 (primary law text) | ✅ VERIFIED |
| **PwC Worldwide Tax Summaries — Lao PDR** | taxsummaries.pwc.com/lao-pdr | 5 (professional, authoritative) | ✅ VERIFIED |
| AsianLII — 1991 Constitution | asianlii.org/la/legis/const/1991/1.html | 5 | ✅ VERIFIED |
| BCEL i-Bank product page | bcel.com.la/bcel/product-review.html | 4 | ✅ VERIFIED |
| Laotian Times (various) | laotiantimes.com | 3 | ✅ Worked |

## Legal disclaimer

This research supports software requirements. It is **NOT legal advice**. The Labour Law text is from 2006 (Law No. 06/NA). A 2013 amendment is commonly cited but not verified. All values should be confirmed as current by a qualified Lao legal/tax professional before production payroll use.