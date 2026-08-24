# 01 — Current Recruitment Model

Audit of recruitment/onboarding concepts in the repository.

## Search result
No existing entities for: Recruitment, Candidate, Applicant, JobOpening, Vacancy, JobRequisition, Interview, Offer, Onboarding.

## Reusable existing entities
| Entity | Reuse for |
|---|---|
| `Position` | Job requisition references Position (title, job code, department) |
| `Department` | Requisition department |
| `WorkLocation` | Requisition/opening location |
| `Employee` | Hiring manager, recruiter, interview panel, onboarding owners |
| `AppUser` | Account provisioning after hire |
| `EmployeeDocument` | Document storage pattern (candidate docs will follow a similar model) |
| `ApprovalRequest`/`ApprovalService` | Requisition + offer approval |
| `NotificationService` | Recruitment notifications |
| `AuditLog` | Audit trail |

## Missing (to build)
- JobRequisition + approval
- JobOpening (vacancy)
- Candidate (person-level)
- Application (candidate → opening)
- ATS pipeline stages + history
- Interview + panel + evaluation/scorecard
- Offer + lifecycle
- Hire conversion (candidate → Employee)
- Onboarding process + tasks

## Decisions
- Candidate ≠ Application (person vs application-to-opening).
- Position reused (no duplicate master data).
- ApprovalService reused (no new approval engine).
- No AI scoring/ranking.
- No biometric/background-check integration.
