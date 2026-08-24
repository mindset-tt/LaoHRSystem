# 14 — Docker Production

## Compose review
`docker-compose.yml` defines postgres (16-alpine), api (.NET 10), web (Next.js standalone). Health checks gate `depends_on`. Secrets via env placeholders (fail-fast).

## Security posture
- API Dockerfile: non-root `appuser`, multi-stage, `ASPNETCORE_URLS` pre-set.
- Web Dockerfile: non-root `app`, standalone output.
- Postgres: named volume, healthcheck.

## Findings
- **Port conflict**: compose maps `5432:5432`, but this host has native PostgreSQL 18.6 on 5432. Deployment must remap or stop native PG.
- No resource limits (CPU/memory) on services.
- No read-only filesystem (API writes uploads — needs writable volume).
- No pinned image digests (uses tags `16-alpine`, `10.0`, `20-alpine`).

## Follow-up
- Add resource limits.
- Pin image digests for reproducible builds.
- Document upload volume mount (uploads must persist outside container).
