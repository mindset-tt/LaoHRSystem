# 22 — Performance / Index Review

## Added indexes (justified by access patterns)
- `JobRequisition (Status)`, `(PositionId)`, `(HiringManagerEmployeeId)`.
- `JobOpening (RequisitionId, Status)`.
- `Candidate (Email)`.
- `Application (CandidateId)`, `(OpeningId)`, `(CurrentStage, Status)`.
- `ApplicationStageHistory (ApplicationId, CreatedAt)`.
- `CandidateDocument (CandidateId)`.
- `Interview (ApplicationId)`, `(ScheduledStart)`.
- `InterviewParticipant (InterviewId, EmployeeId)` unique.
- `InterviewEvaluation (InterviewId, EvaluatorEmployeeId)` unique.
- `Offer (ApplicationId)`, `(Status)`.
- `OnboardingProcess (EmployeeId)`.
- `OnboardingTask (OnboardingProcessId)`, `(OwnerEmployeeId, DueDate)`.

## N+1 prevention
Hire conversion uses `Include` for candidate/opening/requisition/position (single query). No per-candidate loops.

## No fabricated EXPLAIN
Real PostgreSQL unavailable; no fabricated query plans.
