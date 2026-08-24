# 30 — Performance Baseline

## Status: NOT MEASURED
No load test or performance baseline has been run.

## Requirements
- Baseline: request latency (p50/p95/p99), throughput (RPS), error rate.
- Key endpoints: login, employee list, payroll run, dashboard, documents.

## Follow-up
- Run load test (k6/JMeter) against production-like environment.
- Record baseline metrics.
- Identify bottlenecks (DB queries, N+1, missing indexes).
- See 31-LOAD-TEST-RESULTS and 32-POSTGRES-QUERY-REVIEW.
