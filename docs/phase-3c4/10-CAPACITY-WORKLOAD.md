# 10 — Capacity / Workload

## Model
`GetResourceWorkloadAsync` aggregates active allocations per employee (single query, no N+1).

## Overallocation
`sum(active AllocationPercent) > 100` → overallocated. Example: 60% + 70% = 130% → overallocated.

## Capacity vs attendance
Planned capacity (planning) is distinct from actual attendance (HR record). Not conflated.

## No legal working-hour rules
Capacity is a configurable planning concept, not statutory payroll calculation.

## Privacy
Workload shows assignment/allocation only; no salary/loan/tax/private leave.
