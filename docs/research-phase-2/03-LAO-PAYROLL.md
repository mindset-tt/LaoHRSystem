# 03 — Lao Payroll Research (ເງິນເດືອນ / ຄ່າແຮງງານ)

> Research date: 2026-08-21. Sources: Trading Economics (citing Lao MOF/MOLSW), Wikipedia, WageIndicator.
> ⚠️ **LEGAL DISCLAIMER**: This research supports software requirements. It is NOT legal advice.

## Payroll components

| Component | Lao term | Status in LaoHR | Confidence |
|---|---|---|---|
| Gross Salary | ເງິນເດືອນລວມ | Computed (Base + OT + Allowances + Bonus) | VERIFIED |
| Base Salary | ເງິນເດືອນພື້ນຖານ | `SalarySlip.BaseSalary` | VERIFIED |
| Allowances | ຄ່າອຸດໜູນ | `SalarySlip.Allowances` | VERIFIED |
| Bonuses | ເງິນລາງວັນ | `SalarySlip.Bonus` | VERIFIED |
| Commissions | ຄ່ານາຍໜ້າ | Not separately modelled (could be an adjustment) | PARTIAL |
| Overtime | ຄ່າລ່ວງເວລາ | `SalarySlip.OvertimePay` (single amount, no day-type) | PARTIAL — see `02-LAO-LABOUR-LAW.md` |
| Deductions | ການຫັກອອກ | NSSF + Tax + Other | VERIFIED |
| Tax (PIT) | ພາສີເງິນເດືອນ | `SalarySlip.TaxDeduction` (progressive) | VERIFIED (engine exists; brackets need confirmation) |
| Social Security (NSSF) | ປະກັນສັງຄົມ | `NssfEmployeeDeduction` + `NssfEmployerContribution` | VERIFIED (rates confirmed) |
| Loans/Advances | ເງິນກູ້ / ເງິນລ່ວງ | `EmployeeLoan` + `LoanRepayment` (separate module); payroll deduction via `OtherDeductions`? | PARTIAL — loan installment not auto-linked to payroll |
| Net Salary | ເງິນເດືອນສຸດທິ | `SalarySlip.NetSalary` | VERIFIED |
| Retroactive Adjustments | ການແກ້ໄຂຍ້ອນຫຼັງ | `PayrollAdjustment` (dynamic columns) | VERIFIED |
| Arrears | ໜີ້ສົງ | Not modelled | NOT_STARTED |
| Termination Pay | ເງິນຊົດເຊີຍປົດຈ້າງ | Not modelled | NOT_STARTED |
| Severance | ເງິນຊົດເຊີຍ | Not modelled | NOT_STARTED |

## Payroll calculation order (research model)

Based on LaoHR's `PayrollService` + Lao regulatory requirements, the expected calculation order:

```
1. BaseSalary (in contract currency)
2. Convert to LAK via ConversionRate (if foreign currency)
3. OvertimePay (calculate per day-type × rate — GAP: currently single amount)
4. Allowances (taxable vs non-taxable — GAP: not distinguished)
5. Bonus
6. GrossIncome = Base + OT + Allowances + Bonus
7. NSSF:
   a. NssfBase = min(GrossIncome, contribution_ceiling) — GAP: ceiling unknown
   b. NssfEmployeeDeduction = NssfBase × 5.5%
   c. NssfEmployerContribution = NssfBase × 6.0%
8. TaxableIncome = GrossIncome − NssfEmployeeDeduction − deductions (personal allowance, dependants — GAP: not modelled)
9. TaxDeduction = progressive PIT (brackets — GAP: brackets need confirmation)
10. OtherDeductions = loan installments + advances + adjustments
11. NetSalary = GrossIncome − NssfEmployeeDeduction − TaxDeduction − OtherDeductions
12. Convert NetSalary back to contract currency if PaymentCurrency = ORIGINAL
```

**LaoHR's `PayrollService` follows this order** (VERIFIED from Phase 1). The gaps are in the regulatory parameters (brackets, ceiling, deductions, allowances taxability), not the calculation sequence.

## Key payroll gaps

| Gap | Severity | Evidence |
|---|---|---|
| PIT brackets unverified (LaoHR has `TaxBracket` seeded — must confirm thresholds match current law) | HIGH | `04-LAO-PERSONAL-INCOME-TAX.md` |
| NSSF contribution ceiling unknown (LaoHR may not cap `NssfBase`) | HIGH | `05-LAO-SOCIAL-SECURITY.md` |
| Allowances not split into taxable/non-taxable | MEDIUM | Tax compliance |
| Loan installments not auto-deducted from payroll | MEDIUM | `EmployeeLoan` exists but no payroll link |
| Overtime not differentiated by day type | HIGH | `02-LAO-LABOUR-LAW.md` |
| No termination/severance calculation | MEDIUM | Future HR capability |
| Personal allowance + dependant deductions not modelled | HIGH | Tax compliance |
| Multi-currency: LaoHR converts to LAK for calc, back to original for payment | VERIFIED good | `SalarySlip` has `ContractCurrency`, `ExchangeRateUsed`, `PaymentCurrency` |

## Lao fiscal year

- **VERIFIED**: Lao fiscal year = **1 October – 30 September** (Wikipedia, Economy of Laos).
- **GAP**: LaoHR's `PayrollPeriod` uses calendar months (Year/Month). This is fine for monthly payroll runs, but annual tax reconciliation and reporting may need to align with the Oct–Sep fiscal year. `P2` — reporting consideration.

## Lao currency (LAK) payroll considerations

- **VERIFIED**: LAK (₭), att obsolete, smallest circulating note ₭500. Practical convention: store amounts in **whole LAK (integer kip)** or `decimal(18,2)` (current LaoHR approach — fine, as 2 decimal places = att precision, harmless).
- **VERIFIED**: USD ≈ LAK 21,594 (Jan 2026). THB ≈ LAK ~630. CNY ≈ LAK ~2,960 (MEDIUM — derived).
- **VERIFIED**: LaoHR supports multi-currency (LAK/USD/THB/CNY) with `ConversionRate` history (effective/expiry dates). Architecture is sufficient.
- **GAP**: Rounding — when converting foreign salary to LAK for tax, then back to original for payment, rounding differences occur. LaoHR should document the rounding convention. `P3`.