# 18 — API PERFORMANCE

## Review findings

- All large registers use server-side pagination (`PaginatedQuery.MaxPageSize`).
- Read-only queries use `AsNoTracking` + DTO projections (no giant Include graphs).
- Raw SQL is parameterized; row locks (`FOR UPDATE`) are narrow and short-lived.
- Audit is fire-and-forget (bounded channel, `DropOldest`) — does not block requests.

## N+1

No uncontrolled per-row DB calls identified in the audited hot paths (list
endpoints project names via single queries or dictionaries).

## Status

PASS (code-level); load test evidence in 24.
