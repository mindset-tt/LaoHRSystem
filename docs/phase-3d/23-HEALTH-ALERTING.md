# 23 — Health & Alerting

## Health checks
- Docker compose healthchecks exist for postgres, api, web.
- API health endpoint: verify `/health` or `/healthz` exists (ASP.NET Core health checks).

## Alerting
- No alerting rules configured (no Prometheus Alertmanager / Grafana).

## Follow-up
- Add ASP.NET Core health checks (DB, disk, license).
- Wire health checks to orchestrator (Docker/K8s) for auto-restart.
- Define SLOs (availability, latency) and alert thresholds.
- Alert on: DB down, high error rate, high latency, disk full, backup failure.
