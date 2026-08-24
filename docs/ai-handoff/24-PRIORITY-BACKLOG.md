# 24 — Priority Backlog

> P0 — blockers · P1 — important · P2 — improvements · P3 — future.
> Do NOT implement yet — this is a proposed backlog for the next phase.

## P0 — Blockers

| ID | Title | Reason | Dependencies | Affected components | Expected outcome | Risks | How to validate |
|---|---|---|---|---|---|---|---|
| PB-01 | Fix `EmployeesController` ambiguous `{id}` routes | Routing bug — employee vs department conflict | None | `EmployeesController`, frontend employee pages | Routes resolve correctly | Breaking change if frontend relies on current paths | Manual: GET employee by id returns employee; departments have distinct route |
| PB-02 | Generate PostgreSQL migrations; remove EnsureCreated fallback | Schema drift / silent inconsistency | None | `Program.cs`, `Migrations/`, `LaoHRDbContext` | Clean migration-based schema | Data loss if not careful on existing DB | `dotnet ef migrations script` clean; `\d+` in psql matches model |
| PB-03 | Add `[Authorize]` to `CompanySettingsController` GET | Unauthenticated data leak | None | `CompanySettingsController` | No anonymous access | Frontend may need token for public company info | GET without token → 401 |
| PB-04 | Rotate prior committed credentials (DB + JWT key) | Security — credentials in git history | None | env vars, DB | No leaked creds valid | Service downtime during rotation | Confirm new creds work; old creds fail |
| PB-05 | Replace SHA-256 password hashing with PBKDF2/Argon2 | Weak hashing | None (migration of existing hashes needed) | `PasswordHasher`, `AppUser` | Strong password hashing | Existing users must re-hash on next login | Unit test hash+verify; verify existing login still works (rehash on login) |
| PB-06 | Fail-fast if `Jwt:Key` empty in Production | Fallback key risk | None | `Program.cs` | No silent fallback key | Startup failure if misconfigured | Boot without key in Production → clear error |
| PB-07 | Remove/disable frontend mock data (dashboard, employee edit, documents) | Misleading UX | Real API endpoints must work | `page.tsx`, `employees/[id]`, `employees/[id]/edit` | Real data or proper empty/error states | Empty states may reveal missing data | Manual: API down → error state, not mock |

## P1 — Important

| ID | Title | Reason | Dependencies | Affected components | Expected outcome | Risks | How to validate |
|---|---|---|---|---|---|---|---|
| PB-08 | Create `/403` page | Broken redirect | None | frontend `app/403/page.tsx` | Permission denied page renders | None | Navigate to forbidden → 403 page |
| PB-09 | Implement `proxy.ts` middleware (or remove) | No edge route protection | PB-08 | `src/proxy.ts` / `middleware.ts` | Unauthenticated users redirected to /login | SSR redirect loops if misconfigured | Navigate to /employees without token → /login |
| PB-10 | Enforce project-member roles in controllers | Authz gap | None | `ProjectsController`, `ProjectTasksController` | Only members/owners can edit | Existing users lose access if not members | Test: non-member POST update → 403 |
| PB-11 | Add integration tests for PM/Finance/Knowledge controllers | Zero test coverage | None | `LaoHR.Tests/Integration/Controllers/` | Regression safety | None | `dotnet test` passes; coverage increases |
| PB-12 | Add frontend tests (Vitest + RTL) | No FE tests | None | `frontend/` | Component/unit coverage | None | `npm test` passes |
| PB-13 | Add reverse proxy + TLS to Docker stack | No HTTPS | None | `docker-compose.yml`, nginx/Caddy config | HTTPS termination | Cert management complexity | `curl https://localhost` works |
| PB-14 | Add Postgres backup config | Data loss risk | None | `docker-compose.yml`, backup script | Nightly `pg_dump` + offsite | Backup storage cost | Restore test from backup |
| PB-15 | Split `LeaveController` | 541 LOC, mixed concerns | None | `LeaveController` → 4 controllers | Maintainability | Route changes may break frontend | All leave endpoints still work |
| PB-16 | Add global exception handler | Unhandled exceptions leak info | None | `Program.cs` | Problem-details response | None | Force exception → clean 500 JSON |
| PB-17 | File upload magic-byte validation | Extension-only bypass | None | `DocumentsController` | Safe uploads | False positives on valid files | Upload malicious renamed file → rejected |
| PB-18 | Align CI branch triggers (master vs main/dev) | CI may not run | None | `.github/workflows/ci.yml` | CI runs on master | None | Push to master → CI runs |
| PB-19 | Add list virtualization to DataTable | 10k rows render all | None | `DataTable.tsx` | Smooth large lists | Complexity | Render 10k rows → no jank |
| PB-20 | Notification model + in-app notifications | No notifications | None (DB migration) | backend + frontend | Users receive alerts | Polling vs push complexity | Create task → assignee sees notification |

## P2 — Improvements

| ID | Title | Reason | Dependencies | Expected outcome |
|---|---|---|---|---|
| PB-21 | Charts (lightweight SVG) for dashboard/reports | Dashboard is scalars only | None | Visual analytics |
| PB-22 | Audit log UI | API only | None | Admin can view logs in app |
| PB-23 | User management admin UI | Not started | None | Admin can manage users |
| PB-24 | Task dependencies (FS/SS/FF/SF) | PM schedule graph | Task model | Dependency tracking |
| PB-25 | Gantt / Timeline view | PM scheduling | PB-24 | Visual schedule |
| PB-26 | Tags/Labels on tasks | PM organization | Task model | Categorization |
| PB-27 | Timesheet | PM capacity | Task model | Hours tracking |
| PB-28 | Skills + employee skills | Resource matching | None | Skill matrix |
| PB-29 | Project budget / actual cost / revenue / margin | Finance extension | Project model | Profitability |
| PB-30 | @Mentions in comments | Collaboration | Comments | Notify on mention |
| PB-31 | Command palette / global search | Navigation | None | Cmd+K |
| PB-32 | Real activity feed (replace mock) | Misleading | ActivityLog backend | Live timeline |
| PB-33 | Mobile responsive nav drawer | Mobile broken | None | Mobile usable |
| PB-34 | Error boundaries | Render crash kills page | None | Graceful errors |
| PB-35 | Accessibility (focus trap, reduced-motion, keyboard nav) | a11y low | None | a11y compliance |
| PB-36 | `RowVersion` optimistic concurrency | Race conditions | Migration | Concurrent edit safety |
| PB-37 | Background queue for exports | Long blocking requests | None | Non-blocking exports |
| PB-38 | `AsNoTracking` + projection DTOs | N+1 / payload bloat | None | Leaner responses |
| PB-39 | CVE scan + update iText7/Swashbuckle | Older deps | None | No known CVEs |
| PB-40 | SMTP real config | Email stubbed | None | Email notifications work |

## P3 — Future

| ID | Title | Reason | Expected outcome |
|---|---|---|---|
| PB-41 | MFA | Security hardening | 2FA login |
| PB-42 | OAuth/SSO | Enterprise auth | External IdP |
| PB-43 | Multi-tenancy / Organization | Multi-company | Tenant isolation |
| PB-44 | API versioning | Breaking changes | Versioned API |
| PB-45 | Metrics exporter + dashboards | Observability | Prometheus/Grafana |
| PB-46 | Error tracking (Sentry) | Observability | Crash reports |
| PB-47 | Audit log partitioning/archival | Unbounded growth | Bounded audit table |
| PB-48 | Read replicas | Scale | DB scaling |
| PB-49 | Saved views / filters in URL | Shareable views | URL-state filters |
| PB-50 | Mobile app | Native mobile | Mobile app |