# 24 — LOAD TEST

## Approach

Local/manual only (no cloud). A lightweight load harness is recommended (k6,
NBomber, or bombardier). No destructive financial writes against a non-disposable
DB.

## Profiles

- Authentication (login).
- Employee list, back-office command center, finance dashboard, corporate dashboard.
- Search/list endpoints, room availability, document metadata list.

## Scale

Practical levels: 10 / 25 / 50 / 100 concurrent users until saturation.

## Targets (INTERNAL_ENGINEERING_TARGET, not contractual)

- p95 < 500ms for list endpoints; error rate < 1% at 50 concurrent.

## Status

NOT RUN in this phase (harness not executed). Marked honestly; a load test is a
follow-up. No fabricated RPS/latency numbers.
