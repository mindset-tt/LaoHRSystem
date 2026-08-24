# 00 — Phase 3B Baseline

> Established 2026-08-21 before Phase 3B changes.

## Starting state (from Phase 3A)

| Metric | Value |
|---|---|
| Backend build | PASS (0 errors, 5 warnings) |
| Test project build | PASS (0 errors, 12 warnings) |
| Backend tests | 47 passed, 21 failed |
| Frontend typecheck | PASS |
| Frontend build | NOT RUN |
| Frontend lint | NOT RUN |
| Database migrations | NOT COMPLETE (EnsureCreated fallback) |
| Backup/DR | NOT COMPLETE |
| TLS/reverse proxy | NOT COMPLETE |
| Versioned rule engine | NOT COMPLETE |
| Frontend tests | NOT CONFIGURED |

## Root cause of 21 test failures

`System.InvalidOperationException: The logger is already frozen` from `Serilog.Extensions.Hosting.ReloadableLogger.Freeze()`. The `CreateBootstrapLogger()` created a `ReloadableLogger` that conflicted with `UseSerilog`'s own `ReloadableLogger` when `WebApplicationFactory` re-ran the entry point.

## Environment constraints discovered

- Docker daemon: NOT RUNNING (cannot test live migrations/backup/restore)
- PostgreSQL (10.233.141.2:5433): NOT REACHABLE
- `dotnet-ef` tool: NOT INSTALLED (installed during Phase 3B)
- `git` CLI: NOT on PATH (history unavailable)

## Git state

- Branch: `master`
- Substantial uncommitted work (Phase 2-3A changes + new controllers/Docker/CI/docs)