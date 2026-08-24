# 00 — BASELINE

Phase 4D (Production Readiness Closure) baseline.

## Verified state entering 4D

| Area | Status |
|---|---|
| Backend tests | 266 PASS / 0 FAIL (5× gate) |
| Frontend tests | 57 PASS / 0 FAIL |
| Typecheck / build | PASS |
| Lint | 41 errors / 38 warnings (historical) |
| PostgreSQL 16 | canonical |
| Finance/Room/Vehicle concurrency | PASS |
| Backup/restore + representative data + app smoke | PASS |
| CI/CD | NONE BY DESIGN |

## Production surface inventory (audited)

- **Deployment**: `docker-compose.yml` (postgres + api + web), multi-stage Dockerfiles (non-root users), `.env.example` with placeholders.
- **Auth**: PBKDF2-HMAC-SHA256 (600k iters, constant-time compare), HS256 JWT (8h), refresh-token rotation + replay detection (14d).
- **Config**: JWT key fail-fast in Production; empty `ConnectionStrings`/`Jwt.Key` in appsettings (env-driven).
- **Observability**: Serilog (console + optional file), OpenTelemetry (ASP.NET/HTTP/EFCore, optional OTLP).
- **Health**: `/health/live` (liveness) + `/health/ready` (Postgres readiness).
- **Rate limiting**: `auth-login` (5/60s) only.

## Gaps identified (to close in 4D)

1. No TLS/HTTPS, no HSTS, no security headers, no forwarded-headers handling.
2. `DocumentsController.UploadDocument` — no size limit, extension-only validation, no IDOR check.
3. Hardcoded JWT fallback key in `AuthController` (no env guard).
4. `POST /api/auth/refresh` un-rate-limited.
5. Frontend `next@16.1.1` — 3 HIGH vulnerabilities (postcss/sharp transitive).
6. `next.config.ts` standalone gated on `BUILD_STANDALONE` but Dockerfile didn't set it.
7. Postgres port 5432 published to host.
8. No restore script (backup only).
