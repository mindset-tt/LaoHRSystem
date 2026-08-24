# 00 — Production Baseline

Recorded at Phase 3D start (2026-08-21).

## Environment discovery (major change)
- **Docker daemon is NOW RUNNING** (Server Version 29.7.2, Docker Desktop). Prior phases reported it unavailable.
- **Native PostgreSQL 18.6** is installed and running on host port 5432 (service `postgresql-x64-18`).
- **Docker PostgreSQL 16** was started for validation (port 5434 to avoid conflict).

## Baseline commands
- `git branch --show-current`: `master`
- Backend build: PASS (0 warnings / 0 errors)
- Backend tests: 148 PASS / 0 FAIL
- Frontend typecheck: PASS
- Frontend tests: 33 PASS / 0 FAIL
- Frontend build: PASS
- Frontend lint: 41 errors / 38 warnings (pre-existing)
- `dotnet --info`: .NET 10.0.11 runtime (SDK 10.0.400)
- `node --version`: v24; `npm --version`: 11
- `docker version`: 29.7.2; `docker compose version`: v5.4.0
- `psql --version`: PostgreSQL 18.6 (native host)

## Migration chain (verified)
`InitialCreatePostgres` → `AddApprovalEssMssNotifications` → `AddPmPlanningAndDependencies` → `AddPmPlanningDependenciesV2` (no-op) → `AddRecruitmentAndOnboarding` → `AddPerformanceTalentLearning`

## Change freeze
Major schema/product development frozen. Allowed: compliance/security/production/migration/deployment/observability/reliability/performance/accessibility/dependency-security/test/operational fixes.
