# 03 — PM / PL Review

A walkthrough of the application as each operational role would experience it
**today**, with explicit "what's missing" notes. No implementation advice here
— that's in `11-implementation-plan.md`.

## 1. Employee

**What works**

* Can log in and see their dashboard (`/`).
* Can clock in/out (`/attendance`).
* Can see their leave balances and request leave (`/leave`).
* Can view their own payroll slips if filtered by `employeeId`.
* Can see their profile in `/settings`.

**What's missing**

* **My Tasks** — there is no task list. They have nothing to do today.
* **My Timesheet** — actual hours cannot be recorded.
* **My Calendar** — there's a leave calendar for HR but no personal calendar
  combining attendance + leave + (someday) tasks.
* **My Notifications** — no notification bell, no unread count.
* **My Mentions** — comments and mentions don't exist.
* **My Approvals** — an employee who is also an approver has no inbox.
* **My Payslip inbox** — payslips are downloadable but there's no
  notification or per-period list.

**Pain points**

* "What do I need to do today?" — answer requires opening Excel.
* "How much leave do I have left?" — must visit the leave page and remember
  three balance cards.
* "Was my leave approved?" — must refresh the leave page; no email / push.

## 2. Project Manager / Project Leader

This role does **not exist** in the system today. The codebase contains no
Project, Task, Milestone, Risk, or Gantt concept. A PM trying to deliver a
project through this product will:

* Open Excel for the WBS.
* Open Jira / Linear / MS Project for scheduling.
* Open WhatsApp / Teams for collaboration.
* Use this app only for **HR forms** (leave, attendance, payroll).

**What's missing (all P0/P1)**

* Project workspace: scope, owner, sponsor, dates, budget, status.
* Tasks with assignees, due dates, statuses, tags, dependencies.
* Milestones with planned/actual dates.
* Gantt / Timeline.
* Workload view: who is over-allocated this week?
* Risk register: probability × impact, mitigations, owners.
* Issues / blockers: who needs to act?
* Change requests: what changed and why?
* Project dashboard: health, overdue items, blockers, recent activity.
* Project-level RBAC: who can see / edit what on this project.
* Project documents: where do we put deliverables?
* Project wiki / knowledge: where do we put decisions?

## 3. Department Manager

**What works**

* Sees all employees in their department via the filter (`?departmentId=`).
* Can run payroll if HR role.
* Approves leave (HR/Admin).

**What's missing**

* **Team capacity** — who's over-allocated? who's under-utilized?
* **Team utilization** — actual worked hours vs contracted hours.
* **Team KPI** — completion rate, leave taken, late arrivals.
* **Team documents** — no place for departmental SOPs.
* **Team skills coverage** — what skills are missing in the team?

## 4. Finance

**What works**

* Runs payroll per period.
* Downloads NSSF report.
* Manages conversion rates.

**What's missing**

* **Project profitability** — what did this project cost vs what it earned?
* **Project budget** — was the project within budget?
* **Cost allocation** — how much of payroll hit project X?
* **Variance reporting** — forecast vs actual by month.
* **Cash flow view** — payroll run cost + bank transfer status.
* **Audit trail** beyond audit log: who approved what financial action.

## 5. HR

**What works**

* Most of the application — HR is the primary persona.

**What's missing**

* **Onboarding workflow** for new hires.
* **Offboarding workflow** for leavers.
* **Contract management** with renewal alerts.
* **Training records** — what training has each employee completed?
* **Performance review** — annual review cycle.
* **Compensation history** — when did salary last change?
* **Recruitment pipeline** — open positions → candidates → hires.

## 6. Executive

**What works**

* Single dashboard with 4 stat counters (employees, active, present, pending
  leave).

**What's missing**

* **Portfolio health** — projects by status, by department, by priority.
* **Revenue / cost** trend.
* **Utilization** trend.
* **Delivery risk** — projects likely to slip.
* **Headcount trend** — growth, attrition.
* **Decisions pending** — approvals waiting.
* **Cross-company roll-up** (when multi-org).

## 7. Administrator

**What works**

* Settings pages for company, work schedule, holidays, leave policies,
  conversion rates.
* License activation.
* Audit logs (read-only).

**What's missing**

* **User management** — there's no UI to create/edit AppUsers.
  (`SeedDefaultAdminAsync` is the only path.)
* **Role management** — roles are hard-coded strings.
* **Permission management** — the matrix is hard-coded in
  `lib/permissions.ts`.
* **Workflow configuration** — approval policies, task statuses.
* **Integration setup** — Calendar, Teams, etc.
* **Backup / restore** controls.
* **Tenant / Organization** management (if multi-company ever needed).

## 8. Cross-cutting operational gaps

| Question a PM/PL can ask        | Answer today                              |
| -------------------------------- | ----------------------------------------- |
| What are we doing?               | Open Excel.                               |
| Who owns it?                     | Open Excel.                               |
| When is it due?                   | Open Excel.                               |
| What is blocked?                 | Open Excel.                               |
| Are we late?                     | Open Excel.                               |
| Are we over budget?              | Open Excel.                               |
| Who is overloaded?               | Open Excel.                               |
| What changed?                    | Read the AuditLogController, sort manually.|
| What requires my attention?      | Email + memory.                           |
| Why did we make that decision?   | No record.                                |
| What's our capacity next week?   | Open Excel.                               |

**The product cannot answer any project-management question today.** This is
the single largest gap.

## 9. Where users are forced to leave the product

| Need today                              | External tool used  |
| --------------------------------------- | ------------------- |
| Plan a project                          | Excel / MS Project  |
| Track tasks                             | Jira / Linear / Trello |
| Schedule / dependencies / Gantt        | MS Project / Smartsheet |
| Communicate on a work item             | Teams / WhatsApp / Slack |
| Risk register                           | Excel               |
| Issue / bug tracking                    | Jira / Linear       |
| Project wiki / decisions                | Notion / Confluence |
| Customer / contract docs                | SharePoint / Drive  |
| Cross-project portfolio view            | Power BI / Tableau  |
| Calendar (Outlook)                      | Outlook / Google Cal |
| Code / repo link                        | GitHub (and manual copy-paste) |
| Time tracking                           | Toggl / Harvest     |

This is the **feature parity table the product must close** to stop the
"context-switch tax." But the prompt is explicit: *do not clone these tools*.
We must keep the surface area small.

## 10. Council verdict on operational coverage

* **Employee** — partially served. Add My Tasks + Notifications.
* **PM/PL** — entirely absent. **Largest single investment required.**
* **Department Manager** — partial. Add capacity + utilization.
* **Finance** — partial. Add project cost + variance.
* **HR** — mostly served.
* **Executive** — barely served. Needs portfolio.
* **Admin** — partial. Needs user management + workflow config.
