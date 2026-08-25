# 08 — FINAL LIVE SECURITY HEADERS

Measured 2026-08-25 on the final-code prodlike stack (Caddy TLS edge → API/web),
real HTTPS responses:

## Frontend `https://localhost/`

```
HTTP/1.1 200 OK
Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-inline';
    style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:;
    font-src 'self' data:; connect-src 'self' https://localhost;
    object-src 'none'; base-uri 'self'; form-action 'self';
    frame-ancestors 'none'
Permissions-Policy: camera=(), microphone=(), geolocation=()
Referrer-Policy: strict-origin-when-cross-origin
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
```

## API via proxy `https://localhost/api/employees` (401 expected unauthenticated)

```
HTTP/1.1 401 Unauthorized
Permissions-Policy: camera=(), microphone=(), geolocation=()
Referrer-Policy: strict-origin-when-cross-origin
Strict-Transport-Security: max-age=31536000; includeSubDomains   ← exactly ONE
X-Content-Type-Options: nosniff
```

## HSTS duplication — CLOSED (4D.1 P2 #5)

Root cause was dual emission (Caddy edge + API `SecurityHeadersMiddleware`).

Fix this phase, minimal and low-risk:
- `SecurityHeadersMiddleware` now reads `SecurityHeaders:EmitHsts`
  (`IConfiguration`, default **true** so direct-HTTPS deployments keep HSTS).
- Prodlike/proxied topology sets `SecurityHeaders__EmitHsts=false` in
  `docker-compose.prodlike.yml` — canonical ownership = the TLS edge.
- Retest shows a single HSTS header on proxied API responses (above), while
  edge-emitted HSTS remains on frontend responses.

## HTTP→HTTPS

```
GET http://localhost/  →  308 Permanent Redirect → https://localhost/
```

All required headers verified live on final code: PASS.
