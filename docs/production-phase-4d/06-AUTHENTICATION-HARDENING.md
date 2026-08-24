# 06 — AUTHENTICATION HARDENING

## Password storage

PBKDF2-HMAC-SHA256, 600,000 iterations, 16-byte salt, 32-byte hash, constant-time
comparison. Legacy SHA-256 (v1) migrated on login. No plaintext, no reversible
encryption.

## JWT

- HS256, 8-hour expiry, issuer/audience validated.
- Production fail-fast: key required, ≥64 chars (startup validation).
- `AuthController.GenerateJwtToken` no longer silently falls back to a known key
  in Production (throws instead).

## Refresh tokens

- 256-bit random, SHA-256 hash stored (raw never persisted).
- Single-use rotation + replay detection (revokes family).
- 14-day lifetime; revoke/logout-all supported.

## Rate limiting

- `auth-login`: 5/60s per IP.
- `auth-refresh`: 30/60s per IP (new in 4D).

## Account enumeration

Login returns a generic "Invalid username or password" for both unknown user and
wrong password.

## Status

PASS.
