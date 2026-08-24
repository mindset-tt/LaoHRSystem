# 20 — REMAINING BLOCKERS

Honest remaining blockers (technical/legal only; CI/CD absence is NOT a blocker).

## Not blockers

- CI/CD: NONE BY DESIGN (`.github/workflows` absent, intentionally).

## Actual blockers / open items

1. **TLS** — production transport security not yet configured/verified.
2. **Frontend HIGH dependency vulnerabilities** — not yet audited.
3. **Observability** — OpenTelemetry wired but not production-verified.
4. **Load/performance test** — not yet run.
5. **Off-host backup / DR** — backup/restore proven locally; off-host DR unresolved.
6. **Lao payroll statutory blockers** — unresolved (see below).
7. **Lao accounting/tax confirmation** — unresolved (see below).

## Statutory status

- `ACCOUNTING_STATUTORY_READY = NO` — no authoritative Lao VAT/withholding/tax
  values were researched or verified. No guessed values are used.
- `PRODUCTION_PAYROLL_READY = NO` — separate legal blockers unresolved.

## Status

Documented; these are Phase 4C+ concerns, not 4B.2 blockers.
