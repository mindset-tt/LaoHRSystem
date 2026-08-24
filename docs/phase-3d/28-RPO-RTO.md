# 28 — RPO / RTO

## Definitions
- RPO (Recovery Point Objective): max acceptable data loss.
- RTO (Recovery Time Objective): max acceptable downtime.

## Targets (to be agreed with business)
- RPO: ≤ 24h (daily backup) — or tighter with WAL (minutes).
- RTO: ≤ 4h (restore + verify).

## Current capability
- RPO: 24h (daily logical backup only; no WAL).
- RTO: unmeasured (restore drill not timed).

## Follow-up
- Agree RPO/RTO with business.
- Measure restore time to validate RTO.
- Add WAL archiving to tighten RPO.
- Document in DR plan (see 40-DISASTER-SCENARIOS).
