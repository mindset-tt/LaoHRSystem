# 36 — Bilingual LaoHR Requirements (ພາສາລາວ + English)

> Research date: 2026-08-21. Each requirement in English + Lao where practical.

---

# Requirement HR-LAO-001: PIT Tax-Free Threshold

## English
The personal income tax-free threshold is LAK 2,500,000/month (presidential decree 6 Aug 2025, effective June 2026). Income above 2.5M is taxed progressively at 5/10/15/20/25%.

## ພາສາລາວ
ຂີດ່ວນເງິນເດືອນທີ່ບໍ່ຕ້ອງເສຍພາສີແມ່ນ 2,500,000 ກີບຕໍ່ເດືອນ (ດຳລັດປະທານປະເທດເລກທີ 6 ສິງຫາ 2025, ມີຜົນບັງຄັບໃຊ້ ມິຖຸນາ 2026). ເງິນເດືອນເກີນ 2,500,000 ຈະຖືກເກັບພາສີແບບຂັ້ນບັນດາ 5/10/15/20/25 ເປີເຊັນ.

## Category
Tax

## Legal Basis
Income Tax Law No. 67/NA (2019/2020) + Presidential Decree (6 Aug 2025)

## Effective Date
June 2026

## Evidence Quality
3/5 (Laotian Times citing MOF)

## Confidence
HIGH

## Current LaoHR State
Codebase seeds 1,300,000 threshold (OUTDATED). Brackets 3-6 are correct.

## Gap
P0 CRITICAL — threshold must be updated from 1,300,000 to 2,500,000. Bracket 2 lower bound must change from 1,300,001 to 2,500,001.

## Future System Requirement
Update `TaxBracket` seed values. Implement versioned compliance rules with EffectiveFrom/To.

## Requires Professional Confirmation?
No (decree is clear; brackets confirmed by MOF via Laotian Times)

---

# Requirement HR-LAO-002: NSSF Contribution Rates

## English
Employer contributes 6.0%, employee contributes 5.5% of wages to the Lao Social Security Fund (total 11.5%).

## ພາສາລາວ
ນາຍຈ້າງປະກອບ 6.0%, ພະນັກງານປະກອບ 5.5% ຂອງເງິນເດືອນເຂົ້າກອງທຶນປະກັນສັງຄົມ (ລວມ 11.5%).

## Category
Social Security

## Legal Basis
Law on Social Security (2013), as amended

## Effective Date
Current (rates effective since 2016)

## Evidence Quality
2/5 (Trading Economics citing MOLSW)

## Confidence
HIGH

## Current LaoHR State
Codebase seeds `NSSF_EMPLOYEE_RATE=0.055`, `NSSF_EMPLOYER_RATE=0.060` — VERIFIED matches.

## Gap
None for rates. Ceiling (HR-LAO-003) is unresolved.

## Requires Professional Confirmation?
No (rates confirmed)

---

# Requirement HR-LAO-003: NSSF Contribution Ceiling

## English
The NSSF contribution ceiling (maximum monthly wage subject to contribution) is LAK 4,500,000 in the codebase but could not be verified from external sources.

## ພາສາລາວ
ເພດານສູງສຸດຂອງເງິນເດືອນທີ່ໃຊ້ຄິດໄລ່ປະກັນສັງຄົມແມ່ນ 4,500,000 ກີບຕໍ່ເດືອນ (ຢູ່ໃນລະບົບແຕ່ບໍ່ສາມາດຢືນຢັນຈາກແຫຼ່ງພາຍນອກໄດ້).

## Category
Social Security

## Confidence
LOW (codebase only; no external confirmation)

## Current LaoHR State
`NSSF_CEILING_BASE=4500000` in `SystemSetting` seed.

## Gap
UNVERIFIED — must confirm with LSSO.

## Requires Professional Confirmation?
**YES — URGENT: confirm with LSSO or qualified Lao accountant**

---

# Requirement HR-LAO-004: Minimum Wage

## English
The national minimum wage is LAK 2,500,000/month, effective 1 October 2024. For unskilled workers, an additional LAK 900,000 subsistence allowance applies.

## ພາສາລາວ
ຄ່າແຮງງານຕ່ຳສຸດແຫ່ງຊາດແມ່ນ 2,500,000 ກີບຕໍ່ເດືອນ, ມີຜົນບັງຄັບໃຊ້ຕັ້ງແຕ່ 1 ຕຸລາ 2024. ສຳລັບກຳມະກອນບໍ່ມີທັກສະ, ຕ້ອງໃຫ້ຄ່າດຳລົງຊີວິດເພີ່ມ 900,000 ກີບ.

## Category
Labour

## Legal Basis
MOLSW decree (Oct 2024) — decree number unknown

## Effective Date
1 October 2024

## Evidence Quality
3/5 (Laotian Times + WageIndicator)

## Confidence
HIGH

## Current LaoHR State
No minimum wage validation.

## Gap
P1 — add validation that BaseSalary ≥ minimum wage (configurable).

## Requires Professional Confirmation?
No (amount confirmed; decree number would be useful for audit)

---

# Requirement HR-LAO-005: Overtime Day-Type Differentiation

## English
Overtime should be differentiated by day type: ordinary weekday (1.5×), rest day (2×, conflicting), public holiday (3×). Monthly cap: 45h (conflicting with 48h).

## ພາສາລາວ
ຄ່າລ່ວງເວລາຄວນແບ່ງຕາມປະເພດວັນ: ວັນທຳມະດາ (1.5ເທົ່າ), ວັນພັກ (2ເທົ່າ), ວັນພັກລັດຖະການ (3ເທົ່າ).

## Category
Labour / Payroll

## Confidence
MEDIUM (1.5×/3×) / LOW (2× conflicting) / LOW (45h vs 48h conflicting)

## Current LaoHR State
Single `OvertimePay` field — no day-type differentiation.

## Gap
P1 — add `OvertimeEntry` model (date, hours, type, rate).

## Requires Professional Confirmation?
**YES — confirm exact multipliers + monthly cap with Lao labour lawyer**

---

# Requirement HR-LAO-006: Annual Leave

## English
Statutory annual leave is 12-15 days/year after one year of service (conflicting sources).

## ພາສາລາວ
ລາພັກປະຈຳປີຕາມກົດໝາຍແມ່ນ 12-15 ມື້ຕໍ່ປີ ຫຼັງຈາກເຮັດວຽກຄົບ 1 ປີ.

## Category
Labour / Leave

## Confidence
LOW (conflicting: 12 vs 15 days)

## Current LaoHR State
`LeavePolicy.AnnualQuota` configurable. Seed value unverified.

## Gap
P0 — verify seeded value against law.

## Requires Professional Confirmation?
**YES — confirm exact statutory days with Lao labour lawyer**

---

# Requirement HR-LAO-007: Maternity Leave

## English
Statutory maternity leave is 90-105 days at full pay (conflicting sources), funded via NSSF maternity benefit.

## ພາສາລາວ
ລາເກີດລູກຕາມກົດໝາຍແມ່ນ 90-105 ມື້ ໂດຍໄດ້ຮັບເງິນເດືອນເຕັມ (ທຶນຈາກປະກັນສັງຄົມ).

## Category
Labour / Leave

## Confidence
LOW (conflicting: 90 vs 105 days)

## Current LaoHR State
MATERNITY type supported in `LeavePolicy`. Seed value unverified.

## Gap
P0 — verify seeded days + funding source.

## Requires Professional Confirmation?
**YES**

---

# Requirement HR-LAO-008: Timezone

## English
Lao PDR uses Indochina Time (ICT), UTC+07:00, no daylight saving. IANA zone: Asia/Vientiane.

## ພາສາລາວ
ສປປ ລາວ ໃຊ້ເວລາອິນດູຈີນ (ICT), UTC+07:00, ບໍ່ມີການປ່ຽນເວລາຕາມລະດູການ.

## Category
Localization

## Confidence
VERIFIED

## Current LaoHR State
`Asia/Vientiane` in `datetime.ts` + hardcoded UTC+7 in `AttendanceController`. Correct.

## Gap
P2 — centralize `TimeZoneInfo` instead of hardcoded string.

## Requires Professional Confirmation?
No

---

# Requirement HR-LAO-009: Bilingual Interface (Lao + English)

## English
The system must support both Lao (ພາສາລາວ) and English throughout the UI, reports, and PDFs.

## ພາສາລາວ
ລະບົບຕ້ອງຮອງຮັບທັງພາສາລາວ ແລະ ອັງກິດ ທົ່ວທຸກສ່ວນ.

## Category
Localization

## Confidence
VERIFIED (LaoHR already has i18n.ts with en/lo)

## Current LaoHR State
`i18n.ts` dictionary (en/lo). Some inline Lao strings bypass dictionary.

## Gap
P2 — move inline strings to dictionary.

## Requires Professional Confirmation?
No

---

# Requirement HR-LAO-010: Password Security (Argon2id)

## English
Password hashing must use Argon2id (OWASP recommended). SHA-256 is unsuitable.

## ພາສາລາວ
ການເຂົ້າລະຫັດລະຫັດຜ່ານຕ້ອງໃຊ້ Argon2id (ຕາມຄຳແນະນຳ OWASP). SHA-256 ບໍ່ເໝາະສົມ.

## Category
Security

## Legal Basis
OWASP Password Storage Cheat Sheet (© 2026)

## Confidence
VERIFIED

## Current LaoHR State
`PasswordHasher` uses SHA-256.

## Gap
P0 CRITICAL — migrate to Argon2id via rehash-on-login.

## Requires Professional Confirmation?
No (OWASP is authoritative)