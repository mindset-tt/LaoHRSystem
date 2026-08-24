# 08 — Fresh PostgreSQL Validation

## Environment
Docker PostgreSQL 16 (postgres:16-alpine), disposable container `laohr-pg16-val`, port 5434 (avoided native PG18 on 5432).

## Result: PASS
- `dotnet ef database update` applied all 6 migrations.
- 84 tables created.
- `__EFMigrationsHistory` contains all 6 migration IDs.

## Blocking issue found + fixed
`LaoHRDbContextFactory` (design-time) lacked `Npgsql.EnableLegacyTimestampBehavior`, causing "Unspecified DateTime" failure on seed data. Fixed.

## Note on host port conflict
Native PostgreSQL 18.6 occupies host 5432. Docker PG16 was bound to 127.0.0.1:5434. The `docker-compose.yml` maps 5432:5432, which would conflict on this host — documented as a deployment consideration.
