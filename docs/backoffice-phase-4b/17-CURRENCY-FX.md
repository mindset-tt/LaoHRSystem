# 17 — Currency / FX

## Reuse
Existing `ConversionRate` (FromCurrency, ToCurrency, Rate, EffectiveDate, ExpiryDate,
IsActive) is reused. No Finance-specific currency enum.

## Base currency
Organization setting (likely LAK, but not hardcoded). No live FX retrieval.

## Multi-currency journal
Journal lines carry optional Currency + ExchangeRate. Accounting amounts use
decimal; rounding per currency/configuration (no culture-dependent parse).
