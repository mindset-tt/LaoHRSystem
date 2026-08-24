# 17 — ALERTING

## Strategy

Lightweight, manual-first. No PagerDuty/SaaS required.

## Alert conditions (operator-monitored)

- `/health/ready` failure (Postgres down).
- 5xx spike (from request logs).
- Disk usage high (Postgres + documents + logs + backups share the host).
- Backup failure (backup script exit code).
- Audit writer failure (Serilog error log).
- Document storage unavailable.

## Delivery

Local/admin notification mechanism (log-based + manual monitoring). Automated
delivery is not implemented.

## Status

PARTIAL — conditions defined; automated delivery not implemented (manual
monitoring procedure documented in runbook).
