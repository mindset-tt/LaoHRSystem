# 04 — Posting Architecture

## Service
`PostingService` — connects operational events to the accounting kernel via
controlled, idempotent, atomic auto-posting.

## Chain
OPERATIONAL EVENT → VALIDATION → APPROVAL → ACCOUNT MAPPING → JOURNAL CREATION
→ ATOMIC POSTING → GENERAL LEDGER → BUDGET RECONCILIATION → REPORTING → AUDIT.

## Posting transaction (all-or-nothing)
validate source → validate config → create journal → create lines → balance →
post journal → update source state → update budget actual (where applicable).

## Posting failure
If journal fails, the source is not falsely marked POSTED.

## Posting date
Explicit deterministic rule (source date), not local `DateTime.Now`.
