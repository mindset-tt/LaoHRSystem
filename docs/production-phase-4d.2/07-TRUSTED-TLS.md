# 07 — TRUSTED PRODUCTION TLS

## Status: PARTIAL — deployment-appropriate certificate NOT yet provisionable

Operator decision (2026-08-25): neither a public DNS name (internet-facing)
nor an organization internal CA is available to this phase. This machine is
localhost-only; no external trust anchor can be honestly obtained here.

What remains proven from 4D.1 and re-checked this phase on the final code:
TLS 1.3 termination at Caddy, HTTP→HTTPS 308, HSTS/headers pipeline,
certificate replacement mechanics (`local_certs` / `tls internal`).

Certificate type for the record: **LOCAL_ONLY** (evidence harness).

## Path A — public site: ACME / Let's Encrypt (Caddy native)

1. DNS `A/AAAA` record for e.g. `hr.example.la` → server public IP.
2. Ports 80+443 reachable from the internet.
3. `deploy/prodlike/Caddyfile` (production copy):
   ```
   {
       email ops@example.la
   }
   hr.example.la {
       encode gzip
       header Strict-Transport-Security "max-age=31536000; includeSubDomains"
       @backend path /api/* /health/*
       handle @backend { reverse_proxy api:8080 }
       handle { reverse_proxy web:3000 }
   }
   ```
   Remove `local_certs`, `skip_install_trust`, `tls internal`.
4. Caddy obtains/renews automatically (HTTP-01). Verify:
   chain trusted by an external client, hostname match, TLS 1.2+ only,
   HTTP→HTTPS redirect, renewal dry-run.

## Path B — internal-only install: organization CA

1. Issue server cert for the internal FQDN (SAN = FQDN) from the org CA.
2. Mount cert/key into Caddy:
   ```yaml
   caddy:
     volumes:
       - ./certs/hr.internal.crt:/certs/tls.crt:ro
       - ./certs/hr.internal.key:/certs/tls.key:ro
   ```
3. Caddyfile site block:
   ```
   hr.internal {
       tls /certs/tls.crt /certs/tls.key
       ...same routes...
   }
   ```
4. Distribute the org root CA to clients via GPO/MDM (standard trust store).
5. **The TLS private key must never enter Git or images** — it lives in the
   operator cert store and enters the container as a mounted secret only.

## Acceptance tests (either path)

| # | Test | Method |
|---|---|---|
| 1 | Hostname matches certificate SAN | browser + `openssl s_client` |
| 2 | Chain trusted by target clients (no override) | external/internal client |
| 3 | Not expired, auto-renewal configured | cert dates + logs |
| 4 | Modern TLS only (1.2/1.3) | SSL Labs / testssl.sh |
| 5 | HTTPS reachable end-to-end | smoke suite |
| 6 | HTTP→HTTPS 308 | curl |

Until one path executes with real trust, PRODUCTION_DEPLOYMENT stays PARTIAL
(TLS component).
