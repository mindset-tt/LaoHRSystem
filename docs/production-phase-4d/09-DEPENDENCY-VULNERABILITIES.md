# 09 — DEPENDENCY VULNERABILITIES

## Frontend (`npm audit`)

Before: 3 HIGH (all in `next@16.1.1` + transitive `postcss`/`sharp`).
After: upgraded `next` 16.1.1 → 16.3.2 (and `eslint-config-next` to match).

Result: **0 vulnerabilities** (CRITICAL 0, HIGH 0).

Regression: typecheck PASS, 57 tests PASS, build PASS, lint errors unchanged (41),
+1 warning from upgraded `eslint-config-next` (pre-existing `window.location.href`
pattern, not new code).

## Backend (`dotnet list package --vulnerable`)

**No vulnerable packages.** `--outdated` shows OpenTelemetry 1.17.0 → 1.18.0
(non-vulnerable minor bump, left as-is to avoid churn).

## Supply chain

Lockfiles preserved (`package-lock.json`); no floating production dependencies.

## Status

PASS (CRITICAL 0, HIGH 0).
