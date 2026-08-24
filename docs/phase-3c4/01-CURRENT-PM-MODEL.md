# 01 — Current PM Model

Audit of the existing PM domain before Phase 3C4 changes.

| Entity | Purpose | Key fields | Relationships | Status values |
|---|---|---|---|---|
| Project | Top-level project | Code, Name, Description, Status, Priority, Color, StartDate, DueDate, CompletedAt, OwnerId | Owner→Employee, Members, Milestones, Tasks, Activities | PLANNING, ACTIVE, ON_HOLD, COMPLETED, CANCELLED |
| ProjectMember | Project↔Employee membership | ProjectId, EmployeeId, Role, JoinedAt | Project, Employee | OWNER, LEAD, MEMBER, VIEWER |
| Milestone | Zero-duration checkpoint | ProjectId, Name, Description, DueDate, CompletedAt, Status | Project, Tasks | OPEN, COMPLETED, CANCELLED |
| ProjectTask | Unit of work | ProjectId, MilestoneId, TaskNumber, Title, Description, Status, Priority, StartDate, DueDate, CompletedAt, ProgressPercent, EstimatedHours, ActualHours, ReporterId, SortOrder | Project, Milestone, Reporter, Assignees, Comments | TODO, IN_PROGRESS, BLOCKED, REVIEW, DONE, CANCELLED |
| TaskAssignee | Task↔Employee assignment | TaskId, EmployeeId, Role, AssignedAt | Task, Employee | ASSIGNEE, REVIEWER, WATCHER |
| TaskComment | Task comment thread | TaskId, AuthorId, Body, ParentCommentId | Task, Author, Parent | — |
| ActivityLog | Append-only activity | ProjectId, TaskId, ActorId, Action, PayloadJson | Project, Task, Actor | CREATED, UPDATED_STATUS, ASSIGNED, COMMENTED, COMPLETED, MEMBER_ADDED, ... |
| Risk | Project risk | ProjectId, Title, Description, Priority, Likelihood, Impact, Status, Mitigation, OwnerId, DueDate | Project, Owner | OPEN, MITIGATING, CLOSED, ACCEPTED |
| Issue | Ad-hoc blocker | ProjectId, TaskId, Title, Description, Status, Priority, ReporterId, AssigneeId, DueDate, ResolvedAt | Project, Task, Reporter, Assignee, Comments | TODO, IN_PROGRESS, BLOCKED, REVIEW, DONE, CANCELLED |
| Resource | Employee↔project allocation | ProjectId, EmployeeId, Role, AllocationPercent, StartDate, EndDate, Notes | Project, Employee | LEAD, CONTRIBUTOR, REVIEWER, ADVISOR |

## Missing capabilities (gaps)
- Task hierarchy (ParentTaskId) — absent.
- Task dependencies — absent.
- Kanban board endpoint (columns/ordering/move) — absent (SortOrder exists).
- Gantt/timeline endpoint — absent.
- Resource capacity/workload/overallocation — absent.
- RAID Assumptions + Decisions — absent (only Risks + Issues).
- Project health model — absent.
- Portfolio endpoint — absent (only project list).
- PM authorization service — scattered role checks.

## Duplicate concepts
- `ProjectMember` (membership) vs `Resource` (allocation) overlap but serve different purposes (membership vs capacity). Kept separate intentionally.
- `Risk.Score` (Likelihood×Impact) is derived, not stored — correct.

## Technical debt
- `GetCurrentEmployeeId()` duplicated across controllers (username lookup) — superseded by `ICurrentEmployeeService`/`IProjectAccessService`.
- `ReporterId ?? 1` fallback in task create — should resolve server-side.
