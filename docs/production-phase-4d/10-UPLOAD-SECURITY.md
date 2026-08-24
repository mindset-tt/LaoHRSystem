# 10 — UPLOAD SECURITY

## Changes (Phase 4D)

`DocumentsController.UploadDocument`:
- **IDOR check**: `CanViewEmployeeAsync(employeeId)` before upload (and delete).
- **Size limit**: 10 MB server-side.
- **Filename sanitization**: `SanitizeFileName` strips directory components and
  control characters; storage name is generated (`Ticks_Guid.ext`), never derived
  from user input.
- **Extension allow-list**: `.pdf .jpg .jpeg .png .doc .docx`.

## Path traversal

Storage name is generated (not user-controlled); display name is sanitized via
`Path.GetFileName`. `../`, `..\`, encoded traversal, and absolute paths cannot
escape the upload directory.

## MIME / content

Extension allow-list only; no magic-byte sniffing. Malware scanning is DEFERRED
(see 11). MIME validation is not malware scanning.

## Download

`Content-Disposition` exposed via CORS; authorization enforced on read paths.

## Status

PASS (with malware scanning DEFERRED_WITH_RISK).
