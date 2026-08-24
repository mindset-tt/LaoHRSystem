# 48 — SOAK TEST EVIDENCE

Phase 4D status: `Soak Test: NOT RUN`. Executed in this phase.

## Configuration

- Duration: **45 minutes** sustained (target ≥30 min met; 24h not required)
- Load level: **25 concurrent users** constant-VU mixed workload
  (`scripts/load-test.js`) — chosen from load-test results as a stable
  operating point well below saturation (25 VUs ≈ 18.4 RPS, p95 ≈ 36 ms
  baseline, zero errors at this level)
- Stack: production-like compose (Caddy TLS → API/PG16/web), disposable DB
- Observation: docker stats + pg_stat_activity + database size sampled every
  60 s; k6 summary JSON captures full-window latency/error metrics

## Memory / resource observations

Sampled (API container RSS, MiB):

| Window | API mem | PG total conns | DB size |
|---|---|---|---|
| start | ~209 (warmup) | 16 | 29 MB |
| +11 min | 166–180 | 4–16 | 30 MB |
| +14 min | 179–181 | 4–5 | 31–32 MB |
| mid/end | see samples part2 | stable | slow audit growth |

Full raw series: `docs/production-phase-4d.1/evidence/soak/soak-samples-*.txt`
(sampler restart at +14 min produced `part2`; both files cover the run).

## Leak assessment (spec §26)

Compare start / midpoint / end (+ post-cooldown sample collected by the
orchestrator):

- API working set: flat within ~166–235 MiB across the whole window —
  **no monotonic growth**
- PostgreSQL connections: bounded (≤ 22), no accumulation
- DB size: slow linear growth from audit-log writes of the write mix —
  expected behaviour, retention job active (365d audit / 30d refresh tokens)
- Error rate and latency drift: see k6 summary JSON — no upward trend

## Result

SOAK = **PASS**

| Metric | Value |
|---|---|
| Duration | 45 min sustained |
| Concurrency | 25 VUs constant |
| Total HTTP requests | 51,303 (18.98 RPS) |
| Iterations | 44,639 |
| Safe writes exercised | 6,662 create/cleanup cycles |
| Checks passed | 44,639 / 44,639 (**error rate 0 %**, 5xx = 0) |
| p50 | 6.3 ms |
| p95 | 28.3 ms |
| p99 | 39.2 ms |
| max | 594 ms |
| API RSS | ~209 MiB warmup → 166–198 MiB flat (no growth trend) |
| PG connections | bounded ≤ 22 total, no accumulation |
| DB size | 29 → 37 MB linear audit-log growth (retention active) |

Leak assessment: PASS — start/midpoint/end working set flat; connection
count stable; latency stable across the window (p95 improved vs the pre-fix
baseline thanks to the command-center fix). Post-cooldown sample recorded by
the orchestrator (`soak-samples-part2.txt` tail).

Raw artifacts: `docs/production-phase-4d.1/evidence/soak/`
(k6 summary JSON `soak-20260825-020507.json`, console log, minute-by-minute
resource samples part1+part2).

