# 03 — Technology Stack

> Versions `VERIFIED` from `package.json`, `.csproj` files, and `docker-compose.yml`.

## Stack matrix

| Layer | Technology | Version | Location | Purpose | Status |
|---|---|---|---|---|---|
| **Language (backend)** | C# | .NET 10 | all `*.csproj` target `net10.0` (Bridge.Config + Tests target `net10.0-windows`) | API, shared lib, workers, tests | Active |
| **Language (frontend)** | TypeScript | ^5 | `frontend/package.json`, `tsconfig.json` | Frontend type safety (`strict: true`) | Active |
| **Backend framework** | ASP.NET Core | 10.0 | `LaoHR.API` | REST API | Active |
| **ORM** | EF Core | 10.0.1 | `LaoHR.API`, `LaoHR.Shared` | Data access | Active |
| **DB provider (runtime)** | Npgsql EF Core PostgreSQL | 10.0.1 | `LaoHR.API/Program.cs` `UseNpgsql` | PostgreSQL | Active |
| **DB provider (tests)** | EF Core InMemory | 10.0.1 | `Program.cs` (Testing env) | Test isolation | Active |
| **DB provider (referenced, legacy)** | EF Core SqlServer | 10.0.1 | all `.csproj`s | Legacy migrations + Bridge apps still use SqlServer | ⚠️ Partial/legacy |
| **Database** | PostgreSQL | 16-alpine (docker-compose) | runtime DB | Primary store | Active |
| **Cache** | In-memory `MemoryCache` | built-in | `Program.cs` `AddMemoryCache` | License key cache, address cache | Active |
| **Search** | None | — | — | No full-text/global search | ✗ Missing |
| **Auth** | JWT Bearer (HS256) | 10.0.1 (`JwtBearer`) | `Program.cs` | Token auth, 60-min access tokens | Active |
| **Refresh tokens** | Custom `RefreshToken` entity + `RefreshTokenService` | — | `Entities.cs`, `Services/RefreshTokenService.cs` | SHA-256 hashed, rotated, 14-day, replay detection | Active |
| **Authz** | Role-based (Admin/HR/Employee) + default-deny fallback policy | — | `Program.cs` `FallbackPolicy.RequireAuthenticatedUser()` | RBAC | Active |
| **Rate limiting** | `Microsoft.AspNetCore.RateLimiting` | built-in | `Program.cs` `auth-login` policy | 5/60s per IP on login | Active |
| **Validation** | FluentValidation | 11.3.1 | `Validators/` | Input validation (only `LoginRequestValidator` currently) | Partial |
| **PDF (payslip/report)** | QuestPDF | 2025.12.1 | `Services/PayslipPdfService.cs`, `NssfReportService.cs` | PDF generation (Community license) | Active |
| **PDF (form fill)** | iText7 | 7.2.5 | `Services/PdfFormService.cs` | LSSO/NSSF form fill | Active |
| **Excel** | ClosedXML | 0.105.0 | `PayrollController` export | .xlsx export | Active |
| **Swagger** | Swashbuckle | 6.4.0 | `Program.cs` | OpenAPI docs (Dev only, root path) | Active |
| **Logging** | Serilog | 9.0.0 (AspNetCore) + sinks | `Program.cs` | Structured JSON console + optional rolling file | Active |
| **Telemetry** | OpenTelemetry | 1.9.0 (hosting/instrumentation) | `Program.cs` | HTTP/EF tracing, OTLP export (optional) | Active |
| **Health checks** | `AspNetCore.HealthChecks.NpgSql` | 9.0.0 | `Program.cs` | `/health/live`, `/health/ready` | Active |
| **Background jobs** | `BackgroundService` (built-in) | — | `Jobs/LeaveScheduledJobsService.cs`, `Jobs/RetentionService.cs` | Leave accrual + retention pruning | Active |
| **Audit** | `AuditLogInterceptor` + `AuditLogChannel` (fire-and-forget) | — | `Data/AuditLogInterceptor.cs`, `Services/AuditLogWriter.cs` | EF save interceptor → channel → background writer | Active |
| **Frontend framework** | Next.js | 16.1.1 | `frontend/package.json` | App Router, Turbopack, react-compiler | Active |
| **UI library** | React | 19.2.3 | `frontend/package.json` | UI runtime | Active |
| **Styling** | Tailwind CSS | ^4 (`@tailwindcss/postcss`) | `frontend/postcss.config.mjs`, `globals.css` | Utility CSS (but design tokens are hand-authored CSS vars) | Active |
| **Forms** | react-hook-form + @hookform/resolvers | ^7.85 / ^3.10 | `frontend/package.json` | Form state + validation | Active |
| **Validation (FE)** | zod | ^3.25.76 | `frontend/package.json` | Schema validation | Active |
| **HTTP client** | Native `fetch` (custom `apiClient.ts`) | — | `frontend/src/lib/apiClient.ts` | Token mgmt + auto-refresh + retry | Active (no axios) |
| **State management** | React Context (Auth, Theme, Language, Toast) | — | `frontend/src/components/providers/` | Cross-cutting state | Active (no Redux/Zustand) |
| **Server-state cache** | None | — | — | No SWR/React Query | ✗ Missing |
| **Charts** | None | — | — | No charting library | ✗ Missing |
| **i18n** | Custom hand-rolled dictionary (`en`/`lo`) | — | `frontend/src/lib/i18n.ts` | Bilingual | Active (incomplete) |
| **Testing (backend)** | xUnit | 2.9.3 | `LaoHR.Tests` | Unit + integration | Active |
| **Testing (backend mocks)** | Moq | 4.20.72 | `LaoHR.Tests` | Mocking | Active |
| **Testing (backend assertions)** | FluentAssertions | 8.8.0 | `LaoHR.Tests` | Assertions | Active |
| **Testing (backend data)** | Bogus | 35.6.5 | `LaoHR.Tests` | Fake data generation | Active |
| **Testing (backend coverage)** | coverlet | 6.0.4 | `LaoHR.Tests` | Cobertura coverage | Active |
| **Testing (frontend)** | None | — | — | No Vitest/RTL/Playwright | ✗ Missing |
| **Containerization** | Docker | — | `Dockerfile` (api + web), `docker-compose.yml` | Multi-stage builds, compose stack | Active |
| **CI/CD** | GitHub Actions | — | `.github/workflows/ci.yml` | Build + test + docker smoke | Active |
| **Deployment targets** | Docker (compose) | — | `docker-compose.yml` | postgres + api (8080) + web (3000) | Active |
| **Hardware integration** | ZKTeco SDK | — | `LaoHR.Bridge.Config/ZkDevice.cs` | Biometric fingerprint devices | Active (Bridge apps) |
| **License** | Custom RSA license system | — | `LaoHR.LicenseGen`, `LaoHR.Shared/Services/LicenseService.cs`, `Middleware/LicenseMiddleware.cs` | License-key enforcement (HTTP 402 if invalid) | Active |
| **Error tracking** | None | — | — | No Sentry/etc. | ✗ Missing |
| **Monitoring** | OpenTelemetry (optional OTLP) + health checks | — | `Program.cs` | Traces + readiness | Partial |

## Notable technology decisions

- **No UI library, no chart library, no state library, no axios** — deliberate lightweight stance (`VERIFIED` in audit + `package.json`). All UI components are hand-built with CSS Modules.
- **EF Core migrations are SQL Server-flavored** while runtime uses Npgsql — `VERIFIED`. `Program.cs` tries `Migrate()` first, falls back to `EnsureCreated()`. `PerformanceIndexes.cs` applies raw PostgreSQL `CREATE INDEX IF NOT EXISTS` on startup to compensate.
- **JWT uses HS256 (symmetric)** with key from `Jwt:Key` config. `VERIFIED`.
- **Audit is fire-and-forget** via `Channel<AuditLog>` + `AuditLogWriter` background service — `VERIFIED` (decouples audit from user transaction).
- **License enforcement runs before auth** in the middleware pipeline — `VERIFIED`.