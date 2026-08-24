# 01 — Production Gap Matrix

| Area | Prior status | Phase 3D finding | Evidence |
|---|---|---|---|
| Real PostgreSQL | NOT RUN | **PASS** (Docker PG16) | fresh + upgrade migration applied |
| Fresh migration | NOT RUN | **PASS** | 6 migrations, 84 tables |
| Upgrade migration | NOT RUN | **PASS** | partial→full migrate |
| Backup | NOT RUN | **PASS** | pg_dump custom format |
| Restore | NOT RUN | **PASS** | clean drop→create→restore, 84 tables |
| TLS | NOT RUN | NOT RUN | no reverse proxy configured |
| Secrets | PARTIAL | PARTIAL | demo creds guarded; design-time factory has hardcoded `Password=laohr` |
| Dependency vulns | — | backend NONE; frontend 3 HIGH | npm audit |
| Lint | 41 errors | 41 errors (debt register) | eslint |
| Payroll compliance | NO | NO (6 blockers) | professional confirmation pack |

## Key finding — design-time factory bug (FIXED)
`LaoHRDbContextFactory` did NOT set `Npgsql.EnableLegacyTimestampBehavior`, so `dotnet ef database update` failed on seed data with "Unspecified DateTime". Fixed by adding the switch (matches `Program.cs`). This is why real migration was previously impossible even with a database.
