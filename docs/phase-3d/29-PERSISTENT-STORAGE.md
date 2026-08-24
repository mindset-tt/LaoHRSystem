# 29 — Persistent Storage

## Volumes
- PostgreSQL data: named volume (compose).
- Uploads/documents: filesystem — needs persistent volume.

## Findings
- Postgres data volume defined.
- Upload storage location: verify it is a mounted volume (not container layer).

## Follow-up
- Mount uploads to a named volume or host path.
- Back up uploads alongside DB.
- Ensure API container has writable, persistent upload path.
- Document storage growth + capacity planning.
