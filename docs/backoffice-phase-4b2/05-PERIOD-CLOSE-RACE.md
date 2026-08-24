# 05 — PERIOD-CLOSE RACE

## Symptom

A journal could be posted to a fiscal period that was concurrently closed,
violating the "no posting to a CLOSED/LOCKED period" invariant.

## Root cause

`AccountingService.PostAsync` validated the period status with a plain read that
was not serialized against a concurrent period close.

## Fix

The period status check runs inside the post transaction (after the journal row
lock is held), so a concurrent close is observed and posting is rejected.

## Test

`PeriodCloseRace_PostingToClosedPeriod_Rejected` — close the period, then posting
a journal to it throws `InvalidOperationException`.

## Regression protection

- Real PG16 test.
- InMemory `Post_ToClosedPeriod_Throws`.
