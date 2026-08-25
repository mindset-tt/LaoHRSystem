# 11 — GO-LIVE GATE (Phase 4D.2)

Every checkbox references measured evidence in this folder. No status theater.

## License signing root

- [x] Old license signing key retired → 02 (fingerprint recorded; live 402)
- [x] Active private signing key outside repository → operator dir, encrypted PKCS#8
- [x] Key not in images / publish artifacts → publish scan + container grep = 0
- [x] Secret scan PASS → 04 (working tree, full history post-purge, artifacts)
- [x] Valid license using new key PASS → live HTTP 200 (02)
- [x] Tampered license rejected → live HTTP 402 (02) + unit tests
- [x] Reissuance strategy documented → 03 (strategy A executed for demo holders)

## Backup / DR

- [ ] **Real independent backup destination configured → NOT AVAILABLE (operator)** 
- [x] Off-host failure detection re-proven on final code → exit code 1, clear error (05)
- [x] Local backup pipeline integrity re-proven → exit 0 + manifest/checksums (05)
- [ ] Real off-host backup executed → NOT RUN (blocked by destination)
- [ ] Restore FROM off-host copy PASS → NOT RUN (blocked); mechanics documented in 06

## TLS trust

- [ ] **Trusted certificate deployed → PARTIAL (no public DNS / org CA available)** (07)
- [ ] Client trust verified externally → blocked as above
- [x] Hostname/HTTP→HTTPS/modern-TLS pipeline behavior → 4D.1 + 08 (308 verified this phase)
- [x] Security headers live incl. single-HSTS fix → 08

## Performance

- [x] Full consistent post-fix series 10 / 25 / 50 / 100 VU → 09
- [x] No unexplained failures → errors=0 at every level

## Tests

- [x] Backend 0 failures → 295 / 295
- [x] Backend ×5 → 5 × 295/295
- [x] Frontend 0 failures → 57 / 57
- [x] Audit 0 high/critical → npm audit: 0 vulnerabilities
- [x] Lint 0 errors → eslint clean
- [x] Build PASS → next build exit 0 (+ backend builds clean)

## Process

- [x] CI/CD absent → none created (by design)

## READINESS OUTCOME (rule §34 applied honestly)

| Dimension | Value | Because |
|---|---|---|
| PRODUCTION_INFRASTRUCTURE_READY | **YES** | unchanged from 4D.1; topology re-booted and re-measured this phase |
| PRODUCTION_SECURITY_READY | **YES** | P1-SEC-001 closed: key rotated+purged+scanned; rotation proven live |
| PRODUCTION_OPERATIONS_READY | **PARTIAL** | real off-host destination still unavailable (P1-DR-001 open) |
| PRODUCTION_DEPLOYMENT_READY | **PARTIAL** | trusted TLS cert not provisionable yet (TLS-001 open); off-host also gates |

Statutory (unchanged, independent):
- ACCOUNTING_STATUTORY_READY = NO
- PRODUCTION_PAYROLL_READY = NO

## Recommendation

**CONDITIONAL GO** — deploy only after the two remaining P1s are closed by the
operator using the runbooks in 05–07; both are environmental actions with
prepared acceptance criteria. Security-critical work of this phase is complete.
