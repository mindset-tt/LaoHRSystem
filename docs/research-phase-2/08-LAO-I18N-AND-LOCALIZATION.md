# 08 — Lao i18n & Localization (ພາສາລາວ)

> Research date: 2026-08-21. Confidence: VERIFIED (timezone, calendar) / HIGH (fonts, locale) / MEDIUM (Lao-specific UI).

## Timezone

| Item | Value | Confidence |
|---|---|---|
| Lao PDR timezone | **Indochina Time (ICT) = UTC+07:00** | VERIFIED (Wikipedia, Time in Laos) |
| IANA zone | `Asia/Vientiane` | VERIFIED |
| Daylight saving | **Not observed** | VERIFIED |
| Special case | Golden Triangle SEZ observes Beijing Time (UTC+08:00) — edge case, ignore for HR | VERIFIED |

**LaoHR current**: `Asia/Vientiane` used in `lib/datetime.ts` + hardcoded `UTC+7` in `AttendanceController`. `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` in `Program.cs`. **VERIFIED correct.**

**GAP**: Hardcoded `UTC+7` string in `AttendanceController` — should use `TimeZoneInfo.FindSystemTimeZoneById("Asia/Vientiane")` for maintainability. **P2.** The legacy timestamp switch is a workaround — ideally migrate to `timestamptz` (PostgreSQL) and use `DateTimeOffset` throughout. **P3** (breaking change, defer).

## Calendar

| Item | Value | Confidence |
|---|---|---|
| Calendar system | **Gregorian** (official civil calendar) | VERIFIED |
| Buddhist Era (ພຸດທະສັກກະຫຼາດ, BE) | Used informally/culturally (BE = CE + 543). Lao government documents may use BE. | MEDIUM |
| ISO date format | `yyyy-MM-dd` (ISO 8601) | VERIFIED |
| Lao date display | `dd/MM/yyyy` common | MEDIUM |

**LaoHR current**: Gregorian throughout. `formatDate`, `formatDateTime`, `formatDateLao` in `lib/datetime.ts`. **VERIFIED appropriate.**

**GAP**: No Buddhist Era display option. If government-facing documents (NSSF forms, tax forms) require BE dates, add a `formatDateBE` helper. **P2** — confirm if needed for compliance forms.

## Locale

| Item | Value | Confidence |
|---|---|---|
| Lao locale | `lo` (ISO 639-1) / `lo-LA` | VERIFIED |
| Number formatting | `Intl.NumberFormat('lo-LA')` — Lao uses Western digits (0-9) in modern contexts; traditional Lao numerals (໐໑໒໓໔໕໖໗໘໙) exist but are rare in business | MEDIUM |
| Currency formatting | `Intl.NumberFormat('lo-LA', { style: 'currency', currency: 'LAK' })` → ₭21,594 (no decimals in practice) | VERIFIED |
| First day of week | Monday (standard for business) or Sunday | MEDIUM — confirm Lao convention |

**LaoHR current**: Custom `i18n.ts` with `en`/`lo` dictionary. `LanguageProvider` with `localStorage` persistence. No `Intl.NumberFormat`/`Intl.DateTimeFormat` usage confirmed. **GAP**: Add `Intl` formatting for currency/dates. **P1.**

## Lao person names

Lao naming conventions:
- **Order**: Given name + Family name (surname). Traditionally, Lao people have a given name followed by a family name (Western order). However, in formal contexts, the family name may come first (as in official documents).
- **Single name**: Some Lao citizens (especially older/rural) may have only one name historically — though modern ID cards require both.
- **Titles**: Prefixes like ທ້າຽ (Mr.), ນາງ (Ms.), ທ້າວ (young Mr.).

**LaoHR current**: `Employee.LaoName` + `Employee.EnglishName` (two separate fields). `EmployeeCode` (unique).

| Field | LaoHR | Recommendation | Priority |
|---|---|---|---|
| Given name (Lao) | Part of `LaoName` | Split into `FirstNameLao`, `LastNameLao` | P2 |
| Family name (Lao) | Part of `LaoName` | Split | P2 |
| Given name (Latin) | Part of `EnglishName` | Split into `FirstNameEn`, `LastNameEn` | P2 |
| Family name (Latin) | Part of `EnglishName` | Split | P2 |
| Title | ❌ | Add `Title` (Mr/Ms/Dr) | P3 |
| Preferred name | ❌ | Add `PreferredName` | P3 |
| Search name | ❌ | Add `SearchName` (normalized) | P2 |
| Sorting | By `LaoName` or `EnglishName` | Needs collation-aware sort | P2 |

**GAP**: `LaoName`/`EnglishName` as single fields makes sorting, searching, and formal document generation (where you need "Dear Mr. [Surname]") difficult. Splitting into first/last is recommended. **P2** — breaking change, plan carefully.

## Lao address model

**LaoHR current**: `Province` → `District` → `Village` (3-tier, with Lao + English names). `CompanySetting` links to village/district/province.

| Component | LaoHR | Lao reality | Priority |
|---|---|---|---|
| Village (ບ້ານ, ban) | ✅ `Village` | ✅ | — |
| District (ເມືອງ, muang) | ✅ `District` | ✅ | — |
| Province (ແຂວງ, khoueng) | ✅ `Province` | ✅ | — |
| Country | ❌ (implicit Laos) | Add `Country` for foreign employees | P2 |
| Postal code | ❌ | Laos has postal codes (5-digit, e.g., 01000 for Vientiane) | P3 |
| Street address | ❌ | Add `StreetAddress` (house number + street) | P2 |

**GAP**: No postal code, no street address line, no country (for foreign employees). `P2` — add `StreetAddress`, `PostalCode`, `Country`. The province/district/village hierarchy is correct and sufficient for the Lao admin structure. **VERIFIED good.**

**Note**: `DbSeeder.SeedAddresses` fails on PostgreSQL (SQL Server syntax) — address data (provinces/districts/villages) is NOT seeded on PG. **P1** — port seed SQL to PostgreSQL.

## Phone numbers

| Item | Value | Confidence |
|---|---|---|
| Country code | **+856** | VERIFIED |
| Mobile format | `020 XXXX XXXX` (mobile prefix `020`) | HIGH |
| Landline format | `021 XXX XXX` (Vientiane), other provinces vary | MEDIUM |
| Normalized storage | E.164: `+85620XXXXXXXX` | VERIFIED best practice |

**LaoHR current**: `Employee.Phone` (free text, `MaxLength(20)`). No validation, no normalization.

**GAP**: No phone validation or E.164 normalization. **P2** — add validation + normalize to E.164 on save.

## Lao currency display

| Item | Value | Confidence |
|---|---|---|
| Currency | LAK (₭) | VERIFIED |
| Symbol | ₭ or ₭N | VERIFIED |
| Decimal places | 0 in practice (att obsolete) | VERIFIED |
| Display | `₭ 2,500,000` or `2,500,000 ₭` | MEDIUM |
| Lao number words | `LaoNumberUtils` converts to Lao Kip words (ກີບຖ້ວນ) for PDF payslips | VERIFIED (LaoHR has this) |