# 51 — RESTORE DRILL + REAL HTTP SMOKE EVIDENCE

Phase 4D called a DbContext-only check a "Restored HTTP Smoke". Corrected:
this phase performs ACTUAL HTTP calls against an application instance
connected to the restored environment.

## Drill procedure (scripts/restore-drill.ps1)

1. Integrity-verify the OFF-HOST copy (hash-compare all files against
   checksums.sha256) — restore always prefers the off-host copy.
2. Drop any previous `laohr_restore` database; create clean.
3. `pg_restore` custom-format dump into it.
4. Extract `documents.tgz` into a FRESH named volume.
5. Boot a SEPARATE API container (`prodlike4d1-api`) attached to restored DB +
   restored document volume, port bound to 127.0.0.1 only.
6. Run real HTTP smoke + document checksum verification through that API.

## Measured durations (exact observed values; NOT business RTO/RPO claims)

| Phase | Duration |
|---|---|
| Backup (full run incl. off-host copy) | ~17–20 s |
| PostgreSQL restore (11 MB dump → clean PG16) | **11.5 s** |
| Document storage restore | **0.9 s** |
| Application recovery (container start → /health/ready 200) | **7.1 s** |

## Restored HTTP smoke results (real HTTP, separate API process)

```
GET  /health/live                              -> 200
GET  /health/ready                             -> 200
POST /api/auth/login                           -> OK (JWT issued)
GET  /api/employees        (HR surface)        -> 200
GET  /api/suppliers        (back office)       -> 200
GET  /api/supplier-invoices/aging (finance)   -> 200
GET  /api/corporate-documents  (corporate)     -> 200
GET  /api/journals             (general ledger)-> 200
smoke failures: 0
```

## Document restore verification (spec §41)

A document uploaded BEFORE backup (SHA256
`2FC2CBAC779B86AAD8F43F4AEC8D651B1AA93AE8AB44E8221D8276F14050873B`) was
downloaded through the RESTORED application:

```
GET /api/documents/401/file -> 200 (authorized)
CHECKSUM MATCH: 2FC2CBAC779B86AAD8F43F4AEC8D651B1AA93AE8AB44E8221D8276F14050873B
```

Authorization enforced (Bearer token required), HTTP success, byte-identical
content. The same hash was verified at original upload time — proving
end-to-end integrity across backup → off-host copy → restore → serve.

## Classification

OFFHOST_DR_SIMULATION (second local volume/path, not geographic DR).
Restore drill: **PASS**.
