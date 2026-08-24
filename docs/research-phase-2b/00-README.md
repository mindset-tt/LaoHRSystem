# 00 — Phase 2B README

> Lao Evidence Hardening, Lao-Language Deep Research, Regulatory Validation, Bilingual Requirement Specification.
> Research date: 2026-08-21. READ-ONLY — no code modified.

## Purpose

Phase 2 produced broad research but left many Lao-specific findings as UNVERIFIED, MEDIUM, or UNKNOWN. Phase 2B reduces those uncertainties through deeper web research (English + ພາສາລາວ), primary-source hunting, and claim auditing.

## Critical new findings (vs Phase 2)

1. **🚨 PIT tax-free threshold changed**: Presidential decree (6 Aug 2025) raised the threshold from LAK 1,300,000 → **LAK 2,500,000**, effective June 2026. The codebase seed (`LaoHRDbContext.cs:227`) still has the OLD threshold (1,300,000). **This is a P0 compliance bug.**
2. **PIT law identified**: Income Tax Law **No. 67/NA** (approved June 2019, effective 17 Feb 2020), replaced Tax Law No. 70/NA (2015). Phase 2 incorrectly cited "Tax Administration 2015."
3. **NSSF ceiling seeded in code**: `NSSF_CEILING_BASE = 4,500,000` exists in `LaoHRDbContext.cs:236` but **could NOT be verified from any external source** (LOW confidence).
4. **Lao Official Gazette is live**: `laoofficialgazette.gov.la` — authoritative primary source for all Lao legislation. A Lao-speaking researcher should search it directly.
5. **No dedicated data protection law found**: Laos has Electronic Transactions Law (2012) + E-Commerce Decree (2021) but NO confirmed comprehensive PDPL. Constitutional Article 29 covers bodily/house inviolability.
6. **Constitution amended March 2025**: New constitution ratified 10 March 2025; article numbers may have shifted.
7. **Minimum wage under review**: As of March 2026, a further increase to ~LAK 4,100,000 was proposed but not enacted.
8. **proxy.ts confirmed correct**: Next.js 16 renamed `middleware.ts` → `proxy.ts` (VERIFIED in Phase 2). Phase 2B reconfirms — not a bug.

## Document index

| # | Document | Domain |
|---|---|---|
| 00 | `00-README.md` | This file |
| 01 | `01-PHASE2-CLAIM-AUDIT.md` | Audit of every Phase 2 Lao claim |
| 02 | `02-LAO-PRIMARY-SOURCE-REGISTER.md` | Primary sources found + status |
| 03 | `03-LAO-LABOUR-LAW-VERIFICATION.md` | Labour law deep verification |
| 04 | `04-LAO-WORKING-HOURS-OVERTIME.md` | Working hours + overtime verification |
| 05 | `05-LAO-LEAVE-VERIFICATION.md` | Leave day-counts verification |
| 06 | `06-LAO-PAYROLL-MODEL.md` | Full payroll flow reconstruction |
| 07 | `07-LAO-PIT-VERIFICATION.md` | PIT brackets + law verification |
| 08 | `08-LAO-NSSF-VERIFICATION.md` | NSSF rates + ceiling verification |
| 09 | `09-LAO-MINIMUM-WAGE.md` | Minimum wage verification |
| 10 | `10-LAO-PUBLIC-HOLIDAYS.md` | Public holidays verification |
| 11 | `11-LAO-FOREIGN-WORKERS.md` | Foreign worker requirements |
| 12 | `12-LAO-DATA-PRIVACY.md` | Data protection / privacy law |
| 13 | `13-LAO-BANKING-PAYROLL.md` | Banking / payroll transfer |
| 14 | `14-LAO-GOVERNMENT-REPORTING.md` | Government reporting requirements |
| 15 | `15-LAO-HR-GLOSSARY.md` | Bilingual domain glossary |
| 16 | `16-LAO-UI-TERMINOLOGY.md` | Lao UI label research |
| 17 | `17-LAO-NAMES-ADDRESSES-PHONES.md` | Names, addresses, phone validation |
| 18 | `18-LAO-UNICODE-SEARCH.md` | Unicode + search deep research |
| 19 | `19-LAO-FONTS-PDF-PRINT.md` | Fonts, PDF, print, licensing |
| 20 | `20-PAYROLL-RULE-VERSIONING.md` | Payroll rule versioning architecture |
| 21 | `21-HISTORICAL-PAYROLL-REPRODUCIBILITY.md` | Historical payroll reproducibility |
| 22 | `22-LAW-VS-COMPANY-POLICY.md` | Law vs company policy separation |
| 23 | `23-HARDCODE-VS-CONFIGURE-MATRIX.md` | Hardcode vs configure decision matrix |
| 24 | `24-TECHNICAL-CLAIM-REVALIDATION.md` | Technical architecture claim revalidation |
| 25 | `25-SECURITY-CLAIM-REVALIDATION.md` | Security claim revalidation |
| 26 | `26-SCALE-WORKLOAD-RESEARCH.md` | Scale workload models |
| 27 | `27-COMPETITOR-LESSONS.md` | Competitor lessons (deep, not wide) |
| 28 | `28-LAO-PRODUCT-PRIORITY-MATRIX.md` | Lao-specific feature priority |
| 29 | `29-AI-LAO-READINESS.md` | AI/NLP Lao readiness |
| 30 | `30-EVIDENCE-REGISTER.md` | Evidence register (updated) |
| 31 | `31-CLAIM-LEDGER.md` | Claim ledger (old → new confidence) |
| 32 | `32-CORRECTIONS-TO-PHASE-2.md` | Corrections to Phase 2 |
| 33 | `33-CONFLICTING-SOURCES.md` | Conflicting sources register |
| 34 | `34-UNRESOLVED-LAO-QUESTIONS.md` | Unresolved questions |
| 35 | `35-PROFESSIONAL-CONFIRMATION-QUESTIONS.md` | Questions for professionals |
| 36 | `36-BILINGUAL-LAOHR-REQUIREMENTS.md` | Bilingual requirement specifications |
| 37 | `37-PHASE-2B-EXECUTIVE-SYNTHESIS.md` | Final synthesis |
| 38 | `38-PHASE-3-READINESS-GATE.md` | Phase 3 readiness gate |

## Confidence system

A — VERIFIED BY PRIMARY LAO SOURCE (Official Gazette, ministry, law text)
B — VERIFIED BY AUTHORITATIVE INTERNATIONAL SOURCE (ILO, World Bank, W3C, OWASP, official docs)
C — SUPPORTED BY REPUTABLE PROFESSIONAL SOURCE (PwC, KPMG, DFDL, Tilleke)
D — SECONDARY SOURCE ONLY (Trading Economics, Wikipedia, Laotian Times, WageIndicator)
E — CONFLICTING SOURCES
F — OUTDATED
G — UNVERIFIED
H — UNKNOWN

## Source quality score

5 = Lao primary official source (Official Gazette, ministry, law PDF)
4 = International primary/authoritative (ILO, W3C, PostgreSQL docs, Microsoft Learn)
3 = Recognized professional interpretation (PwC, KPMG, DFDL, Laotian Times citing MOF)
2 = Secondary reference (Wikipedia, Trading Economics, WageIndicator)
1 = Weak/general source (blogs, SEO articles)
0 = Unusable

## Legal disclaimer

This research supports software requirements. It is **NOT legal advice**. All Lao tax/NSSF/labour law parameters must be confirmed by a qualified Lao legal/tax/labour professional before production use.