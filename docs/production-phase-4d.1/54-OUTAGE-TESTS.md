# 54 — OUTAGE TESTS EVIDENCE

Executed against the production-like stack (disposable environment).

## 1. Database outage (spec §64)

```
$ docker stop laohr-prodlike-postgres

GET /health/live   -> 200   (process alive; liveness intentionally DB-free)
GET /health/ready  -> "Unhealthy"
POST /api/auth/login -> 500 ProblemDetails:
     {"title":"An unexpected error occurred",
      "detail":"An internal server error occurred. Please contact support
                if the problem persists.",
      "traceId":"0HNO229J44L3A:00000004"}
```

- Liveness behaviour understood: stays up so orchestrators don't restart a
  process that will recover on its own.
- Readiness = unhealthy: load balancers stop routing. ✓
- API failure is SANITIZED: RFC 7807 envelope, no stack trace, no connection
  string, no host/path leak. traceId present for log correlation. ✓

Restart:

```
$ docker start laohr-prodlike-postgres   (wait ~12 s)
GET /health/ready -> 200
login            -> OK
GET /api/employees -> 200
```

Readiness recovers automatically without application restart. **PASS**

## 2. Document storage outage (spec §65)

Made the storage tree read-only inside the running container
(`chmod -R a-w /data/documents`, verified with `touch` → Permission denied):

```
POST /api/documents (upload) -> 500 sanitized ProblemDetails
                                (no filesystem path leaked)
GET /api/employees           -> 200   (rest of API unaffected)
GET /api/documents/employee/3-> 200   (reads of already-stored files fine)
```

- Upload fails safely; other API surfaces remain functional. ✓
- No path leak in responses. ✓
- Readiness policy (documented): readiness tracks DATABASE reachability only;
  document-storage unavailability manifests as failed uploads/downloads and
  must be detected by `scripts/ops-checks.ps1` check #4 (storage write probe),
  which exits CRITICAL. This split is deliberate and documented in the
  production runbook.

Permissions restored after the test. **PASS**

## 3. Configuration fail-fast (spec §68–69)

Each started in Production with exactly one missing/bad value:

| Scenario | Result |
|---|---|
| `Jwt__Key` missing | FATAL: "JWT signing key (Jwt:Key) is missing... Production/Staging MUST NOT use the hardcoded fallback key." |
| `Jwt__Key` = dev fallback key (47 chars) | FATAL: "must be at least 64 characters in Production/Staging." — dev key cannot boot production |
| Connection string missing | FATAL: "ConnectionStrings:DefaultConnection is missing..." |
| CORS origins missing | FATAL: "Cors:AllowedOrigins is missing..." |
| Compose without POSTGRES_PASSWORD | compose interpolation error before any container starts ("POSTGRES_PASSWORD must be set") |

No secret VALUES printed in any failure output (messages name the missing
variable, never its content). Default credential posture: seeded users exist
only via explicit seed SQL (Development-only seeding is disabled in
Production); laohr/laohr cannot authenticate because no such user exists and
passwords are PBKDF2-hashed. **PASS**

## 4. Log rotation (spec §63)

Configuration asserted in code (`Serilog` file sink):
`RollingInterval.Day`, `retainedFileCountLimit: 14`.
Live evidence after fix: `/app/Logs/laohr-20260824.json` created and written
by the running container (the directory had to be pre-created for the
non-root user — fixed in this phase). Daily rotation produces one file per
UTC day; retention caps at 14 files. **PASS**
