# 28 — Do-Not-Break Contracts

> What a future AI must avoid breaking. `VERIFIED` from code.

## DO NOT BREAK

| Contract | Detail | Evidence | Why |
|---|---|---|---|
| **API route structure** | `/api/{controller}/...` conventions; ~150 endpoints across 28 controllers | `09-API-INVENTORY.md` | Frontend depends on exact routes |
| **PaginatedResponse<T> envelope** | `{ data, total, page, pageSize }` shape | `lib/types/pagination.ts`, controllers | Frontend pagination expects this |
| **JWT auth model** | HS256, claims: `UserId`, `Username`, `Role`; `Jwt:Issuer`/`Audience`/`Key` config | `Program.cs`, `AuthController` | All tokens + frontend depend on it |
| **Refresh token rotation + replay detection** | SHA-256 hashed, one-time use, family revocation | `RefreshTokenService`, `RefreshToken` entity | Security model; breaking = vulnerability |
| **Default-deny authorization** | `FallbackPolicy = RequireAuthenticatedUser()`; `[AllowAnonymous]` whitelist | `Program.cs:116` | Security boundary |
| **License enforcement** | `LicenseMiddleware` before auth; HTTP 402; `SystemSettings` LICENSE_KEY; `public.key` file | `LicenseMiddleware`, `LicenseService` | Commercial model |
| **EF entity model** | 40 entity classes in `Entities.cs`; `LaoHRDbContext` DbSets + `OnModelCreating` seed | `Entities.cs`, `LaoHRDbContext.cs` | Schema + migrations depend on it |
| **Audit pipeline** | `AuditLogInterceptor` → `Channel<AuditLog>` → `AuditLogWriter` (fire-and-forget, separate scope) | `Data/AuditLogInterceptor.cs`, `Services/AuditLogWriter.cs` | Decoupling; breaking = user-write rollback risk |
| **Npgsql + legacy timestamp switch** | `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` before `CreateBuilder` | `Program.cs`, repo memory | DateTime mapping; breaking = seed/runtime failures |
| **PerformanceIndexes raw SQL** | PG `CREATE INDEX IF NOT EXISTS` applied on startup | `PerformanceIndexes.cs` | Compensates for migration mismatch |
| **Payroll calculation engine** | NSSF + progressive tax + multi-currency + adjustments in `PayrollService` | `PayrollService.cs` | Business-critical domain logic |
| **Payslip PDF format** | QuestPDF bilingual (Lao/English) "Ryobi style" + Phetsarath font | `PayslipPdfService.cs` | Compliance/document format |
| **Bank transfer file formats** | BCEL (text H/D/T records) + LDB (CSV) | `BankTransferService.cs` | Bank integration |
| **NSSF/LSSO PDF form fill** | iText7 form fill on `PDF/LSSO_Payment_Form.pdf` template | `PdfFormService.cs` | Government form |
| **Lao progressive tax brackets** | `TaxBracket` seeded in `OnModelCreating` | `LaoHRDbContext.cs` | Compliance |
| **Leave accrual + carry-over logic** | Monthly accrual (1st), year-end carry-over (Jan 1) in `LeaveScheduledJobsService` + `LeaveService` | `Jobs/`, `LeaveService.cs` | Business rule |
| **Work schedule singleton** | `WorkSchedule` single row; Saturday config; `StandardMonthlyHours=160` | `WorkSchedule` entity | Payroll calc depends on it |
| **i18n dictionary structure** | `lib/i18n.ts` nested `en`/`lo` object; `t` typed accessor | `lib/i18n.ts` | All UI strings |
| **Frontend permission matrix** | `lib/permissions.ts` Admin/HR/Employee → permission arrays | `lib/permissions.ts` | UX gating |
| **apiClient token storage** | `localStorage` keys: `accessToken`, `tokenExpiresAt`, `refreshToken`, `refreshTokenExpiresAt` | `lib/apiClient.ts` | Auth state |
| **CSS variable design tokens** | `globals.css` `--color-*`, `data-theme` attribute for dark mode | `globals.css`, `ThemeProvider` | All component styling |
| **Polymorphic comment entity types** | PROJECT/TASK/ISSUE/EXPENSE/LOAN/RISK/RESOURCE | `EntityComment`, `CommentsController` | Comment system integrity |
| **ZKTeco bridge architecture** | Separate Windows Service + WPF config app | `LaoHR.Bridge.*` | Hardware integration isolation |
| **Docker compose service names + healthchecks** | `postgres`/`api`/`web`; `depends_on` conditions | `docker-compose.yml` | Deployment |
| **CI env conventions** | `ASPNETCORE_ENVIRONMENT=Testing` = InMemory + no license mw + seed | `Program.cs`, `ci.yml` | Test isolation |

## When to modify vs NOT modify

- **Do NOT** change API response shapes without updating frontend endpoint modules.
- **Do NOT** change `Jwt:Issuer`/`Audience` without updating all token validation + frontend.
- **Do NOT** remove `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true)` — breaks seed.
- **Do NOT** change `PerformanceIndexes` to EF migrations without ensuring PG compatibility.
- **Do NOT** alter `PayrollService` tax/NSSF formulas without confirming Lao compliance.
- **Do NOT** change payslip PDF layout without stakeholder approval (document format).
- **Do NOT** rename `localStorage` token keys without migration path for existing sessions.
- **Do NOT** change `EntityComment` entity type strings without updating all callers.
- **DO** add new endpoints/controllers/services without breaking existing ones.
- **DO** add migrations (PG-compatible) carefully — test against existing data.