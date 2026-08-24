# 18 — File Upload Security

## Upload surface
- Documents/attachments (employee documents, announcements, etc.).
- Uploads stored on filesystem (writable volume required in production).

## Controls present
- Size limits (configurable).
- Extension/content-type validation (validators).
- Auth required on upload endpoints.

## Findings / follow-up
- Verify content-type sniffing vs extension allow-list (prefer allow-list).
- Ensure uploads are NOT served with executable MIME types.
- Store uploads outside web root; serve via authenticated controller.
- Add malware scanning (org policy) before production.
- Ensure upload volume is persistent and backed up.
