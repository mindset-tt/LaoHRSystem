# 13 — AUDIT / LOGGING SECURITY

## Redaction

`AuditLogInterceptor` redacts sensitive fields (password hashes, tokens, secrets,
bank account numbers, SWIFT) into `[REDACTED]` (Phase 4B.2). No Authorization
header is logged (Serilog request logging enriches only method/path/status/host/
scheme/user-agent/client-IP).

## Log injection

Structured logging (Serilog compact JSON) — user-controlled strings are values,
not format strings.

## Production log level

`Microsoft.AspNetCore` and `Microsoft.EntityFrameworkCore` overridden to Warning;
no Debug/Trace by default.

## Log rotation

File sink (opt-in) uses daily rolling with `retainedFileCountLimit: 14`.

## PII

Minimized; no full confidential file contents, visitor IDs, or bank data in logs.

## Status

PASS.
