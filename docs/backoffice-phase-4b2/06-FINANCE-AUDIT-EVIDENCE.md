# 06 — FINANCE AUDIT EVIDENCE

Persisted finance audit verification and secret exclusion.

## Audit architecture

`AuditLogInterceptor` (a `SaveChangesInterceptor`) captures Added/Modified/Deleted
entries and enqueues `AuditLog` records onto a bounded `Channel<AuditLog>`
(capacity 10,000, `DropOldest`), drained by `AuditLogWriter` (a `BackgroundService`)
that batches (256) and writes via a fresh scoped `LaoHRDbContext`.

Fields captured: `UserId`, `EntityName`, `Action` (ADDED/MODIFIED/DELETED),
`KeyValues` (PK JSON), `OldValues`/`NewValues` (JSON), `Timestamp`.

## Persisted audit evidence

The interceptor is exercised by `AuditLogTests` (ADDED and MODIFIED) and the new
`AuditSecretExclusionTests`. The fire-and-forget writer persists to the
`AuditLogs` table; `AuditLogsController` (Admin-only) exposes paged reads.

## Secret exclusion (NEW in 4B.2)

`AuditLogInterceptor` now redacts sensitive property values before serializing
audit JSON. The following property names (case-insensitive) are replaced with
`[REDACTED]`:

- Credentials/tokens/secrets: `PasswordHash`, `Password`, `TokenHash`,
  `ReplacedByHash`, `RefreshToken`, `AccessToken`, `Jwt`, `Secret`, `ApiKey`,
  `ConnectionString`.
- Banking identifiers: `AccountNumber`, `BankAccount`, `Swift`, `Iban`.

The audit still records that the field changed (via the marker) without leaking
the value.

## Tests

- `AppUser_Add_RedactsPasswordHash`
- `BankAccount_Add_RedactsAccountNumberAndSwift`
- `RefreshToken_Add_RedactsTokenHash`

## Export audit

`FinanceReportsController` writes a persisted `AuditLog` record
(`EntityName = "FINANCE_EXPORT"`, `Action = "EXPORT"`) for every sensitive bulk
export (General Ledger, Trial Balance, AP Aging, and all other report exports).

## Status

PASS — persisted audit evidence + secret exclusion verified.
