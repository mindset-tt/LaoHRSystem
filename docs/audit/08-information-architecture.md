# 08 — Information Architecture

## 1. Current IA

```
LaoHR
├── Login
└── Dashboard
    ├── /
    │   └── Stats: Employees, Active, Present Today, Pending Leave
    │   └── Quick actions: Add employee, View attendance, Manage leave, Run payroll
    │   └── Mocked "Recent activity"
    ├── /employees
    │   ├── /employees              list + filter
    │   ├── /employees/new          create
    │   ├── /employees/[id]         detail (tabs: personal, employment, documents)
    │   └── /employees/[id]/edit    edit
    ├── /attendance                calendar/list, clock in/out
    ├── /leave                     tabs: my-leave, approvals, calendar
    ├── /payroll                   periods + slips + export
    ├── /reports                   NSSF download
    └── /settings
        ├── /settings              profile + (disabled) theme/language
        ├── /settings/company
        ├── /settings/work-schedule
        ├── /settings/holidays
        ├── /settings/leave
        └── /settings/currency-rates
```

## 2. Current IA — Pain Points

* **Single shared dashboard** for HR, PM, and Employee.
* **No "My Work"** — employees can't see tasks, timesheets, approvals.
* **No global search or command palette** — sidebar is the only navigator.
* **No notifications** — must refresh pages.
* **No breadcrumbs** (except on `/employees/new`).
* **No mobile navigation** — sidebar will overflow at small widths.
* **No recent / favorites / pinned** — sidebar is static.

## 3. Proposed IA (derived from PM/PL needs, not from "what Jira has")

### 3.1 Group: My Work

For every user (every role).

| Route                        | Purpose                                | Audience          |
| ---------------------------- | -------------------------------------- | ----------------- |
| `/my`                        | Personal home: today's tasks, timesheet, leave, approvals | All |
| `/my/tasks`                  | Tasks assigned to me (across projects) | All               |
| `/my/timesheet`              | Daily/weekly hours, submit for approval | All               |
| `/my/leave`                  | Leave balance + history + request       | All               |
| `/my/attendance`             | Personal attendance + clock in/out      | All               |
| `/my/approvals`              | Inbox for approvers                    | HR/PM/Admin       |
| `/my/notifications`          | Full notification history              | All               |

### 3.2 Group: Projects (PM/HR/Admin only by default)

| Route                                  | Purpose                          |
| -------------------------------------- | -------------------------------- |
| `/projects`                            | Portfolio + filter + sort        |
| `/projects/new`                        | Create                           |
| `/projects/[id]`                       | Workspace overview               |
| `/projects/[id]/board`                 | Kanban                           |
| `/projects/[id]/list`                  | Task list                        |
| `/projects/[id]/calendar`              | Calendar (tasks)                 |
| `/projects/[id]/timeline`              | Timeline                         |
| `/projects/[id]/gantt`                 | Gantt                            |
| `/projects/[id]/milestones`            | Milestones                       |
| `/projects/[id]/team`                  | Members, allocation              |
| `/projects/[id]/risks`                 | Risk register                    |
| `/projects/[id]/issues`                | Issue / blocker list             |
| `/projects/[id]/budget`                | Budget, cost, revenue, margin    |
| `/projects/[id]/wiki`                  | Project wiki                     |
| `/projects/[id]/documents`             | Project docs                     |
| `/projects/[id]/activity`              | Activity feed                    |
| `/projects/[id]/settings`              | Project settings + workflow      |

### 3.3 Group: People (HR/Admin primary)

| Route                     | Purpose                                |
| ------------------------- | -------------------------------------- |
| `/people`                 | Employees list                         |
| `/people/[id]`            | Profile + tabs                         |
| `/people/skills`          | Skill matrix                           |
| `/people/capacity`        | Capacity by week/month                 |
| `/people/utilization`     | Utilization trends                     |
| `/people/reports`         | Org chart + reporting line             |

### 3.4 Group: Time

| Route                          | Purpose                                 |
| ------------------------------ | --------------------------------------- |
| `/time/attendance`             | Org-wide attendance                     |
| `/time/leave`                  | Org-wide leave requests                 |
| `/time/holidays`               | Holiday calendar                        |
| `/time/work-schedule`          | Schedule config (already under settings)|
| `/time/overtime`               | Overtime register                       |

### 3.5 Group: Finance (HR/Finance/Admin)

| Route                          | Purpose                                 |
| ------------------------------ | --------------------------------------- |
| `/finance/payroll`             | Periods, slips, export (moved here)     |
| `/finance/payroll/[id]`        | Period detail                           |
| `/finance/nssf`                | NSSF reports                            |
| `/finance/budgets`             | Project budgets overview                |
| `/finance/reports`             | Financial reports                       |

### 3.6 Group: Knowledge (all)

| Route                  | Purpose                                |
| ---------------------- | -------------------------------------- |
| `/knowledge/documents` | All docs                               |
| `/knowledge/wiki`      | Wiki landing                           |
| `/knowledge/wiki/[id]` | Wiki page                              |

### 3.7 Group: Reports

| Route                       | Purpose                              |
| --------------------------- | ------------------------------------ |
| `/reports`                  | Operational dashboards + saved       |
| `/reports/formal/[id]`      | Formal status reports                |

### 3.8 Group: Admin

| Route                          | Purpose                              |
| ------------------------------ | ------------------------------------ |
| `/admin/users`                 | User management                      |
| `/admin/roles`                 | Role management                      |
| `/admin/permissions`           | Permission matrix                    |
| `/admin/workflows`             | Approval policies                    |
| `/admin/audit`                 | Audit log viewer                     |
| `/admin/settings`              | Company, work schedule, leave, currencies |
| `/admin/license`               | License activation                   |
| `/admin/integrations`          | Future: integrations                 |

### 3.9 Group: System

| Route         | Purpose                |
| ------------- | ---------------------- |
| `/403`        | Permission denied      |
| `/404`        | Not found              |
| `/500`        | Server error           |
| `/healthz`    | Public health check    |

## 4. Navigation Components

### 4.1 Sidebar

Top-level groups (collapsed/expanded state persisted):

```
▾ My Work
  • Home
  • Tasks
  • Timesheet
  • Leave
  • Attendance
  • Approvals
  • Notifications

▾ Projects
  • Portfolio
  • + New project

▾ People
  • Employees
  • Skills
  • Capacity

▾ Time
  • Attendance
  • Leave
  • Holidays

▾ Finance
  • Payroll
  • Budgets
  • NSSF

▾ Knowledge
  • Documents
  • Wiki

▸ Reports

▸ Admin
```

* Items hidden by permission.
* Collapsed state persists per user.

### 4.2 Top Bar

* Breadcrumbs (derived from route)
* **Search button** → opens `Cmd+K` palette
* **Theme toggle** (Light / Dark / System)
* **Language toggle** (en / lo)
* **Notifications bell** with unread count
* **Avatar / user menu** (profile, sign out)

### 4.3 Mobile Drawer

* Slide-out navigation at `<md`.
* Same items as sidebar.
* Backdrop close on click.

## 5. Command Palette

`Cmd/Ctrl + K` opens a centered palette with:

* Recent items (max 5)
* Global search across Projects / Tasks / People / Documents / Wiki
* Quick actions: "Create task", "Submit timesheet", "Apply for leave", …
* Fuzzy match with debounced server search (300 ms)

## 6. Breadcrumbs

A `<PageHeader>` primitive renders:

```
Dashboard / Projects / ABC Delivery / Board
```

* Each segment is a link (except the last).
* Right side: page actions (e.g., "New task", "Export").

## 7. Empty States

* "No projects yet — Create your first project" with CTA.
* "No tasks assigned to you — Relax, or pick from the backlog."
* "You have no notifications."
* "Your timesheet is empty — log your hours."

All `<EmptyState/>` primitives share one shape: icon, title, description,
primary CTA.

## 8. Saved Views

* "Filter chips" with a save button.
* Saved views appear in:
  * Top of the list page (chips)
  * Sidebar (Recent / Starred)
  * As URL bookmarks

## 9. Page Header Standard

Every page renders a `<PageHeader>` with:

* Title (i18n)
* Subtitle (i18n)
* Breadcrumbs (above)
* Page actions (right side)
* Optional "saved view" picker

## 10. Routing Conventions

* Singular nouns (`/employee/[id]` → not `/employees/[id]`). Existing app uses
  plural — keep plural for compatibility during migration, then unify.
* `[id]/edit` for editable resources.
* `?from=&to=&filter=` in URL for filtered views.
* `/api/v1/...` for backend versioning.

## 11. Permissions → IA Gating

| Group     | Employee | HR | PM | Admin |
| --------- | -------- | -- | -- | ----- |
| My Work   | ✓        | ✓  | ✓  | ✓     |
| Projects  | (read assigned) | ✓ | ✓ | ✓ |
| People    | self only | ✓  | (own team) | ✓ |
| Time      | self only | ✓  | ✓  | ✓     |
| Finance   | (own slips) | ✓  | (own project budget) | ✓ |
| Knowledge | ✓        | ✓  | ✓  | ✓     |
| Reports   | (basic)  | ✓  | ✓  | ✓     |
| Admin     |          |    |    | ✓     |
