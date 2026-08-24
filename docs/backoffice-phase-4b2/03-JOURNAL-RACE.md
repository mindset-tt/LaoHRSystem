# 03 — JOURNAL RACE (CONC-002)

## Symptom

Two concurrent POST actions on the same draft journal could both observe
`Status == DRAFT` and both transition it to POSTED, producing duplicate postings.

## Root cause

`AccountingService.PostAsync` read the journal with a plain `FirstOrDefaultAsync`
(no row lock), so two transactions could both pass the DRAFT check.

## Fix

`LockJournalAsync` issues `SELECT ... FOR UPDATE` on PostgreSQL inside the post
transaction, serializing concurrent posts. Only one transaction transitions
DRAFT → POSTED.

## Test

`JournalPostRace_TwoConcurrentPosts_OnePosting` — two concurrent `PostAsync` on
the same draft journal; exactly one succeeds, status `POSTED`, exactly 2 lines
(no duplicates).

## Regression protection

- Real PG16 test.
- InMemory `Post_BalancedJournal_Succeeds` / `Post_UnbalancedJournal_Throws`.
