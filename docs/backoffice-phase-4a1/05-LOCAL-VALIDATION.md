# 05 — Local Validation

## Scripts (no hosted runner)
- `scripts/validate.ps1` — runs `dotnet restore/build/test`, then `npx tsc --noEmit`,
  `npm test`, `npm run build`, `npm run lint`. Reports exit codes; does not swallow failures.
- `scripts/validate-postgres.ps1` — applies migrations to disposable PostgreSQL 16
  (sets `NPGSQL_LEGACY_TIMESTAMP=1` for `database update`).
- `scripts/backup.ps1` — pg_dump custom format; requires `PGPASSWORD` env (no embedded secret).

## Release process
Developer runs `scripts/validate.ps1` then `scripts/validate-postgres.ps1`
before a manual release. No automation server required.
