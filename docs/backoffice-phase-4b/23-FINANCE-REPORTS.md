# 23 — Finance Reports

## Implemented (via list endpoints)
Supplier Invoice Register, Payment Register, Chart of Accounts, Journal list,
Trial Balance, AP Aging.

## Deferred
Dedicated report endpoints + CSV/Excel export (reuse existing export architecture,
UTF-8). No new PDF engine.

## Export authorization
Same query scope as screen/API; no report endpoint can bypass financial permissions.

## Terminology
Management reports are distinct from statutory financial statements (no regulatory
certification claimed).
