# 02 — Gap Analysis

> Format: `| Capability | Current State | Problem | Recommendation | Priority |`
> Priorities: **P0** (must fix) · **P1** (high value) · **P2** (management) · **P3** (advanced) · **P4** (defer / reject).

## A. Data Model Gaps

| Capability | Current State | Problem | Recommendation | Priority |
| ---------- | ------------- | ------- | -------------- | -------- |
| Project entity | Missing | No way to group work, plan capacity, or track profitability | Introduce `Project` (id, code, name, owner, sponsor, status, priority, start/end, budget, currency, dept, health, parent) | P0 |
| Task entity | Missing | "Work" doesn't exist; Kanban/Gantt/Calendar impossible | Introduce `Task` (id, projectId, title, description, statusId, priority, assigneeId, reporterId, start, due, estimateMin, spentMin, parentTaskId, type, tags) | P0 |
| Milestone | Missing | No key-date tracking | `Milestone` (id, projectId, name, due, owner, status, completedAt) | P0 |
| Task dependency | Missing | No schedule graph, no blocked-by reasoning | `TaskLink` (id, fromTaskId, toTaskId, type [FS/SS/FF/SF]) with cycle detection | P1 |
| Sprint / Iteration | Missing | No Scrum cadence | `Sprint` (id, projectId, name, start, end, goal, status) | P2 |
| Risk register | Missing | No risk-impact scoring, no mitigation tracking | `Risk` (id, projectId, title, prob, impact, score, owner, mitigation, status, reviewDate) | P1 |
| Issue / Blocker | Missing | No first-class blocker workflow | `Issue` (id, projectId, title, severity, status, owner, relatedTaskId, resolvedAt) | P1 |
| Change Request | Missing | No formal change control | `ChangeRequest` (id, projectId, title, impactSchedule, impactBudget, requesterId, reviewerId, status) | P3 |
| Project baseline | Missing | No variance reporting | `ProjectBaseline` (id, projectId, snapshotAt, totals JSON, milestoneSnapshots) | P2 |
| Project member | Missing | No project-level RBAC or capacity assignment | `ProjectMember` (id, projectId, employeeId, role, allocationPct, start, end) | P0 |
| Skill / Position | Missing | No skill match for assignments | `Skill` + `EmployeeSkill` (id, employeeId, skillId, level, lastUsed) | P1 |
| Timesheet | Missing | No actual hours per task per day | `TimesheetEntry` (id, employeeId, date, taskId, minutes, note, status, approverId) | P0 |
| Budget / Cost / Revenue | Missing | No project financial view | `BudgetLine`, `ActualCost`, `RevenueEntry` per project | P1 |
| Comment / Mention | Missing | No collaboration | `Comment` (id, parentType, parentId, authorId, body, createdAt) + `Mention` | P2 |
| Notification | Missing | No alerts | `Notification` (id, userId, type, payload, readAt, createdAt) + `NotificationPreference` | P1 |
| Activity feed | Missing (audit exists, but untyped) | No user-facing timeline | `ActivityEvent` (id, actorId, verb, targetType, targetId, payload, createdAt) | P2 |
| Saved view | Missing | No personal filters | `SavedView` (id, ownerId, scope, name, filtersJson) | P2 |
| Approval workflow | Ad-hoc (`LeaveRequest.ApprovedById`) | Duplicated per domain | Generic `ApprovalRequest` (id, entityType, entityId, requesterId, approverId, status) + policy table | P2 |
| Tenant / Organization | Implicit (singleton) | Can't isolate multi-company | `Organization` table + `OrganizationId` on every domain entity | P3 |
| Document / Wiki | Only `EmployeeDocument` on disk | No project docs, no versioning | Introduce `Document` (id, projectId, folderId, ownerId, version, status, mime, size) + `WikiPage` (id, projectId, parentId, title, body, lastReviewedAt) | P2 |
| Approval policies | Hardcoded | No configurable approval chain | `ApprovalPolicy` (id, entityType, steps JSON) | P3 |

## B. API / Backend Gaps

| Capability | Current State | Problem | Recommendation | Priority |
| ---------- | ------------- | ------- | -------------- | -------- |
| Pagination | **None** | OOM risk at 10k+ rows on Employees/Attendance/Slips | Add `?page=&pageSize=&sort=` + cursor for huge lists. Return `{data, total, page, pageSize}` envelope. | P0 |
| Filtering consistency | Ad-hoc per controller | Can't build cross-domain saved views | Centralize filter parsing (e.g., `IFilterParser` per entity) | P1 |
| Authorization | Inconsistent | `Address`, `BankTransfer`, `CompanySettings`, `License`, `Reports`, `Holidays(POST/PUT/DELETE)` lack `[Authorize]` | Move to a policy-based `[Authorize(Policy="...")]` | P0 |
| `AuthController.SeedDefaultAdminAsync` on every login | Yes | Wasteful + race | Move to `DbSeeder` once, with a `IsSeeded` flag | P0 |
| `EnsureCreated` vs migrations | Mismatch | Schema drift risk | Switch to `db.Database.Migrate()` and convert seed data to migration | P0 |
| License middleware | DB hit per request | Performance + hot path | In-memory cache with 5-minute TTL | P1 |
| CORS `SetIsOriginAllowed(_ => true)` | Yes | Open credentialed CORS | Lock to known origins via config | P0 |
| Hard-coded production DB credentials in `appsettings.json` | Yes | Leak risk | Use environment variables / user-secrets / Secret Manager | P0 |
| `AuditLogInterceptor` writes audit in same transaction | Yes | Audit failures roll back user ops | Run audit in a fire-and-forget queue or separate scope | P1 |
| `LeaveController` is 541 LOC | Yes | Hard to maintain | Split into `LeaveRequestsController`, `LeaveBalancesController`, `LeaveCalendarController`, `LeavePoliciesController` | P1 |
| Background job polling every hour | Yes | Wasteful, no execution history | Use Quartz.NET, or trigger on demand from `LeaveController` | P1 |
| No health checks | Yes | Cannot monitor | Add `/health` with EF + SMTP checks | P1 |
| Telemetry / OpenTelemetry | None | Cannot trace requests | Add `OpenTelemetry.Extensions.Hosting` + EF instrumentation | P1 |
| API versioning | None | Future breaking changes hurt clients | Add `Microsoft.AspNetCore.Mvc.Versioning` | P2 |
| Rate limiting | None | Brute-force risk | Add `Microsoft.AspNetCore.RateLimiting` | P1 |
| Bulk operations | None | Repetitive client calls | Accept batch endpoints for assignments, status updates | P2 |
| Soft-delete consistency | Mixed (`IsActive`) | Some entities hard-delete | Standardize on `IsArchived` + filter query filters | P2 |
| Optimistic concurrency | None | Last-write-wins on slips/balance | Add `RowVersion` column on critical tables | P1 |
| Time zones | Hard-coded `UTC+7` literal | Bug-prone | Centralize via `TimeZoneInfo` constant + Npgsql `timestamp with time zone` | P1 |

## C. Frontend Gaps

| Capability | Current State | Problem | Recommendation | Priority |
| ---------- | ------------- | ------- | -------------- | -------- |
| Dark mode | Not implemented (select disabled) | Marketing says it exists, doesn't | Add Tailwind v4 `@variant dark(...)` + theme toggle in Header + persist in `localStorage` | P1 |
| Command palette / global search | Missing | Navigation will explode | `Cmd+K` palette using a lightweight indexer (MeiliSearch/Typesense) or server search endpoint | P1 |
| Virtualized tables | None | Will choke on 10k rows | Use `@tanstack/react-virtual` or TanStack Table row-virtualization | P0 |
| Data table primitive | Per-page table markup | Duplication | Create `DataTable` (sort, filter, column-visibility, sticky header, pagination) | P0 |
| Form primitives | Inline in pages | Duplication, no validation engine | Standardize on react-hook-form + Zod; share `FormField` wrapper | P1 |
| Charts | None | Reporting page is download-only | Add a tiny SVG-based chart set (bar, line, donut, sparkline) — no chart lib unless really needed | P2 |
| Kanban board | Missing | No `Task` data model | Build after Task model lands | P1 |
| Gantt | Missing | No schedule view | After Task+Dependency lands, build a tiny custom Gantt (HTML+CSS grid) — defer heavy lib | P1 |
| Saved filters in URL | Missing | Can't share filtered views | Push filters into URL search params (Next.js `useSearchParams`) | P1 |
| Toast/notification center | `alert()` used in 1+ places | Bad UX | Lightweight `<Toast />` system + `useToast()` hook | P1 |
| Loading skeletons | Yes (SkeletonCard) but inconsistent | Mixed UX | Standardize `<SkeletonTable/>`, `<SkeletonForm/>` | P1 |
| Empty states | Partial | Sometimes blank | Standardize `<EmptyState/>` primitive | P1 |
| Mobile navigation | Off-canvas not implemented | Sidebar will not fit | Add a slide-out drawer at `<md` | P1 |
| Accessibility | Minimal | Buttons have `aria-busy`; little else | Add focus trap in Modal, `prefers-reduced-motion`, keyboard nav | P1 |
| i18n completeness | 70% | Inline Lao strings, no plural/number/currency formatting | Switch to `react-intl` or `i18next`, add `Intl.NumberFormat`/`Intl.DateTimeFormat` | P1 |
| Dashboard per role | Single | HR/PM/Employee same view | Build widget composition with role-based widget registry | P2 |
| Activity feed widget | Mock data | Misleading | Build after ActivityEvent backend exists | P2 |
| Error boundaries | None | A render crash kills the page | Add `<ErrorBoundary>` at route level | P1 |
| Tests | None on frontend | Regressions invisible | Add Vitest + RTL for `lib/`, Playwright for happy paths | P1 |
| CI / Lint | `npm run lint` only | No typecheck gate | Add `tsc --noEmit` to lint script | P1 |

## D. PM Capability Coverage

| PM Knowledge Area | Status      | Evidence                                                  |
| ----------------- | ----------- | --------------------------------------------------------- |
| Scope             | **Missing** | No requirements / WBS / acceptance criteria               |
| Schedule          | **Missing** | No Gantt, no baseline, no dependencies                    |
| Cost              | **Partial** | Payroll only; no project cost / budget / margin           |
| Quality           | **Missing** | No defect tracking                                        |
| Resources         | **Partial** | Employees + Departments, no allocation / capacity         |
| Communication     | **Missing** | No comments, no mentions, no notifications                |
| Risk              | **Missing** | No risk register, no mitigation tracking                  |
| Procurement       | **Missing** | No purchase orders, no vendor registry                    |
| Stakeholders      | **Partial** | Users exist, no Stakeholder model with influence/interest |
| Integration       | **Missing** | No integrations (Calendar, Teams, Slack, GitHub, BI)       |

## E. Operational Gaps

| Capability | Current State | Problem | Recommendation | Priority |
| ---------- | ------------- | ------- | -------------- | -------- |
| Health check | None | Can't monitor | `/health` with EF + license checks | P1 |
| Logging | Default ASP.NET | No structured logs | Serilog + JSON sink | P1 |
| Telemetry | None | Blind in prod | OpenTelemetry + traces for HTTP, EF, custom spans | P1 |
| Audit retention | Forever | Table grows unbounded | Policy: 1y hot, 5y cold archive, configurable | P1 |
| Backup strategy | Unclear | Single Postgres | Document; nightly `pg_dump` + offsite | P2 |
| Docker / Compose | None | Hard to deploy | Add `Dockerfile.api` + `compose.yml` for dev | P1 |
| CI/CD | None | Manual deploys | Add GitHub Actions: build, test, docker push, deploy | P2 |
| Secrets | In repo | Leak | Move to env vars / user-secrets / Vault | P0 |
| Documented runbook | None | Onboarding pain | `docs/runbook.md` with common operations | P2 |

## F. Security Gaps

| Capability | Current State | Problem | Recommendation | Priority |
| ---------- | ------------- | ------- | -------------- | -------- |
| Authorization attributes | Missing on 6 controllers | Public-by-default | Default-deny `[Authorize]` at controller level; whitelist anonymous | P0 |
| CORS credentials + wildcard | Yes | Open credentialed CORS | Lock to known origins | P0 |
| JWT key in `appsettings.json` | Yes | Leak | `dotnet user-secrets` + env var | P0 |
| Default passwords seeded (`admin/admin123`, `hr/hr123`) | Yes | Demo only — must disable in prod | Conditional seeding only in `IsDevelopment` | P0 |
| Refresh tokens | Stateless | Can't revoke | Persist refresh tokens with revocation list | P2 |
| Brute-force protection | None | Login abuse | Rate-limit `/api/auth/login` | P1 |
| Password policy | None (PBKDF2 hash ok) | Weak passwords allowed | Add complexity rules in `PasswordHasher.Register` | P1 |
| File upload validation | Extension only | MIME bypass | Verify content-type + magic bytes; store outside `wwwroot` | P1 |
| Audit log integrity | Append-only | Could be tampered | Write to separate DB / append-only sink | P3 |
| PII handling | Plaintext | GDPR / Lao PDPA risk | At-rest encryption plan, masking in non-prod | P3 |

## G. Scalability Gaps (10x → 100x → 1000x)

| Capability | Current State | Risk at scale | Recommendation | Priority |
| ---------- | ------------- | ------------ | -------------- | -------- |
| Indexes on `Attendance(AttendanceDate)` | No | Slow monthly reports | Add index | P0 |
| Indexes on `LeaveRequest(StartDate)` | No | Slow calendar / overlap queries | Add index | P1 |
| Indexes on `AuditLog(Timestamp)` | No | Slow audit lookups | Add index; partition by month | P1 |
| Indexes on `SalarySlip(PeriodId)` | No | Slow payroll period detail | Add index | P0 |
| Indexes on `Employee(IsActive, DepartmentId)` | No | Slow list filter | Add index | P1 |
| N+1 risks | `Include(Employee)` in many queries | OK now, but no `AsNoTracking` for read-only | Use `AsNoTracking()` + `Select(...)` projections | P1 |
| Connection pooling | Npgsql default | Probably ok | Verify `Maximum Pool Size` and set per env | P2 |
| Caching | None | Repeated identical reads | Add `IMemoryCache` for `/api/dashboard/stats`, license, tax brackets | P1 |
| Aggregation tables | None | Reports scan large tables | Materialized view or summary table for monthly summaries | P2 |
| Read replicas | None | Single DB bottleneck | Configure connection string routing later | P3 |
| Background queue | None | Long exports block requests | Move Excel / PDF generation to a queue (e.g., `IHostedService` channel) | P2 |
| Frontend bundle | Next auto-splits | OK | Verify with `@next/bundle-analyzer` in CI | P2 |

## H. UI/UX Gaps

| Capability | Current State | Problem | Recommendation | Priority |
| ---------- | ------------- | ------- | -------------- | -------- |
| Light + Dark + System | Light only | Theme selector is fake | Add tokens + Tailwind `@variant dark`; persist per user | P1 |
| Loading states | Inconsistent | Some pages flicker | Standardize skeletons | P1 |
| Error states | Mostly `alert()` | Bad UX | Toast + retry | P1 |
| Empty states | Partial | Sometimes blank | `EmptyState` primitive with CTA | P1 |
| Form validation | Inline `errors` state | Re-implemented per page | react-hook-form + Zod | P1 |
| Confirm dialogs | Yes (`ConfirmationModal`) | Inconsistent copy | Standardize copy + danger style | P1 |
| Density | Spacious | Will feel wasteful for power users | Add `compact` density toggle | P3 |
| Responsive | Desktop-only verified | Mobile/tablet likely broken | Audit + fix grid breakpoints | P1 |
| Breadcrumbs | Only on `/employees/new` | Inconsistent | Add breadcrumbs via `PageHeader` | P2 |
| Page titles (HTML `<title>`) | Default | SEO + tab UX bad | Set per page metadata | P2 |
| Keyboard shortcuts | None | Power-user unfriendly | `Cmd+K`, `/`, `g+i` for go-to | P2 |
| Focus management in modals | Partial | Accessibility risk | Trap focus, ESC closes, return focus on close | P1 |
| Reduced motion | Not respected | Accessibility risk | Honor `prefers-reduced-motion` | P2 |

## I. What's *not* missing (already good)

* JWT auth + refresh + retry (`apiClient.ts`).
* Time-zone helpers (`lib/datetime.ts`).
* Auto audit interceptor (`AuditLogInterceptor.cs`).
* Bilingual employee fields.
* Conversion rate history with effective/expiry.
* PDF payslip + NSSF form fill.
* Excel export with dynamic adjustment columns.
* Background leave accrual.
* Permission matrix in `permissions.ts`.
* Form-state patterns are consistent.
* Skeletons already exist (just under-used).
