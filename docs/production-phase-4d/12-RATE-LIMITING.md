# 12 — RATE LIMITING

## Policies

- `auth-login`: 5 requests / 60s per IP (sliding window).
- `auth-refresh`: 30 requests / 60s per IP (new in 4D).

## Other abuse protections

- Server-side page-size caps (`PaginatedQuery.MaxPageSize`) on all registers.
- Export endpoints gated by capability + audited.
- No global arbitrary rate limit (endpoint-specific where it matters).

## Status

PASS (login + refresh protected; page-size caps enforced).
