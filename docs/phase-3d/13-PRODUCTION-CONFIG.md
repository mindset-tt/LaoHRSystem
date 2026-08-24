# 13 — Production Config

## Environment boundaries
- Development: `appsettings.Development.json` (minimal).
- Testing: `ASPNETCORE_ENVIRONMENT=Testing` (InMemory DB, rate limiter disabled).
- Production: `ASPNETCORE_ENVIRONMENT=Production` (Npgsql, migrations, rate limiter enabled).

## Critical config (fail-fast)
- `Jwt__Key` (64+ chars) — fail-fast exists.
- `ConnectionStrings__DefaultConnection` — empty in production fails at startup.
- `Cors__AllowedOrigins` — empty defaults to "no origins" (safe).

## Rate limiter
Production: enabled (5 login/60s). Testing: disabled (Phase 3C3). Verified the Testing exception does NOT affect Production.

## Retention
`Retention__Enabled` default false. `Retention__AuditLogDays=365`, `Retention__RefreshTokenDays=30`.

## Follow-up
- `.env.example` with placeholders (no actual secrets).
- Explicit production admin bootstrap.
