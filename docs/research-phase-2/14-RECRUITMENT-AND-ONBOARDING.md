# 14 — Recruitment & Onboarding + 15 — Performance & Talent + 16 — Attendance & Shifts

> Research date: 2026-08-21.

## Recruitment / ATS (P3 — not started)

Research from competitors (OrangeHRM, ERPNext, Odoo, BambooHR, Zoho):
- **Vacancy** → **Job Posting** → **Candidate/Application** → **Interview** → **Evaluation** → **Offer** → **Hire** → **Onboarding**.

| Stage | LaoHR | Priority |
|---|---|---|
| Vacancy management | ❌ | P3 |
| Job posting | ❌ | P3 |
| Candidate tracking | ❌ | P3 |
| Interview scheduling | ❌ | P3 |
| Offer letter generation | ❌ | P3 |

**Verdict**: Recruitment is a major module. Build only if customer demand exists. For now, employee creation (`POST /api/employees`) is the entry point. **P3 — defer.**

## Onboarding / Offboarding (P2)

| Stage | LaoHR | Priority |
|---|---|---|
| Onboarding task checklist | ❌ | P2 |
| Document collection | ✅ (EmployeeDocument) | — |
| Equipment/account provisioning | ❌ | P3 |
| Probation tracking | ❌ | P2 |
| Offboarding exit checklist | ❌ | P2 |
| Asset return | ❌ | P3 |
| Final payroll + severance | ❌ | P2 |
| Knowledge transfer | ❌ | P3 |

**Recommendation**: Build a lightweight `OnboardingTask` / `OffboardingTask` checklist model (task name, assignee, due date, status). No full BPM. **P2.**

## Performance management (P2)

| Feature | LaoHR | Priority |
|---|---|---|
| Goals / KPIs | ❌ | P2 |
| OKRs | ❌ | P3 |
| Review cycles | ❌ | P2 |
| 360 feedback | ❌ | P3 |
| Manager reviews | ❌ | P2 |
| Calibration | ❌ | P3 |
| Development plans | ❌ | P3 |

**Recommendation**: Start with goal-setting + annual/semi-annual review cycle. Defer 360/OKR/calibration. **P2.**

## Attendance architecture (current vs mature)

| Feature | LaoHR | Priority | Notes |
|---|:--:|:--:|---|
| Clock in/out | ✅ | — | Geolocation |
| Breaks | ❌ | P2 | No break tracking |
| Shifts | ❌ | P2 | No shift definition |
| Rotations | ❌ | P3 | |
| Late detection | ✅ | — | `IsLate` vs `LateThresholdMinutes` |
| Early leave | ✅ | — | `IsEarlyLeave` |
| Missing punch | ❌ | P2 | No detection |
| Correction workflow | ❌ | P2 | |
| Overtime tracking | ⚠️ | P1 | No day-type differentiation |
| Holiday work | ❌ | P2 | Linked to OT rates |
| Timesheet | ❌ | P1 (per audit) | PM capacity |

**Recommendation**: Add shift definition + break tracking + missing-punch detection + correction workflow. Timesheet is a P1 for PM capacity but requires task model (already exists). **P2** for most attendance enhancements.

## Biometric integration (current)

- **LaoHR**: ZKTeco via `LaoHR.Bridge.Service` (Windows Service) + `LaoHR.Bridge.Config` (WPF).
- **Architecture**: Separate worker service syncs attendance from devices → DB. Good isolation.
- **Gaps**: Bridge apps still reference SQL Server (not migrated to Npgsql). SDK is vendor-specific (ZKTeco).
- **Recommendation**: Migrate Bridge apps to Npgsql (P2). Do NOT add other vendor integrations unless customer demand. **Keep ZKTeco.**