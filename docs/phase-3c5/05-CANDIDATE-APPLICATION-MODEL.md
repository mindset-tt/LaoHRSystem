# 05 — Candidate / Application Model

## Candidate (person-level)
`Candidate` (FirstName, LastName, FirstNameLao, LastNameLao, Email, Phone, CurrentLocation, CurrentCompany, CurrentTitle, Summary, Source, Status).

## Application (candidate → opening)
`Application` (CandidateId, OpeningId, AppliedAt, Source, CurrentStage, Status, RejectionReason, RejectionComment).

## Separation
Candidate ≠ Application. One candidate may have multiple applications (e.g. Accountant + Finance Analyst).

## Duplicate application
Same candidate cannot apply twice to the same active opening (server rejects with 409).

## PII principle
Only necessary PII collected. No religion/ethnicity/health/biometric/criminal history.
