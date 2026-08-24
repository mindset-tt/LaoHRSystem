# 05 — CORS / PROXY

## CORS

- Non-Development: `WithOrigins(corsOrigins)` from `Cors:AllowedOrigins`
  (comma-separated allow-list). Misconfiguration → no origins (fail closed).
- Development: localhost/127.0.0.1 + configured origins.
- `AllowCredentials()` with explicit origins (never `AllowAnyOrigin` + credentials).

## Forwarded headers

- `UseForwardedHeaders` trusts a single `ForwardedHeaders:KnownProxy` IP.
- `X-Forwarded-For` + `X-Forwarded-Proto` only.

## Status

PASS.
