# 33 — FRONTEND

## Scope

Phase 4C added the corporate API client (`corporate.ts`) and a `/corporate`
command-center page. Full per-domain UI (documents, facilities, fleet, travel,
visitors detail pages) is deferred to a later phase; the backend APIs are ready.

## Validation

| Check | Result |
|---|---|
| `npm test` | 48 passed / 0 failed |
| `npx tsc --noEmit` | PASS |
| `npm run build` | PASS ("Compiled successfully") |
| `npm run lint` | 41 errors / 38 warnings (all pre-existing); **0 new** in corporate files |

## Status

PASS (no regressions; 0 new lint errors).
