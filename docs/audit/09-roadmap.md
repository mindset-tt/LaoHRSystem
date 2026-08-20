# 09 — Roadmap

> Phases are derived from evidence (current gaps, value, risk), not from a
> fashionable agile template. Each phase ships as **vertical slices** that
> return user value end-to-end.

## Phase 0 — Stabilization (1–2 weeks)

Goal: stop the bleeding. Fix correctness, security, and runtime hygiene
**without** introducing new features.

| Item | Why | Depends on | Complexity | Value |
| ---- | --- | ---------- | ---------- | ----- |
| Replace `EnsureCreated` with `Migrate` | Schema drift | none | LOW | HIGH |
| Move audit write to fire-and-forget | Avoid rollback of user ops | none | MED | HIGH |
| Add `[Authorize]` defaults | Public-by-default is unsafe | none | LOW | HIGH |
| Remove committed DB credentials | Secret leak | none | LOW | HIGH |
| Lock CORS to known origins | Open credentialed CORS | none | LOW | HIGH |
| Move admin seed out of `AuthController` | Wasteful + race | none | LOW | MED |
| Add pagination envelopes to list endpoints | OOM risk | none | MED | HIGH |
| Add indexes on `Attendance(AttendanceDate)`, `SalarySlip(PeriodId)`, `LeaveRequest(StartDate)`, `Employee(IsActive, DepartmentId)` | Slow queries | Migration | LOW | HIGH |
| Cache license key (5 min) | Hot path DB hit | none | LOW | MED |
| Disable mock-data fallback in frontend | Misleading UX | none | LOW | MED |
| Add health checks (`/health`) | Operability | none | LOW | MED |
| Add Serilog JSON sink | Observability | none | LOW | MED |

## Phase 1 — Design System (1 week)

Goal: one set of tokens, one set of primitives.

| Item | Why | Depends on | Complexity | Value |
| ---- | --- | ---------- | ---------- | ----- |
| Define `:root` tokens (color, space, radius, shadow, type) | Single source | none | LOW | HIGH |
| Add `ThemeProvider` + light/dark/system | Required by users | tokens | LOW | HIGH |
| Add `<DataTable>` (sort, filter, paging, virtualization) | Lists everywhere | tokens | MED | HIGH |
| Add `<Toast>`, `<EmptyState>`, `<ErrorState>` | Consistent UX | tokens | LOW | MED |
| Add `<PageHeader>` with breadcrumbs | Consistency | tokens | LOW | MED |
| Add `<Icon>` primitive | Reuse | tokens | LOW | MED |
| Add `<MobileNav>` | Responsive | tokens | LOW | MED |
| Adopt react-hook-form + Zod + shared `<FormField>` | Forms everywhere | tokens | LOW | MED |
| Replace hard-coded Lao strings with i18n keys | i18n completeness | tokens | LOW | MED |
| Wire `/settings` theme & language selectors | Currently fake | ThemeProvider | LOW | MED |

## Phase 2 — Project Workspace Foundation (3–4 weeks)

Goal: introduce Project / Milestone / Task / ProjectMember / Tag — the core
of the new product layer.

### Data

* `Project` (code, name, owner, sponsor, status, priority, start/end, budget,
  currency, dept, health, parent)
* `ProjectMember` (projectId, employeeId, role, allocationPct, start/end)
* `Milestone` (projectId, name, due, owner, status, completedAt)
* `Tag` (projectId, name, color)
* `Task` (projectId, parentTaskId?, milestoneId?, title, description, statusId,
  priority, reporterId, assigneeId?, start?, due?, estimateMin?, spentMin?,
  type)
* `TaskLink` (fromTaskId, toTaskId, type FS/SS/FF/SF)
* `TaskAssignee` / `TaskWatcher` (optional split from a single `Assignees`
  table for many-to-many)

### API

* `ProjectsController`, `MilestonesController`, `TasksController`,
  `TaskLinksController`, `TagsController`, `ProjectMembersController`
* All list endpoints use the pagination envelope.
* All write endpoints audit + broadcast activity.

### Frontend

* `/projects` (portfolio list)
* `/projects/[id]` workspace with tabs: Overview, Team, Milestones, Activity
* `/projects/[id]/list` (ListView over Tasks)
* `/projects/[id]/board` (Kanban — drag/drop via `@dnd-kit`)
* `/projects/[id]/calendar` (CalendarView over Tasks)
* `/projects/[id]/timeline` (TimelineView)
* `/projects/[id]/gantt` (lightweight CSS-grid Gantt)
* `/projects/[id]/milestones` (CRUD)
* `/projects/[id]/settings` (statuses, priorities, tags)

### My Work

* `/my` (combined widget home: today's tasks, upcoming milestones, pending
  approvals)
* `/my/tasks` (filterable list across projects)

### Cross-cutting

* `Cmd+K` command palette with project / task / people search
* Notification model + `/my/notifications`
* Activity event per write
* Project-level RBAC policies

## Phase 3 — Risk, Issue, Resource (2 weeks)

Goal: let PMs see health, blockers, capacity.

### Data

* `Risk` (projectId, title, prob, impact, score, owner, mitigation, status,
  reviewDate)
* `Issue` (projectId, title, severity, status, owner, relatedTaskId?,
  resolvedAt)
* `Skill` + `EmployeeSkill`
* `TimesheetEntry` (employeeId, date, taskId, minutes, note, status,
  approverId?)

### API

* `RisksController`, `IssuesController`, `SkillsController`,
  `TimesheetController`

### Frontend

* `/projects/[id]/risks` (matrix + list)
* `/projects/[id]/issues` (inbox-style board)
* `/people/capacity` (heatmap)
* `/people/skills` (matrix)
* `/my/timesheet` (week grid)
* `/time/overtime` (register)

## Phase 4 — Finance Extension (2 weeks)

Goal: project cost, budget, revenue, margin.

### Data

* `Budget` (projectId, category, planned)
* `ActualCost` (projectId, source [Timesheet/Purchase/Manual], amount, date)
* `Revenue` (projectId, milestoneId?, amount, date)

### API

* `BudgetsController`, `ActualCostsController`, `RevenuesController`
* `ProjectReportsController` for portfolio roll-ups

### Frontend

* `/projects/[id]/budget` (planned vs actual, variance)
* `/finance/budgets` (cross-project)
* Portfolio dashboard widgets: total revenue, margin, variance

## Phase 5 — Knowledge & Collaboration (1–2 weeks)

Goal: documents + wiki + comments + mentions.

### Data

* `Document` (projectId?, folderId?, ownerId, version, status, mime, size)
* `WikiPage` (projectId?, parentId?, title, body, lastReviewedAt)
* `Comment` (parentType, parentId, authorId, body)
* `Mention` (commentId, userId)

### Frontend

* `/projects/[id]/documents` + `/projects/[id]/wiki`
* `/knowledge/*`
* Inline comments on tasks / risks / issues

## Phase 6 — Operations (2 weeks)

Goal: observability, deployment, ops.

* OpenTelemetry + Serilog + Sentry
* `/health/live`, `/health/ready`
* `Dockerfile.api`, `compose.yml`, GitHub Actions
* Audit log retention policy + archival
* Rate limiting (`/api/auth/login`)
* Refresh tokens persisted + revocable
* File storage abstraction (local → S3)
- Backup / restore runbook

## Phase 7 — Advanced (post P6, only on evidence)

* Saved views (personal + shared)
* Project templates
* Approval policies UI
* Charts library expansion
* Webhooks
* Calendar integration
* Slack / Teams deep links
* Multi-tenant (only if business case emerges)
* Mobile PWA

## P4 — Reject / Defer

* Critical-path auto-compute (too complex; visual proxy enough)
* Visual automation (Zapier-style) — overkill
* Built-in chat / IM — Slack/Teams already exist
* Wiki-as-Notion (rich-text editor) — keep markdown + simple links
* Customer-facing portal — separate product

## Sequencing Rationale

* Phase 0 buys safety; cannot be skipped.
* Phase 1 buys consistency; everything else looks better with tokens.
* Phase 2 is the **single largest** capability gap. Without it, the product is
  still "HR only."
* Phase 3 unlocks real PM workflows (risk + capacity).
* Phase 4 unlocks real finance workflows (margin).
* Phase 5 makes it collaborative.
* Phase 6 makes it operable.
* Phase 7 only on demand.

## Cost vs Value Summary

| Phase | Effort (weeks) | New user value |
| ----- | -------------- | -------------- |
| 0     | 1–2            | Operational    |
| 1     | 1              | UX              |
| 2     | 3–4            | **Massive**     |
| 3     | 2              | High            |
| 4     | 2              | High            |
| 5     | 1–2            | Medium          |
| 6     | 2              | Operational     |
| 7     | varies         | Conditional     |
