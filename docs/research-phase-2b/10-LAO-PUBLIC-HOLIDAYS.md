# 10 — Public Holidays + 11 — Foreign Workers + 12 — Data Privacy + 13 — Banking + 14 — Government Reporting

> Research date: 2026-08-21. Sources: Laotian Times, BCEL (live), JDB (live), GlobaLex, AsianLII (Constitution), Official Gazette (live).

## 10 — Public holidays

### Verified 2026 holidays (from codebase seed — `LaoHRDbContext.cs:243-252`)

| # | Date | English name | Lao name | Recurring? |
|---|---|---|---|---|
| 1 | 1 Jan | International New Year | ປີໃໝ່ສາກົນ | ✅ |
| 2 | 8 Mar | International Women's Day | ວັນແມ່ຍິງສາກົນ | ✅ |
| 3-5 | 14-16 Apr | Lao New Year (Pi Mai, 3 days) | ວັນປີໃໝ່ລາວ | ❌ (fixed civil date) |
| 6 | 1 May | International Labour Day | ວັນກຳມະກອນສາກົນ | ✅ |
| 7 | 1 Jun | International Children's Day | ວັນເດັກນ້ອຍສາກົນ | ✅ |
| 8 | 20 Jul | Lao Women's Union Day | ວັນແມ່ຍິງລາວ | ✅ |
| 9 | 7 Oct | National Teacher's Day | ວັນຄູແຫ່ງຊາດ | ✅ |
| 10 | 2 Dec | National Day | ວັນຊາດ | ✅ |

**Confidence**: MEDIUM — these are seeded in the codebase and align with commonly reported Lao holidays, but the **official 2026 decree** should be confirmed from the Official Gazette.

**Missing from seed** (lunisolar holidays commonly observed):
- Boun Makha Bucha (ບຸນມາຂະບູຊາ) — full moon, 3rd lunar month (Feb/Mar)
- Boun Visakha Bucha (ບຸນວິສາຂະບູຊາ) — full moon, 6th lunar month (May)
- Boun Khao Phansa (ບຸນເຂົ້າພັນສາ) — full moon, 8th lunar month (July)
- Boun Ok Phansa (ບຸນອອກພັນສາ) — full moon, 11th lunar month (Oct)
- Boun That Luang (ບຸນທາດຫຼວງ) — November

**GAP**: Lunisolar holidays not seeded (they change dates each year). Annual government decree may add/substitute days. Recommendation: P2 — add annual holiday import + lunar calendar helper.

### Announcement mechanism
- **VERIFIED (HIGH)**: Annual government decree. The `HolidaysController.seed-defaults` endpoint seeds fixed holidays; lunisolar holidays need manual entry or annual import.

## 11 — Foreign workers

| Item | Value | Classification | Confidence |
|---|---|---|---|
| Governing law | Labour Law No. 006/NOC (2006, amended) | D | HIGH |
| Work permit required | Yes, for foreign nationals employed in Laos | D | MEDIUM |
| Work permit process | UNKNOWN — not retrieved from authoritative source | H | UNKNOWN |
| Foreign worker quota | Commonly cited 10% of workforce; UNVERIFIED | H | UNKNOWN |
| Visa categories | UNKNOWN | H | UNKNOWN |
| Stay permit process | UNKNOWN | H | UNKNOWN |
| Employer sponsorship | UNKNOWN specifics | H | UNKNOWN |
| Required documents | UNKNOWN | H | UNKNOWN |
| NSSF for foreigners | Generally required; bilateral exemptions may exist | D | MEDIUM |
| Tax for foreigners | Subject to Lao PIT on Lao-source income; specific differences UNKNOWN | D | MEDIUM |

**DFDL 2026 seminar** references "Employment, immigration, and workforce planning for foreign investors" and "Employment law changes and HR impacts for 2026" — confirming the topic is regulated and evolving, but details behind registration.

**LaoHR data model needs**: `Nationality`, `WorkPermitNumber`, `WorkPermitExpiry`, `VisaType`, `VisaExpiry`, `StayPermitExpiry`. Priority: P2 — add fields, but validate regulatory requirements with MOLSW/lawyer before encoding validation rules.

## 12 — Data privacy / cybersecurity

### Summary
Laos does **NOT** appear to have a dedicated comprehensive personal data protection law (no GDPR/PDPA equivalent). It has a patchwork of electronic transactions legislation + constitutional provisions.

| Law/Instrument | Year | Status | Classification | Confidence |
|---|---|---|---|---|
| Law on Electronic Transactions (ກົດໝາຍທຸລະກຳເອເລັກໂຕຣນິກ) | 2012 | Effective | D (Laotian Times) | HIGH (exists) |
| Decree on Electronic Commerce | 2021 | Effective | D | HIGH (exists) |
| Dedicated PDPL | — | NOT FOUND | H | MEDIUM (absence of evidence) |
| Dedicated cybersecurity law | — | NOT FOUND | H | MEDIUM |
| Constitution (1991, amended 2003/2015/2025) | 1991/2025 | Current | A (AsianLII + Wikipedia) | VERIFIED |

### Constitutional provisions (1991 text — VERIFIED)

| Article | Content | Relevance |
|---|---|---|
| **Article 29** | "The right of Lao citizens in their bodies and houses are inviolable. Lao citizens cannot be arrested or searched without warrant..." | Bodily/house privacy (not data-specific) |
| Article 6 | Prohibits acts "physically harmful to the people and detrimental to their honour, lives, consciences and property" | General rights |
| Article 38 | "Grants asylum to foreigners who are persecuted..." | **NOT privacy** (Phase 2 incorrectly hypothesized this) |

**2025 Constitution**: Ratified 10 March 2025. Full English text not retrieved. Article numbers may have shifted. Classification: MEDIUM.

### Data localization
- OECD/JETRO survey (data through 2022): Laos has **data localization restrictions** (how data can be processed in territory). Classification: D. Confidence: MEDIUM.
- Cross-border transfer restrictions: UNKNOWN.

### Biometric data
- No specific biometric data regulation found. Classification: H. Confidence: UNKNOWN.
- LaoHR stores fingerprints on ZKTeco devices (not in HR DB) — good practice regardless.

### Practical implications for LaoHR
1. No GDPR-equivalent compliance required, but apply best practices: encrypt PII at rest, mask in non-prod, audit access, retention policy.
2. Electronic Transactions Law (2012) likely governs electronic employee records.
3. Data localization may apply — consider where employee data is stored.
4. This area is evolving (LAeID digital identity app launched July 2026).
5. **REQUIRES CONFIRMATION**: Engage a Lao-licensed law firm to confirm whether a PDPL or cybersecurity law has been promulgated.

## 13 — Banking / payroll transfer

### Major Lao banks

| Bank | Website | Status | Corporate banking |
|---|---|---|---|
| BCEL | bcel.com.la | ✅ LIVE | i-Bank V.3 online portal + BCEL One mobile |
| JDB | jdbbank.com.la | ✅ LIVE | Corporate banking section (deposits, loans, transfers) |
| LDB | laobank.com.la | ⚠️ Failed | UNKNOWN |
| BIC | bicbank.com.la | ⚠️ Failed | UNKNOWN |

### Salary batch transfer

| Item | Value | Confidence |
|---|---|---|
| Supported? | Likely YES (BCEL i-Bank + JDB corporate banking) | MEDIUM (inferred from portal existence) |
| File format | UNKNOWN — not published publicly | UNKNOWN |
| Required fields | UNKNOWN | UNKNOWN |
| API | UNKNOWN — no public API docs | UNKNOWN |
| Encoding | Likely UTF-8 (Lao Unicode standard) | MEDIUM |
| Fees | UNKNOWN — BCEL fees page exists but salary-batch fees not extracted | UNKNOWN |

**Recommendation**: Contact BCEL and JDB corporate banking desks directly for salary batch file specifications. This information is typically provided only to corporate account holders.

### BCEL exchange rates (VERIFIED 2026-08-21)

| Currency | Buy | Sell |
|---|---|---|
| USD | 22,324 | 22,585 |
| THB | 677.14 | 690.61 |
| EUR | 25,723 | 26,224 |
| CNY | 3,284 | 3,348 |

**LaoHR**: `EX_RATE_USD=22000`, `EX_RATE_THB=650` in seed — slightly outdated vs current BCEL rates (USD 22,324/22,585). These are configurable via `SystemSetting` (good). GAP: P2 — consider auto-fetching BCEL rates or periodic manual update reminder.

## 14 — Government reporting

| Report | Authority | Frequency | Deadline | Format | Confidence |
|---|---|---|---|---|---|
| PIT monthly withholding | MOF (ກະຊວງການເງິນ) | Monthly | UNKNOWN | UNKNOWN | HIGH (exists) / UNKNOWN (details) |
| NSSF monthly contribution | LSSO (under MOLSW) | Monthly | UNKNOWN | UNKNOWN | HIGH (exists) / UNKNOWN (details) |
| Annual employer report | UNKNOWN | Annual | UNKNOWN | UNKNOWN | UNKNOWN |
| Labour department report | UNKNOWN | UNKNOWN | UNKNOWN | UNKNOWN | UNKNOWN |
| Foreign worker report | UNKNOWN | UNKNOWN | UNKNOWN | UNKNOWN | UNKNOWN |

**DFDL 2026 seminar** mentions "key annual filings, license renewals, sector permits, and mandatory disclosures" + "What filings will shift to a digital platform?" — confirming obligations exist and digitalisation is underway. Detailed content behind registration.

**LaoHR**: NSSF report PDF (`NssfReportService`) + LSSO Payment Form (`PdfFormService`) exist. GAP: P1 — add PIT monthly withholding report. P2 — enhance NSSF report + add annual reports.

### Fiscal year
**VERIFIED**: 1 October – 30 September (Wikipedia, Economy of Laos). Annual tax reconciliation should align.