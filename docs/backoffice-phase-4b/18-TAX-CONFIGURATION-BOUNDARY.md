# 18 — Tax Configuration Boundary

## Absolute rule
No guessed Lao VAT rate, input/output VAT treatment, withholding tax, corporate
tax, statutory chart, official invoice format, or accounting retention period.

## Status
`LEGAL_CONFIRMATION_REQUIRED` for all statutory tax concepts.

## Foundation (if needed later)
Configurable `TaxCode` (Code, Name, Rate, Recoverable, Account mappings,
EffectiveDate, Status, VerificationStatus) — architecture only, no seeded rates.

## No fabricated percentages
No 0.07 / 0.10 / any percentage seeded from memory.
