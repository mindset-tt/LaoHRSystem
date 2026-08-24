# 19 — Talent Review

## Model
`TalentReview` (EmployeeId, CycleId, Potential, Performance, Readiness, Notes, ReviewedByEmployeeId).

## Human-entered
Performance × Potential are explicit human assessments (LOW/MEDIUM/HIGH). No opaque algorithm.

## 9-box
If visualized, it is a display of explicit human-entered Performance × Potential. It does NOT decide promotion/termination/compensation/training access.

## Readiness
`READY_NOW`, `READY_1_2_YEARS`, `DEVELOPING`, `NOT_ASSESSED`.

## Authorization
HR/Admin only (`CanViewTalentReviewAsync`). Not exposed through ordinary ESS.

## Succession
Full succession engine deferred (readiness foundation only).
