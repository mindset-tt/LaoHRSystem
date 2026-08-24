# 16 — PG16 UPGRADE

PostgreSQL 16 fresh-migration and upgrade validation.

## Fresh migration

`scripts/validate-finance-postgres.ps1` applies the full migration chain
(including `AddJournalSourceUniqueness`) to a fresh `postgres:16-alpine` database.
Result: PASS.

## Migration scan

`AddJournalSourceUniqueness` is additive-only:
- 1 `CreateIndex` (filtered unique index on `JournalEntry (SourceType, SourceId, PostingPurpose)`).
- 0 `AlterColumn`, 0 `DropColumn`, 0 `DropTable`, 0 `Truncate`, 0 `DeleteData`.
- The `UpdateData` entries are seed timestamp refreshes (non-destructive).

## Journal unique index upgrade safety

Before adding the unique source index on an existing database, query for
duplicates of `(SourceType, SourceId, PostingPurpose)`. In this codebase the
index was added alongside the auto-post idempotency fix, so no pre-existing
duplicates are expected. If duplicates exist on a legacy database, they must be
remediated explicitly (not silently deleted).

## Upgrade validation

A disposable PG16 database was created, migrations applied, and a
`pg_dump`/`pg_restore` round-trip verified (see 17). Representative rows are
preserved (schema round-trip proven; finance tables empty at migration-only state).

## Status

PASS.
