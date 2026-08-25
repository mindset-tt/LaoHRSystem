# 09 — FINAL CONSISTENT LOAD BASELINE

Purpose: 4D.1 mixed pre-fix (10/25) and post-fix (50/100) numbers. This phase
re-runs **all levels on the final code** for one coherent baseline.

## Harness identity (identical across all four runs)

- k6 v1.4.0, `scripts/load-test.js`, `INTERNAL_ENGINEERING_PROFILE`
- dataset: `scripts/seed-load-test.sql` (500 employees, full synthetic scale)
- topology: docker-compose.prodlike.yml (Caddy TLS → API → PG16), fresh volume
- duration: 120 s per level, 20 s cooldown, resource sampling every 5 s
- code: final Phase 4D.2 commit (rotation + HSTS fix included)
- license: NEW-key ENTERPRISE license active in DB
- date: 2026-08-25

## Results (expected_response:true)

| VU | Reqs | RPS | p50 ms | p95 ms | p99 ms | max ms | errors | checks |
|---|---|---|---|---|---|---|---|---|
| 10 | 917 | 7.49 | 6.3 | 26.5 | 92.0 | 410 | 0 | 100% |
| 25 | 2,322 | 18.99 | 5.9 | 20.7 | 48.5 | 139 | 0 | 100% |
| 50 | 4,692 | 38.34 | 5.9 | 15.5 | 28.1 | 144 | 0 | 100% |
| 100 | 9,164 | 74.57 | 6.6 | 17.2 | 52.4 | 475 | 0 | 100% |

`laohr_failed_requests rate = 0` at every level.

## Resources (steady-state peak; first two sampler ticks excluded as API
restart warmup — noted honestly)

| VU | API CPU | API RAM | PG CPU | PG connections |
|---|---|---|---|---|
| 10 | 6.5% | 215 MiB | 3.8% | 8 |
| 25 | 12.1% | 222.6 MiB | 6.7% | 15 |
| 50 | 19.8% | 179.8 MiB | 10.4% | 12 |
| 100 | 43.3% | 206.9 MiB | 22.7% (128.8% single-tick spike) | 35 |

## Comparison vs 4D.1 post-fix runs (50/100)

| Level | RPS then → now | p95 then → now | p99 then → now |
|---|---|---|---|
| 50 | 37.24 → 38.34 | 30.2 → 15.5 | 221 → 28.1 |
| 100 | 74.24 → 74.57 | 32.3 → 17.2 | 99.4 → 52.4 |

Same throughput, better tails on this host/run.

## Decision

Results are healthy and linear; no unexplained failures; connection headroom
(35 of pool max under 100 VU). Per phase rule §26: **no tuning applied**.

Raw evidence: `evidence/load/*.json`, `*.log`, `resources-*.txt`.
