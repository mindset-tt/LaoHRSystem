# 22 — Missing Capabilities

> Separated by requirement category. Never mix requirements with speculative features.

## Required for correctness

| Capability | Why | Evidence |
|---|---|---|
| Fix `EmployeesController` ambiguous routes | Routing bug | BR-01 |
| PostgreSQL-compatible migrations (replace EnsureCreated fallback) | Schema drift | TD-01, BR-05 |
| Enforce project-member roles in controllers | Authz gap | BR-12 |
| `/403` page | Broken redirect | BR-10 |
| Disable/remove frontend mock data (dashboard, employee edit, documents) | Misleading UX | BR-06/07/08 |

## Required for production

| Capability | Why | Evidence |
|---|---|---|
| Reverse proxy + TLS termination | No HTTPS in Docker | BR-19 |
| Postgres backup strategy | Data loss risk | BR-20 |
| Rotate prior committed credentials | Security | BR-03 |
| Strong password hashing (PBKDF2/Argon2/bcrypt) | Security | BR-04, TD-02 |
| Global exception handler | Operability | TD-07, BR-24 |
| Frontend route protection (proxy.ts) | Defense in depth | BR-09, TD-15 |
| SMTP configuration (real host) | Email service stubbed | `appsettings.json` |
| CI branch alignment (master vs main/dev) | CI may not run | BR-21 |
| Resource limits in Docker (RAM/CPU) | OOM risk on payroll export | `docker-compose.yml` |

## Required for maintainability

| Capability | Why | Evidence |
|---|---|---|
| Frontend tests (Vitest + RTL) | No FE tests | BR-17, TD-19 |
| Integration tests for PM/Finance/Knowledge controllers | Zero coverage | BR-18, TD-20 |
| Split `LeaveController` | 541 LOC, mixed concerns | TD-04 |
| Migrate `EmployeeForm` to react-hook-form | Inconsistency | TD-13 |
| Move inline Lao strings to i18n dictionary | i18n completeness | TD-14 |
| Roslyn analyzers / lint gate in CI | Code quality | CI, TD-30 |

## Required for scalability

| Capability | Why | Evidence |
|---|---|---|
| List virtualization (DataTable) | 10k rows render all | TD-17 |
| `AsNoTracking` + projection DTOs for read paths | N+1 / payload bloat | audit |
| `RowVersion` optimistic concurrency on SalarySlip/LeaveBalance | Race conditions | TD-09, BR-14 |
| Background queue for Excel/PDF export | Long blocking requests | audit |
| Postgres connection pool sizing | Default 100 | audit |
| `AuditLog` partitioning / archival | Unbounded growth | audit |

## Required for security

| Capability | Why | Evidence |
|---|---|---|
| File upload magic-byte/MIME validation | Extension-only bypass | BR-13 |
| Rate limiting on write endpoints | Abuse risk | BR-16 |
| Fail-fast if `Jwt:Key` empty in Production | Fallback key risk | BR-11, TD-28 |
| PII at-rest encryption / masking in non-prod | GDPR/Lao PDPA | `11-AUTH-AND-SECURITY.md` |
| Password complexity policy | Weak passwords allowed | `10-FEATURE-INVENTORY.md` |
| CVE scan (iText7, Swashbuckle) | Older deps | BR-22 |
| httpOnly cookie auth alternative | localStorage XSS | TD-03 |

## Required for UX

| Capability | Why | Evidence |
|---|---|---|
| Mobile responsive nav drawer | Mobile broken | BR-25, `16-DESIGN-AND-UX.md` |
| Error boundaries | Render crash kills page | TD-18 |
| Notification center | No notifications | `10-FEATURE-INVENTORY.md` |
| Charts (lightweight SVG) | Dashboard is scalars only | `16-DESIGN-AND-UX.md` |
| Audit log UI | API only | `10-FEATURE-INVENTORY.md` |
| User management admin UI | Not started | `10-FEATURE-INVENTORY.md` |
| Saved filters in URL | Can't share views | audit |
| Accessibility (focus trap, reduced-motion) | a11y low | `16-DESIGN-AND-UX.md` |

## Nice-to-have (not blocking)

| Capability | Notes |
|---|---|
| Gantt / Timeline view | After task dependencies |
| Tags/Labels on tasks | PM enhancement |
| Timesheet | PM capacity |
| Skills + employee skills | Resource matching |
| Project budget / actual cost / revenue / margin | Finance extension |
| @Mentions in comments | Collaboration |
| Command palette / global search | Navigation |
| Activity feed (real, replace mock) | After ActivityEvent backend |
| Multi-tenancy / Organization | Deferred (P4 per audit) |
| MFA | Security hardening |
| API versioning | When breaking changes expected |
| Metrics exporter + dashboards | Observability |
| Error tracking (Sentry) | Observability |