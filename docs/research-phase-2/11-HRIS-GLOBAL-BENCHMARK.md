# 11 — HRIS Global Benchmark

> Research date: 2026-08-21. Sources: Official sites + Wikipedia (VERIFIED for OrangeHRM, ERPNext, Odoo, Zoho People; MEDIUM for BambooHR).

## Competitor feature comparison (open-source focus)

| Feature | LaoHR | OrangeHRM (OS) | ERPNext HR (OS) | Odoo HR (OS) | BambooHR | Zoho People |
|---|:--:|:--:|:--:|:--:|:--:|:--:|
| Employee core | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Attendance | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Leave | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Payroll (native) | ✅ | ❌ (connectors) | ✅ | ✅ | ✅ (US/partners) | ✅ (separate product) |
| Recruitment/ATS | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ (separate) |
| Onboarding | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Performance | ❌ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Expenses | ✅ | ❌ (connectors) | ✅ | ✅ | ✅ | ✅ (separate) |
| Loans | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Self-service | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| Mobile app | ❌ (responsive) | ✅ | ⚠️ (responsive) | ✅ | ✅ | ✅ |
| Workflow/approvals | Partial (hardcoded) | ⚠️ | ✅ (Frappe) | ✅ | ✅ | ✅ |
| Reporting | Partial | ✅ | ✅ | ✅ | ✅ | ✅ |
| Biometric (ZKTeco) | ✅ | ❌ | ⚠️ | ⚠️ | ❌ | ❌ |
| Lao localization | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Project management | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Knowledge base | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Multi-currency payroll | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ |

## LaoHR differentiators

1. **Lao compliance** (NSSF, Lao PIT, LAK, bilingual) — no competitor has this natively.
2. **ZKTeco biometric bridge** — rare in SaaS HRIS.
3. **Employee loans** — uncommon in mainstream HRIS (Lao market need).
4. **Integrated PM layer** — most HRIS don't include project management.
5. **Knowledge base + announcements** — included natively.

## LaoHR gaps vs competitors

| Capability | Competitors have | LaoHR | Priority |
|---|---|---|---|
| Recruitment/ATS | ✅ | ❌ | P3 |
| Onboarding/offboarding flows | ✅ | ❌ | P2 |
| Performance management | ✅ | ❌ | P2 |
| Mobile app (native) | ✅ | ❌ (responsive) | P2 (PWA first) |
| Configurable workflow engine | ✅ | ❌ (hardcoded) | P1 |
| Advanced reporting/BI | ✅ | Partial | P2 |
| Cost center / accounting export | ✅ | ❌ | P2 |
| Bulk employee import | ✅ | ❌ | P1 |
| Organization chart | ✅ | ❌ | P2 |

## Key takeaway

LaoHR's HR core + payroll + attendance + leave + biometric is competitive with open-source peers, and its Lao localization is a unique differentiator. The main gaps are in the "talent management" suite (recruitment, onboarding, performance, L&D) and in configurable workflows. These should be added **incrementally based on actual customer demand**, not cloned wholesale. **Keep lightweight.**