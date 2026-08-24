# 16 — Recruitment Authorization

## Service
`IRecruitmentAccessService` / `RecruitmentAccessService`.

## Rules
- **HR/Admin**: full recruitment access.
- **Requester** (RequestedByEmployeeId): view/edit own requisition.
- **Hiring manager** (HiringManagerEmployeeId): view requisition/application/candidate/offer.
- **Interview panel member**: view assigned interview + submit evaluation.
- **Unrelated employee**: 403.

## Role vs context
Global HR permission ≠ hiring manager for one requisition. Interview panel membership ≠ recruiter permission. Contextual access used.

## Methods
`CanViewRequisition`, `CanEditRequisition`, `CanViewCandidate`, `CanViewApplication`, `CanMoveApplication`, `CanViewInterview`, `CanSubmitEvaluation`, `CanViewOffer`, `CanCreateOffer`, `CanHire`, `CanManageOnboarding`.

## Hire
Only HR/Admin may perform hire conversion (`CanHireAsync`).

## IDOR
`RecruitmentIdorTests` (3): unrelated employee cannot view candidate/offer or hire.
