# 05 — DOCUMENT SECURITY

## File security

- Filename sanitization and safe-extension validation are the responsibility of
  the upload path (the existing `DocumentsController` already validates extensions
  and sanitizes filenames; the corporate DMS stores metadata + `StorageReference`).
- Path traversal prevention: `StorageReference` is an opaque identifier, not a
  user-supplied filesystem path.
- Download authorization: enforced via `CanViewDocuments`/`CanManageDocuments`.

## Malware scanning

MALWARE_SCANNING = DEFERRED. MIME validation is not malware scanning; no AV
integration exists. Classified DEFERRED (single-host deployment risk).

## Confidentiality

`Confidentiality` (INTERNAL/CONFIDENTIAL/RESTRICTED) is stored and returned; the
authorization layer gates access independently of parent-entity visibility.

## Status

PASS (with malware scanning explicitly DEFERRED).
