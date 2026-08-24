# 11 — Production Seed Strategy

## Current state
`DbSeeder.Seed()` runs only in `IsDevelopment || IsTesting` (guarded in `Program.cs`). Demo users (admin/admin123, hradmin/hr123, employee/emp123) are Dev/Testing only.

## Production
- No predictable accounts seeded in production.
- `appsettings.json` has empty `ConnectionStrings__DefaultConnection` and `Jwt__Key` (placeholders, with rotation notes).
- `docker-compose.yml` requires `POSTGRES_PASSWORD` and `JWT_KEY` env vars (fail-fast via `${VAR:?...}`).

## Admin bootstrap
Production-safe first-admin creation is NOT yet defined (no explicit bootstrap command). Documented as a required operational step.

## Config fail-fast
`Jwt__Key` fail-fast exists (Program.cs). Connection string empty in production would fail at startup.

## Follow-up
- Define production admin bootstrap (one-time env-provided or manual command).
- Remove hardcoded `Password=laohr` from `LaoHRDbContextFactory` (design-time only, but should use env).
