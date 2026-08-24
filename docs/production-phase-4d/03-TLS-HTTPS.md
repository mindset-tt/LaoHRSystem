# 03 — TLS / HTTPS

## Architecture

TLS terminates at a reverse proxy (Caddy/Nginx/Traefik — operator's choice, one
proxy only). The API and frontend run on plain HTTP behind it on the internal
network.

## Changes

- `Program.cs`: `UseHttpsRedirection()` in non-Development (harmless behind a
  proxy; correct when the API is directly reachable).
- `Program.cs`: `UseForwardedHeaders()` trusts a single `ForwardedHeaders:KnownProxy`
  IP (X-Forwarded-For + X-Forwarded-Proto). Not blindly trusting arbitrary headers.
- `SecurityHeadersMiddleware`: HSTS emitted only over HTTPS (or behind a trusted
  proxy reporting `X-Forwarded-Proto: https`), and only in non-Development.

## Operator prerequisites

- Provision a TLS certificate (Let's Encrypt or org CA) at the reverse proxy.
- Redirect HTTP (80) → HTTPS (443).
- Set `ForwardedHeaders:KnownProxy` to the proxy's internal IP.

## Status

PASS (code-level); certificate provisioning is an operator step.
