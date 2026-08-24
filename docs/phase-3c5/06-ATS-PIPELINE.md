# 06 — ATS Pipeline

## Stages
`APPLIED → SCREENING → INTERVIEW → ASSESSMENT → OFFER → HIRED` (+ REJECTED, WITHDRAWN).

## Source of truth
ONE `Application` record. Pipeline board / candidate list / recruiter view / hiring-manager view are views over the same Application.

## Move
`POST /applications/{id}/move` — server validates stage, authorization, terminal state. Drag/drop is not the source of truth.

## History
`ApplicationStageHistory` records who/from/to/when/comment (reproducible timeline).

## Reject/withdraw
Explicit terminal states with reason category + optional comment (not exposed to candidate-facing surfaces).
