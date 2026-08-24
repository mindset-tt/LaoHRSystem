# 04 — SECURITY HEADERS

## Implementation

`SecurityHeadersMiddleware` (Phase 4D) adds:

- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy: camera=(), microphone=(), geolocation=()`
- `Strict-Transport-Security: max-age=31536000; includeSubDomains` (HTTPS + non-Dev only)

## CSP

CSP is NOT set on the API (JSON responses, not HTML). The frontend (Next.js) owns
its CSP. No `unsafe-eval`/`unsafe-inline` is introduced by the API.

## Tests

`SecurityHeadersTests` — asserts `X-Content-Type-Options`, `X-Frame-Options`,
`Referrer-Policy` present; unauthenticated protected API returns 401.

## Status

PASS.
