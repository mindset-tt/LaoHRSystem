# 01 — PG16 CONCURRENCY HARNESS

Real PostgreSQL 16 concurrency evidence, distinct from the InMemory unit suite.

## Harness

`scripts/validate-finance-postgres.ps1`:

1. Starts a disposable `postgres:16-alpine` container on `127.0.0.1:5434`.
2. Applies the full migration chain (`NPGSQL_LEGACY_TIMESTAMP=1`).
3. Sets `LAOHR_TEST_CONNECTION` and runs `Pg16ConcurrencyTests`.
4. Destroys the container.

## Test project

`Backend/LaoHR.Tests/Integration/Pg16/Pg16ConcurrencyTests.cs`:

- `CreateContext()` builds a `LaoHRDbContext` with `UseNpgsql(ConnectionString!)`.
- Tests `return` early (skip) when `LAOHR_TEST_CONNECTION` is unset, so the InMemory suite is unaffected.
- Seed dates use `DateTimeKind.Utc` (Npgsql rejects Unspecified for `timestamptz`).
- Config seeding is idempotent via `UpsertSettingAsync`.

## Result

4 / 4 PASS (see 02–05 for per-race detail).
