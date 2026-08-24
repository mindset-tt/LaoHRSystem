# 39 — Smoke Tests

## Status: NOT RUN (production smoke tests)

## Smoke test list
- Health endpoint returns 200.
- Login (valid creds) succeeds.
- Login (invalid creds) fails + rate-limited.
- Employee list returns data.
- Payroll run (dry) succeeds.
- Dashboard loads.
- Document upload/download works.
- DB connectivity (migrations applied).

## Follow-up
- Automate smoke tests (script / Postman / Playwright).
- Run after every deploy.
- Record results.
