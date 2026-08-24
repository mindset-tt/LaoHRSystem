# 10 — Lao HR Data Model Research (ໂມເດລະບົບຂໍ້ມູນ)

> Research date: 2026-08-21. Compares current LaoHR entities to Lao regulatory + HRIS requirements.

## Current entity inventory (40 entities)

See `docs/ai-handoff/08-DATABASE.md` for the full list. Key HR entities: `Department`, `Employee`, `Attendance`, `PayrollPeriod`, `SalarySlip`, `LeaveRequest`, `LeavePolicy`, `LeaveBalance`, `TaxBracket`, `SystemSetting`, `Holiday`, `EmployeeDocument`, `CompanySetting`, `WorkSchedule`, `ConversionRate`, `AppUser`, `RefreshToken`.

## Data model gap matrix

| Current Entity | Required Capability | Gap | Severity | Migration Complexity | Evidence |
|---|---|---|---|---|---|
| `Employee` | `EmploymentType` (permanent/fixed-term/seasonal/probation) | Missing | HIGH | LOW (add column) | `02-LAO-LABOUR-LAW.md` |
| `Employee` | `ProbationEndDate` | Missing | MEDIUM | LOW | `02`, `07` |
| `Employee` | `ManagerId` (reporting line) | Missing | HIGH | LOW (add self-FK) | `07` |
| `Employee` | `Nationality` / `TaxResidency` | Missing | MEDIUM | LOW | `04` (foreign employee tax) |
| `Employee` | `HireDate` exists, but no `TerminationDate`/`TerminationReason` | Missing | MEDIUM | LOW | `02`, `07` |
| `Employee.LaoName`/`EnglishName` | Split into first/last name | Single field | MEDIUM | MEDIUM (data migration) | `08` |
| `Employee.Phone` | E.164 normalization + validation | Free text | LOW | LOW | `08` |
| `Employee` | `StreetAddress`, `PostalCode`, `Country` | Missing | LOW | LOW | `08` |
| `Department` | `ParentDepartmentId` (hierarchy) | Flat | MEDIUM | LOW (add self-FK) | `07` |
| `SalarySlip` | `OvertimeEntry` (date, hours, type: normal/rest/holiday/night) | Single `OvertimePay` | HIGH | MEDIUM (new entity) | `02`, `03` |
| `SalarySlip` | Taxable vs non-taxable allowances split | Single `Allowances` | HIGH | MEDIUM | `04` |
| `SalarySlip` | Personal allowance + dependant deductions | Not modelled | HIGH | MEDIUM | `04` |
| `SalarySlip` | `LoanInstallmentDeduction` (auto from `EmployeeLoan`) | Manual via `OtherDeductions` | MEDIUM | MEDIUM | `03` |
| `SystemSetting` | NSSF contribution ceiling | Unknown if set | HIGH | LOW (set value) | `05` |
| `TaxBracket` | Verify seeded values match current law | Unverified | CRITICAL | LOW (update seed) | `04` |
| `LeavePolicy` | Verify seeded values (annual/sick/maternity days) match law | Unverified | HIGH | LOW (update seed) | `06` |
| `LeavePolicy` | PATERNITY/MARRIAGE/BEREAVEMENT types | May not be seeded | MEDIUM | LOW | `06` |
| `LeaveBalance` | Carry-over expiration enforcement | Not enforced | MEDIUM | MEDIUM | `06` |
| `Holiday` | Lunisolar holiday support | `IsRecurring` is Gregorian-only | MEDIUM | MEDIUM (lunar calc or annual import) | `06` |
| `Holiday` | `SubstituteDate` | Missing | LOW | LOW | `06` |
| `WorkSchedule` | Night work definition + premium | Not modelled | MEDIUM | MEDIUM | `02` |
| `EmployeeDocument` | Document expiry tracking (work permit, visa, contract) | Missing | MEDIUM | LOW (add `ExpiryDate`) | `24` (HR docs) |
| `CompanySetting` | Multi-entity/multi-branch | Singleton | LOW (if single-org) | HIGH | `07`, `56` |
| — | `Position` entity (slot-based, FTE) | Missing | MEDIUM | MEDIUM (new entity) | `07` |
| — | `CostCenter` | Missing | LOW | LOW | `07`, `68` |
| — | `EmployeeJobHistory` (promotion/transfer log) | Missing | MEDIUM | MEDIUM | `07` |
| — | `Termination`/`Severance` model | Missing | MEDIUM | MEDIUM | `02`, `07` |
| `AuditLog` | Partition by month | Unbounded | MEDIUM | MEDIUM (partition migration) | `29` |

## Foreign employee tracking

| Required field | LaoHR | Priority | Evidence |
|---|---|---|---|
| `Nationality` | ❌ | P2 | `04`, `25` |
| `WorkPermitNumber` + `WorkPermitExpiry` | ❌ | P2 | `25` |
| `VisaNumber` + `VisaExpiry` | ❌ | P2 | `25` |
| `StayPermitExpiry` | ❌ | P2 | `25` |
| `TaxResidency` | ❌ | P2 | `04` |

**GAP**: LaoHR cannot track foreign employee documents/expiries. For organizations with expat staff, this is a compliance risk. **P2.**

## HR document types

| Document | Legal requirement? | LaoHR `EmployeeDocument.DocumentType` | Priority |
|---|---|---|---|
| Employment contract (ສັນຍາແຮງງານ) | YES | `Contract` ✅ | — |
| ID card (ບັດປະຈຳຕົວ) | YES | `ID_Card` ✅ | — |
| Passport (foreign) | YES for foreigners | ❌ not in enum | P2 |
| Visa (foreign) | YES for foreigners | ❌ | P2 |
| Work permit (foreign) | YES for foreigners | ❌ | P2 |
| Tax ID document | Common | `TaxId` field on Employee, no document | P3 |
| Social security document | Common | `NssfId` field on Employee, no document | P3 |
| Resume/CV | Optional | `Resume` ✅ | — |
| Certificate/degree | Optional | ❌ | P3 |
| Salary letter | Optional | ❌ (PDF payslip exists) | P3 |
| Disciplinary document | Legal if disciplinary action | ❌ | P3 |
| Performance record | Optional | ❌ | P3 |
| Termination document | Legal if terminated | ❌ | P2 |

**GAP**: `EmployeeDocument.DocumentType` enum should be extended with: `Passport`, `Visa`, `WorkPermit`, `Certificate`, `Termination`. Add `ExpiryDate` for time-sensitive docs (visa, work permit, contract). **P2.**