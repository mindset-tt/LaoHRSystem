# 49 — Remaining Blockers

## Lao compliance (6 BLOCKED — payroll legal freeze)
1. OT divisor (statutory value not confirmed).
2. Leave carry-over rules (not confirmed).
3. NSSF floor (not confirmed).
4. NSSF base definition (not confirmed).
5. Bank transfer format (not confirmed).
6. Visa categories + rounding (not confirmed).

## Security
- Frontend 3 HIGH vulns (Next.js 16.1.1, postcss, sharp).
- TLS not provisioned.
- Security headers not configured.
- MFA for admin/HR not confirmed.
- Malware scanning for uploads not configured.

## Operations
- Observability (metrics/tracing) not configured.
- Alerting not configured.
- Load test not run.
- CD pipeline not configured.
- Off-site backup not configured.
- PITR/WAL not configured.
- DR site not configured.

## Process
- Retention policy not finalized.
- RPO/RTO not agreed with business.
- Professional confirmations pending (see 04-PROFESSIONAL-CONFIRMATION-REGISTER).
- Accessibility audit not run.
- Penetration test not run.

## Note
None of these are "fake data" or "guessed values" — they are genuine gaps requiring real inputs (statutory values, business decisions, infrastructure).
