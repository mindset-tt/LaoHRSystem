# 47a — UPLOAD SECURITY & MAGIC BYTE EVIDENCE

Phase 4D final report said "MIME/extension validation"; the implementation
was extension allow-list only. Phase 4D.1 closes the gap.

## What changed

1. **Content-signature (magic byte) validation** — new
   `Services/FileSignatureValidator.cs`, wired into both upload endpoints
   BEFORE any byte is written to storage:

   | Extension | Required content signature |
   |---|---|
   | .pdf | `%PDF-` within first 1024 bytes |
   | .jpg / .jpeg | FF D8 FF |
   | .png | 89 50 4E 47 0D 0A 1A 0A |
   | .docx | PK\x03\x04 (ZIP/OOXML) |
   | .doc | D0 CF 11 E0 A1 B1 1A E1 (OLE2) |

   Unknown extensions: rejected. Stream position is reset so callers can
   continue reading.

2. **Protected storage root** — documents/photos moved OUT of the web root
   (`Storage__DocumentsRoot`, default `{ContentRoot}/App_Data/uploads`).
   Static-file middleware bypasses authorization, so anything under wwwroot
   was anonymously downloadable; now files are only reachable through
   `GET /api/documents/{id}/file` and `GET /api/employees/{id}/photo`,
   both IDOR-checked (`IDataScopeService.CanViewEmployeeAsync`) and served
   with `Content-Disposition: attachment`.

3. **Metrics** — `laohr_uploads_rejected_total{reason}` counter increments on
   every rejection (verified live: counter appeared and incremented after
   exe-renamed-pdf attempts).

## Test evidence

Automated (291 total backend tests pass, including these):

`Unit/Services/FileSignatureValidatorTests.cs`
- genuine PDF/JPEG/PNG/DOCX/DOC signatures accepted; stream position reset
- MZ executable renamed .pdf → REJECTED
- HTML renamed .pdf → REJECTED
- script renamed .png → REJECTED
- empty stream → REJECTED; unknown extension → REJECTED
- truncated JPEG prefix → REJECTED
- PDF with leading junk in scan window → accepted (spec-tolerant)

`Integration/Security/UploadSecurityTests.cs` (full HTTP surface)
| Case | Result |
|---|---|
| valid PDF upload → authorized download → byte equality | PASS (201 + 200) |
| .exe renamed .pdf | 400 |
| HTML renamed .pdf | 400 |
| double extension invoice.pdf.exe | 400 |
| path traversal filename ..\..\evil.pdf | sanitized, stored under generated key |
| zero-byte file | 400 |
| >10 MB file | 400 (size limit) |
| .svg (active-content risk) | 400 by allow-list |
| employee downloading another's document | 403 |
| storage key escapes web root (`documents/...`, not `/uploads/...`) | PASS |

Dangerous-format policy (spec §48): exe/dll/bat/cmd/ps1/sh/js/html/hta/svg
and macro-enabled Office formats are denied by the extension allow-list;
content signatures additionally prevent masquerading as an allowed type.
Live download uses attachment disposition so even a polyglot file cannot
execute inline in the browser context.

## Malware scanning decision — see 52-MALWARE-UPLOAD-DECISION.md
