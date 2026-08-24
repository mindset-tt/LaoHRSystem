# 06 — Journal Architecture

## Entities
- `JournalEntry`: JournalNumber, PostingDate, FiscalPeriodId, SourceType, SourceId,
  Description, Status (DRAFT/POSTED/REVERSED), Currency, ReversesJournalEntryId.
- `JournalLine`: AccountId, Description, Debit, Credit, CostCenterId, DepartmentId,
  ProjectId, Currency, ExchangeRate, Reference.

## Source linkage
`SourceType` ∈ SUPPLIER_INVOICE, PAYMENT, EXPENSE, ASSET, MANUAL_JOURNAL (not
fragile free-text only).

## Numbering
`JE-{yyyy}-{000000}` via `NumberSequenceService` (no COUNT+1).

## Posting
`AccountingService.PostAsync` validates balance, period, posting accounts, and
line invariants before marking POSTED.

## Reversal
`ReverseAsync` creates a new entry with inverted debit/credit, linked via
`ReversesJournalEntryId`, and posts it immediately.
