# 21 — Migration Review

## Chain (append-only)
```
InitialCreatePostgres
  → AddApprovalEssMssNotifications
  → AddPmPlanningAndDependencies
  → AddPmPlanningDependenciesV2   (no-op: seed timestamp refresh only)
  → AddRecruitmentAndOnboarding
```

## `AddRecruitmentAndOnboarding` review
- **New tables**: Candidates, JobRequisitions, JobOpenings, Applications, ApplicationStageHistories, CandidateDocuments, Interviews, InterviewParticipants, InterviewEvaluations, Offers, OnboardingProcesses, OnboardingTasks.
- **No DROP/TRUNCATE/destructive ALTER** in `Up` (all `DropTable` are in `Down`).
- **FK delete behavior**: Candidate deletion does NOT cascade into hired Employee (no FK from Employee to Candidate). Employee deletion does NOT destroy recruitment history (OnboardingProcess references Employee with no cascade). Application/Interview/Offer cascade within recruitment domain.
- **Unique constraints**: `InterviewParticipant (InterviewId, EmployeeId)` and `InterviewEvaluation (InterviewId, EvaluatorEmployeeId)` unique. Candidate.Email NOT unique (shared/family email possible).

## Note on `AddPmPlanningDependenciesV2`
A spurious no-op migration (seed timestamp refresh only, no schema change) created by a package-version update between phases. Harmless and append-only; left in place.
