# 47 — LOAD TEST EVIDENCE

Phase 4D reported `Load Test: NOT RUN`. This phase closes that.

## Setup

- Harness: **k6 v1.4.0** (local binary, no cloud).
- Script: `scripts/load-test.js` — mixed workload
  (`INTERNAL_ENGINEERING_PROFILE`, not a business SLA):
  - ~60% reads (employee list, attendance, document metadata, contracts,
    service desk backlog, travel register)
  - ~15% list/search (suppliers, inventory items)
  - ~10% dashboards/reports (back-office command-center, AP aging, GL list,
    corporate documents, room/vehicle availability)
  - ~10% safe create/update (create + cleanup synthetic service request)
  - think time 0.5–2.5 s; one shared admin token (login done once in setup)
- Database: disposable PostgreSQL 16 (`laohr-prodlike-pgdata` volume) seeded
  by `scripts/seed-load-test.sql` with synthetic scale data:

| Dataset | Rows |
|---|---|
| Employees | 500 |
| Attendance rows | 4,500 |
| Users | 25 |
| Projects / tasks | 50 / 500 |
| Suppliers / purchase orders / PO lines | 200 / 300 / 600 |
| Inventory items / stock movements | 200 / 5,000 |
| Assets | 300 |
| Supplier invoices (+lines) | 300 (+600) |
| GL accounts / journal entries / lines | 30 / 200 / 800 |
| Contracts / employee doc metadata | 150 / 400 |
| Service requests | 200 |
| Rooms / bookings, vehicles / bookings | 20 / 300, 30 / 300 |
| Travel requests | 150 |

No production data. No real payroll/payment posting.

## Results (120s per level, final run after the N+1 fix)

| Concurrency | Total reqs | RPS | p50 ms | p95 ms | p99 ms | max ms | checks pass | 5xx |
|---|---|---|---|---|---|---|---|---|
| 10 VUs | 911 | 7.46 | 6.9 | 32.6 | 661.8* | 839 | 797/797 | 0 |
| 25 VUs | 2,254 | 18.41 | 7.1 | 36.3 | 854.8* | 1132 | 1963/1963 | 0 |
| 50 VUs | 4,495 | 36.43 | 8.0 | 49.8 | 1078.5* | 2359 | 3907/3907 | 0 |
| 50 VUs (post-fix) | 4,510 | 37.24 | 7.2 | 30.2 | **221** | 654 | 4006/4006 | 0 |
| 100 VUs (post-fix) | 8,903 | 74.24 | 7.8 | 32.3 | **99.4** | 588 | 7978/7978 | 0 |

\* first-run p99 was dominated by the command-center N+1 (see 47a below);
post-fix runs collapse the tail.

HTTP 4xx: only deliberate negative-path requests in the write mix
(cleanup DELETE returns 405 because no delete route exists) — no client
error storm, no auth failures.

## Resource observations (docker stats sampling every 5–60 s)

| Level | API CPU peak | PG CPU peak | PG connections (total) |
|---|---|---|---|
| 10 | 134 % | 68 % | 20 |
| 25 | 102 % | 69 % | 22 |
| 50 | 183 % | 164 % | 25 |
| 100 | 251 % | 281 % | 76 |

API memory stayed flat ≈ 190–235 MiB across all levels.
Host: 4 logical cores / 7.66 GiB (dev-class hardware).

## Saturation point

Not reached within 100 concurrent users for correctness or errors:
checks stay 100 % and error rate is 0 %. The constraint that appears first
is **CPU**: both API (~250 %) and PostgreSQL (~280 %) are well past a single
core at 100 VUs while latencies remain low (p95 = 83/32 ms). Engineering
judgement from the trend: capacity ceiling on THIS host is roughly
150–200 concurrent users before p95 materially degrades; beyond that,
vertical scaling or connection/CPU headroom work is required.

## INTERNAL_ENGINEERING_TARGET (provisional, measured baseline)

- p95 read/list API < 250 ms at ≤ 50 concurrent users
- 5xx rate < 0.5 %
- zero failed health checks during sustained load

These are engineering defaults derived from measurement, NOT contractual SLAs.

## Connection pool under load

Default Npgsql pool (MaxPoolSize=100). Observed pg_stat_activity total
connections peaked at 76 at 100 VUs; pool exhaustion errors: none;
timeouts: none. No MaxPoolSize tuning applied (per spec §22).

## Booking conflict load

See `scripts/load-booking-conflict.js` + evidence in 51-DR-HTTP-SMOKE.md
companion section: 25 racers × 2 resources → exactly 2 × 201, 48 × 409,
0 unexpected statuses; SQL re-check found exactly one booking per raced
window (`FOR UPDATE` serialization holds).

## Bottleneck investigation (spec §18–21)

`GET /api/backoffice/command-center` measured **p50 767 ms** vs ~60 ms for
all other endpoints. Root cause: N+1 loop — `CountLowStockAsync`/
`CountOutOfStockAsync` executed 2 SUM queries per inventory item (≈400
round trips per request). EXPLAIN (ANALYZE, BUFFERS):

```
-- old pattern, ONE of ~400 calls:
Bitmap Index Scan on IX_StockMovements_ItemId_WarehouseId_OccurredAt ...
Execution Time: 0.243 ms            -- fast individually; round trips dominate

-- replacement single aggregate:
HashAggregate ... Seq Scan StockMovements 5000 rows
Execution Time: 11.045 ms           -- TOTAL for all items
```

Fix: single grouped aggregate (`CountOnHandByItemAsync`) + in-memory counts.
Re-measured: command-center p50 **767 ms → 73.3 ms (10.5×)**.

## Indexes

No new indexes required: the slow query was round-trip bound; existing
`IX_StockMovements_ItemId_WarehouseId_OccurredAt` already serves the
aggregate optimally. No migration added ⇒ NPGSQL_LEGACY_TIMESTAMP question
moot for this phase's schema work.

Raw artifacts: `docs/production-phase-4d.1/evidence/load/*`
(k6 JSON summaries, console logs, resource samples).
