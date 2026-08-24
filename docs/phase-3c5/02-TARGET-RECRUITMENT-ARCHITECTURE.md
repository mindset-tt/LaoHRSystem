# 02 — Target Recruitment Architecture

## Principle
ONE Position source of truth, ONE Candidate identity before hire, ONE Employee identity after hire, ONE Approval infrastructure, ONE Notification infrastructure, ONE Document infrastructure. No disconnected ATS island.

## New entities (Phase 3C5)
- `JobRequisition` — approval to recruit (references Position/Department/WorkLocation).
- `JobOpening` — vacancy derived from an approved requisition.
- `Candidate` — person-level recruitment identity.
- `Application` — candidate → opening (separate from Candidate).
- `ApplicationStageHistory` — immutable pipeline movement.
- `CandidateDocument` — CV/cover letter/certificate.
- `Interview` + `InterviewParticipant` — scheduling + panel.
- `InterviewEvaluation` — structured scorecard (1-5).
- `Offer` — compensation + lifecycle.
- `OnboardingProcess` + `OnboardingTask` — post-hire checklist.

## New services
- `IRecruitmentAccessService` — contextual access (HR/Admin vs hiring manager vs panel).
- `IHireConversionService` — candidate → Employee conversion (idempotent).

## New endpoints (`RecruitmentController`, `/api/recruitment`)
Requisitions (CRUD + submit/approve), openings, candidates, applications (CRUD + move + history), interviews (+ evaluations), offers (CRUD + accept/decline), hire, onboarding (tasks).

## Reused (not duplicated)
`Position`, `Department`, `WorkLocation`, `Employee`, `AppUser`, `ApprovalService`, `NotificationService`, `EmployeeDocument` pattern.

## Deferred (not justified)
Public careers site, referrals, candidate portal, calendar integrations, email automation, agency portal, background check, digital signature, AI scoring/ranking, configurable scorecard templates.
