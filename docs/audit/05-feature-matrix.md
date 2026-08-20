# 05 — Feature Matrix

> `| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |`

Legend:
* **Exists**: ✓ full / ◐ partial / ✗ missing / — N/A
* **Quality**: LOW / MEDIUM / HIGH (engineering quality of what's there)
* **Value** (to user): VERY_LOW / LOW / MEDIUM / HIGH / VERY_HIGH
* **Complexity** (to build correctly): LOW / MEDIUM / HIGH / VERY_HIGH
* **Runtime Cost** (browser/DB/API footprint): LOW / MEDIUM / HIGH
* **Decision**: `DO NOW` / `DO NEXT` / `LATER` / `DO NOT BUILD`
* **Priority**: P0 / P1 / P2 / P3 / P4

## A. Work Management

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| Project entity & workspace | ✗ | — | VERY_HIGH | HIGH | MED | DO NOW | P0 |
| Task model (single source of truth) | ✗ | — | VERY_HIGH | HIGH | MED | DO NOW | P0 |
| Multiple task views (List / Board / Calendar / Timeline / Gantt) | ✗ | — | VERY_HIGH | VERY_HIGH | HIGH | DO NEXT (after Task model) | P1 |
| Kanban (Board) | ✗ | — | HIGH | MED | MED | DO NEXT | P1 |
| Gantt | ✗ | — | HIGH | HIGH | HIGH | DO NEXT | P1 |
| Timeline | ✗ | — | MED | MED | MED | LATER | P2 |
| Dependencies (FS/SS/FF/SF) | ✗ | — | MED | MED | LOW | DO NEXT | P1 |
| Critical Path | ✗ | — | LOW | HIGH | LOW | DO NOT BUILD initially | P4 |
| Milestones | ✗ | — | HIGH | LOW | LOW | DO NOW | P0 |
| Project baseline | ✗ | — | MED | MED | LOW | LATER | P2 |
| Project Portfolio (Executive) | ✗ | — | VERY_HIGH | MED | MED | DO NEXT | P1 |
| Risk register | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| Issue / Blocker | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| Change Requests | ✗ | — | MED | MED | LOW | LATER | P3 |
| Sprint / Iteration | ✗ | — | LOW | LOW | LOW | DO NOT BUILD | P4 |
| Epic / Story / Subtask hierarchy | ✗ | — | MED | MED | LOW | DO NEXT | P2 |
| Backlog | ✗ | — | MED | LOW | LOW | LATER | P2 |
| Tags / Labels | ✗ | — | MED | LOW | LOW | DO NOW (with Task) | P0 |
| Checklists | ✗ | — | MED | LOW | LOW | DO NEXT | P2 |
| Watchers / Subscribers | ✗ | — | LOW | LOW | LOW | LATER | P3 |

## B. People & Capacity

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| Employee CRUD | ✓ | HIGH | VERY_HIGH | LOW | LOW | KEEP | — |
| Department CRUD | ◐ | MED | HIGH | LOW | LOW | KEEP, complete nested CRUD | P1 |
| Profile photo upload | ✓ | MED | LOW | LOW | LOW | KEEP | — |
| Employee documents | ✓ | MED | HIGH | LOW | MED | KEEP, improve versioning | P1 |
| Skills | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| Reporting line | ✗ | — | MED | LOW | LOW | LATER | P2 |
| Timesheet | ✗ | — | VERY_HIGH | HIGH | MED | DO NOW | P0 |
| Resource / Capacity planning | ✗ | — | HIGH | HIGH | MED | DO NEXT | P1 |
| Workload view | ✗ | — | HIGH | MED | MED | DO NEXT | P1 |
| Attendance (clock in/out) | ✓ | HIGH | VERY_HIGH | LOW | LOW | KEEP | — |
| Geolocation capture | ✓ | HIGH | MED | LOW | LOW | KEEP | — |
| Leave request / approval | ✓ | HIGH | VERY_HIGH | LOW | LOW | KEEP | — |
| Leave balance + accrual | ✓ | HIGH | HIGH | LOW | LOW | KEEP | — |
| Half-day leave | ✓ | HIGH | MED | LOW | LOW | KEEP | — |
| Sick-leave attachment | ✓ | MED | MED | LOW | LOW | KEEP | — |
| Attendance calendar view | ◐ | MED | MED | LOW | LOW | IMPROVE | P2 |
| Overtime tracking | ✗ (UI uses PayrollAdjustment but no concept) | — | HIGH | MED | LOW | DO NEXT | P1 |
| Public holidays | ✓ | HIGH | HIGH | LOW | LOW | KEEP | — |
| Work schedule config | ✓ | HIGH | HIGH | LOW | LOW | KEEP | — |

## C. Finance

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| Payroll calculation | ✓ | HIGH | VERY_HIGH | HIGH | LOW | KEEP | — |
| NSSF + Lao PIT tax | ✓ | HIGH | HIGH | HIGH | LOW | KEEP | — |
| Multi-currency payroll | ✓ | HIGH | HIGH | MED | LOW | KEEP | — |
| Bank transfer files (BCEL/LDB) | ✓ | HIGH | HIGH | MED | LOW | KEEP | — |
| Payslip PDF | ✓ | HIGH | HIGH | MED | LOW | KEEP | — |
| Payroll Excel export | ✓ | HIGH | HIGH | MED | LOW | KEEP, but stream large periods | P1 |
| Dynamic adjustment columns | ✓ | HIGH | MED | MED | LOW | KEEP | — |
| NSSF form fill + ZIP | ✓ | HIGH | HIGH | MED | LOW | KEEP | — |
| Project Budget | ✗ | — | VERY_HIGH | MED | LOW | DO NEXT | P1 |
| Actual project cost | ✗ | — | VERY_HIGH | MED | LOW | DO NEXT | P1 |
| Revenue | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| Margin / Profitability | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| Forecast | ✗ | — | MED | HIGH | LOW | LATER | P3 |
| Variance reporting | ✗ | — | HIGH | MED | LOW | LATER | P2 |
| Cost allocation to project | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |

## D. Knowledge

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| Employee documents | ✓ | MED | HIGH | LOW | LOW | IMPROVE (versioning, preview) | P1 |
| Project wiki | ✗ | — | HIGH | MED | LOW | LATER | P2 |
| Document linking to project/task | ✗ | — | MED | LOW | LOW | LATER | P2 |
| Templates | ✗ | — | MED | LOW | LOW | LATER | P2 |
| Comments on work items | ✗ | — | HIGH | LOW | LOW | DO NEXT | P1 |
| @Mentions | ✗ | — | MED | LOW | LOW | DO NEXT | P1 |
| Activity feed | ✗ | — | MED | LOW | LOW | LATER | P2 |
| Search | ✗ (no global) | — | HIGH | HIGH | MED | DO NEXT | P1 |
| Command palette | ✗ | — | MED | MED | LOW | DO NEXT | P1 |

## E. Communication & Notifications

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| In-app notifications | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| Email notifications | ◐ (EmailService stub) | LOW | HIGH | LOW | LOW | DO NEXT (complete) | P1 |
| Notification preferences | ✗ | — | MED | LOW | LOW | LATER | P2 |
| Daily / weekly digest | ✗ | — | LOW | MED | LOW | LATER | P3 |

## F. Reporting

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| NSSF monthly report | ✓ | HIGH | HIGH | MED | LOW | KEEP | — |
| Payroll period export | ✓ | HIGH | HIGH | MED | LOW | KEEP | — |
| Excel exports (general) | ✗ | — | MED | MED | LOW | LATER (only when needed) | P2 |
| PDF payslip | ✓ | HIGH | HIGH | MED | LOW | KEEP | — |
| Charts in dashboard | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| Portfolio report | ✗ | — | HIGH | MED | LOW | LATER | P2 |

## G. Administration

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| Auth (login/refresh/me) | ✓ | HIGH | VERY_HIGH | MED | LOW | KEEP | — |
| Role-based permissions (3 hard roles) | ◐ | MED | HIGH | MED | LOW | IMPROVE (configurable) | P2 |
| User management UI | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| License activation | ✓ | HIGH | MED | LOW | LOW | KEEP | — |
| Audit logs (UI) | ✗ (read-only via API) | — | HIGH | LOW | LOW | DO NEXT | P1 |
| Settings: company / work schedule / holidays / leave / conversion | ✓ | HIGH | HIGH | LOW | LOW | KEEP | — |
| ZKTeco bridge | ✓ | MED | HIGH | HIGH | LOW | KEEP | — |
| Theming (light only) | � | LOW | HIGH | LOW | LOW | DO NEXT (dark + system) | P1 |

## H. Platform & Ops

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| PostgreSQL persistence | ✓ | MED | HIGH | LOW | LOW | KEEP, fix EnsureCreated → Migrate | P0 |
| Migrations vs EnsureCreated mismatch | ◐ (BUG) | LOW | VERY_HIGH | LOW | LOW | FIX NOW | P0 |
| Health check | ✗ | — | MED | LOW | LOW | DO NEXT | P1 |
| Logging (Serilog) | ✗ | — | MED | LOW | LOW | DO NEXT | P1 |
| OpenTelemetry | ✗ | — | MED | MED | LOW | LATER | P2 |
| Rate limiting | ✗ | — | HIGH | LOW | LOW | DO NEXT | P1 |
| Docker / compose | ✗ | — | HIGH | LOW | LOW | DO NEXT | P1 |
| CI / GitHub Actions | ✗ | — | HIGH | LOW | LOW | DO NEXT | P2 |
| Backups / archival | ✗ | — | HIGH | LOW | LOW | DO NEXT | P2 |
| Multi-tenancy | ✗ | — | LOW (for now) | HIGH | HIGH | DO NOT BUILD initially | P4 |

## I. UX & Design System

| Feature | Exists | Quality | Value | Complexity | Runtime Cost | Decision | Priority |
| ------- | ------ | ------- | ----- | ---------- | ------------ | -------- | -------- |
| Tailwind v4 setup | ✓ | MED | HIGH | LOW | LOW | KEEP, complete tokens | P1 |
| Design tokens (CSS vars) | ◐ | LOW | HIGH | LOW | LOW | DO NOW | P0 |
| Light + Dark + System theme | ◐ (disabled) | LOW | HIGH | LOW | LOW | DO NEXT | P1 |
| `<DataTable>` primitive | ✗ | — | VERY_HIGH | MED | LOW | DO NOW | P0 |
| `<Toast>` | ✗ | — | HIGH | LOW | LOW | DO NEXT | P1 |
| `<EmptyState>` / `<ErrorState>` | ✗ | — | MED | LOW | LOW | DO NEXT | P1 |
| `<PageHeader>` / `<Breadcrumbs>` | ◐ | LOW | MED | LOW | LOW | DO NEXT | P2 |
| Form library (react-hook-form + Zod) | ✗ | — | HIGH | LOW | LOW | DO NEXT | P1 |
| Charts (lightweight SVG) | ✗ | — | HIGH | MED | LOW | DO NEXT | P1 |
| Date picker | ✗ (native) | LOW | MED | LOW | LOW | KEEP native, upgrade if needed | P2 |
| Accessibility (focus trap, reduced-motion) | ◐ | LOW | HIGH | MED | LOW | DO NEXT | P1 |
| i18n (en/lo) | ◐ | MED | HIGH | LOW | LOW | COMPLETE | P1 |

## J. Decision Principles Applied

* Features marked **DO NOT BUILD** either need a heavier stack than the
  product justifies (Critical Path, Multi-tenancy, Zapier-style automation)
  or duplicate existing tools the user already has (Email-as-comms).
* Features marked **DO NOW** are foundation work the rest of the roadmap
  depends on (design tokens, DataTable, Project/Task/Milestone/ProjectMember/
  Tags, Timesheet).
* Features marked **DO NEXT** are the visible value uplift after foundation
  (dark mode, Kanban, Gantt, risk, portfolio, charts).
* Features marked **LATER** need evidence of user demand before build.
