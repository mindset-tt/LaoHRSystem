# 02 — PostgreSQL 16 Canonicalization

## Canonical target
`SUPPORTED_POSTGRES_VERSION = 16` (per `docker-compose.yml` `postgres:16-alpine`
and the Phase 3D production target). PostgreSQL 18.6 is present on the host but
is NOT the production target — 18 succeeding does not prove 16 compatibility.

## Validation (this phase)
Disposable `postgres:16-alpine` container on `127.0.0.1:5434` (non-conflicting port).

| Test | Result |
|---|---|
| Fresh migration (full chain) | PASS — 103 tables |
| Upgrade migration (pre-BO → BO) | PASS — 84 → 103 tables, data preserved |
| Backup (pg_dump custom) | PASS |
| Restore (drop → create → pg_restore) | PASS — 103 tables |
| Back Office tables present | PASS (19 tables) |
| Seed data (ServiceRequestCategories) | PASS (6 rows) |

## Rule
Do not silently upgrade the production target to 18. All future schema batches
must be validated against PostgreSQL 16.
