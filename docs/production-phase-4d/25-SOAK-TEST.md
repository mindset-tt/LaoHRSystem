# 25 — SOAK TEST

## Approach

A 30–60 min local soak (not an arbitrary 24h requirement). Watch memory growth,
handles, connections, error rate.

## Async audit channel

Bounded channel (10,000, `DropOldest`) — high load cannot cause unlimited RAM
growth. Background jobs (leave scheduler, retention) have exception handling +
cancellation.

## Status

NOT RUN in this phase. Marked honestly; soak is a follow-up.
