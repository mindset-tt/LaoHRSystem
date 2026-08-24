# 33 — Dependency Security

## Backend (.NET)
- `dotnet list package --vulnerable`: **NONE** vulnerable.

## Frontend (npm)
- `npm audit`: **3 HIGH** vulnerabilities:
  1. Next.js 16.1.1 — SSRF / DoS / Server-Function (HIGH).
  2. postcss — (HIGH).
  3. sharp — (HIGH).

## Action
- Upgrade Next.js to patched version (16.1.x+ or latest).
- Upgrade postcss and sharp to patched versions.
- Re-run `npm audit` until 0 HIGH/CRITICAL.

## Follow-up
- Add `npm audit` + `dotnet list package --vulnerable` to CI (fail on HIGH/CRITICAL).
- Enable Dependabot/Renovate for automated updates.
