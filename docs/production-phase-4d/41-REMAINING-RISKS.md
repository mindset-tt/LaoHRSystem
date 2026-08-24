# 41 — REMAINING RISKS

## P0 (production blockers)

None remaining in code. (Statutory gates are separate — see below.)

## P1 (should fix before release)

- Load/soak testing (harness not executed).
- Off-host backup sync (design documented; operator step).

## P2 (acceptable follow-up)

- Prometheus-compatible metrics endpoint.
- Frontend bundle-size analysis.
- Malware scanning (ClamAV) — DEFERRED_WITH_RISK.
- Lint debt (41 errors / 39 warnings, historical).

## Statutory gates (independent)

- `ACCOUNTING_STATUTORY_READY = NO` (no authoritative Lao tax/VAT/withholding).
- `PRODUCTION_PAYROLL_READY = NO` (separate legal blockers).

## Status

Documented.
