# 42 — Backup & Restore Runbook

## Backup (validated)
```
pg_dump -h 127.0.0.1 -p 5434 -U laohr -Fc -d laohr_design -f backup.dump
```

## Restore (validated)
```
# drop + recreate
dropdb -h 127.0.0.1 -p 5434 -U laohr laohr_design
createdb -h 127.0.0.1 -p 5434 -U laohr laohr_design
pg_restore -h 127.0.0.1 -p 5434 -U laohr -d laohr_design backup.dump
```

## Verify
- Table count (84 tables).
- Row counts + spot-check.
- App starts against restored DB.

## Schedule
- Daily full backup (off-peak).
- Off-site copy (encrypted).

## Follow-up
- Automate (cron/systemd timer).
- Encrypt at rest.
- Test restore quarterly (26).
