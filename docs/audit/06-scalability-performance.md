# 06 — Scalability & Performance

## 1. Performance Budgets (proposed)

Targets are derived from a **lightweight-first** product running on modest
infrastructure (2 vCPU, 4 GB RAM Postgres, single Next.js node).

| Metric                              | Target         | Today (estimated) | Status |
| ----------------------------------- | -------------- | ----------------- | ------ |
| First contentful paint (FCP)        | < 1.0 s        | ~0.8 s (no UI lib)| OK     |
| Largest contentful paint (LCP)      | < 1.5 s        | ~1.2 s            | OK     |
| Time to interactive (TTI)           | < 2.0 s        | ~1.6 s            | OK     |
| Initial JS (gzip)                   | < 150 kB       | ~120 kB (Next 16) | OK     |
| Route chunk (gzip)                  | < 80 kB        | ~30–60 kB         | OK     |
| API latency p95 (auth path)         | < 200 ms       | ~50 ms            | OK     |
| API latency p95 (dashboard stats)   | < 150 ms       | ~40 ms (4 counts) | OK     |
| API latency p95 (list 1k rows)      | < 300 ms       | 600 ms+ (no paging) | **FIX** |
| API latency p95 (calendar month)    | < 400 ms       | 800 ms+ (no idx)  | **FIX** |
| DB connections (steady state)       | < 30           | ~10               | OK     |
| Memory per Next.js node             | < 400 MB       | ~250 MB           | OK     |
| Frontend JS heap on dashboard       | < 80 MB        | ~30 MB            | OK     |
| Frontend JS heap on payroll (1000 slips) | < 150 MB | ~90 MB            | OK     |

## 2. Frontend

### 2.1 Bundle

* **Stack**: Next.js 16 + React 19 + Tailwind v4, **no UI lib, no chart lib,
  no date lib, no state lib**. That's an excellent starting position.
* `next.config.ts` enables `reactCompiler` + Turbopack.
* No `@next/bundle-analyzer` wired. Add to CI.

### 2.2 Render Performance

* No virtualization. A payroll period with 5,000 slips will render all rows.
* `EmployeesPage` falls back to **mock data** on API error, masking failure
  (`employees/page.tsx:49-58`).
* `DashboardPage` polls `/api/dashboard/stats` once, no caching.
* `apiClient.ts` has **in-memory token cache only**, no `fetch` caching.

### 2.3 Re-render Risks

* Sidebar sets `expandedMenu` via local state — fine.
* `useAuth` returns a context object; any change re-renders the entire
  `(dashboard)` tree. Acceptable for current size; will be revisited when
  widget count grows.

### 2.4 Recommendations

| Recommendation                                  | Priority |
| ----------------------------------------------- | -------- |
| Add `@next/bundle-analyzer` to CI               | P2       |
| Use `@tanstack/react-virtual` for any list > 100 rows | P0       |
| Use URL search params for filters               | P1       |
| Add `useSWR`/`@tanstack/react-query` for client cache (optional; only if perf demands) | P3 |
| Move SVG icons to `<Icon name="…" />` primitive | P2       |
| Memoize `StatCard`, `QuickActionButton`         | P3       |

## 3. Backend / API

### 3.1 Latency hotspots

| Endpoint                                  | Concern                                                         |
| ----------------------------------------- | --------------------------------------------------------------- |
| `GET /api/employees`                      | Returns all employees incl. department. No paging.               |
| `GET /api/attendance`                     | Limited `Take(100)` but `Date` filter is full table scan.        |
| `GET /api/leave`                          | All leave requests ever, no paging. Includes `Include(Employee)`. |
| `GET /api/payroll/periods/{id}/slips`     | All slips for period, no paging.                                |
| `GET /api/payroll/periods/{id}/export`    | ClosedXML in-process; long blocking call.                        |
| `GET /api/leave/calendar`                 | Range filter, no index on `StartDate`.                          |
| `GET /api/dashboard/stats`                | Four sequential counts; could be parallel or one SQL.           |
| `LicenseMiddleware`                       | DB hit per request for license lookup.                          |
| `AuditLogInterceptor`                     | JSON serialization per change in same transaction.              |

### 3.2 N+1 Risks

* `EmployeesController.GetEmployees` uses `Include(Department)` — fine for one
  call but repeated across pages will amplify.
* `PayrollController.GetSlips` includes `Employee` — fine.
* `PayrollController.ExportPayroll` loads all slips + all adjustments + then
  loops **per slip** through adjustments to compute dynamic columns. **This is
  O(slips × adjustments)**. Refactor to a single group-by query.
* `LeaveController.GetLeaveBalance` computes `usedDays` by `GroupBy` in memory;
  fine for now, but should become a SQL group-by at scale.

### 3.3 Serialization

* `ReferenceHandler.IgnoreCycles` is enabled — good for cyclic nav.
* Returning full entity graphs (`Employee` with `Department` + `LeaveRequests`
  + `SalarySlips` + `AttendanceRecords` if anyone calls `Include` on them)
  will bloat responses. Use DTOs / projections.

### 3.4 Recommendations

| Recommendation                                                  | Priority |
| --------------------------------------------------------------- | -------- |
| Add `?page=&pageSize=&sort=&filter=` everywhere                 | P0       |
| Replace `Include` with `Select(...).AsNoTracking()` projections | P1       |
| Add `[Authorize]` defaults; whitelist anonymous endpoints       | P0       |
| Cache license key for 5 minutes (memory or `IDistributedCache`) | P1       |
| Move PDF / Excel generation to background queue                 | P2       |
| Add OpenTelemetry + EF instrumentation                          | P2       |
| Add rate limiting (`/api/auth/login`)                           | P1       |
| Move audit write to fire-and-forget after commit                | P1       |
| Add `OutputCache` for `/api/dashboard/stats` and `/api/holidays` | P1       |
| Add `AsNoTrackingWithIdentityResolution()` for read-only paths  | P2       |

## 4. Database

### 4.1 Schema-level risks

| Issue                                                              | Impact                                                     |
| ------------------------------------------------------------------ | ---------------------------------------------------------- |
| `EnsureCreated()` instead of `Migrate()`                           | Schema drift; EF migrations folder is unused.              |
| No indexes on `LeaveRequest(StartDate, EndDate, Status)`           | Calendar / range queries scan full table.                  |
| No indexes on `SalarySlip(PeriodId)`                               | Period detail scan.                                        |
| No indexes on `Employee(IsActive, DepartmentId)`                   | List filter scan.                                          |
| No indexes on `AuditLog(Timestamp)` / `(EntityName, Timestamp)`    | Audit log slow.                                            |
| No partitioning on `AuditLog`                                      | Unbounded growth.                                          |
| `Attendance` table will explode (1 per employee/day).              | Need archive strategy.                                     |
| Decimal precision OK; money stored as `decimal(18,2)` (LAK scale). | OK.                                                        |
| No soft-delete column on most entities                             | Inconsistent (`IsActive` vs `IsArchived`).                 |
| No `RowVersion` column on SalarySlip / LeaveBalance                | Race conditions during concurrent edits.                   |

### 4.2 Connection / Pooling

* `Npgsql` default max pool size is 100. Should set explicit `Maximum Pool Size`
  via connection string and env-specific overrides.

### 4.3 Recommendations

| Recommendation                                              | Priority |
| ----------------------------------------------------------- | -------- |
| Switch to `db.Database.Migrate()` and convert seed to migration | P0       |
| Add the indexes above                                       | P0/P1    |
| Add `RowVersion` on `SalarySlip`, `LeaveBalance`, `Project` | P1       |
| Partition `AuditLog` by month (Postgres native partitioning) | P2       |
| Archive `Attendance` older than 3 years to cold table        | P2       |
| Materialized view for monthly dashboards                    | P3       |
| Configure `Maximum Pool Size` per environment               | P1       |

## 5. Pagination strategy

* For **Employees / Departments / Documents**: server offset paging is fine
  up to ~50k rows.
* For **Attendance / SalarySlips**: cursor pagination (`?afterId=`) is better
  for append-only time-series.
* For **AuditLog**: cursor with `(Timestamp DESC, Id DESC)` and a date filter.
* For **Leave calendar**: by-month window.

## 6. Caching strategy

| Cache layer          | What                                          | TTL          |
| -------------------- | --------------------------------------------- | ------------ |
| Client `localStorage`| token + theme + language                      | session      |
| Client `fetch` cache | none today                                    | —            |
| Server memory        | license key, tax brackets, work schedule      | 5–15 min     |
| Server `OutputCache` | `/api/holidays?year=…`, `/api/dashboard/stats` | 5 min        |
| DB query plan cache  | enabled by default                            | —            |

## 7. Background work / queues

* Today: `LeaveScheduledJobsService` polls every hour.
* Future: long-running exports (Excel/PDF) should move to a queue
  (`Channel<T>` + `BackgroundService`) so the HTTP request returns a job id
  and the client polls or receives a notification.

## 8. Logging / Telemetry

* Default ASP.NET Core logging. No structured logs.
* Recommend Serilog + JSON sink + OpenTelemetry traces for HTTP/EF.

## 9. Frontend bundle growth risk

As features are added (Kanban, Gantt, charts, forms), the temptation will be
to add libraries:

| Need                | Avoid                            | Prefer                              |
| ------------------- | -------------------------------- | ----------------------------------- |
| Charts              | recharts / chart.js              | Lightweight SVG components          |
| Forms               | formik                           | react-hook-form + Zod               |
| Date picker         | react-datepicker                 | Native + small wrapper if needed    |
| Drag/drop (Kanban)  | react-dnd (heavy)                | `@dnd-kit/core` (~12 kB)            |
| Gantt               | dhtmlx-gantt (heavy)             | Custom CSS-grid based mini Gantt    |
| Virtualization      | react-virtualized                | `@tanstack/react-virtual` (~3 kB)   |
| Search              | fuse.js client-side              | Server endpoint with debounce       |

## 10. Capacity estimates (rough)

| Dataset                       | 10 users | 100 users | 1k users | 10k users |
| ----------------------------- | -------- | --------- | -------- | --------- |
| Employees                     | 50       | 500       | 5k       | 50k       |
| Attendance rows / year        | 12k      | 120k      | 1.2M     | 12M       |
| Leave requests / year         | 100      | 1k        | 10k      | 100k      |
| SalarySlips / year            | 600      | 6k        | 60k      | 600k      |
| Projects (when added)         | 10       | 100       | 1k       | 10k       |
| Tasks (when added)            | 1k       | 10k       | 100k     | 1M        |
| Timesheets (when added) / yr  | 12k      | 120k      | 1.2M     | 12M       |
| AuditLog rows / year          | 50k      | 500k      | 5M       | 50M       |

With current pagination missing, the system will degrade at **> 100 users /
> 5k rows** on `EmployeesController.GetEmployees` alone. With the P0
recommendations above, it should scale to 10k users / 1M rows.

## 11. Risk hotspots at scale

1. `EmployeesController.GetEmployees` unbounded — fix in P0.
2. `Attendance` table scan — fix with index in P0.
3. `AuditLog` write amplification — fix with fire-and-forget.
4. License middleware per-request DB hit — fix with 5-minute cache.
5. ClosedXML export O(slips × adjustments) — fix with grouped query.
6. Frontend list virtualization missing — fix with `@tanstack/react-virtual`.
7. No client-side data cache — acceptable; add only if dashboards become slow.

## 12. Recommended SLOs

| Metric                                    | SLO         |
| ----------------------------------------- | ----------- |
| Availability (API)                        | 99.5%       |
| API success rate (non-4xx)                | 99.9%       |
| p95 dashboard render                      | < 2 s       |
| p95 list page render                      | < 2.5 s     |
| Payroll run (200 employees)               | < 5 s       |
| Payroll export (200 employees, XLSX)      | < 15 s      |
| Payslip PDF (one)                        | < 2 s       |
| Timesheet save                            | < 300 ms    |
