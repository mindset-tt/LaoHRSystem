# 25 — Phase 3C6 Handoff

## Recruitment feature matrix

| Capability | API | UI | Auth | Tests | Status |
|---|---|---|---|---|---|
| Requisition | `/api/recruitment/requisitions` | `/recruitment` | RecruitmentAccess | Yes | PASS |
| Approval | submit/approve | — | HR role | Yes | PASS |
| Opening | `/api/recruitment/openings` | — | RecruitmentAccess | Yes | PASS |
| Candidate | `/api/recruitment/candidates` | — | RecruitmentAccess | Yes | PASS |
| Application | `/api/recruitment/applications` | — | RecruitmentAccess | Yes | PASS |
| Pipeline | move + history | — | RecruitmentAccess | Yes | PASS |
| Screening | SCREENING stage | — | RecruitmentAccess | Yes | PASS |
| Interview | `/api/recruitment/interviews` | — | Panel scope | Yes | PASS |
| Scorecard | evaluations | — | Panel scope | Yes | PASS |
| Offer | `/api/recruitment/offers` | — | HR/HM | Yes | PASS |
| Hire Conversion | hire | — | HR/Admin | Yes | PASS |
| Preboarding | onboarding tasks | — | Onboarding owner | Yes | PASS |
| Onboarding | `/api/recruitment/onboarding` | — | Onboarding owner | Yes | PASS |
| Recruiter Workspace | — | `/recruitment` | HR/Admin | Yes | PASS |
| Hiring Manager | — | (scoped) | HM | Yes | PASS |
| Notifications | NotificationService | — | — | Partial | PASS |
| Analytics | KPI definitions | — | — | Partial | PASS |
| Reports | (deferred) | — | — | — | PARTIAL |

## Current employee lifecycle (for Phase 3C6)
- `Employee` (core HR), `Position` (slot), `Department` (hierarchy), `WorkLocation`, `ManagerId`.
- Recruitment: `JobRequisition` → `JobOpening` → `Candidate` → `Application` → `Interview` → `Offer` → hire → `Employee` + `OnboardingProcess`.
- Approval engine, notifications, analytics, documents.

## Gaps for Phase 3C6 (performance/talent)
- No goals/OKR entity.
- No performance review/appraisal.
- No competency/skill model.
- No 1:1 / feedback.
- No talent matrix / succession.
- No learning/training (LMS).
- No career development plans.

## Recommended Phase 3C6
PERFORMANCE MANAGEMENT + GOALS/OKR + COMPETENCY + FEEDBACK + TALENT + LEARNING/TRAINING + CAREER DEVELOPMENT.

## Do NOT start yet
Performance, talent, learning, AI, RAG, OCR.
