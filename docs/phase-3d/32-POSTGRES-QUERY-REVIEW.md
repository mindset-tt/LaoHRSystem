# 32 — PostgreSQL Query Review

## Status: NOT REVIEWED
No systematic query/EXPLAIN review has been performed.

## Requirements
- Review hot-path queries (payroll, employee list, dashboard).
- Check for N+1 queries (EF Core eager loading).
- Verify indexes on FK columns and filter columns.
- Check for missing `AsNoTracking` on read-only queries.

## Follow-up
- Run `EXPLAIN ANALYZE` on hot queries.
- Add missing indexes.
- Fix N+1 (use `.Include()` / projections).
- Enable EF Core query logging in dev to detect slow queries.
