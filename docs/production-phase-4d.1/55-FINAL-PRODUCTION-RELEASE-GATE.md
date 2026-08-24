# 55 — FINAL PRODUCTION RELEASE GATE

Every PASS below references measured evidence (command output, HTTP
responses, test runs, or files) — not "implemented therefore PASS".
Evidence locations: `docs/production-phase-4d.1/evidence/*` and the numbered
documents 46–54 in this folder.

## Release gate table

| Gate | Evidence | Status | Severity |
|---|---|---|---|
| HTTPS live | TLS1.3 handshake captured; 308 redirect; 200 responses (46-LIVE-TLS-EVIDENCE.md) — local CA, public-CA NOT RUN | PARTIAL | P2 (public cert = operator step) |
| Security headers live | HSTS/CSP/XCTO/Referrer/Permissions + frame-ancestors on real responses, frontend AND API | PASS | — |
| Dependency scan | npm audit: 0 vulns; dotnet list --vulnerable --include-transitive: none (API/Shared/Tests) | PASS | — |
| Secret scan | pattern scan clean; stray key copies removed; LicenseGen private.key committed-by-design → flagged | PARTIAL | P2 |
| Upload security | exe-as-pdf/html-as-pdf/double-ext/traversal/zero-byte/oversize/svg all rejected; authorized download w/ checksum roundtrip (47a) | PASS | — |
| Malware decision | ClamAV measured 1.014 GiB RSS on 7.66 GiB host → DEFERRED_WITH_RISK with strengthened controls + documented enablement path (52) | DEFERRED_WITH_RISK | P2 accepted-risk |
| Rate-limit test | live: 5×401 then 429 Too Many Requests; Retry-After added | PASS | — |
| Auth smoke | invalid/expired/garbage token 401; employee→finance 403; HR→payments/bank-accounts/AP-aging 403; employee→payroll 403; IDOR document access 403 | PASS | — |
| Load 10 | 7.46 RPS, p50 6.9 / p95 32.6 / p99 661→(post-fix tail collapsed), 0 errors | PASS | — |
| Load 25 | 18.41 RPS, p50 7.1 / p95 36.3, 0 errors | PASS | — |
| Load 50 | 37.24 RPS, p50 7.2 / p95 30.2 / p99 221, 0 errors | PASS | — |
| Load 100 | 74.24 RPS, p50 7.8 / p95 32.3 / p99 99.4, 0 errors | PASS | — |
| Soak | 45 min @25 VUs, 51,303 reqs, 0 failures, p95 28.3 ms, memory flat (48) | PASS | — |
| Metrics | /metrics internal-only (404 via proxy), series verified changing incl. laohr_uploads_rejected_total (49) | PASS | — |
| Alerting | scripts/ops-checks.ps1 executable; found+fixed storage-permission bug during validation; thresholds labelled INTERNAL_TECHNICAL_DEFAULTS | PASS | — |
| DB performance | endpoint latency sweep; EXPLAIN (ANALYZE, BUFFERS) recorded for representative queries | PASS | — |
| Indexes | no new index required (bottleneck was N+1); existing IX_StockMovements_ItemId... confirmed optimal by planner | PASS (no change needed) | — |
| Connection pool under load | pg_stat_activity sampled per level; peak 76 conns @100 VUs vs default max 100; zero exhaustion/timeouts; no tuning applied | PASS | — |
| Off-host backup | executed + hash-re-verified at destination; OFFHOST_DR_SIMULATION labelled (50) | SIMULATED (P1: real off-host host pending) | P1 |
| Backup failure detection | unset password → exit≠0; unwritable/nonexistent destination → exit 1, clear error, no false SUCCESS | PASS | — |
| Restore drill | clean DB + fresh doc volume from off-host copy (51) | PASS | — |
| HTTP restore smoke | REAL HTTP against restored app: health/login/employees/suppliers/finance/corporate/GL all 200 | PASS | — |
| Document restore | downloaded via API from restored storage; SHA256 MATCH end-to-end | PASS | — |
| DB outage | live=200, ready=Unhealthy, sanitized 500s, auto-recovery after restart | PASS | — |
| Storage outage | uploads fail sanitized; rest of API unaffected; detection via ops-check #4; readiness policy documented | PASS | — |
| Backend tests | 291/291 (268 prior + 23 new upload/signature tests) | PASS | — |
| Frontend tests | 57/57 | PASS | — |
| Lint | 0 errors / 0 warnings (was 19/31) | PASS | — |
| PG16 races | payment/journal/auto-post/period-close/room/vehicle — 7/7 against REAL PostgreSQL 16 (LAOHR_TEST_CONNECTION harness); earlier "passes" were skips and are corrected here | PASS | — |

Additional gates:

| Gate | Evidence | Status |
|---|---|---|
| Production config fail-fast | missing JWT/conn/origins + dev-JWT rejection + compose password guard (54) | PASS |
| Container security | non-root users (API appuser, web `app`); no privileged mode; healthchecks functional (curl installed) | PASS |
| Network exposure | only 80/443 published; PG/API/web/metrics internal; /metrics unreachable via proxy | PASS |
| Frontend bundle review | total static 2.51 MB; largest chunk 224 KB (framework); deps minimal (react-hook-form/zod); no heavy additions | PASS |
| Dashboard waterfalls | dashboards use single/promise-all fetches; one minor serial pair in my-team (documented, non-blocking) | PASS |
| Backend build | dotnet build 0 errors | PASS |
| Backend ×5 | 5 × 291/291 consecutive | PASS |
| Frontend typecheck/build | tsc exit 0; next build PASS | PASS |
| Runbooks | docs/production-phase-4d/43+44 retained; ops-checks integrated into incident procedures | PASS |

## Readiness rule outcomes

- **PRODUCTION_INFRASTRUCTURE_READY = YES**
  (no infrastructure P0 remains: topology boots, TLS works, outages behave,
  backup/restore proven)
- **PRODUCTION_SECURITY_READY = YES**
  (no security P0 remains; malware scanning is an accepted P2 risk with
  compensating controls — sign-off recorded in 52)
- **PRODUCTION_OPERATIONS_READY = YES**
  (backup/restore/load/soak/monitoring/runbooks operationally proven;
  remaining items are operator-side: public CA cert, true off-host host)
- **PRODUCTION_DEPLOYMENT_READY = YES** (conditional notes below)

Statutory (independent of the above):
- ACCOUNTING_STATUTORY_READY = NO (requires independent verification)
- PRODUCTION_PAYROLL_READY = NO (requires independent verification)

## Remaining items

### P0
None.

### P1
1. Off-host backup currently OFFHOST_DR_SIMULATION (second local path).
   Provision a real second host/NAS target and re-run
   `scripts/backup-production.ps1 -OffhostDest <real>`.

### P2
2. Public-CA TLS certificate provisioning (Let's Encrypt/org CA) at the
   edge; local-CA evidence covers pipeline behaviour only.
3. LicenseGen `private.key` is committed to the repository (pre-existing
   design). Before commercial operation, remove from VCS, rotate keys, and
   re-issue customer licenses.
4. Malware scanning deferred with risk (52) — enable when host capacity
   allows (+1 GiB RAM) using the documented clamd INSTREAM hook.
5. Duplicate HSTS header on proxied API responses (Caddy + API both emit);
   harmless (browsers take first/max) but tidy up by disabling API-side HSTS
   behind a trusted proxy.
6. k6 cleanup DELETEs return 405 (no service-request delete route) — noise
   in load metrics only.
7. my-team page has one serial await pair; parallelize opportunistically.

## Production launch recommendation

**GO** — conditional on operator completion of P1 (real off-host target)
before go-live, with P2 items tracked. All code-level and operational
evidence gates pass; statutory/payroll readiness remains separately NO until
independently verified (does not block infrastructure deployment).
