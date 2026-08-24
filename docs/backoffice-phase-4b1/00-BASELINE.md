# 00 — Baseline (Phase 4B.1)

Re-baselined 2026-08-24.

| Area | Result |
|---|---|
| Backend build | PASS (0 warnings, 0 errors) |
| Backend tests | 195 PASS / 0 FAIL (before 4B.1 additions) |
| Frontend tests | 45 PASS / 0 FAIL (before 4B.1 additions) |
| Frontend typecheck | PASS |
| Frontend build | PASS |
| Frontend lint | 41 errors / 38 warnings (pre-existing) |
| CI/CD | NONE |
| Canonical DB | PostgreSQL 16 |

## Finance state (before 4B.1)
- Finance RBAC was effectively Admin-only (`FinanceAccessService.IsFinance() => IsInRole("Admin")`).
- Segregation of duties was PARTIAL (capabilities only, no enforcement).
- No accounting configuration (control accounts) — auto-posting not possible.
- No auto-posting (invoice/payment/expense → journal).
- No bank data masking.
- No finance report/export endpoints.
