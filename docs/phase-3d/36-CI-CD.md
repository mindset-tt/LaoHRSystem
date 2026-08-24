# 36 — CI/CD

## CI (`.github/workflows/ci.yml`)
- Backend: build + test with postgres:16-alpine service.
- Frontend: typecheck + build.

## Findings
- CI exists and runs backend tests + frontend build.
- No `npm audit` / `dotnet list package --vulnerable` gate.
- No deployment (CD) pipeline.

## Follow-up
- Add dependency-vulnerability gate (fail on HIGH/CRITICAL).
- Add `npm test` (Vitest) to CI.
- Add `npm run lint` to CI.
- Add CD pipeline (build image → push registry → deploy).
- Add migration step to CD (or manual gate).
