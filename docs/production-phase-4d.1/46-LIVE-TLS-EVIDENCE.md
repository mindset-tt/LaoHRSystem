# 46 — LIVE TLS / HTTPS EVIDENCE

Phase 4D claimed TLS PASS at code level ("certificate provisioning is an
operator step"). Phase 4D.1 proves the deployed topology under real HTTP.

## Topology exercised

Production-like stack (`docker-compose.prodlike.yml`):

| Service | Image | Exposure |
|---|---|---|
| caddy | caddy:2-alpine | **80, 443 published** — the ONLY published ports |
| api | prodlike4d1-api (.NET 10) | internal only (8080/tcp) |
| web | prodlike4d1-web (Next.js 16 standalone) | internal only (3000/tcp) |
| postgres | postgres:16-alpine | internal only |

Caddy terminates TLS and proxies `path /api/* /health/*` → api:8080,
everything else → web:3000. `ForwardedHeaders__KnownProxy=172.31.0.10`
(static compose subnet IP of Caddy).

## Certificate distinction (IMPORTANT)

The test uses `local_certs` in Caddy — a **locally generated CA**, NOT a
public CA (Let's Encrypt). This validates TLS termination, redirect, HSTS
and header behaviour end-to-end but does NOT validate public trust chains.
A production deployment must provision its own certificate.

## Captured evidence (2026-08-24/25)

### HTTP → HTTPS redirect

```
$ curl -s -o NUL -w "%{http_code} %{redirect_url}" http://localhost/
308 https://localhost/
```

### TLS handshake details

```
NegotiatedProtocol=Tls13
CipherAlgorithm=Aes128 strength=128
Issuer=CN=Caddy Local Authority - ECC Intermediate   <-- LOCAL TEST CA
SAN=DNS Name=localhost
NotBefore=2026-08-24T17:52:28Z  NotAfter=2026-08-25T05:52:28Z
```

### HTTPS responses

```
GET https://localhost/          -> 200 OK
GET https://localhost/health/live  -> 200 "Healthy"
GET https://localhost/health/ready -> 200 "Healthy"
```

## Findings fixed during live validation

1. `auto_https off` in the first Caddyfile draft broke cert provisioning
   (TLS alert internal error on handshake). Fixed by using the canonical
   single-site block; Caddy then serves :443 + auto-redirect on :80.
2. HSTS was missing on browser-facing responses (Caddy does not add it by
   default). Added `header Strict-Transport-Security` at the TLS layer:

```
Strict-Transport-Security: max-age=31536000; includeSubDomains
```

## Status

HTTP→HTTPS redirect: **PASS** (308)
HTTPS serving: **PASS** (TLS 1.3)
Public-CA validation: **NOT RUN** (local test CA used — documented above)
