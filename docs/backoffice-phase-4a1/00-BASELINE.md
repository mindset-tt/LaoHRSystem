# 00 — Baseline (Phase 4A.1)

Re-baselined 2026-08-24 (not trusting the Phase 4A summary blindly).

| Area | Result |
|---|---|
| Backend build | PASS (0 warnings, 0 errors) |
| Backend tests | 161 PASS / 0 FAIL (before 4A.1 additions) |
| Frontend tests | 33 PASS / 0 FAIL (before 4A.1 additions) |
| Frontend typecheck | PASS |
| Frontend build | PASS |
| Frontend lint | 41 errors / 38 warnings (pre-existing) |
| CI/CD | `.github/workflows/ci.yml` PRESENT (removed in this phase) |
| Design-time factory | hardcoded `Password=laohr` PRESENT (removed in this phase) |
| PostgreSQL | native 18.6 on 5432; Docker daemon running |

## Key findings
1. CI/CD was NOT actually removed in Phase 4A (`.github/workflows/ci.yml` existed).
2. Design-time factory had a hardcoded DB password.
3. Phase 4A validated against PostgreSQL 18.6, not the canonical 16 target.
4. Budget enforcement (hard block on PR/PO) was unfinished.
5. Asset auto-generation from Goods Receipt was unfinished.
6. Back Office dashboard/analytics/reports were PARTIAL.
