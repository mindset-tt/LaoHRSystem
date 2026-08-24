# 01 — PRODUCTION ARCHITECTURE

## Target shape (single host, lightweight)

```
CLIENT (browser)
  ↓ HTTPS (443)
REVERSE PROXY (TLS termination — Caddy/Nginx/Traefik, operator's choice)
  ↓ internal HTTP
NEXT.JS frontend (standalone, port 3000)
  ↓ internal HTTP
ASP.NET CORE API (port 8080)
  ↓ internal TCP
POSTGRESQL 16 (internal compose network only)
```

## Principles

- No Kubernetes, no service mesh, no Kafka, no Redis, no microservices.
- Only port 443 (and 80 for redirect) exposed publicly.
- Postgres and API ports are NOT published to the host (internal compose network).
- TLS terminates at the reverse proxy; the API trusts a single known proxy via
  `ForwardedHeaders:KnownProxy`.
- Non-root container users; pinned `postgres:16-alpine`.

## Changes made in 4D

- `docker-compose.yml`: Postgres `5432` and API `8080` ports commented out
  (internal-only); `CORS_ALLOWED_ORIGINS` made required (`:?`).
- `frontend/Dockerfile`: set `BUILD_STANDALONE=1` (fixes latent standalone bug).
- `Program.cs`: forwarded-headers (known proxy), HTTPS redirection, security
  headers middleware, production startup config validation.

## Status

PASS.
