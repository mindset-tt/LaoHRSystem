# 36 — NETWORK / FIREWALL

## Exposure

Public: 443 (and 80 for redirect). Do NOT expose 5432 (Postgres), 8080 (API), or
OTLP/metrics to untrusted networks.

## Firewall

Host firewall should allow only 80/443 inbound. Postgres/API reachable only on
the internal compose network.

## Status

PASS (compose ports commented out; firewall is an operator prerequisite).
