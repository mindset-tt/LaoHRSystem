# 13 — CI/CD

> `VERIFIED` from `.github/workflows/ci.yml`.

## Pipeline

```mermaid
flowchart LR
    Push[push/PR to main|dev] --> Backend[backend job]
    Push --> Frontend[frontend job]
    Backend --> Docker[docker-smoke job]
    Frontend --> Docker
```

### `backend` job
- Runner: `ubuntu-latest`.
- Service: `postgres:16-alpine` (`laohr_test` DB, healthcheck `pg_isready`).
- Env: `ASPNETCORE_ENVIRONMENT=Testing`, `ConnectionStrings__DefaultConnection` → localhost postgres, `Jwt__Key` (ci-only test key ≥64 chars), `Retention__Enabled=false`.
- Steps: checkout → setup .NET 10 → `dotnet restore LaoHR.API/LaoHR.API.csproj` → `dotnet build -c Release` → `dotnet test LaoHR.Tests -c Release --verbosity minimal`.

### `frontend` job
- Runner: `ubuntu-latest`.
- Steps: checkout → setup Node 20 (npm cache) → `npm ci` → `npx tsc --noEmit` → `node scripts/check-i18n.mjs` (parity) → `BUILD_STANDALONE=1 npm run build` (`NEXT_PUBLIC_API_URL=http://localhost:8080`).

### `docker-smoke` job
- Needs: `[backend, frontend]`.
- Steps: `docker build -t laohr-api:ci -f Backend/LaoHR.API/Dockerfile Backend` → `docker build -t laohr-web:ci frontend`.

## Concurrency

`ci-${{ github.ref }}` group, `cancel-in-progress: true` — cancels superseded runs.

## Gates that exist

- Backend: restore + build + test (xUnit, postgres-backed).
- Frontend: typecheck (`tsc --noEmit`) + i18n parity + standalone build.
- Docker: image build smoke (both).

## Gates that are missing

- No lint gate (backend has no Roslyn analyzer rules; frontend `npm run lint` not in CI).
- No coverage threshold enforcement (coverlet configured but no gate).
- No security scanning (no Trivy/Dependabot/Snyk).
- No deploy step (build + smoke only; no registry push, no environment deploy).
- No E2E tests.
- No integration test job for PM/Finance/Knowledge controllers (tests exist only for HR-core controllers — see `14-TESTING.md`).

## Triggers

`push` to `main`/`dev`; `pull_request` to `main`/`dev`. Note: current branch is `master` — CI may not trigger unless branch mapping is adjusted (`INFERRED`).