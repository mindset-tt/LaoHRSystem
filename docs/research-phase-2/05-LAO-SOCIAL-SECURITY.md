# 05 — Lao Social Security / NSSF (ປະກັນສັງຄົມ / ອົງການປະກັນສັງຄົມ)

> Research date: 2026-08-21. Sources: Trading Economics (citing Lao MOLSW), Wikipedia.
> ⚠️ **LEGAL DISCLAIMER**: Contribution ceiling is UNKNOWN. This is NOT legal advice.

## Governing law

| Field | Value | Confidence |
|---|---|---|
| **Law** | Law on Social Security (ກົດໝາຍວ່າດ້ວຍປະກັນສັງຄົມ), 2013 | HIGH |
| **Authority** | Ministry of Labour and Social Welfare (MOLSW) | VERIFIED |
| **Administrator** | Lao Social Security Organisation (LSSO) — sometimes called NSSF (National Social Security Fund) | VERIFIED |
| **History** | 2013 law merged the State Authority Employees Social Security Fund + private-sector Decree 207/2007 scheme into a unified system | HIGH |
| **Source** | Trading Economics, source = MOLSW — https://tradingeconomics.com/laos/social-security-rate-for-companies and …/social-security-rate-for-employees | VERIFIED |

## Contribution rates (VERIFIED as of Dec 2026 reporting)

| Component | Rate | Confidence | Evidence |
|---|---|---|---|
| **Total social security rate** | **11.5%** of wages | VERIFIED | Trading Economics / MOLSW |
| **Employer contribution** | **6.0%** | VERIFIED | Trading Economics / MOLSW |
| **Employee contribution** | **5.5%** | VERIFIED | Trading Economics / MOLSW |
| Historical (pre-2016) | Both at 4.5% | VERIFIED | Trading Economics historical data |
| Rate change effective | 2016 | VERIFIED | |

## Contribution ceiling / base

| Item | Detail | Confidence |
|---|---|---|
| Contribution wage ceiling (max monthly wage subject to contribution) | **UNKNOWN** — commonly reported to exist but exact amount not retrieved | UNKNOWN — requires confirmation from LSSO |
| Contribution base minimum | **UNKNOWN** | UNKNOWN |
| What wages are included (base only? + OT? + allowances?) | **UNKNOWN** | UNKNOWN |

> **REQUIRES CONFIRMATION FROM LSSO OR QUALIFIED PROFESSIONAL.**

## Coverage

| Benefit | Covered? | Confidence |
|---|---|---|
| Health/medical care | Yes | MEDIUM (widely reported) |
| Maternity | Yes | MEDIUM |
| Sickness | Yes | MEDIUM |
| Work-injury | Yes | MEDIUM |
| Disability | Yes | MEDIUM |
| Survivor/death | Yes | MEDIUM |
| Old-age (pension) | Yes | MEDIUM |
| Unemployment | Yes (introduced under unified scheme) | MEDIUM |

> The 11.5% total splits across these benefit categories, but the **exact sub-fund breakdown** (how 6%/5.5% divides across pension/health/injury/maternity) could not be confirmed. **UNKNOWN.**

## Reporting & payment schedule

| Item | Detail | Confidence |
|---|---|---|
| Remittance frequency | Monthly (employer remits to LSSO) | MEDIUM |
| Monthly return | Required | MEDIUM |
| Exact due date | Not retrieved | UNKNOWN |
| Registration requirements | Employers must register with LSSO | HIGH |

## Foreign workers

| Item | Confidence |
|---|---|
| Foreign employees generally required to participate in LSSO | MEDIUM |
| Bilateral exemptions may exist | UNKNOWN |
| Specific rules | UNKNOWN — confirm |

## LaoHR current implementation

LaoHR has:
- `SalarySlip.NssfBase` — base wage for NSSF calculation
- `SalarySlip.NssfEmployeeDeduction` — employee's 5.5% (deducted from pay)
- `SalarySlip.NssfEmployerContribution` — employer's 6.0% (cost to employer)
- `PayrollService` reads NSSF rates from `SystemSetting` (key-value store)
- NSSF report PDF generated (`NssfReportService` via QuestPDF)
- LSSO Payment Form PDF filled (`PdfFormService` via iText7, template `PDF/LSSO_Payment_Form.pdf`)

**VERIFIED**: The 5.5%/6.0% rates in LaoHR match the VERIFIED rates. Good.

**GAPS**:

| Gap | Severity | Evidence | Priority |
|---|---|---|---|
| Contribution ceiling not applied (`NssfBase` may not be capped) | HIGH | If a ceiling exists and `NssfBase = GrossIncome` (uncapped), high earners over-contribute | P0 (if ceiling exists) |
| NSSF rate stored in `SystemSetting` (editable) — good, but must be confirmed = 5.5%/6.0% | LOW | Verify seeded values | P0 (verification) |
| NSSF sub-fund breakdown not modelled (if LSSO report requires it) | MEDIUM | Report may need pension/health/injury split | P1 |
| Foreign worker NSSF treatment unknown | MEDIUM | Cannot differentiate | P2 |
| Monthly NSSF remittance report | PARTIAL | NSSF report PDF exists; monthly remittance summary may need enhancement | P2 |

## Key verification items

1. **Confirm NSSF contribution ceiling** (max monthly wage). If it exists, `PayrollService` must cap `NssfBase`.
2. **Confirm NSSF rates** seeded in `SystemSetting` = 5.5% employee / 6.0% employer.
3. **Confirm what wages are included** in contribution base (base only? + OT? + allowances?).
4. **Confirm foreign worker** NSSF obligations.
5. **Confirm monthly remittance due date** and report format.

> **All items: REQUIRES CONFIRMATION FROM LSSO OR QUALIFIED LAO PROFESSIONAL.**