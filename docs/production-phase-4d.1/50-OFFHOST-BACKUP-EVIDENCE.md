# 50 — OFF-HOST BACKUP EVIDENCE

Phase 4D status: `Off-host Backup: PARTIAL / DESIGN ONLY`. Executed now.

## Script

`scripts/backup-production.ps1` — environment-driven, no embedded
credentials (`POSTGRES_PASSWORD` must come from the environment; the script
refuses to run without it).

Produces per run (timestamped folder):
- `laohr-db.dump` — PostgreSQL **custom-format** dump (pg_dump -Fc)
- `documents.tgz` — full document-storage archive (named volume)
- `manifest.json` — NON-SECRET metadata only: timestamps, file names/sizes,
  DB name, restore hints, DR classification. No JWT key, no DB password,
  no encryption keys.
- `checksums.sha256` — SHA256 of dump + archive + manifest

Verification inside the script (any failure ⇒ non-zero exit, no SUCCESS):
1. dump exists and size > 0
2. `pg_restore --list` succeeds AND contains TABLE DATA entries
3. document archive exists, size > 0
4. manifest written; checksums generated

## Off-host execution (OFFHOST_DR_SIMULATION)

No real NAS/second host is available in this environment, so a separate
destination directory outside the repository
(`C:\Users\khamp\Documents\laohr-offhost-sim`) simulates the off-host copy.
**This is an OFFHOST_DR_SIMULATION, not true geographic DR** — recorded as
such in the manifest (`drClassification`) and in the release gate.

Run output (2026-08-25T02:13 local):

```
=== [1/5] Database dump (laohr -> custom format) ===
    dump size: 1,885,165 bytes
=== [2/5] pg_restore --list verification === OK
=== [3/5] Document storage archive === OK
=== [4/5] Manifest === OK
=== Checksums === documents.tgz, laohr-db.dump, manifest.json
=== [5/5] Off-host copy + re-verification ===
    verified: documents.tgz
    verified: laohr-db.dump
    verified: manifest.json
OFF-HOST COPY VERIFIED: ...\laohr-offhost-sim\20260825-021310 (OFFHOST_DR_SIMULATION)
EXIT=0
```

Every copied file is re-hashed at the destination and compared byte-for-byte.

## Backup duration measured

Backup wall time ≈ **17–20 s** for this dataset (~1.9 MB compressed dump,
500 employees / 30k+ rows synthetic dataset) on dev-class hardware.
Not a business RPO/RTO claim.

## Failure tests (spec §38)

| Scenario | Expected | Observed |
|---|---|---|
| `POSTGRES_PASSWORD` unset | non-zero exit, clear error, before any work | PASS ("POSTGRES_PASSWORD environment variable is not set") |
| Off-host destination does not exist / unreachable (`Q:\nope`) | non-zero exit, clear error, NO success message | PASS (exit code 1, "BACKUP FAILED: off-host destination ... not reachable") |
| Destination not writable | non-zero exit via write-probe | implemented (probe write + delete) |

## Secret scan of artifacts

Manifest contains only file names, sizes, timestamps, database name,
volume name, restore hint, DR classification. Verified by reading the
manifest post-run; checksums file contains hashes only.
