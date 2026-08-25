# 06 — REAL OFF-HOST RESTORE

## Status: NOT RUN — no real off-host copy exists yet

A restore "from off-host" cannot be honestly executed this phase because the
real independent destination is not available (see 05). Restoring from the
local folder and calling it off-host DR would be exactly the status theater
this phase exists to remove.

Mechanics already proven in Phase 4D.1 (evidence: `production-phase-4d.1/51-DR-HTTP-SMoke.md`,
`50-OFFHOST-BACKUP-EVIDENCE.md`) on a copy of a backup set:

- clean PG16 restore (`pg_restore --clean --if-exists --no-owner`),
- fresh document volume populated from `documents.tgz`,
- restored-app REAL HTTP smoke: health/live, health/ready, auth login,
  employees, suppliers, finance, corporate, GL,
- document authorized download with SHA256 == original.

## Operator procedure — first restore FROM THE REAL OFF-HOST COPY

Prerequisite: at least one backup set copied to the real destination (05).

1. Copy the timestamped set FROM `<REAL_DESTINATION>\<timestamp>\` to a
   scratch dir on the restore host. Never mix in files from the source host's
   local `backup/` directory.
2. Verify integrity before use:
   ```powershell
   Get-FileHash <scratch>\* -Algorithm SHA256   # compare against checksums.sha256
   ```
3. Database:
   ```powershell
   docker run --name laohr-restore-pg -e POSTGRES_PASSWORD=<new> -d postgres:16-alpine
   Get-Content <scratch>\laohr-db.dump | docker cp - laohr-restore-pg:/tmp/dump
   docker exec -e PGPASSWORD=<new> laohr-restore-pg `
     pg_restore -U laohr -d laohr --clean --if-exists --no-owner /tmp/dump
   ```
4. Documents: extract `documents.tgz` into a fresh documents volume/root.
5. Configuration: non-secret env only (JWT key NEW value, CORS origins,
   ForwardedHeaders proxy IP, Storage root). Secrets come from the operator
   store, never from backups.
6. Boot the API against the restored DB/storage and run the 4D.1 HTTP smoke
   suite (health, auth, employee/supplier/finance/corporate reads, authorized
   document download).
7. Record SHA256(original) vs SHA256(downloaded-from-restored) — must be equal.

When executed for real, append the evidence table here and flip the DR gate.
