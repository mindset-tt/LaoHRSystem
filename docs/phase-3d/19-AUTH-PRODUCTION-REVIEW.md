# 19 — Auth Production Review

## Auth stack
- JWT bearer (HS256, `Jwt__Key` 64+ chars).
- Refresh tokens (rotating, 30-day retention).
- Password hashing (ASP.NET Identity / PBKDF2).
- Rate limiting on login (5/60s in production).
- License verification middleware.

## Findings
- Demo credentials (admin/admin123, hradmin/hr123, employee/emp123) are Dev/Testing only — NOT in production.
- `Jwt__Key` fail-fast exists.
- Refresh token rotation + retention configured.

## Follow-up
- Enforce password policy (min length, complexity) — verify.
- Account lockout policy — verify.
- MFA for admin/HR roles — recommended before production.
- Session/refresh token revocation on password change — verify.
