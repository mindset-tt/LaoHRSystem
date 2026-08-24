# 03 — Migration Strategy + 04 — Migration Review + 05 — Migration Validation

> Phase 3B workstreams 3B.2/3B.3.

## 03 — Migration strategy

### Problem
The repository had 18 SQL Server-flavored migrations (`nvarchar`, `datetime2`, `SqlServer:Identity`) but the runtime uses Npgsql/PostgreSQL. `Program.cs` used `Migrate()` with a fallback to `EnsureCreated()` because the SQL Server migrations were incompatible with PostgreSQL.

### Decision (ADR)
Generate a **fresh PostgreSQL baseline migration** from the current EF model, replacing the SQL Server migrations. Production/development use `Migrate()` only (no `EnsureCreated` fallback). Testing uses InMemory `EnsureCreated()`.

### Implementation
1. Created `LaoHRDbContextFactory : IDesignTimeDbContextFactory<LaoHRDbContext>` (design-time factory using Npgsql + `MigrationsAssembly("LaoHR.API")`).
2. Removed the 18 SQL Server migrations.
3. Generated `InitialCreatePostgres` baseline migration (PostgreSQL types: `uuid`, `character varying`, `timestamp with time zone`, `integer`, `Npgsql:ValueGenerationStrategy`).
4. Updated `Program.cs` to use `Migrate()` only in non-Testing (removed `EnsureCreated` fallback).

### Production startup rule
- Production/Development: `db.Database.Migrate()` — a migration failure surfaces loudly (no silent schema rebuild).
- Testing: `db.Database.EnsureCreated()` (InMemory).

## 04 — Migration review

The generated `InitialCreatePostgres` migration was reviewed for destructive operations:

| Check | Result |
|---|---|
| `DROP TABLE` | None (fresh baseline) |
| `DROP COLUMN` | None |
| `TRUNCATE` | None |
| SQL Server syntax (`nvarchar`, `datetime2`) | None — all PostgreSQL types |
| `PasswordHashVersion` column | Present (default 0 → but see note below) |
| `ComplianceRules` table | Present |
| `PayrollRuleSnapshots` table | Present |

### PasswordHashVersion migration note
The `AppUser.PasswordHashVersion` column defaults to `0` in the migration (EF default for `int`). The entity default is `1` (legacy SHA-256). For **existing** rows, the migration must set `PasswordHashVersion = 1` (legacy) so login migration works correctly. For **new** rows, the seeder sets `PasswordHashVersion = 2` (PBKDF2).

> ⚠️ **Important**: The baseline migration is a fresh create. For an EXISTING deployed database (created via `EnsureCreated` without migration history), a manual baseline bootstrap is required: mark the existing schema as the baseline (insert into `__EFMigrationsHistory`) OR apply the migration to a fresh DB and migrate data. This is documented as a deployment checkpoint, not automated.

## 05 — Migration validation

| Test | Result | Notes |
|---|---|---|
| Fresh DB → migration | **NOT RUN** | Docker daemon + PostgreSQL unavailable in this environment |
| Existing DB upgrade | **NOT RUN** | Same environment limitation |
| Migration SQL review | **PASS** | No destructive SQL, PostgreSQL types confirmed |
| `dotnet ef migrations script` | **PASS** | Generates idempotent PostgreSQL DDL |

### Environment limitation
Docker daemon is not running and the PostgreSQL instance (10.233.141.2:5433) is unreachable. Live migration/backup/restore testing could not be executed. This is documented honestly as NOT RUN, not as PASS.