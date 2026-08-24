# 06 — Lao Leave & Holidays (ລາພັກ / ວັນພັກລັດຖະການ)

> Research date: 2026-08-21. Sources: Wikipedia (Public holidays in Laos), HR guides.
> ⚠️ **LEGAL DISCLAIMER**: Exact statutory leave day-counts are MEDIUM/LOW confidence. This is NOT legal advice.

## Public holidays (ວັນພັກລັດຖະການ)

### Announcement mechanism
- **VERIFIED (HIGH)**: Public holidays are announced **annually by government decree** (Prime Minister's Office / MOLSW). May add or substitute days each year.
- **Lao term**: ວັນພັກລັດຖະການ (wan phak lat tha kan) = official holiday. ບຸນ (bun) = festival/holiday.

### Fixed-date holidays (commonly observed)

| Date | Holiday (English) | Holiday (Lao) | Confidence |
|---|---|---|---|
| 1 January | New Year's Day | ປີໃໝ່ສາກົນ | HIGH |
| 22 March | Lao People's Party Day | ວັນສ້າຕັ້ງພັກ | MEDIUM |
| 13–15 April | Lao New Year (Pi Mai Lao) | ປີໃໝ່ລາວ / ສັງຂຣານ | HIGH |
| 1 May | International Labour Day | ວັນແຮງງານສາກົນ | HIGH |
| 2 December | Lao National Day | ວັນຊາດ | HIGH |

### Lunisolar / variable-date holidays

| Holiday (English) | Holiday (Lao) | Date basis | Confidence |
|---|---|---|---|
| Boun Makha Bucha | ບຸນມາຂະບູຊາ | Full moon, 3rd lunar month (Feb/Mar) | MEDIUM |
| Boun Visakha Bucha | ບຸນວິສາຂະບູຊາ | Full moon, 6th lunar month (May) | MEDIUM |
| Boun Khao Phansa (Buddhist Lent begins) | ບຸນເຂົ້າພັນສາ | Full moon, 8th lunar month (July) | MEDIUM |
| Boun Ok Phansa (Buddhist Lent ends) | ບຸນອອກພັນສາ | Full moon, 11th lunar month (Oct) | MEDIUM |
| Boun That Luang | ບຸນທາດຫຼວງ | November (full moon) | MEDIUM |
| Lao Women's Union Day | ວັນສ້າຕັ້ງສະຫະພາບແມ່ຍິງ | Late March (variable) | MEDIUM |

**Total**: ~12–14 official days/year (varies by annual decree).

### LaoHR current implementation
- `Holiday` entity: `Date` (unique), `Name`, `NameLao`, `Year`, `IsRecurring`, `IsActive`.
- `HolidaysController` with `seed-defaults` endpoint.
- `WorkDayService` checks holidays when calculating work days.

**GAPS**:

| Gap | Severity | Priority |
|---|---|---|
| Holidays are hardcoded per year; lunisolar holidays must be manually entered each year (no lunar calendar calculation) | MEDIUM | P2 — add lunar calendar helper or annual import |
| No mechanism to import annual government holiday decree | MEDIUM | P2 — add holiday import (CSV/Excel) |
| Substitute holidays (when a holiday falls on a weekend, a substitute day may be declared) not modelled | LOW | P3 |
| `IsRecurring` flag exists but lunisolar holidays recur on lunar dates, not Gregorian — `IsRecurring=true` would only work for fixed-date holidays | MEDIUM | P2 — distinguish fixed-recurring vs lunar |

## Leave entitlements (ລາພັກ / ລາປ່ວຍ / ລາເກີດລູກ)

> Governed by Labour Law (2006, as amended). Commonly reported provisions — **exact day-counts require confirmation**.

| Leave type | Lao term | Entitlement | Confidence | LaoHR status |
|---|---|---|---|---|
| Annual leave | ລາພັກປະຈຳປີ | ~12–15 days/year after 1 year of service + public holidays | MEDIUM | `LeavePolicy` with `AnnualQuota` (configurable) — DONE |
| Sick leave | ລາປ່ວຍ | ~30 days/year (paid, longer-term via social security) | MEDIUM | `LeavePolicy` supports SICK type — DONE |
| Maternity leave | ລາເກີດລູກ | ~90–105 days, full pay (funded via social security maternity benefit) | MEDIUM (existence HIGH; exact days MEDIUM) | `LeavePolicy` supports MATERNITY type — DONE |
| Paternity leave | ລາພັກພໍ່ | ~3 days | LOW | `LeavePolicy` could support PATERNITY — check if seeded |
| Marriage leave | ລາພັກແຕ່ງງານ | ~3 days (own marriage) | LOW | Not confirmed seeded |
| Bereavement leave | ລາພັກຍ້າຍສົບ | ~3 days (close family death) | LOW | Not confirmed seeded |
| Special/unpaid leave | ລາພັກບໍ່ໄດ້ຮັບຄ່າຈ້າງ | By agreement | LOW | `LeavePolicy` could support UNPAID — check if seeded |

### Carry-over / accrual / expiration
- Annual leave should generally be taken in the year earned; carry-over and encashment rules exist but specifics not retrieved. **UNKNOWN.**
- LaoHR: `LeaveBalance` has `CarriedOverDays`, `LeavePolicy.MaxCarryOver`, `AccrualPerMonth`, `LeaveScheduledJobsService` (monthly accrual + year-end carry-over). **VERIFIED good architecture.**
- **GAP**: Carry-over limits and expiration rules must be confirmed against the law. If the law mandates expiration of carried-over days after X months, LaoHR should enforce it. **P2.**

### Seniority rules
- Leave may increase with years of service (seniority-based accrual). **UNKNOWN — confirm.**
- **GAP**: LaoHR's `LeavePolicy.AccrualPerMonth` is flat (not seniority-based). If seniority rules apply, add a seniority-based accrual table. **P3.**

## LaoHR gap summary (leave + holidays)

| LAW | CURRENT IMPLEMENTATION | GAP | REQUIRED FUTURE CHANGE | Priority |
|---|---|---|---|---|
| Annual leave ~12–15 days | Configurable `AnnualQuota` | Confirm default matches law | Verify seeded `LeavePolicy` values | P0 |
| Maternity ~90–105 days | MATERNITY type supported | Confirm days + funding source (NSSF maternity benefit?) | Verify seeded values; link to NSSF maternity | P1 |
| Paternity ~3 days | May not be seeded | Add if missing | Seed PATERNITY policy | P2 |
| Marriage ~3 days | May not be seeded | Add if missing | Seed MARRIAGE policy | P2 |
| Bereavement ~3 days | May not be seeded | Add if missing | Seed BEREAVEMENT policy | P2 |
| Carry-over limits | `MaxCarryOver` exists | Confirm limits match law | Verify seeded values | P1 |
| Carry-over expiration | Not enforced | May need expiration after X months | Add expiration logic | P2 |
| Seniority-based accrual | Flat `AccrualPerMonth` | May need seniority tiers | Add seniority accrual table | P3 |
| Lunisolar holidays | `IsRecurring` (Gregorian only) | Cannot auto-calculate lunar holidays | Add lunar calendar helper or annual import | P2 |
| Annual holiday decree import | `seed-defaults` (fixed) | No annual import | Add CSV/Excel holiday import | P2 |
| Substitute holidays | Not modelled | May need when holiday falls on weekend | Add `SubstituteDate` field | P3 |