# 44 — Certificate Runbook

## Certificates
- TLS cert (production domain) — NOT provisioned.
- License RSA public key (license verification).

## TLS renewal
- Let's Encrypt (ACME) auto-renewal via certbot/caddy.
- Monitor expiry (alert 30/14/7 days before).

## License key
- `public.key` used for license verification.
- Rotation process to be documented.

## Follow-up
- Provision TLS cert.
- Configure auto-renewal.
- Add expiry monitoring + alerting.
- Document license key rotation.
