# 00 — Executive Summary

> **Audit window**: full repository under `D:/LaoHRSystem`.
> **Method**: read-only inventory + analysis. No code was changed.
> **Output**: 13 supporting docs in `docs/audit/` + this summary.

## One-line verdict

LaoHR has a **strong, lean technical foundation** but is positioned as an
"HR-only" product while the strategic intent of the master prompt is to
become an **HR + work-management** product. The largest single gap is the
absence of any project / task / milestone data model — every other PM/PL
capability (risk, issue, timesheet, project budget) is structurally
impossible without it.

The product must evolve, not be rewritten. The backend, frontend stack, and
domain primitives are reusable.

## What's strong

* **Lightweight stack**: Next.js 16 + React 19 + Tailwind v4 with no UI
  library, no chart library, no state library. The bundle ceiling is in our
  hands.
* **Bilingual i18n** with `en` / `lo` and `Asia/Vientiane` timezone already
  wired.
* **Working payroll + NSSF + bank-transfer domain logic** — this is the
  moat that an HR-only clone does not have.
* **Audit log** is captured at the EF level (a single, well-placed
  interceptor), so we have a baseline for governance.
* **Bridge to ZKTeco** biometric devices — non-trivial domain integration
  already shipped.

## What's risky

* **Production credentials in `appsettings.json`** (`10.233.141.2`,
  `bi_owner`, `superset`). This is in the repo.
* **Schema is created via `EnsureCreated()`** while migrations target SQL
  Server and the runtime uses Npgsql. Schema drift is silent.
* **6 controllers lack `[Authorize]`** — public-by-default.
* **`AuditLogInterceptor`** writes the audit row in the same transaction
  as the user operation. Any audit failure rolls back the user write — a
  hidden coupling that turns audit into a single point of failure.
* **No pagination** anywhere; one day the employees list will OOM the
  process.
* **Frontend has silent mock-data fallbacks** on employees + departments;
  the user sees "success" when the API is actually down.
* **`DateTime.Now` is used in business logic**, no central timezone
  boundary; the legacy timestamp switch is enabled to paper over it.
* **No CI/CD**, no `/health`, no structured logs, no Docker.

## What's missing (PM/PL lens)

| Capability today          | Reality |
| ------------------------- | ------- |
| Projects                  | **No data model.** |
| Milestones                | **No data model.** |
| Tasks                     | **No data model.** |
| Risks                     | **No data model.** |
| Issues / blockers         | **No data model.** |
| Timesheet                 | **No data model.** |
| Skills / capacity         | **No data model.** |
| Project budget / cost     | **No data model.** |
| Documents / wiki          | **No data model.** |
| Comments / mentions       | **No data model.** |
| Notifications             | **No data model.** |
| Activity feed             | **No data model.** (frontend currently shows mock entries.) |
| Saved views               | **No data model.** |
| Approval workflows        | Only leave has a partial one. |
| Org / multi-tenant        | `CompanySetting` is a singleton; no `Organization`. |
| Admin users / roles       | Only seed-time demo accounts. |

> This is not a "missing features" problem — it's a **missing foundation**
> problem. We cannot add risk or capacity views without Project, and we
> cannot add project budget without Project + Cost + Revenue.

## Recommended strategy

1. **Stabilize first** (Phase 0, 1–2 weeks) — see `09-roadmap.md`.
2. **Tokenize the design** (Phase 1, 1 week).
3. **Build the work-management layer** (Phase 2, 3–4 weeks):
   Project, Milestone, Task, TaskLink, Tag, ProjectMember, plus My Work,
   Cmd+K palette, role-aware dashboard.
4. **PM extension** (Phase 3) — Risk, Issue, Skill, Timesheet, capacity.
5. **Finance extension** (Phase 4) — Budget, ActualCost, Revenue.
6. **Collaboration** (Phase 5) — Documents, Wiki, Comments, Mentions.
7. **Ops** (Phase 6) — OpenTelemetry, Docker, CI, health, retention.

Phases are vertical slices, not waterfalled. Each phase ships a usable
product. See `11-implementation-plan.md` for the slicing.

## Decision principle (the one that matters)

> **More Features vs Simpler Product → prefer simpler, unless the feature
> delivers significant operational value.**

This audit applies this to every candidate capability. The list of things
that were considered and **not** added is in `10-product-decisions.md`
under "Why we do NOT clone Jira/Linear/ClickUp."

## How scale is achieved

We do **not** scale by adding infrastructure. We scale by:

* Pagination + projection DTOs (already on the list).
* A small set of indexes that hit the actual hot paths.
* One fire-and-forget audit pipeline.
* One background worker per cross-cutting concern (export, retention,
  notifications).
* A small, single-page bundle per route.

See `06-scalability-performance.md` for the performance budgets and
capacity estimates (10 → 10k+ users; tens → millions of records).

## How lightweight is preserved

We deliberately avoid:

* UI libraries, chart libraries, state libraries.
* Rich-text editor — markdown + preview.
* Server-side critical-path computation.
* Built-in chat / IM.
* Visual automation engine.
* A general-purpose workflow engine.

See `07-target-architecture.md` for the modular target and `08-information-architecture.md` for the navigation that keeps every page < 4 clicks deep.

## Critical "do this first" list (the 12 items)

1. Replace `EnsureCreated` with `Migrate`; commit the migration.
2. Move audit write off the request transaction.
3. Add `[Authorize]` on every controller; default-deny.
4. Move secrets to env vars; rotate the leaked DB password.
5. Lock CORS to a known-origin list.
6. Add pagination envelopes to list endpoints.
7. Add the recommended indexes.
8. Disable the frontend mock-data fallbacks.
9. Add `/health` and Serilog JSON sink.
10. Define design tokens and theme provider.
11. Build `<DataTable>` and replace per-page tables.
12. Introduce Project + Milestone + Task (Phase 2 first slice).

## Risks of NOT acting on this audit

* **Security**: production credentials + public-by-default endpoints are
  live.
* **Operability**: silent failures on the frontend + audit-coupling in the
  backend will eventually hide a real outage.
* **Strategic**: without Phase 2, the product cannot credibly serve as the
  PM/PL platform the master prompt requires. The pivot from "HR-only" to
  "HR + PM" is the single biggest decision in this audit.

## What we deliberately did NOT do

* We did not implement anything. The master prompt forbids it.
* We did not clone Jira/Linear/ClickUp.
* We did not add multi-tenant.
* We did not introduce a new language, framework, or database.
* We did not introduce a separate service for the audit log.
* We did not introduce a UI library.

## Pointer to all supporting docs

| #  | File                                | Audience |
| -- | ----------------------------------- | -------- |
| 01 | `01-current-system-map.md`          | engineering, PM |
| 02 | `02-gap-analysis.md`                | PM, PL |
| 03 | `03-pm-pl-review.md`                | PM, PL, exec |
| 04 | `04-ui-ux-review.md`                | design, frontend |
| 05 | `05-feature-matrix.md`              | product, PM |
| 06 | `06-scalability-performance.md`     | tech lead |
| 07 | `07-target-architecture.md`         | architect, tech lead |
| 08 | `08-information-architecture.md`    | design, PM |
| 09 | `09-roadmap.md`                     | PM, exec |
| 10 | `10-product-decisions.md`           | product, PM |
| 11 | `11-implementation-plan.md`         | engineering, PL |
| 12 | `12-page-inventory.md`              | frontend, design |
| 13 | `13-data-relationship-map.md`       | data architect |

---

**Recommendation**: approve the audit and begin **Phase 0** immediately.
The cost of doing nothing this week is one more week of credentials in
the repo, one more week of silent mock-data on the dashboard, and one
more week of an HR-only product with no credible PM story.
