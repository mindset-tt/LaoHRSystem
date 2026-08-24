# 02 — THREAT MODEL

Refreshed threat model for the production deployment.

## Assets

- Employee PII, payroll data, financial/accounting records, supplier banking data,
  corporate documents, visitor data, contracts.

## Trust boundaries

1. Internet → reverse proxy (TLS).
2. Reverse proxy → frontend/API (internal network).
3. API → PostgreSQL (internal network).
4. API → local document storage (filesystem).

## Threats and mitigations

| Threat | Mitigation | Status |
|---|---|---|
| Credential stuffing / brute force | `auth-login` rate limit (5/60s) | PASS |
| Refresh-token abuse | `auth-refresh` rate limit (30/60s) + rotation + replay detection | PASS |
| Weak JWT key | fail-fast + ≥64 char validation in Production | PASS |
| XSS via user content | React escaping; no `dangerouslySetInnerHTML` | PASS |
| SQL injection | parameterized `FromSqlRaw`; static seed/index SQL only | PASS |
| Path traversal on upload | sanitized filename + generated storage name | PASS |
| Unauthorized upload (IDOR) | `CanViewEmployeeAsync` check on upload/delete | PASS |
| MIME spoofing | extension allow-list (malware scanning DEFERRED) | PARTIAL |
| Secret leakage in logs/audit | audit redaction + no Authorization header logging | PASS |
| DB exposure | Postgres port not published | PASS |
| Missing security headers | SecurityHeadersMiddleware | PASS |

## Out of scope (documented)

- Malware scanning (DEFERRED_WITH_RISK — see 11).
- SSRF: application does not fetch arbitrary URLs → NOT APPLICABLE.
- CSRF: bearer-token model (no ambient cookie auth) → low risk, documented.

## Status

PASS.
