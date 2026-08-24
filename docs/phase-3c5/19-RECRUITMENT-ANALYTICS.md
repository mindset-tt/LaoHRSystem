# 19 — Recruitment Analytics

## KPI definitions (canonical)
- `REC-OPEN-REQUISITIONS`: requisitions with status APPROVED/OPEN.
- `REC-OPEN-APPLICATIONS`: applications with status ACTIVE.
- `REC-CANDIDATES-BY-STAGE`: application count grouped by CurrentStage.
- `REC-TIME-TO-HIRE`: Application.AppliedAt → HireDate (NOT requisition-approved → hire).
- `REC-TIME-TO-FILL`: opening created → filled (distinct from time-to-hire).
- `REC-OFFER-ACCEPTANCE`: accepted offers / sent offers.
- `REC-HIRES`: applications with status HIRED.
- `REC-SOURCE-DISTRIBUTION`: candidate/application count by Source.
- `ONB-ACTIVE`, `ONB-OVERDUE-TASKS`, `ONB-COMPLETION`.

## Pipeline conversion
Applied → Screening → Interview → Offer → Hire counts + conversion rates (denominator=0 handled safely).

## Source effectiveness
Distinguish applications/interviews/offers/hires (not raw applicant volume alone).

## Deferred
Full analytics dashboard (Phase 3C3 analytics extension) — KPI definitions documented; dashboard UI deferred to a later pass.
