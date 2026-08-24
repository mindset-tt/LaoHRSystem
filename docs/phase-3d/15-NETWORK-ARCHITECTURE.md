# 15 — Network Architecture

## Target topology
```
Internet → TLS reverse proxy (443) → API (internal) → PostgreSQL (internal)
                                   → Web (static/SSR)
```

## Current state
- No reverse proxy configured in repo (no nginx/traefik/caddy config).
- API listens on HTTP only (no TLS termination in-app).
- CORS allow-list via `Cors__AllowedOrigins`.

## Requirements
- TLS termination at reverse proxy (not in-app).
- API and DB on private network (not exposed publicly).
- Web served over HTTPS only.

## Follow-up
- Provide nginx/caddy reverse-proxy config with TLS.
- Restrict DB to internal network (no public port).
