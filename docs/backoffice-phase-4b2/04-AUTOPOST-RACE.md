# 04 — AUTO-POST RACE

## Symptom

Two concurrent auto-posts of the same supplier invoice could both create a
`SUPPLIER_INVOICE` source journal, double-posting the invoice to the GL.

## Root cause

`PostingService.PostSupplierInvoiceAsync` checked idempotency with
`HasSourceJournalAsync` (a plain read) before creating the journal, but the check
was not protected against a concurrent duplicate.

## Fix

A filtered unique index on `JournalEntry (SourceType, SourceId, PostingPurpose)`
(migration `AddJournalSourceUniqueness`) enforces source uniqueness at the
database level, so a second auto-post of the same source is rejected.

## Test

`AutoPostRace_TwoConcurrentPosts_OneSourceJournal` — two concurrent
`PostSupplierInvoiceAsync`; exactly one `SUPPLIER_INVOICE` source journal
(`PostingPurpose == "INVOICE"`).

## Regression protection

- Real PG16 test.
- InMemory `PostSupplierInvoice_Twice_Throws`.
