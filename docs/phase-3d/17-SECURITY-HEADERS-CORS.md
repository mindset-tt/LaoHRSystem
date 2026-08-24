# 17 — Security Headers & CORS

## CORS
- `Cors__AllowedOrigins` allow-list (empty = no origins, safe default).
- No wildcard `*` in production.

## Security headers
- No explicit security-header middleware found (X-Frame-Options, CSP, X-Content-Type-Options, Referrer-Policy, Permissions-Policy).
- HSTS not set (no TLS yet).

## Follow-up
- Add security-header middleware or reverse-proxy headers:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Content-Security-Policy`
  - `Referrer-Policy: strict-origin-when-cross-origin`
  - `Permissions-Policy`
- HSTS once TLS is live.
