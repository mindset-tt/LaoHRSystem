# 16 — TLS / Reverse Proxy

## Status: NOT RUN
No TLS certificate has been provisioned or tested. No reverse-proxy config exists in the repo.

## Requirements
- TLS 1.2+ (1.3 preferred).
- HSTS header.
- Certificates via Let's Encrypt (ACME) or org CA.
- Auto-renewal (certbot / caddy).

## Follow-up
- Provision TLS cert for production domain.
- Configure reverse proxy (nginx/caddy) with TLS termination.
- Enable HSTS.
- Document cert renewal (see 44-CERTIFICATE-RUNBOOK).
