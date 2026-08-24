# 30 — Current Development Frontier

> The exact place where completed implementation ends and unfinished implementation begins. `VERIFIED` from git status + code.

## CURRENT DEVELOPMENT FRONTIER

The project has just completed a **large feature expansion** (PM/Finance/Knowledge layers + Docker + CI + ops hardening) that is **uncommitted** in git (branch `master`). The work is functionally complete but not yet stabilized/hardened.

### What was just completed (uncommitted, `??` new files + `M` modified)

1. **Finance layer**: `ExpensesController`, `EmployeeLoansController` + frontend `/finance/expenses`, `/finance/loans` routes + `lib/endpoints/finance.ts`.
2. **Knowledge layer**: `KnowledgeArticlesController`, `AnnouncementsController` + frontend `/knowledge`, `/knowledge/announcements` routes + `lib/endpoints/knowledge.ts`.
3. **Polymorphic comments**: `CommentsController` + `EntityComment` entity + `lib/endpoints/knowledge.ts` (commentsApi).
4. **Retention service**: `Jobs/RetentionService.cs` (disabled by default).
5. **Refresh token server-side**: `Services/RefreshTokenService.cs` + `RefreshToken` entity + `AuthController` modified.
6. **Docker**: `Dockerfile` (api + web) + `docker-compose.yml` + `.dockerignore`.
7. **CI**: `.github/workflows/ci.yml`.
8. **Env template**: `.env.example`.
9. **Frontend updates**: `Sidebar` (new nav items), `AuthProvider` (refresh), `apiClient` (refresh), `i18n` (new strings), `types` (new types), `endpoints/index` (barrel), `next.config.ts`, `projects/[id]/page.tsx`.

### Where completed ends / unfinished begins

The **frontier is stabilization + production readiness**, not new features. Specifically:

1. **Commit the uncommitted work** — the Finance/Knowledge/Docker/CI/Retention/RefreshToken work is not yet committed. This is the immediate next action.
2. **Add tests for the new layers** — PM/Finance/Knowledge controllers have zero tests (BR-18). This is the next quality gate.
3. **Remove frontend mock data** — dashboard activity feed, employee edit fallback, employee documents mock (BR-06/07/08).
4. **Fix known bugs** — ambiguous employee routes (BR-01), CompanySettings auth (BR-02), `/403` page (BR-10), proxy.ts (BR-09).
5. **Enforce project roles** — `ProjectMember.Role` stored but not enforced (BR-12).
6. **Production hardening** — TLS, backups, credential rotation, password hashing (BR-03/04/19/20).

### What the previous developer was working toward

Based on the audit roadmap (`docs/audit/09-roadmap.md`) and the implemented phases:
- Phase 0 (stabilization) — **mostly done** (auth, secrets, pagination, indexes, health, Serilog, audit decoupling).
- Phase 1 (design system) — **mostly done** (tokens, theme, DataTable, Toast, Form, i18n).
- Phase 2 (project workspace) — **done** (Project, Milestone, Task, ProjectMember, ActivityLog, Comments).
- Phase 3 (risk/issue/resource) — **done** (Risk, Issue, Resource).
- Phase 4 (finance extension) — **partially done** (expenses + loans done; project budget/cost/revenue NOT started).
- Phase 5 (collaboration) — **partially done** (comments + announcements done; @mentions, notifications, activity feed real NOT done).
- Phase 6 (ops) — **done** (Docker, CI, health, Serilog, OpenTelemetry, retention).

The developer was in the **finalization/polish phase** of a major feature expansion, about to commit and stabilize before production deployment.