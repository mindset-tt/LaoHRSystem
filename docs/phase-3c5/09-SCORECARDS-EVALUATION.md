# 09 — Scorecards / Evaluation

## Model
`InterviewEvaluation` (InterviewId, EvaluatorEmployeeId, CommunicationScore, ExperienceScore, RoleFitScore, Recommendation, Comments).

## Score range
1–5 (documented). No mixing 1–5 and 0–100.

## Recommendation
`Recommend`, `Neutral`, `DoNotRecommend`.

## One evaluation per interviewer
Unique `(InterviewId, EvaluatorEmployeeId)` — duplicate submission rejected (409).

## Interviewer privacy
Panel members submit their own scorecard; no anchoring-bias hiding implemented (deferred — not required).

## Human decision authoritative
Final hire is NOT inferred from average score.
