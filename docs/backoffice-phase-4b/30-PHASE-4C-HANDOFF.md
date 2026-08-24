# 30 — Phase 4C Handoff

## Phase 4B outcome
- Finance + Accounting foundation: SupplierInvoice (+3-way match), AP (+aging),
  Payment (+allocation), BankAccount, Chart of Accounts, Fiscal Year/Period,
  Journal (+double-entry, immutability, reversal), General Ledger, Trial Balance,
  AR foundation (Customer/CustomerInvoice/Receipt).
- Budget actual transition (Committed → Actual, no double count).
- Finance authorization (Admin only; HR ≠ Finance).
- 195 backend tests, 45 frontend tests, 5× backend gate, PG16 fresh/backup/restore.

## Recommended Phase 4C (choose from actual business gaps)
- OPTION A — Finance 4C: Tax configuration + Bank Reconciliation + Asset
  Depreciation (after legal/accounting confirmation).
- OPTION B — Operations 4C: Facilities + Fleet + Travel + Contract lifecycle + DMS.
- OPTION C — Sales/AR 4C: Customer billing / Receivables.

## Accounting safety
Phase 4C must NOT invent Lao VAT, withholding tax, corporate tax, statutory chart,
or financial reporting regulation. Those require separate verified configuration/research.

## Key constraints carried forward
- No fake data; no guessed Lao tax/accounting rules.
- 0 backend failures; 0 frontend failures.
- No migration rebase (additive only).
- No demo credentials in production.
- No client-supplied financial authority.
- No CI/CD, no GitHub Actions, no cloud runners.
- STOP feature expansion until Phase 4C is explicitly requested.
