# 04 — Lao Personal Income Tax (ພາສີເງິນເດືອນ / ພາສີລາຍໄດ້ບຸກຄົນ)

> Research date: 2026-08-21. Sources: Trading Economics (citing Lao Ministry of Finance), Wikipedia.
> ⚠️ **LEGAL DISCLAIMER**: Exact bracket thresholds are UNVERIFIED. This is NOT legal advice. The bracket table below must be confirmed against the official MOF salary-tax schedule before encoding in payroll logic.

## Governing law

| Field | Value | Confidence |
|---|---|---|
| **Law** | Law on Tax Administration (ກົດໝາຍວ່າດ້ວຍສາຍການຄຸ້ມຄອງພາສີ), originally 2015, with amendments (revised ~2021–2022) | HIGH (law exists) |
| **Salary tax** | ພາສີເງິນເດືອນ (pa si nern due an) — employment income tax, withheld at source | VERIFIED |
| **Issuing authority** | Ministry of Finance (MOF / ກະຊວງການເງິນ) | VERIFIED |
| **Source** | Trading Economics, source = Lao Ministry of Finance — https://tradingeconomics.com/laos/personal-income-tax-rate | VERIFIED |

## Top marginal PIT rate (VERIFIED)

| Metric | Value | Confidence |
|---|---|---|
| Top marginal PIT rate | **25%** | VERIFIED (Trading Economics / MOF) |
| Historical: 2011 | 28% | VERIFIED |
| Historical: 2013 | 24% | VERIFIED |
| Average 2009–2026 | 24.94% | VERIFIED |
| Stable since | 2014–2015 reforms | VERIFIED |

## PIT brackets for employment income (⚠️ UNVERIFIED)

> The following bracket table is **widely circulated in HR/payroll guidance for Laos** but could NOT be confirmed against the official MOF law text during this research. **Treat as UNKNOWN/UNVERIFIED.**

| Monthly taxable income (LAK) | Rate |
|---|---|
| 0 – 1,300,000 | 0% |
| 1,300,001 – 2,000,000 | 5% |
| 2,000,001 – 8,500,000 | 10% |
| 8,500,001 – 12,500,000 | 15% |
| 12,500,001 – 18,500,000 | 20% |
| Over 18,500,000 | 25% |

**Confidence**: UNKNOWN/UNVERIFIED — requires confirmation from the Lao Ministry of Finance or a qualified Lao tax adviser.

> **REQUIRES CONFIRMATION FROM QUALIFIED LAO TAX PROFESSIONAL.**

## LaoHR current implementation

LaoHR has `TaxBracket` entity (seeded in `OnModelCreating`):
- `MinIncome`, `MaxIncome`, `TaxRate`, `SortOrder`, `IsActive`
- `PayrollService` applies progressive tax via these brackets

**VERIFIED**: The engine is correct (progressive bracket calculation). The question is whether the **seeded bracket values match the current law**. The seed values were not inspected in detail during Phase 1. **This is a P0 compliance verification item** — someone with access to the official MOF schedule must confirm the seeded `TaxBracket` values are current.

## Exemptions, deductions, non-taxable components

| Item | Detail | Confidence |
|---|---|---|
| Personal allowance | Reported ~LAK 1,300,000/month (matches 0% bracket threshold) | UNKNOWN — confirm |
| Dependant deductions | Reported to exist | UNKNOWN — confirm |
| Non-taxable allowances | UNKNOWN which allowances are non-taxable | UNKNOWN — confirm |
| NSSF employee contribution | Likely deductible from taxable income (LaoHR's `PayrollService` subtracts it: `TaxableIncome = GrossIncome − NssfEmployeeDeduction`) | MEDIUM — common practice, confirm |

**GAP**: LaoHR's `SalarySlip.Allowances` is a single field — no split between taxable and non-taxable allowances. If Lao law exempts certain allowances (e.g., transport, meal), the payroll engine cannot handle it. **P1 compliance gap.**

## Foreign employees vs Lao employees

| Item | Confidence |
|---|---|
| Foreign employees subject to Lao PIT on Lao-source employment income | MEDIUM (general principle) |
| Specific differences (expat secondment, residency rules) | UNKNOWN — confirm |
| Tax rates for foreigners | UNKNOWN — may be same or different |

**GAP**: LaoHR's `Employee` has no `Nationality` or `TaxResidency` field. If foreign employees have different tax treatment, the system cannot differentiate. **P2** — depends on legal confirmation.

## Withholding & reporting

| Item | Detail | Confidence |
|---|---|---|
| Employer withholding | Employer withholds salary tax (ພາສີເງິນເດືອນ) at source monthly | HIGH |
| Remittance frequency | Monthly withholding + remittance; annual reconciliation | MEDIUM |
| Filing deadline | Exact dates not retrieved | UNKNOWN — confirm |

## Recent changes (2022/2023/2024 reforms)

- The amended Tax Administration Law (~2021–2022) modernised administration.
- **UNKNOWN** whether 2024-specific PIT bracket changes occurred.
- **REQUIRES CONFIRMATION** of current brackets as of 2025/2026.

## LaoHR compliance gap summary

| Gap | Severity | Required action | Priority |
|---|---|---|---|
| Seeded `TaxBracket` values unverified | CRITICAL | Confirm seeded values match current MOF schedule | P0 |
| Personal allowance / dependant deductions not modelled | HIGH | Add deduction fields to payroll calculation | P1 |
| Taxable vs non-taxable allowances not distinguished | HIGH | Split `Allowances` into taxable/non-taxable or add allowance types | P1 |
| Foreign employee tax treatment unknown | MEDIUM | Add `Nationality`/`TaxResidency`; confirm rules | P2 |
| Annual tax reconciliation (fiscal year Oct–Sep) | MEDIUM | Add annual tax report aligned to fiscal year | P2 |
| PIT withholding report (monthly remittance) | MEDIUM | Add monthly tax remittance report | P1 |