# 08 — SECRETS / CONFIGURATION

## Committed-secret scan

No real production secrets committed. Findings classified:

- `.env.example` — placeholders only.
- `appsettings.json` — empty `ConnectionStrings`/`Jwt.Key` (env-driven).
- `AuthController`/`Program.cs` — hardcoded dev/test fallback key (guarded; 4D
  removed the unguarded controller fallback).
- `scripts/validate-*.ps1` — `laohr/laohr` local test credentials (low-value).
- `scripts/validate.ps1` — explicitly-labeled test-only JWT key.
- No `BEGIN PRIVATE KEY` blocks committed.

## Production config validation (new in 4D)

Startup fails fast in Production/Staging if:
- `Jwt:Key` missing or < 64 chars.
- `ConnectionStrings:DefaultConnection` missing.
- `Cors:AllowedOrigins` missing.

## Database credentials

`docker-compose.yml` requires `POSTGRES_PASSWORD` via `${VAR:?}` (no default).
Test containers may use `laohr/laohr`; production must not.

## Status

PASS.
