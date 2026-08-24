# 22 — Accounting Roadmap

## Status
DEFERRED (Phase 4B). No General Ledger in Phase 4A.

## Planned foundation
- `ChartOfAccount` (types: ASSET, LIABILITY, EQUITY, REVENUE, EXPENSE — configurable).
- `JournalEntry` + `JournalLine` (double-entry).
- `FiscalPeriod` (configurable, not guessed).

## Invariants (future)
- Every posted journal must balance: Debit == Credit (no exceptions).
- Journal status: DRAFT, POSTED, REVERSED. Posted entries immutable; correction
  via reversal/new journal.
- Period lock: no silent edits to historical accounting.

## Caution
Do not hardcode Lao statutory chart without verified requirement. Do not invent
Lao VAT/tax/withholding rules. Build configurable foundations first.
