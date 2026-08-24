# 22 — CONNECTION POOL

## Current

Npgsql defaults (MaxPoolSize 100, 15s command timeout). No custom tuning.

## Review

- DbContext is scoped (per-request); no long-lived contexts.
- Audit writer uses a fresh scoped context per batch (no captured disposed context).
- No connection leak identified in the audited services.

## Recommendation

Measure under load before tuning `MaxPoolSize`/`Timeout`. Do not invent tuning.

## Status

PASS (defaults; measure before tuning).
