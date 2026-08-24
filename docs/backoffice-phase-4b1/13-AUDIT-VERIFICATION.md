# 13 — Audit Verification

## Mechanism
`AuditLogInterceptor` automatically captures Added/Modified/Deleted entries for
ALL entities (including finance) and enqueues them to a bounded channel drained
by `AuditLogWriter`.

## High-risk finance operations (audited automatically)
Invoice approve/post, payment approve/post, journal post, journal reversal,
period close, account mapping change, bank-account change, COA change.

## Secrets
No full bank info logged.

## Note
The interceptor is registered only for the relational (Npgsql) path; InMemory
tests do not attach it. Audit persistence is verified against real PostgreSQL.
