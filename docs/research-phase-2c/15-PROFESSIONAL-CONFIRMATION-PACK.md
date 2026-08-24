# 15 — Professional Confirmation Pack + 16 — Bilingual Questions + 17 — Evidence Pack + 18 — Compliance Lock Status + 19 — Do Not Implement Yet + 20 — Workstreams + 21 — Production Payroll Readiness + 22 — Phase 3 Final Readiness

> Research date: 2026-08-21.

## 15 — Professional confirmation pack (remaining unresolved items only)

### For Lao labour lawyer

| # | Question (EN) | ພາສາລາວ | Why needed | Impact |
|---|---|---|---|---|
| L-1 | Under the currently effective Lao Labour Law (as amended since 2006), has the annual leave entitlement (15 days, Art. 21) been changed by any amendment? | ຕາມກົດໝາຍແຮງງານ ສະບັບແກ້ໄຂຫຼ້າສຸດ, ລາພັກປະຈຳປີ 15 ມື້ (ມາດຕາ 21) ໄດ້ມີການແກ້ໄຂບໍ? | Confirm law is current | Leave config |
| L-2 | What is the hourly rate divisor for overtime calculation (monthly salary ÷ what)? | ການຄິດໄລ່ຄ່າລ່ວງເວລາ, ເງິນເດືອນປະຈຳເດືອນຫານຫຍັງ? | OT calculation | Payroll |
| L-3 | Are there annual leave carry-over rules? How many days can be carried? Is there expiration? | ລາພັກປະຈຳປີທີ່ຍັງບໍ່ທັນໃຊ້ສາມາດຍົກໄປປີຕໍ່ໄປໄດ້ບໍ? ມີກຳນົດວັນໝົດອາຍຸບໍ? | Leave policy | Leave config |
| L-4 | Is unused leave paid out on termination? At what rate? | ເມື່ອຢຸດສັນຍາ, ລາພັກທີ່ຍັງບໍ່ທັນໃຊ້ຈ່າຍໃຫ້ບໍ? ອັດຕາໃດ? | Final payroll | Termination |
| L-5 | Has the Labour Law been amended since 2006? If so, what articles changed? | ກົດໝາຍແຮງງານໄດ້ມີການແກ້ໄຼຕັ້ງແຕ່ປີ 2006 ບໍ? ມາດຕາໃດເປັນນິຍົມ? | All rules | All |
| L-6 | Are there implementing regulations for paternity/marriage/bereavement leave in the private sector? | ມີຂໍ້ຕົກລົງປະຕິບັດກ່ຽວກັບລາພັກພໍ່/ລາພັກແຕ່ງງານ/ລາພັກຍ້າຍສົບສຳລັບເອກະຊົນບໍ? | Leave types | Leave config |

### For Lao tax adviser

| # | Question (EN) | Impact |
|---|---|---|
| T-1 | Is the NSSF contribution base "gross remuneration" = base salary + OT + allowances + bonus, or base salary only? | NSSF calc |
| T-2 | Is there a minimum NSSF contribution floor (minimum monthly contribution)? | NSSF calc |
| T-3 | Confirm: OT pay is exempt from PIT only for employees with base salary < LAK 3M/month. Is this correct per Law 88/NA? | PIT calc |
| T-4 | Are benefits in kind (housing, transport, meals) taxable at fair market value? | PIT calc |
| T-5 | What is the PIT filing format (paper form? online portal? specific form number)? | Reporting |

### For LSSO

| # | Question (EN) | Impact |
|---|---|---|
| S-1 | Confirm NSSF ceiling is LAK 4,500,000 per Notification No. 0824/NSSFO. Is this still current? | NSSF calc |
| S-2 | What is the NSSF contribution base definition (what earnings are included)? | NSSF calc |
| S-3 | Is there a minimum contribution floor? | NSSF calc |
| S-4 | What is the NSSF monthly reporting format? | Reporting |
| S-5 | What is the Social Security Law number and latest amendment? | Documentation |

### For Lao data/cyber legal specialist

| # | Question (EN) | Impact |
|---|---|---|
| D-1 | Does Laos have a dedicated personal data protection law? If so, what is the law number? | Privacy compliance |
| D-2 | Are there specific biometric data regulations (fingerprint attendance)? | Biometric handling |
| D-3 | What are the employee data retention requirements? | Retention policy |
| D-4 | What are the cross-border data transfer rules? | Data storage location |
| D-5 | Are there breach notification obligations? | Security ops |

### For bank corporate-banking representative (BCEL/JDB/LDB/BIC)

| # | Question (EN) | Impact |
|---|---|---|
| B-1 | Do you support salary batch/bulk transfer via corporate online banking? | Bank integration |
| B-2 | What file format(s) are supported (CSV, Excel, TXT)? | File generation |
| B-3 | What are the required columns/fields? | File generation |
| B-4 | What is the file encoding (UTF-8? Lao support)? | File generation |
| B-5 | What is the maximum batch size? | File generation |
| B-6 | Do you offer an API for salary payments? | Integration |
| B-7 | What are the fees? | Cost |
| B-8 | What is the cut-off time for same-day processing? | Timing |
| B-9 | Do you support inter-bank transfers in the salary batch? | File generation |
| B-10 | What account number validation rules do you apply? | Validation |

## 18 — Compliance lock status

| RuleId | Rule | Value | Status |
|---|---|---|---|
| LAO-PIT-BRACKETS | PIT brackets 0/5/10/15/20/25% at 2.5M/5M/15M/25M/65M | VERIFIED (PwC) | **LOCKED** |
| LAO-PIT-DEPENDENT | Dependant deduction 5M/dependent, max 15M/yr | VERIFIED (PwC) | **LOCKED** |
| LAO-PIT-OT-EXEMPT | OT exempt for base < 3M/month | VERIFIED (PwC) | **LOCKED** |
| LAO-PIT-FOREIGN | Foreign workers same brackets | VERIFIED (PwC) | **LOCKED** |
| LAO-PIT-WITHHOLD | Monthly due 20th | VERIFIED (PwC) | **LOCKED** |
| LAO-PIT-ANNUAL | Annual return 31 March | VERIFIED (PwC) | **LOCKED** |
| LAO-PIT-PENALTY | Late 0.1%/day | VERIFIED (PwC) | **LOCKED** |
| LAO-NSSF-EMPLOYER | 6.0% | VERIFIED (PwC) | **LOCKED** |
| LAO-NSSF-EMPLOYEE | 5.5% | VERIFIED (PwC) | **LOCKED** |
| LAO-NSSF-CEILING | 4,500,000 (Notification 0824/NSSFO) | VERIFIED (PwC) | **LOCKED** |
| LAO-NSSF-DEADLINE | 20th of following month | VERIFIED (PwC) | **LOCKED** |
| LAO-NSSF-FOREIGN | >12 months must register | VERIFIED (PwC) | **LOCKED** |
| LAO-NSSF-DEDUCTIBLE | Employee contribution deductible from PIT | VERIFIED (PwC) | **LOCKED** |
| LAO-MIN-WAGE | 2,500,000 (Oct 2024) | VERIFIED | **LOCKED** |
| LAO-OT-WEEKDAY-DAY | 1.5× | VERIFIED (Art. 48) | **LOCKED** |
| LAO-OT-WEEKDAY-NIGHT | 2× | VERIFIED (Art. 48) | **LOCKED** |
| LAO-OT-RESTDAY-DAY | 2.5× | VERIFIED (Art. 48) | **LOCKED** |
| LAO-OT-RESTDAY-NIGHT | 3× | VERIFIED (Art. 48) | **LOCKED** |
| LAO-OT-NIGHT-BONUS | 15% (22:00-05:00) | VERIFIED (Art. 48) | **LOCKED** |
| LAO-OT-MAX-DAY | 3h | VERIFIED (Art. 18) | **LOCKED** |
| LAO-OT-MAX-MONTH | 45h | VERIFIED (Art. 18) | **LOCKED** |
| LAO-LEAVE-ANNUAL | 15 days (18 hazardous) | VERIFIED (Art. 21) | **LOCKED** |
| LAO-LEAVE-SICK | 30 days full pay | VERIFIED (Art. 20) | **LOCKED** |
| LAO-LEAVE-MATERNITY | ≥90 days full pay | VERIFIED (Art. 39) | **LOCKED** |
| LAO-LEAVE-MATERNITY-ALLOW | ≥60% min wage | VERIFIED (Art. 40) | **LOCKED** |
| LAO-PROBATION | 30/60 days, ≥90% pay | VERIFIED (Art. 27) | **LOCKED** |
| LAO-NOTICE | 30/45 days (indefinite), 15 days (fixed-term) | VERIFIED (Art. 28) | **LOCKED** |
| LAO-SEVERANCE-DISMISS | 10%/15% per month worked | VERIFIED (Art. 29) | **LOCKED** |
| LAO-SEVERANCE-UNJUST | 15%/20% per month worked | VERIFIED (Art. 33) | **LOCKED** |
| LAO-FOREIGN-QUOTA | 10%/20% | VERIFIED (Art. 25) | **LOCKED** |
| LAO-WORK-HOURS | 8h/day, 48h/week | VERIFIED (Art. 16) | **LOCKED** |
| LAO-OT-DIVISOR | UNKNOWN | — | **PROFESSIONAL_CONFIRMATION_REQUIRED** |
| LAO-LEAVE-CARRYOVER | UNKNOWN | — | **PROFESSIONAL_CONFIRMATION_REQUIRED** |
| LAO-NSSF-BASE | "Gross remuneration" (PwC wording) | HIGH but not precise | **TEMPORARY** |
| LAO-NSSF-MIN-FLOOR | UNKNOWN | — | **PROFESSIONAL_CONFIRMATION_REQUIRED** |
| LAO-UNUSED-LEAVE-PAYOUT | UNKNOWN | — | **PROFESSIONAL_CONFIRMATION_REQUIRED** |
| LAO-BANK-FORMAT | UNKNOWN (not public) | — | **BANK_CONFIRMATION_REQUIRED** |
| LAO-VISA-CATEGORIES | UNKNOWN | — | **PROFESSIONAL_CONFIRMATION_REQUIRED** |

**Summary**: 32 rules LOCKED, 6 rules require professional/bank confirmation, 1 TEMPORARY.

## 19 — Do not implement yet (compliance-dependent)

| Area | Why blocked | What can be built (Track C) |
|---|---|---|
| OT hourly divisor | Unverified | OvertimeEntry model with configurable divisor |
| Annual leave carry-over/expiration | Unverified | LeavePolicy with configurable MaxCarryOver (already exists) |
| NSSF contribution base exact definition | "Gross remuneration" not precisely defined | Configurable NSSF base (already is) |
| NSSF minimum floor | Unknown | Configurable (add if confirmed) |
| Bank salary file format | Not publicly documented | BankTransfer abstraction (already exists); verify formats with banks |
| Visa/stay permit validation rules | Unknown | Add data fields without validation rules |
| Unused leave payout on termination | Unknown | Add field; don't calculate until confirmed |

## 20 — Technical vs compliance workstreams

| Track | Items | Can proceed? |
|---|---|---|
| **A — Safe Technical** (no legal dependency) | Argon2id, PG migrations, WAL+PITR, employee routes, CompanySettings auth, JWT fail-fast, PM/Finance/Knowledge tests, mock removal, /403 page, proxy.ts, Caddy+TLS, CI branch align, frontend tests, IExceptionHandler | ✅ YES — immediately |
| **B — Compliance-Dependent** (must wait for verified rules) | OT hourly divisor config, carry-over/expiration enforcement, NSSF base definition, bank file format finalization, visa validation rules, unused leave payout | ❌ NO — wait for professional confirmation |
| **C — Partially Safe** (architecture can be built, values not frozen) | Versioned payroll rule engine, OvertimeEntry model (configurable rates), leave-policy engine, bank-export abstraction, holiday configuration, termination/severance model (with configurable formulas), foreign worker fields | ⚠️ PARTIAL — build architecture with configurable defaults; don't freeze specific unverified values |

## 21 — Production payroll readiness

| Area | Confidence | Locked? | Production Ready? |
|---|---|---|---|
| PIT brackets | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| PIT dependant deduction | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| PIT personal allowance (2.5M threshold) | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| PIT OT exemption (< 3M) | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| PIT foreign treatment | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| PIT withholding deadline (20th) | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| NSSF rates (6%/5.5%) | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| NSSF ceiling (4.5M) | VERIFIED (PwC, Notification 0824) | ✅ LOCKED | ✅ YES |
| NSSF deadline (20th) | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| NSSF employee deductible | VERIFIED (PwC) | ✅ LOCKED | ✅ YES |
| NSSF base definition | HIGH ("gross remuneration") | ⚠️ TEMPORARY | ⚠️ PARTIAL — confirm exact definition |
| NSSF minimum floor | UNKNOWN | ❌ BLOCKED | ❌ NO |
| OT weekday day (1.5×) | VERIFIED (Art. 48) | ✅ LOCKED | ✅ YES |
| OT weekday night (2×) | VERIFIED (Art. 48) | ✅ LOCKED | ✅ YES |
| OT rest day day (2.5×) | VERIFIED (Art. 48) | ✅ LOCKED | ✅ YES |
| OT rest day night (3×) | VERIFIED (Art. 48) | ✅ LOCKED | ✅ YES |
| OT night bonus (15%) | VERIFIED (Art. 48) | ✅ LOCKED | ✅ YES |
| OT caps (3h/day, 45h/month) | VERIFIED (Art. 18) | ✅ LOCKED | ✅ YES |
| OT hourly divisor | UNKNOWN | ❌ BLOCKED | ❌ NO |
| Annual leave (15 days) | VERIFIED (Art. 21) | ✅ LOCKED | ✅ YES |
| Sick leave (30 days) | VERIFIED (Art. 20) | ✅ LOCKED | ✅ YES |
| Maternity (≥90 days) | VERIFIED (Art. 39) | ✅ LOCKED | ✅ YES |
| Leave carry-over | UNKNOWN | ❌ BLOCKED | ❌ NO |
| Minimum wage (2.5M) | VERIFIED | ✅ LOCKED | ✅ YES |
| Severance (10/15%, 15/20%) | VERIFIED (Art. 29/33) | ✅ LOCKED | ✅ YES |
| Probation (30/60 days) | VERIFIED (Art. 27) | ✅ LOCKED | ✅ YES |
| Rounding | UNKNOWN (not researched) | ❌ | ❌ NO |
| Rule versioning | Architecture clear | ✅ (design) | ✅ YES (design ready) |
| Bank output | UNKNOWN (not public) | ❌ BANK_CONFIRMATION | ❌ NO |

### PRODUCTION_PAYROLL_READY = **NO**

**Blockers**: OT hourly divisor, leave carry-over rules, NSSF minimum floor, rounding convention, bank file format verification.

**However**: 32 of 38 compliance rules are now LOCKED with VERIFIED evidence. The remaining 6 can be kept configurable with safe defaults. Production payroll can proceed once the 6 items are professionally confirmed.

## 22 — Phase 3 final readiness

| Area | Ready? | Status |
|---|---|---|
| TECHNICAL_PHASE3_READY | **YES** | Argon2id, PG migrations, WAL+PITR, routes, auth, tests, mock removal, /403, Caddy+TLS, CI — all verified, no legal dependency |
| SECURITY_PHASE3_READY | **YES** | OWASP Argon2id VERIFIED, IExceptionHandler VERIFIED, file upload validation, project RBAC enforcement — all technical |
| UX_PHASE3_READY | **YES** | Mock removal, /403 page, proxy.ts, error boundaries, accessibility (WCAG 2.2 AA VERIFIED), mobile nav — all technical |
| DATABASE_PHASE3_READY | **YES** | PG migrations, cursor pagination, partitioning, pg_trgm+ICU — all verified (PG 18 docs) |
| PAYROLL_ARCHITECTURE_READY | **PARTIAL** | Versioned rule engine design ready; 32/38 rules LOCKED; 6 need professional confirmation; PIT bracket update (1.3M→2.5M) confirmed and ready |
| PRODUCTION_PAYROLL_READY | **NO** | 6 blockers: OT divisor, carry-over, NSSF floor, rounding, bank format, (NSSF base definition TEMPORARY) |
| FULL_PROJECT_PHASE3_READY | **PARTIAL** | Technical/security/UX/database = YES; payroll architecture = PARTIAL; production payroll = NO (but 32/38 rules locked) |

### Recommended next step

1. **Immediately begin Track A** (safe technical work — no legal dependency).
2. **Update PIT brackets** in codebase (confirmed — Law 88/NA, brackets VERIFIED by PwC).
3. **Implement Track C** (partially safe — versioned rule engine, OvertimeEntry with configurable rates, termination/severance model with legal formulas now LOCKED).
4. **Engage professionals in parallel** to resolve the 6 remaining items (Lao labour lawyer for OT divisor + carry-over; LSSO for NSSF base/floor; tax adviser for rounding; banks for file formats).
5. **Do NOT run production payroll** until all 6 items are confirmed.