# 12 — Employee Self Service (ESS) + 13 — Manager Self Service (MSS)

> Research date: 2026-08-21.

## ESS — current vs mature

| ESS feature | LaoHR | Priority | Notes |
|---|:--:|:--:|---|
| View/edit own profile | ✅ (settings) | — | |
| View payslip | ✅ | — | PDF download |
| View attendance | ✅ | — | Clock in/out |
| Request leave | ✅ | — | Full workflow |
| View leave balance | ✅ | — | |
| View own documents | ⚠️ (mock on detail) | P1 | Fix mock data |
| View own loans | ✅ | — | |
| Submit expense claims | ✅ | — | |
| View announcements | ✅ | — | Read tracking |
| Search knowledge | ✅ | — | |
| View assigned tasks | ✅ | — | My-tasks |
| Update bank details | ❌ | P2 | |
| Request address change | ❌ | P3 | |
| View performance/goals | ❌ | P2 | After performance module |
| View benefits | ❌ | P3 | |
| Download tax certificate | ❌ | P2 | Annual tax doc |
| Download employment certificate | ❌ | P2 | |

## MSS — current vs mature

| MSS feature | LaoHR | Priority | Notes |
|---|:--:|:--:|---|
| View team | ❌ | P1 | No `ManagerId` → no team concept |
| Approve leave | ✅ | — | |
| View team attendance | ⚠️ | P2 | Can filter by employeeId but no "my team" filter |
| View team payroll | ❌ | P2 | No manager visibility |
| Approve expenses | ✅ | — | |
| Approve loans | ✅ | — | |
| Assign tasks | ✅ | — | Project tasks |
| View team performance | ❌ | P2 | After performance module |
| Team headcount/analytics | ❌ | P2 | |
| Approve attendance corrections | ❌ | P2 | No correction workflow |

## Recommendation

1. **P1**: Add `ManagerId` to `Employee` → enables "my team" views across all modules.
2. **P1**: Fix mock document data on employee detail (ESS documents).
3. **P2**: Add ESS: bank details edit, tax/employment certificate download.
4. **P2**: Add MSS: team dashboard, team attendance, team payroll visibility.