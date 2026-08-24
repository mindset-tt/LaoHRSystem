# 12 — Secret Inventory

| Secret | Purpose | Source | Required at startup | Rotation |
|---|---|---|---|---|
| Jwt__Key | JWT signing | env var | Yes (fail-fast) | Manual |
| ConnectionStrings__DefaultConnection | DB connection | env var | Yes | Manual |
| POSTGRES_PASSWORD | DB password | env var (compose) | Yes | Manual |
| Cors__AllowedOrigins | CORS allow-list | env var | No (defaults empty) | Manual |
| SmtpSettings__Pass | SMTP password | config | No | Manual |
| License key | License verification | SystemSettings | Yes (middleware) | Manual |
| public.key | License RSA verify | file | Yes | Manual |

## Findings
- `appsettings.json` has empty placeholders (good) + rotation notes.
- `LaoHRDbContextFactory` has hardcoded `Password=laohr` (design-time only — should use env).
- `.github/workflows/ci.yml` has `Password=laohr_test` (CI test DB — acceptable, non-production).
- No real production secrets found in repo.

## No secrets in repo
No private keys, real passwords, or tokens committed. Demo credentials are Dev/Testing-only.

## Git history
No evidence of committed real secrets requiring rotation (the prior committed connection string was already removed with a rotation note).
