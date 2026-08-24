# 26 — Restore Drill

## Validated
- Restore from `pg_dump` custom format: PASS (clean drop → create → restore, 84 tables).

## Drill procedure
1. Take fresh backup.
2. Drop target DB.
3. Create empty DB.
4. `pg_restore` custom format.
5. Verify table count + row counts + spot-check data.
6. Verify app starts against restored DB.

## Frequency
- Quarterly full restore drill (minimum).
- After any schema change.

## Follow-up
- Automate the drill.
- Measure restore time (feeds RTO).
- Document restore runbook (see 42-BACKUP-RESTORE-RUNBOOK).
