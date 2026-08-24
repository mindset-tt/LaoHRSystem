# 09 — Journal Idempotency

## Source uniqueness
`JournalEntry` has `SourceType` + `SourceId` + `PostingPurpose` (new). A source
can only be auto-posted once.

## Auto-posting idempotency
`PostingService` checks `HasSourceJournalAsync` before creating a journal, so
calling `PostSupplierInvoice`/`PostPayment`/`PostExpense` twice does not create
two journals.

## Journal post race
`AccountingService.PostAsync` is transition-guarded (only DRAFT posts).

## Reversal
`ReverseAsync` creates a new entry with inverted lines, linked via
`ReversesJournalEntryId` (queryable both directions). Original remains.

## Tests
`PostingServiceTests.PostSupplierInvoice_Twice_Throws`; `AccountingServiceTests`
(post/reverse/balance).
