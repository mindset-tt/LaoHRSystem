# 23 — CORPORATE DASHBOARD

## Design

A `/corporate` page aggregates counts across facilities, vehicles, travel
requests, and work orders. The existing `/backoffice` command center remains the
primary attention surface; corporate cards can be added there.

## Frontend

`frontend/src/app/(dashboard)/corporate/page.tsx` — lightweight KPI cards.

## Status

PASS (aggregator page; no per-entity dashboard bloat).
