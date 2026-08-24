# 23 — Phase 3C5 Handoff

## PM feature matrix

| Capability | API | UI | Auth | Tests | Status |
|---|---|---|---|---|---|
| Project Workspace | `/api/projects` | `/projects/[id]` | ProjectAccess | Yes | PASS |
| Task List | `/api/projects/{id}/projecttasks` | list view | ProjectAccess | Yes | PASS |
| Task Hierarchy | `ParentTaskId` | — | ProjectAccess | Yes | PASS |
| Kanban | `/api/pm/projects/{id}/board` | board view | ProjectAccess | Yes | PASS |
| Dependencies | `/api/pm/projects/{id}/dependencies` | — | ProjectAccess | Yes | PASS |
| Gantt | `/api/pm/projects/{id}/timeline` | — | ProjectAccess | Yes | PASS |
| Milestones | `/api/projects/{id}/milestones` | — | ProjectAccess | Yes | PASS |
| Resource Planning | `/api/resources` | — | ProjectAccess | Yes | PASS |
| Capacity | `/api/pm/capacity` | `/capacity` | Any | Yes | PASS |
| Portfolio | `/api/pm/portfolio` | `/portfolio` | Any | Yes | PASS |
| Risks | `/api/projects/{id}/risks` | — | ProjectAccess | Yes | PASS |
| Issues | `/api/projects/{id}/issues` | — | ProjectAccess | Yes | PASS |
| Assumptions | `/api/pm/projects/{id}/assumptions` | — | ProjectAccess | Yes | PASS |
| Decisions | `/api/pm/projects/{id}/decisions` | — | ProjectAccess | Yes | PASS |
| Activity | `/api/projects/{id}/activities` | activity tab | ProjectAccess | Yes | PASS |
| My Tasks | `/api/my-tasks` | `/my-tasks` | Self | Yes | PASS |
| PM Dashboard | `/api/analytics/pm` | — | Any | Yes | PASS |

## Current employee lifecycle (for Phase 3C5)
- `Employee` (core HR), `Position` (slot), `Department` (hierarchy), `WorkLocation`.
- `AppUser` ↔ `Employee` link (optional).
- Approval engine (Phase 3C1/3C2) for leave/expense/loan/attendance.
- `EmployeeDocument` (documents), `NotificationService` (notifications).

## Recruitment/onboarding gaps (Phase 3C5)
- No job requisition entity.
- No candidate/ATS pipeline.
- No interview/offer workflow.
- No onboarding checklist.

## Recommended Phase 3C5
RECRUITMENT + ATS + CANDIDATE PIPELINE + JOB REQUISITION + INTERVIEW + OFFER + ONBOARDING.

## Do NOT start yet
Recruitment, onboarding, performance, learning, AI, RAG, OCR.
