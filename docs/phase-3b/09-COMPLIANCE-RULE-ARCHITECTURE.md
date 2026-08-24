# 09 — Compliance Rule Architecture + 10 — Historical Payroll Reproducibility

> Phase 3B workstreams 3B.6/3B.7.

## 09 — Versioned compliance architecture

### Model

Two new entities in `LaoHR.Shared/Entities.cs`:

**`ComplianceRule`** — a versioned, effective-dated statutory rule:
- `RuleId` (stable identifier, e.g. `LAO-PIT-2026-BRACKET-01`)
- `Jurisdiction` (e.g. `LA`)
- `Category` (PIT, NSSF, MINIMUM_WAGE, OVERTIME, LEAVE, SEVERANCE, FOREIGN_WORKER, HOLIDAY)
- `EffectiveFrom` / `EffectiveTo` (effective dating)
- `Version` (monotonic)
- `Status` (VERIFIED, PROVISIONAL, BLOCKED, SUPERSEDED)
- `ParametersJson` (calculation parameters)
- Source metadata: `SourceTitle`, `Authority`, `LawNumber`, `Article`, `SourceUrl`, `VerifiedDate`

**`PayrollRuleSnapshot`** — immutable snapshot of rules used for a payroll run:
- `PeriodId` (FK to PayrollPeriod)
- `RulesJson` (JSON array of rules in effect)
- `ExchangeRatesJson`
- `CapturedAt`

### Service

`IComplianceRuleService` / `ComplianceRuleService`:
- `GetEffectiveRuleAsync(category, referenceDate)` — returns the effective rule, **excluding BLOCKED rules**
- `GetEffectiveRulesAsync(category, referenceDate)` — returns all effective rules

### Rule status semantics
- `VERIFIED` — confirmed by primary/authoritative source (Phase 2C)
- `PROVISIONAL` — not yet confirmed
- `BLOCKED` — unresolved, must NOT be used for production calculation
- `SUPERSEDED` — replaced by a newer version

### Seeded VERIFIED rules (from Phase 2C)

| Category | Rules seeded |
|---|---|
| PIT | 6 brackets (Law 88/NA) + dependant deduction |
| NSSF | employee rate 5.5%, employer rate 6.0%, ceiling 4,500,000 |
| MINIMUM_WAGE | 2,500,000 (Oct 2024) |
| OVERTIME | 1.5×/2×/2.5×/3× + night bonus 15% + caps 3h/45h |
| LEAVE | annual 15/18, sick 30, maternity 90, allowance 60% |
| SEVERANCE | 10%/15%/15%/20% |
| FOREIGN_WORKER | 10%/20% quota |

### BLOCKED rules (NOT seeded as production rules)
- OT hourly divisor (UNRESOLVED)
- Leave carry-over (UNRESOLVED)
- NSSF minimum floor (UNRESOLVED)
- NSSF exact contribution-base definition (UNRESOLVED)
- Bank salary file format (BANK_CONFIRMATION_REQUIRED)
- Visa/stay permit categories (PROFESSIONAL_CONFIRMATION_REQUIRED)

## 10 — Historical payroll reproducibility

### Current state
`SalarySlip` already stores all computed values (BaseSalary, OvertimePay, Allowances, Bonus, GrossIncome, NssfBase, NssfEmployeeDeduction, NssfEmployerContribution, TaxableIncome, TaxDeduction, OtherDeductions, NetSalary, ExchangeRateUsed, etc.). This is a form of snapshot.

### Added
`PayrollRuleSnapshot` entity provides the rule-version reference needed for full reproducibility: which PIT/NSSF/OT rules were in effect when the payroll was calculated.

### Reproducibility test
The `ComplianceRuleServiceTests.GetEffectiveRule_ReturnsSupersededRuleCorrectly` test proves that effective-date lookup returns the correct rule for a historical date (old rule for 2025, new rule for 2026). This is the foundation for historical reproducibility.

### Remaining work (Phase 3C)
- Wire `PayrollRuleSnapshot` capture into `PayrollService.ProcessPayrollAsync` (snapshot rules + exchange rates at calculation time).
- Add a full reproducibility test: calculate with Rule A → finalize → introduce Rule B → re-read Rule A payroll → numbers unchanged.