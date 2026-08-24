# 06 — Termination & Severance + 07 — Foreign Workers + 08 — Data Privacy & Biometrics + 09 — Bank Payroll + 10 — Government Reporting

> Research date: 2026-08-21. Sources: AsianLII Labour Law (quality 5/5), PwC (quality 5/5), BCEL (quality 4), Laotian Times (quality 3).

## 06 — Termination & severance (ALL VERIFIED from primary law text)

Source: **Labour Law Articles 27, 28, 29, 30, 32, 33, 35, 37**. VERIFIED.

### Probation (Article 27 — VERIFIED)

| Rule | Value | Article | Confidence |
|---|---|---|---|
| Physical labour | **≤ 30 days** | Art. 27 | VERIFIED |
| Specialised skills | **≤ 60 days** | Art. 27 | VERIFIED |
| Extension | +30 days (if worker lacks skills) | Art. 27 | VERIFIED |
| Probation pay | **≥ 90%** of regular salary | Art. 27 | VERIFIED |
| Termination notice (non-skilled, during probation) | 3 days | Art. 27 | VERIFIED |
| Termination notice (skilled, during probation) | 5 days | Art. 27 | VERIFIED |
| Written confirmation | 7 days before probation ends | Art. 27 | VERIFIED |

### Termination notice periods (Article 28 — VERIFIED)

| Contract type | Notice period | Article | Confidence |
|---|---|---|---|
| Indefinite (physical work) | **30 days** | Art. 28 | VERIFIED |
| Indefinite (skilled work) | **45 days** | Art. 28 | VERIFIED |
| Fixed-term | **15 days** before expiry | Art. 28 | VERIFIED |

### Severance — dismissal (lack of skills/health/redundancy) (Article 29 — VERIFIED)

| Service duration | Allowance (% of basic monthly salary per month worked) | Article | Confidence |
|---|---|---|---|
| < 3 years | **10%** | Art. 29 | VERIFIED |
| > 3 years | **15%** | Art. 29 | VERIFIED |

**Calculation base**: Basic monthly salary before termination. For piece-rate: average of last 3 months.

### Severance — unjustified termination (employer's fault) (Article 33 — VERIFIED)

| Service duration | Allowance (% of basic monthly salary per month worked) | Article | Confidence |
|---|---|---|---|
| < 3 years | **15%** | Art. 33 | VERIFIED |
| > 3 years | **20%** | Art. 33 | VERIFIED |

### Other termination provisions (VERIFIED)

| Rule | Value | Article | Confidence |
|---|---|---|---|
| Fault-based termination (misconduct) | No allowance, 3 days notice | Art. 32 | VERIFIED |
| Cannot terminate during sickness | Protected | Art. 30 | VERIFIED |
| Cannot terminate during pregnancy/newborn <1yr | Protected | Art. 30 | VERIFIED |
| Cannot terminate during annual leave | Protected | Art. 30 | VERIFIED |
| All terminations in writing with reasons | Required | Art. 35 | VERIFIED |
| Payment of all amounts owed | Required | Art. 35 | VERIFIED |
| Work certificate | Within 7 days | Art. 37 | VERIFIED |

### Severance formula (for system implementation)

```
Severance = months_worked × basic_monthly_salary × rate

Where rate depends on:
- Dismissal (Art. 29): 10% (<3yr) or 15% (>3yr)
- Unjustified (Art. 33): 15% (<3yr) or 20% (>3yr)
```

### Termination compliance lock

| Rule | Value | Status |
|---|---|---|
| Probation 30/60 days | VERIFIED (Art. 27) | **LOCKED** |
| Probation pay ≥90% | VERIFIED (Art. 27) | **LOCKED** |
| Notice: indefinite 30/45 days | VERIFIED (Art. 28) | **LOCKED** |
| Notice: fixed-term 15 days | VERIFIED (Art. 28) | **LOCKED** |
| Severance dismissal 10%/15% | VERIFIED (Art. 29) | **LOCKED** |
| Severance unjustified 15%/20% | VERIFIED (Art. 33) | **LOCKED** |
| Fault termination no allowance | VERIFIED (Art. 32) | **LOCKED** |
| Work cert within 7 days | VERIFIED (Art. 37) | **LOCKED** |
| Unused leave payment | NOT specified | **PROFESSIONAL_CONFIRMATION_REQUIRED** |

---

## 07 — Foreign workers

### Quota (Article 25 — VERIFIED)

| Category | Max % of workforce | Article | Confidence |
|---|---|---|---|
| Physical labourers | **10%** | Art. 25 | VERIFIED |
| Intellectual expertise | **20%** | Art. 25 | VERIFIED |
| Exceeding quota | Government approval required | Art. 25 | VERIFIED |

### Work permit (PARTIAL)

| Item | Value | Confidence |
|---|---|---|
| Issuing authority | MOLSW (provincial labour departments) | MEDIUM |
| Duration | 6-month/1-year (business/service/investment); 3-month (hawkers) | MEDIUM |
| Renewal | UNKNOWN | UNRESOLVED |
| Required documents | UNKNOWN | UNRESOLVED |
| Employer sponsorship | UNKNOWN specifics | UNRESOLVED |

### NSSF for foreign workers (VERIFIED — PwC)

| Rule | Value | Confidence |
|---|---|---|
| NSSF registration | Required if working >12 months in Laos | VERIFIED |

### Tax for foreign workers (VERIFIED — PwC)

| Rule | Value | Confidence |
|---|---|---|
| PIT brackets | Same as Lao nationals | VERIFIED |
| Surcharge | None | VERIFIED |
| Residence (>183 days) | Obligated to pay PIT on all income | VERIFIED |

### Visa/stay permit

| Item | Status | Confidence |
|---|---|---|
| Visa categories | UNKNOWN | UNRESOLVED — PROFESSIONAL_CONFIRMATION_REQUIRED |
| Stay permit process | UNKNOWN | UNRESOLVED |
| Transfer between employers | UNKNOWN | UNRESOLVED |

### HRIS data requirements (for future implementation)

| Field | Purpose | Priority |
|---|---|---|
| `Nationality` | Foreign worker identification | P2 |
| `WorkPermitNumber` | Compliance tracking | P2 |
| `WorkPermitExpiry` | Expiry alerts | P2 |
| `VisaType` | Visa tracking | P2 |
| `VisaExpiry` | Expiry alerts | P2 |
| `StayPermitExpiry` | Compliance | P2 |
| `IsForeignWorker` | Quota calculation (10%/20%) | P2 |
| `ForeignWorkerCategory` | Physical (10%) vs intellectual (20%) | P2 |

---

## 08 — Data privacy & biometrics

### Legal framework

| Instrument | Year | Status | Confidence |
|---|---|---|---|
| Law on Electronic Transactions | 2012 | Effective | HIGH (exists) |
| Decree on E-commerce No. 296/Gov | 12 April 2021 | Effective | VERIFIED (PwC) |
| Decision on fines No. 2828/MOIC | 11 November 2025 | Effective | VERIFIED (PwC) |
| Dedicated PDPL | — | NOT FOUND | HIGH (absence confirmed) |
| Dedicated cybersecurity law | — | NOT FOUND | MEDIUM |
| Constitution (amended 2025) | 2025 | Current | VERIFIED |

### E-commerce data fines (VERIFIED — PwC)

| Violation | Fine | Source |
|---|---|---|
| Unauthorised use of customer data | **LAK 5,000,000** per occurrence | PwC (Decision 2828/MOIC) |
| False info / unauthorised disclosure / harmful fake data | **LAK 20,000,000** per occurrence | PwC |
| VAT-related info retention | **10 years** | PwC |

### Data localization (VERIFIED)

Laos has data localization restrictions (OECD/JETRO 2022 survey). Confidence: HIGH.

### Biometric data

| Item | Status | Confidence |
|---|---|---|
| Specific biometric regulations | NOT FOUND | UNRESOLVED |
| Employee data retention requirements | NOT FOUND | UNRESOLVED |
| Breach notification obligations | NOT FOUND | UNRESOLVED |
| Cross-border transfer rules | NOT FOUND (localization exists) | UNRESOLVED |

### Practical implications for LaoHR

1. No GDPR-equivalent, but E-commerce Decree (2021) has data-use fines.
2. Data localization may apply — consider where employee data is stored.
3. Apply best practices: encrypt PII at rest, mask in non-prod, audit access, retention policy.
4. Biometric (fingerprint) data is on ZKTeco devices, not in HR DB — good practice.
5. **REQUIRES CONFIRMATION**: Engage Lao legal firm to confirm any sectoral or upcoming regulations.

---

## 09 — Bank payroll

### BCEL i-Bank (VERIFIED)

| Item | Value | Confidence |
|---|---|---|
| Payroll batch supported | **YES** — "Payroll transactions can be made by yourself" | VERIFIED (BCEL product page) |
| Funds transfer | BCEL accounts, other domestic banks, international | VERIFIED |
| Security | OTP Token + SMS | VERIFIED |
| Scheduled transactions | Supported | VERIFIED |
| Current version | i-Bank V.3 | VERIFIED |
| Contact | Tel (856-21) 264959, Hotline 1555, e-banking@bcel.com.la | VERIFIED |

### File format / required fields (BANK_CONFIRMATION_REQUIRED)

| Item | Status |
|---|---|
| File format (CSV/Excel/TXT) | NOT publicly documented |
| Required columns/fields | NOT publicly documented |
| Batch size limits | NOT publicly documented |
| API | NOT publicly documented |
| Encoding | NOT publicly documented |
| Fees | NOT publicly documented (fees page exists) |

### Other banks

| Bank | Payroll batch? | Status |
|---|---|---|
| JDB | Likely (corporate banking section exists) | BANK_CONFIRMATION_REQUIRED |
| LDB | UNKNOWN | BANK_CONFIRMATION_REQUIRED |
| BIC | UNKNOWN | BANK_CONFIRMATION_REQUIRED |

### LaoHR current bank transfer implementation

LaoHR has `BankTransferService` generating BCEL (text H/D/T records) and LDB (CSV) files. These formats need verification against actual bank specifications. **BANK_CONFIRMATION_REQUIRED.**

---

## 10 — Government reporting

### PIT reporting (VERIFIED — PwC)

| Obligation | Authority | Frequency | Deadline | Format | Confidence |
|---|---|---|---|---|---|
| Monthly PIT withholding + payment | MOF (Tax Department) | Monthly | **20th of following month** | UNKNOWN (digitalization underway) | VERIFIED (deadline) |
| Annual PIT return | MOF | Annual | **31 March** | UNKNOWN | VERIFIED (deadline) |
| Late penalty | — | — | **0.1%/day** | — | VERIFIED |

### NSSF reporting (VERIFIED — PwC)

| Obligation | Authority | Frequency | Deadline | Format | Confidence |
|---|---|---|---|---|---|
| Monthly NSSF contribution + return | LSSO (MOLSW) | Monthly | **20th of following month** | UNKNOWN | VERIFIED (deadline) |

### Other reporting

| Obligation | Authority | Status | Confidence |
|---|---|---|---|
| Annual employer report | UNKNOWN | UNRESOLVED | PROFESSIONAL_CONFIRMATION_REQUIRED |
| Labour department report | UNKNOWN | UNRESOLVED | PROFESSIONAL_CONFIRMATION_REQUIRED |
| Foreign worker report | MOLSW | UNRESOLVED | PROFESSIONAL_CONFIRMATION_REQUIRED |
| Employee registration | LSSO + Tax Dept | UNRESOLVED | PROFESSIONAL_CONFIRMATION_REQUIRED |

### Fiscal year

**VERIFIED**: 1 October – 30 September. Annual reconciliation should align.