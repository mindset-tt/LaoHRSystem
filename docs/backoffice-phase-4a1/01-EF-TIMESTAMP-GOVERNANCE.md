# 01 — EF Timestamp Governance

## Root cause of the "354 AlterColumn" surprise
`Npgsql.EnableLegacyTimestampBehavior` changes how Npgsql maps `DateTime`:
- **OFF (default)**: `DateTime` → `timestamp with time zone` (Npgsql 6+ default).
- **ON**: `DateTime` → `timestamp without time zone`.

The existing migration chain was generated with the switch **OFF**, so all
`DateTime` columns are `timestamp with time zone`. When the design-time factory
had the switch **ON** (a Phase 3D fix for `database update` seed application),
`migrations add` rebuilt the model with `timestamp without time zone` and
scaffolded hundreds of non-additive `AlterColumn` operations.

## Why the switch is needed at all
Historical seed data (e.g. holidays) contains `DateTimeKind.Unspecified` values.
Npgsql rejects `Unspecified` DateTime for `timestamp with time zone` columns
("a UTC DateTime is required"). The legacy switch makes Npgsql accept them
during `database update`.

## Governance (implemented)
- `migrations add` runs with the switch **OFF** (canonical `timestamp with time zone`).
- `database update` runs with the switch **ON**, gated by the `NPGSQL_LEGACY_TIMESTAMP=1`
  environment variable (set by `scripts/validate-postgres.ps1`).
- The design-time factory reads `LAOHR_EF_CONNECTION_STRING` (no hardcoded password).

## Rules
- Do NOT rebase/edit historical migrations to "clean" timestamp history.
- New migrations must be additive (no AlterColumn/DropColumn/DropTable in `Up()`).
- A no-op `migrations add` against the current model must produce zero timestamp churn.
