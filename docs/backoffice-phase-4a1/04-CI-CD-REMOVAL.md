# 04 — CI/CD Removal

## Removed
- `.github/workflows/ci.yml` (GitHub Actions workflow) — deleted.

## Verified absent
- No `gitlab-ci.yml`, `.gitlab-ci.yml`, `azure-pipelines.yml`, `Jenkinsfile`,
  `.circleci/`, or `buildkite` config.

## Preserved
- `.git`, `.gitignore`, Git history, branches, normal GitHub remote (source control only).

## Current architecture
**LOCAL VALIDATION** (not CI/CD):
- `scripts/validate.ps1` — backend restore/build/test + frontend typecheck/test/build/lint.
- `scripts/validate-postgres.ps1` — disposable PostgreSQL 16 migration validation.
- `scripts/backup.ps1` — pg_dump backup (no embedded password).

## Cloud CI cost
`CLOUD_CI_COST = $0 BY DESIGN` (cloud CI intentionally absent; this is not a
claim about total infrastructure cost).
