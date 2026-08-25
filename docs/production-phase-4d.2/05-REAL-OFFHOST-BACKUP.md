# 05 — REAL OFF-HOST BACKUP

## Status: REAL DESTINATION **NOT AVAILABLE** (operator-confirmed 2026-08-25)

Per readiness rule §34 this keeps:

- PRODUCTION_OPERATIONS_READY = PARTIAL
- PRODUCTION_DEPLOYMENT_READY = PARTIAL

No status inflation: a second directory on the same disk/host does NOT count,
and none of NAS / second server / removable encrypted disk / org remote share
was available to this phase.

## What WAS re-proven this phase (final code, real stack)

1. **Off-host failure detection** — `scripts/backup-production.ps1
   -OffhostDest \\NONEXISTENT-HOST\share`:
   - full local pipeline runs (dump → pg_restore --list verify → documents.tgz
     → manifest → checksums),
   - then: `BACKUP FAILED: off-host destination '\\NONEXISTENT-HOST\share'
     does not exist or is not reachable.`
   - process exit code = **1** (measured via Start-Process ExitCode; no false
     "BACKUP COMPLETE" is printed).
2. **Local pipeline integrity on final code** — run without `-OffhostDest`:
   exit code = 0, `BACKUP COMPLETE AND VERIFIED`, manifest + checksums.sha256
   produced (`backup/<timestamp>/`).

## Operator runbook — first REAL off-host execution

When the destination exists (examples: `\\nas\laohr-backups`,
`D:`-on-second-server via SMB, removable encrypted disk letter):

```powershell
$env:POSTGRES_PASSWORD = <prompted, never stored>
.\scripts\backup-production.ps1 `
    -BackupDir D:\backups `
    -OffhostDest <REAL_DESTINATION> `
    -DbName laohr
```

Acceptance criteria (all must hold):

| # | Criterion |
|---|---|
| 1 | exit code 0 |
| 2 | `OFF-HOST COPY VERIFIED` line names the real host/volume |
| 3 | every hash re-verified at destination byte-for-byte |
| 4 | unplug/disconnect test: repeat with destination offline → exit 1 |
| 5 | restore drill performed FROM THE OFF-HOST COPY (see 06) |

Update `drClassification` expectation: the script labels the copy honestly;
once the destination is genuinely independent hardware, record the evidence in
this file and flip OPERATIONS/DEPLOYMENT readiness.
