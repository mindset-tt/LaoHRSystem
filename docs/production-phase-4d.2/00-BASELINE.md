# 00 — BASELINE (Phase 4D.2)

Date: 2026-08-25
Branch: `dev-v4` @ `17b9556` (start of phase)

## Starting status (honest, carried from 4D.1)

- PRODUCTION_INFRASTRUCTURE_READY: YES
- PRODUCTION_SECURITY_READY: PARTIAL — P1-SEC-001
- PRODUCTION_OPERATIONS_READY: PARTIAL — P1-DR-001
- PRODUCTION_DEPLOYMENT_READY: PARTIAL

Open P1s at phase start:

| ID | Finding |
|---|---|
| P1-SEC-001 | License signing **private key committed since the initial commit** (`9dfea53`) and **pushed to GitHub** (`origin/dev-v4`, `origin/master`). Treated as signing-root compromise. |
| P1-DR-001 | Off-host backup only ever proven to a second local path (`OFFHOST_DR_SIMULATION`). |
| TLS-001 | Production-trusted certificate not provisioned (local CA evidence only). |

## Environment corrections discovered this phase

- The repository path given in the tasking (`C:\Users\khamp\Documents\LaoHRSystem`)
  does not exist on this machine. The real repository is `D:\LaoHRSystem`.
- .NET 10 SDK and k6 were NOT installed on this machine at phase start.
  Both were installed as part of this phase:
  - .NET SDK 10.0.400 → `%LOCALAPPDATA%\Microsoft\dotnet` (user-local, official dotnet-install)
  - k6 v1.4.0 (Windows amd64) → `%LOCALAPPDATA%\k6\k6-v1.4.0-windows-amd64\k6.exe`

## Operator decisions recorded this phase (explicit approval)

1. Install missing tooling (.NET SDK + k6): **APPROVED**.
2. Real off-host backup destination: **NOT AVAILABLE** at this time.
   Per readiness rule §34, OPERATIONS/DEPLOYMENT stay PARTIAL.
3. TLS trust model: **NEITHER public CA nor organization CA available yet**.
   TLS stays PARTIAL; provisioning procedures documented in 07.
4. Git secret-history rewrite: **APPROVED** (destructive) for the leaked key paths,
   with force-push to all affected branches. DB migration history is untouched —
   this is a source-control operation on commit history only; EF migrations files
   are not modified or reordered by the purge.
5. License reissuance strategy: **A — reissue all licenses under the new key**
   (no live customers; single-key verification retained; no dual-key window).

## Scope discipline

No new business features, no AI, no CI/CD, no architecture changes.
Code deltas are limited to: license key handling, one HSTS ownership flag,
and tests that prove rotation behavior.

## End-of-phase status

See `11-GO-LIVE-GATE.md` for the full gate table and final readiness values.
