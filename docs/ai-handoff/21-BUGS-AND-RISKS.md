# 21 — Bugs & Risks Register

> Severity: CRITICAL / HIGH / MEDIUM / LOW. Confidence: HIGH / MEDIUM / LOW.

| ID | Issue | Severity | Confidence | Impact | Evidence |
|---|---|---|---|---|---|
| BR-01 | `EmployeesController` ambiguous `[HttpGet("{id}")]` routes — `GetEmployee` and `GetDepartments`/`GetDepartment` may conflict | HIGH | HIGH | Routing errors / wrong data returned | `Controllers/EmployeesController.cs` |
| BR-02 | `CompanySettingsController` GET is `[AllowAnonymous]` — unauthenticated company info leak | MEDIUM | HIGH | Data exposure | `Controllers/CompanySettingsController.cs` |
| BR-03 | Prior production credentials committed (DB host `10.233.141.2`, `bi_owner`/`superset`, JWT key) — must be rotated | HIGH | HIGH | Unauthorized DB access | repo memory, git history |
| BR-04 | `PasswordHasher` SHA-256 — weak password hashing (no salt/work factor) | HIGH | HIGH | Credential compromise if DB leaks | `Services/PasswordHasher.cs` |
| BR-05 | `EnsureCreated()` fallback — schema drift; migrations unused for new entities | HIGH | HIGH | Silent schema inconsistency | `Program.cs:344-355` |
| BR-06 | Frontend mock data in dashboard activity feed — misleading UX | MEDIUM | HIGH | Users see fake activity | `(dashboard)/page.tsx:130-141` |
| BR-07 | Frontend mock data fallback in `employees/[id]/edit` — masks API failures | MEDIUM | HIGH | Stale/wrong data shown | `employees/[id]/edit/page.tsx:38` |
| BR-08 | Frontend mock documents in `employees/[id]` — placeholder docs | MEDIUM | HIGH | Misleading | `employees/[id]/page.tsx:59` |
| BR-09 | `proxy.ts` middleware redirect commented out — no frontend route protection at edge | MEDIUM | HIGH | Relies solely on client `useRequireAuth` | `src/proxy.ts` |
| BR-10 | No `/403` page — `useRequirePermission` redirects to non-existent route | MEDIUM | HIGH | Broken UX / 404 | `useRequirePermission`, route tree |
| BR-11 | Hardcoded JWT fallback key in `Program.cs` — used if env not set | MEDIUM | HIGH | Token forgery if deployed without env | `Program.cs` |
| BR-12 | Project-member roles not enforced — any authenticated user can edit any project | MEDIUM | MEDIUM | Unauthorized project changes | `ProjectsController`, `ProjectMember.Role` |
| BR-13 | File upload validation extension-only — no magic-byte/MIME check | MEDIUM | MEDIUM | Malicious file upload | `DocumentsController` |
| BR-14 | No `RowVersion` on `SalarySlip`/`LeaveBalance` — concurrent edits last-write-wins | MEDIUM | MEDIUM | Payroll/balance data corruption | `Entities.cs` |
| BR-15 | `PayrollController.ExportPayroll` O(slips×adjustments) loop — slow at scale | MEDIUM | MEDIUM | Export timeout on large periods | `PayrollController` (audit) |
| BR-16 | No rate limiting on endpoints except login | LOW | HIGH | Abuse of write endpoints | `Program.cs` |
| BR-17 | No frontend tests — regressions invisible | HIGH | HIGH | Quality regression | `frontend/` |
| BR-18 | PM/Finance/Knowledge controllers have no tests | HIGH | HIGH | Regression risk | `Tests/` |
| BR-19 | No reverse proxy / TLS in Docker | MEDIUM | HIGH | No HTTPS in prod | `docker-compose.yml` |
| BR-20 | No Postgres backup config | MEDIUM | HIGH | Data loss | `docker-compose.yml` |
| BR-21 | CI triggers on `main`/`dev` but branch is `master` — CI may not run | LOW | MEDIUM | CI silently inactive | `.github/workflows/ci.yml` |
| BR-22 | iText7 7.2.5 / Swashbuckle 6.4.0 older versions — potential CVEs | LOW | MEDIUM | Security vulnerabilities | `.csproj` |
| BR-23 | `DbSeeder.SeedAddresses` fails on PostgreSQL (SQL Server syntax) — address data not seeded | LOW | HIGH | Provinces/districts/villages empty on PG | `Data/DbSeeder.cs`, repo memory |
| BR-24 | No global exception handler — unhandled exceptions leak stack traces in dev | LOW | MEDIUM | Info disclosure / poor UX | `Program.cs` |
| BR-25 | No mobile/responsive verification — sidebar likely broken on mobile | MEDIUM | LOW | Poor mobile UX | frontend structure |