# 27 — CORPORATE IDOR

## Tests

`CorporateAuthorizationTests` proves:

- Employee cannot manage facilities/fleet/visitors/documents (403).
- Employee cannot view facilities/fleet (403).
- HR can view/manage facilities (200/201).
- Employee cannot view another employee's travel request (IDOR → 403).

## Document IDOR

Document access is gated by `CanViewDocuments`/`CanManageDocuments`, independent
of parent-entity visibility (a user who can see a PO does not automatically see a
RESTRICTED document).

## Status

PASS.
