# 21 — Authorization / Privacy

## Service
`IPerformanceAccessService` / `PerformanceAccessService`.

## Rules
- **Self**: own goals/reviews/feedback/1:1s/development/assessments/certifications.
- **Manager**: direct reports (via `IsManagerOfAsync`).
- **HR/Admin**: broader administration + talent review.
- **Unrelated**: 403.

## Sensitive fields
- Manager-private 1:1 notes: never returned through employee-facing DTOs.
- Talent/potential: HR-restricted.
- Offer/compensation (Phase 3C5): HR/hiring-manager only.

## Historical manager access
Review created under Manager A stays with A (snapshot). New cycle uses current manager.

## IDOR
`PerformanceIdorTests` (3): unrelated employee cannot view goal/talent/feedback.

## Fairness
No ranking/automated decision uses sensitive attributes. No proxy discrimination.
