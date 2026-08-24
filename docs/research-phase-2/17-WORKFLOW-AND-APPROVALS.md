# 17 — Workflow & Approvals + 18-22 — PM Research

> Research date: 2026-08-21. Sources: Stateless (GitHub, VERIFIED), Hangfire (VERIFIED), OWASP.

## Approval engine research

### Current state
LaoHR hardcodes approval flows:
- Leave: `LeaveRequest.Status` (PENDING → APPROVED/REJECTED) via `POST /api/leave/{id}/approve|reject`.
- Expense: same pattern.
- Loan: same pattern + activate/cancel.

No reusable engine. No multi-step, delegation, escalation, or SLA.

### Options compared

| Option | Complexity | Persistence | LaoHR fit | Confidence |
|---|---|---|---|---|
| **Stateless** (state machine library) | 🟢 Low | External (store state in entity) | ✅ Best fit | VERIFIED |
| **Hangfire** (background jobs) | 🟢 Low–Med | SQL/Redis | ✅ For reminders/scheduled steps | VERIFIED |
| **Quartz.NET** | 🟡 Medium | ADO.NET | ✅ Clustered cron jobs | MEDIUM |
| **Elsa Workflows** | 🔴 High | EF Core | ⚠️ Overkill | LOW |
| **Camunda** | 🔴 Very High | JVM/SaaS | ❌ Overkill | LOW |
| **MassTransit** | 🟡 Med–High | RabbitMQ/etc | ⚠️ Only if messaging adopted | MEDIUM |

### Verdict: Stateless + Hangfire (HIGH confidence)

For HR approvals (leave, expense, loan, attendance correction, onboarding steps):
- State space is small and enumerable.
- Approvals are per-entity, short-lived.
- **Stateless** (per-entity state machine, state stored in entity `Status` column) + **Hangfire** (reminders, auto-escalation, scheduled steps) is the right pairing.
- No extra infrastructure. No BPM engine. Store state in existing Postgres.
- A full BPM is justified ONLY if: business users need runtime-editable workflows, dozens of cross-cutting processes, or BPMN compliance. **None apply to LaoHR.**

### Recommendation
- **P1**: Adopt `Stateless` library for leave/expense/loan state machines (formalize existing hardcoded flows).
- **P2**: Add `Hangfire` for: "remind approver after 3 days", "auto-escalate to HR after 7 days", "auto-approve timesheets after 7 days".
- **P3**: Configurable approval chains (multi-step) only if customer demand.

## Project management research (18-22)

### Current PM layer
LaoHR has: Project, ProjectMember, Milestone, ProjectTask, TaskAssignee, TaskComment, ActivityLog, Risk, Issue, IssueComment, Resource, EntityComment.

### What's essential vs valuable vs overengineering

| Feature | Classification | Priority | Evidence |
|---|---|---|---|
| Kanban board | ESSENTIAL | P1 | Already done (frontend) |
| Task dependencies (FS/SS/FF/SF) | VALUABLE | P2 | Schedule graph |
| Gantt | VALUABLE | P2 | After dependencies |
| Timeline | VALUABLE | P2 | |
| Critical path | OVERENGINEERING (initially) | P3 | Audit: "DO NOT BUILD initially" |
| Baseline | VALUABLE | P2 | Variance reporting |
| Workload view | VALUABLE | P2 | After timesheet |
| Resource capacity | VALUABLE | P2 | |
| RAID log | VALUABLE | P2 | Risk + Issue already exist; add Assumptions/Decisions |
| Decision log | VALUABLE | P2 | |
| Project health | VALUABLE | P2 | (status + risk + schedule) |
| Portfolio | VALUABLE | P2 | Project list exists; add exec view |
| Templates | OVERENGINEERING | P3 | |
| Sprint/Iteration | OVERENGINEERING | P3 | Audit: "DO NOT BUILD" |
| Epic/Story/Subtask hierarchy | VALUABLE | P2 | |

### Kanban research (19)
Studied: Jira, Linear, GitHub Projects, Azure DevOps, Trello.
- Board → Columns (status) → WIP limits → Swimlanes → Drag/drop → Filters → Backlog.
- **LaoHR already has a Kanban board** (`projects/[id]/page.tsx` board tab). Drag/drop not confirmed.
- **GAP**: No WIP limits, no swimlanes, no backlog view. **P2** — enhance if PM layer is actively used.

### Gantt (20)
- Requires: start, end, duration, dependencies, milestones, progress, baseline.
- **LaoHR data model**: `ProjectTask` has `DueDate` but no `StartDate`, no `Duration`, no dependencies.
- **GAP**: Add `StartDate`, `EstimatedHours` to `ProjectTask`; add `TaskLink` (dependencies). Then build lightweight CSS-grid Gantt. **P2.** No heavy Gantt library.

### Resource planning (21)
- **LaoHR**: `Resource` entity (role, allocation %, start/end). `ResourcesController` + frontend.
- **GAP**: No capacity calculation (sum of allocations vs available hours), no utilization view, no forecasting. **P2** — after timesheet.

### Risk & issue (22)
- **LaoHR**: `Risk` (prob, impact, score, mitigation, status) + `Issue` (severity, status, related task). Both done.
- **GAP**: No risk matrix visualization (SVG), no RAID log (Assumptions + Decisions missing). **P2.**