# 00 — Phase 3C4 Baseline

Recorded before Phase 3C4 modifications.

## Backend
- Build: PASS (0 errors, 5 pre-existing warnings)
- Tests: 120 passed / 0 failed

## Frontend
- Typecheck: PASS
- Tests: 23 passed / 0 failed
- Production build: PASS
- Lint: 41 errors, 38 warnings (pre-existing)

## Migration chain
`InitialCreatePostgres` → `AddApprovalEssMssNotifications`

## Git
Branch `master`, substantial uncommitted work.

## Existing PM domain (audited)
- `Project` (status, priority, dates, owner), `ProjectMember` (role), `Milestone`, `ProjectTask` (status, priority, progress, hours, SortOrder), `TaskAssignee`, `TaskComment`, `ActivityLog`, `Risk` (likelihood/impact/score), `Issue` (+ comments), `Resource` (allocation %).
- Controllers: Projects, ProjectTasks (incl. `/api/my-tasks`), Milestones, Risks, Issues, Resources.
- Frontend: `/projects`, `/projects/[id]` (+ tasks/risks/issues/resources subpages), `/my-tasks`.

## Missing capabilities (gaps to close)
- Task hierarchy (ParentTaskId) — absent.
- Task dependencies — absent.
- Kanban board endpoint (columns/ordering/move) — absent (SortOrder exists).
- Gantt/timeline endpoint — absent.
- Resource capacity/workload/overallocation — absent.
- RAID Assumptions + Decisions — absent (only Risks + Issues).
- Project health model — absent.
- Portfolio endpoint — absent (only project list).
- PM authorization service — scattered role checks.
