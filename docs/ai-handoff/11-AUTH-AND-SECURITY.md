# 11 — Authentication & Security

> `VERIFIED` from `Program.cs`, `AuthController`, `RefreshTokenService`, `LicenseMiddleware`, `appsettings.json`.

## Authentication

| Aspect | Implementation | Status |
|---|---|---|
| Login method | Username + password (`POST /api/auth/login`) | DONE |
| Password hashing | **SHA-256** (`PasswordHasher` static) | ⚠️ WEAK — not bcrypt/argon2/PBKDF2 |
| Token strategy | JWT Bearer HS256, 60-min (`Jwt:DurationInMinutes`) | DONE |
| Token signing key | `Jwt:Key` config (env var; was hardcoded, now removed from appsettings) | DONE (must be set via env) |
| Refresh tokens | Server-side `RefreshToken` entity, SHA-256 hashed, 14-day, rotated on every use, replay detection (family revocation) | DONE |
| Token storage (frontend) | `localStorage` (access + refresh + expiry timestamps) | ⚠️ XSS-vulnerable (see below) |
| Cookie strategy | `credentials: 'include'` on fetch, but no httpOnly cookie auth — tokens in localStorage | N/A |
| OAuth / SSO | None | NOT_STARTED |
| MFA | None | NOT_STARTED |

## Authorization

| Aspect | Implementation | Status |
|---|---|---|
| RBAC roles | Admin, HR, Employee (hard-coded strings on `AppUser.Role`) | DONE |
| Default-deny | `FallbackPolicy = RequireAuthenticatedUser()` — all endpoints require auth unless `[AllowAnonymous]` | DONE |
| Controller-level | Most controllers `[Authorize]`; Payroll `[Authorize(Roles="Admin,HR")]`; AuditLogs `[Authorize(Roles="Admin")]` | DONE |
| Frontend permissions | `lib/permissions.ts` RBAC matrix; `useRequirePermission()` (UX-only, backend authoritative) | DONE |
| Policy-based authz | No — uses raw Roles strings, no `[Authorize(Policy=...)]` | NOT_STARTED |
| Project-level RBAC | `ProjectMember.Role` (OWNER/LEAD/MEMBER/VIEWER) exists but not enforced in controllers | PARTIAL |
| Tenant isolation | None — `CompanySetting` is singleton; no `Organization` | NOT_STARTED |

## Auth gaps (UI vs backend)

- `CompanySettingsController` GET is `[AllowAnonymous]` — `BROKEN`/security gap. `VERIFIED`.
- `LicenseController` is `[AllowAnonymous]` — intentional (must activate before auth).
- `useRequirePermission` redirects to `/403` but **no `/403` page exists** — `BROKEN`.
- `proxy.ts` middleware redirect logic is **commented out** — no server-side route protection on frontend (relies on `useRequireAuth` in layout). `VERIFIED`.
- Project-member roles are stored but **not enforced** in `ProjectsController`/`ProjectTasksController` — any authenticated user can create/edit any project. `INFERRED`.

## Security review (lightweight, architectural)

| Concern | Finding | Severity | Evidence |
|---|---|---|---|
| Secrets in source | `appsettings.json` cleaned (ConnectionStrings + Jwt:Key empty); `.env.example` has placeholders. ⚠️ Prior committed credentials (DB host `10.233.141.2`, `bi_owner`/`superset`, JWT key) MUST be rotated. | HIGH (historical) | `appsettings.json`, repo memory |
| Hardcoded credentials | Default demo users seeded (admin/admin123, hr/hr123, employee/emp123) — Dev/Testing only via `DbSeeder.Seed`. | MEDIUM | `DbSeeder.cs` |
| CORS | Dev: any `http://localhost:*`/`127.0.0.1:*` + allow-list. Prod: `Cors:AllowedOrigins` comma-separated. `AllowCredentials` + all methods/headers. | LOW (if configured correctly) | `Program.cs` |
| JWT in localStorage | Tokens in `localStorage` → vulnerable to XSS. No httpOnly cookie alternative. | MEDIUM | `apiClient.ts` |
| Password hashing (SHA-256) | Not a password-hashing KDF (no salt/work factor visible). Susceptible to brute force if DB leaks. | HIGH | `PasswordHasher.cs` |
| SQL injection | EF Core parameterized queries — low risk. Raw SQL only in `PerformanceIndexes` (static DDL) + `DbSeeder` (seed SQL). | LOW | `PerformanceIndexes.cs` |
| File upload validation | Extension-only (PDF, images, Word) — no magic-byte/MIME check. Files in `wwwroot/uploads`. | MEDIUM | `DocumentsController` |
| XSS | React escapes by default; no `dangerouslySetInnerHTML` confirmed. | LOW | — |
| CSRF | JWT in Authorization header (not cookie) → CSRF not applicable. `credentials: include` set but no cookie auth. | LOW | — |
| Path traversal | Document paths constructed from uploads — `INFERRED` risk if filenames not sanitized. | MEDIUM | `DocumentsController` |
| Insecure deserialization | Newtonsoft.Json used; no `TypeNameHandling` confirmed. | LOW | — |
| Rate limiting | Login only (5/60s). No rate limiting on other endpoints. | LOW | `Program.cs` |
| Dependency risks | iText7 7.2.5 (older); Swashbuckle 6.4.0 (older). `INFERRED` — check CVEs. | MEDIUM | `.csproj` |
| Exposed internal services | Swagger UI enabled in Development only (root path). | LOW | `Program.cs` |
| Audit log integrity | Append-only (no update endpoint); but in same DB → tamperable by DB admin. | LOW | `AuditLogsController` |
| PII handling | Employee PII in plaintext (no at-rest encryption, no masking in non-prod). | MEDIUM | `Entities.cs` |
| License bypass | `LicenseMiddleware` skipped in Testing; in prod, runs before auth. If `SystemSettings` has valid key, all routes open. | LOW | `LicenseMiddleware.cs` |

## Security state summary

Significantly improved from the prior audit (secrets removed, default-deny auth, rate limiting, refresh tokens, CORS locked). Remaining concerns: SHA-256 password hashing, localStorage tokens, CompanySettings GET anonymous, project-level RBAC unenforced, file upload validation, no MFA. No penetration testing has been performed (`UNKNOWN`).