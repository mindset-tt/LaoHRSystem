# 01 — Phase 2 Claim Audit

> Audit of every important Lao-related claim from Phase 2. Classified A–H per the confidence system.
> Research date: 2026-08-21.

## Claim audit table

| # | Claim (Phase 2) | Phase 2 Confidence | Classification | New Evidence | New Confidence | Status |
|---|---|---|---|---|---|---|
| 1 | Minimum wage = LAK 2,500,000/month, eff. 1 Oct 2024 | VERIFIED | D (secondary) | Laotian Times + WageIndicator (both quality 3); decree number unknown | HIGH | CONFIRMED |
| 2 | PIT top marginal rate = 25% | VERIFIED | D (secondary) | Trading Economics citing MOF; Laotian Times confirms 25% top rate | HIGH | CONFIRMED |
| 3 | PIT brackets: 0/5/10/15/20/25% at 1.3M/2M/8.5M/12.5M/18.5M | UNKNOWN/UNVERIFIED | G (unverified) | **CORRECTED**: New brackets are 0/5/10/15/20/25% at **2.5M/5M/15M/25M/65M** (presidential decree 6 Aug 2025, eff. June 2026). Codebase has 1.3M threshold (OUTDATED). | HIGH | **CORRECTED** |
| 4 | PIT law = "Law on Tax Administration 2015, amended ~2021-2022" | HIGH | G (unverified) | **CORRECTED**: Income Tax Law **No. 67/NA** (June 2019, eff. 17 Feb 2020), replaced Tax Law No. 70/NA (2015). Amendment proposed June 2025. | HIGH | **CORRECTED** |
| 5 | NSSF employer = 6.0% | VERIFIED | D (secondary) | Trading Economics citing MOLSW; codebase seed matches | HIGH | CONFIRMED |
| 6 | NSSF employee = 5.5% | VERIFIED | D (secondary) | Trading Economics citing MOLSW; codebase seed matches | HIGH | CONFIRMED |
| 7 | NSSF total = 11.5% | VERIFIED | D (secondary) | Derived from 6.0+5.5 | HIGH | CONFIRMED |
| 8 | NSSF contribution ceiling = UNKNOWN | UNKNOWN | H (unknown) | Codebase has `NSSF_CEILING_BASE=4,500,000` — but NO external source confirms this value | LOW | UNRESOLVED |
| 9 | NSSF law = "Law on Social Security 2013" | HIGH | D (secondary) | ILO NATLEX ISN 91506 returned 403; law number not confirmed from primary source | MEDIUM | PARTIALLY_CONFIRMED |
| 10 | Working hours = 8h/day, 48h/week | MEDIUM | D (secondary) | Consistently reported across HR guides; article text NOT retrieved | MEDIUM | UNRESOLVED |
| 11 | OT normal day = 1.5× | MEDIUM | D (secondary) | Consistently reported; article NOT verified | MEDIUM | UNRESOLVED |
| 12 | OT rest day = 2× | MEDIUM | E (conflicting) | Some sources cite 2×, others 2.5× or 3× | LOW | UNRESOLVED (CONFLICTING) |
| 13 | OT public holiday = 3× | MEDIUM | D (secondary) | Consistently reported; article NOT verified | MEDIUM | UNRESOLVED |
| 14 | OT max/day = 3h | MEDIUM | D (secondary) | Commonly cited; unverified | MEDIUM | UNRESOLVED |
| 15 | OT max/month = 45h | MEDIUM | E (conflicting) | Some sources: 45h, others 48h | LOW | UNRESOLVED (CONFLICTING) |
| 16 | Annual leave = 12-15 days | MEDIUM | E (conflicting) | 12 vs 15 days across sources | LOW | UNRESOLVED (CONFLICTING) |
| 17 | Sick leave = ~30 days | MEDIUM | D (secondary) | Commonly cited; unverified | LOW | UNRESOLVED |
| 18 | Maternity leave = 90-105 days | MEDIUM | E (conflicting) | 90 vs 105 days across sources | LOW | UNRESOLVED (CONFLICTING) |
| 19 | Paternity leave = ~3 days | LOW | D (secondary) | Low confidence; may not be statutory | LOW | UNRESOLVED |
| 20 | Marriage leave = ~3 days | LOW | D (secondary) | Low confidence; may not be statutory | LOW | UNRESOLVED |
| 21 | Bereavement leave = ~3 days | LOW | D (secondary) | Low confidence; may not be statutory | LOW | UNRESOLVED |
| 22 | Probation = 30/60 days | MEDIUM/UNKNOWN | D (secondary) | Commonly cited; unverified | LOW | UNRESOLVED |
| 23 | Termination notice periods = UNKNOWN | UNKNOWN | H (unknown) | Not resolved | UNKNOWN | UNRESOLVED |
| 24 | Severance formula = UNKNOWN | UNKNOWN | H (unknown) | Not resolved | UNKNOWN | UNRESOLVED |
| 25 | Labour Law = Law No. 006/NOC, 27 Dec 2006 | HIGH | D (secondary) | Widely cited; ILO NATLEX record exists (403 on fetch); GlobaLex confirms | HIGH | CONFIRMED |
| 26 | Labour Law amended 2013 | MEDIUM | D (secondary) | Commonly cited as Law No. 052/NOC (26 Dec 2013); unverified from primary | MEDIUM | PARTIALLY_CONFIRMED |
| 27 | Timezone = UTC+07:00 (Asia/Vientiane) | VERIFIED | B (international) | Wikipedia + timeanddate.com | VERIFIED | CONFIRMED |
| 28 | LAK: att obsolete, whole-kip | VERIFIED | D (secondary) | Wikipedia | HIGH | CONFIRMED |
| 29 | Fiscal year = 1 Oct – 30 Sep | VERIFIED | D (secondary) | Wikipedia (Economy of Laos) | HIGH | CONFIRMED |
| 30 | Public holidays: fixed + lunisolar, annual decree | HIGH | D (secondary) | Wikipedia + commonly reported | HIGH | CONFIRMED |
| 31 | Lao data protection law = UNKNOWN | UNKNOWN | H (unknown) | **CONFIRMED**: No dedicated PDPL found; Electronic Transactions Law 2012 + E-Commerce Decree 2021 exist; Constitution amended March 2025 | MEDIUM | PARTIALLY_CONFIRMED (no PDPL, but ET law exists) |
| 32 | proxy.ts is a bug (wrong name) | VERIFIED (Phase 2 correction) | B (international) | Next.js 16 docs confirm proxy.ts is correct | VERIFIED | CONFIRMED (not a bug) |
| 33 | NSSF rates seeded in code = 5.5%/6.0% | — | A (codebase) | `LaoHRDbContext.cs:237-238` confirms seeds match | VERIFIED | CONFIRMED |
| 34 | PIT brackets seeded in code = old (1.3M threshold) | — | A (codebase) | `LaoHRDbContext.cs:227` has 1.3M threshold — **OUTDATED** vs new 2.5M decree | VERIFIED | **CORRECTED** (code is wrong) |
| 35 | NSSF ceiling seeded in code = 4,500,000 | — | A (codebase) | `LaoHRDbContext.cs:236` has 4,500,000 — **UNVERIFIED** from external sources | LOW | UNRESOLVED |
| 36 | Foreign worker quota = UNKNOWN | UNKNOWN | H (unknown) | Not resolved; commonly cited 10% but unverified | UNKNOWN | UNRESOLVED |
| 37 | BCEL exchange rates | — | A (codebase+web) | BCEL website live, rates 2026-08-21: USD 22,324/22,585 | VERIFIED | CONFIRMED |
| 38 | Constitution Article 38 = privacy | UNKNOWN | G (unverified) | **CORRECTED**: Article 38 (1991) = asylum for foreigners, NOT privacy. Article 29 = bodily/house inviolability. | VERIFIED | **CORRECTED** |

## Summary

| Status | Count |
|---|---|
| CONFIRMED | 13 |
| CORRECTED | 5 |
| PARTIALLY_CONFIRMED | 3 |
| UNRESOLVED | 12 |
| UNRESOLVED (CONFLICTING) | 4 |

**Most critical correction**: PIT tax-free threshold changed from 1.3M → 2.5M (presidential decree 6 Aug 2025, effective June 2026). Codebase seed is outdated.

**Most critical unresolved**: NSSF contribution ceiling (LAK 4,500,000 in code — unverified), leave day-counts (conflicting), overtime multipliers (conflicting), termination/severance (unknown).