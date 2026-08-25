# 12 — NEXT PHASE HANDOFF

## Operator action list (blocking go-live)

| # | Action | Runbook | Blocks |
|---|---|---|---|
| 1 | Provision REAL off-host destination (NAS / second server / encrypted removable disk), run backup + restore-from-off-host drill, record evidence | 05, 06 | OPERATIONS, DEPLOYMENT |
| 2 | Provision deployment-appropriate trusted certificate (public ACME or org CA); never commit the TLS key | 07 | DEPLOYMENT (TLS) |
| 3 | Move `license-signing-private.key` + passphrase from this workstation to permanent secure storage (offline media / org vault); delete local copies; destroy `RETIRED-license-signing-private.key` after acceptance | 02 | security hygiene |
| 4 | If repo stays on GitHub public: file GitHub Support secret-purge request for pre-rewrite objects; else make repo private | 04 | residual exposure |

## Residual P2 items carried from 4D.1 (unchanged)

1. Malware scanning deferred with accepted risk (+1 GiB RAM needed; clamd
   INSTREAM hook documented in 4D.1/52).
2. k6 cleanup DELETE returns 405 (no service-request delete route) — load-metric noise only.
3. my-team page one serial await pair — opportunistic parallelization.
4. Documentation prose mentions PEM marker phrases → future scans should
   whitelist docs paths or refine patterns.

## Environment notes for the next session

- .NET SDK lives at `%LOCALAPPDATA%\Microsoft\dotnet` (prepend to PATH);
  k6 at `%LOCALAPPDATA%\k6\k6-v1.4.0-windows-amd64\k6.exe` (`$env:K6_EXE`).
- Prodlike stack: `docker-compose.prodlike.yml`; secrets via env vars only;
  DB volume is disposable; seed via `scripts/seed-load-test.sql` with
  `@LICENSE@` + `@HASH_*@` substitution (license = operator new-key license).
- LicenseGen requires `LICENSEGEN_PRIVATE_KEY` (+ `LICENSEGEN_KEY_PASSPHRASE`
  for the encrypted key). It can no longer mint keys itself by design.

## What a Phase 4D.3 / go-live verification should re-check

1. Off-host backup + restore evidence tables filled (05/06).
2. Trusted-certificate acceptance table filled (07) + headers retest on final host.
3. Secret scan rerun on the production host's artifacts (not just dev machine).
4. Backend ×5 + frontend suite re-run after any env change (expected unchanged).
