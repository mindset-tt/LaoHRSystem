# 27 — Decisions & Constraints

> `VERIFIED` from code + audit docs + repo memory.

## Important technical decisions

| Decision | Rationale | Evidence |
|---|---|---|
| .NET 10 + Next.js 16 + React 19 + Tailwind v4 | Modern, lightweight, current | `.csproj`, `package.json` |
| PostgreSQL (not SQL Server) | Switched at some point (repo memory); runtime uses Npgsql | `Program.cs`, repo memory |
| No UI library, no chart library, no state library, no axios | Deliberate lightweight stance — bundle ceiling controlled | `package.json`, audit `00` |
| EF Core (code-first) + EnsureCreated fallback | Migrations are SQL Server-flavored; EnsureCreated builds from model | `Program.cs`, `PerformanceIndexes.cs` |
| Raw SQL indexes via `PerformanceIndexes.cs` | Compensate for migration mismatch; PG `CREATE INDEX IF NOT EXISTS` | `PerformanceIndexes.cs` |
| JWT HS256 + server-side refresh tokens (rotation + replay detection) | Stateless access + revocable refresh | `Program.cs`, `RefreshTokenService` |
| Default-deny authorization (`FallbackPolicy`) | All endpoints require auth unless explicitly anonymous | `Program.cs:116` |
| Fire-and-forget audit (`Channel<AuditLog>` + `AuditLogWriter`) | Decouple audit from user transaction | `AuditLogInterceptor`, `AuditLogWriter` |
| License enforcement before auth (HTTP 402) | Commercial licensing | `LicenseMiddleware` |
| Custom RSA license system (offline generation) | Air-gapped deployment support | `LaoHR.LicenseGen`, `LicenseService` |
| ZKTeco biometric integration via separate Windows Service | Hardware isolation | `LaoHR.Bridge.Service` |
| Hand-rolled i18n (not next-intl/i18next) | Avoid dependency; simple en/lo | `lib/i18n.ts` |
| react-hook-form + zod (not formik) | Modern, lightweight | `package.json` |
| QuestPDF (Community) + iText7 for PDF | Different PDF needs (generation vs form fill) | `.csproj` |
| ClosedXML for Excel | Payroll export | `.csproj` |
| OpenTelemetry traces (optional OTLP) | Observability without forced collector | `Program.cs` |
| CSS Modules + CSS variables (not Tailwind utilities for design system) | Fine-grained control; dark mode | `globals.css` |
| Polymorphic comments (`EntityComment` with EntityType + EntityId) | Single comment system for 7 entity types | `Entities.cs`, `CommentsController` |

## Constraints

| Constraint | Detail | Evidence |
|---|---|---|
| Lao compliance | NSSF social security + Lao progressive PIT + LAK currency + Asia/Vientiane timezone | `PayrollService`, `TaxBracket`, `WorkSchedule` |
| Bilingual | Lao + English throughout (names, holidays, payslips, UI) | `Entities.cs`, `i18n.ts`, `PayslipPdfService` |
| License-gated | System unusable without valid license (HTTP 402) | `LicenseMiddleware` |
| ZKTeco hardware | Attendance sync depends on biometric devices | `LaoHR.Bridge.*` |
| .NET 10 SDK required | All projects target net10.0 | `.csproj` |
| Node 20+ + npm | Frontend build | `Dockerfile`, CI |
| PostgreSQL 16 | Runtime DB | `docker-compose.yml` |
| No git CLI on current machine | `git` not on PATH — history limited | terminal attempt |

## Assumptions embedded in code

- Single company (singleton `CompanySetting`) — no multi-tenancy.
- 3 hard roles (Admin/HR/Employee) — no fine-grained permissions.
- `DateTime.UtcNow` + legacy timestamp switch — no central timezone boundary.
- Demo users seeded in Dev/Testing only.
- SMTP host `smtp.example.com` = mock mode.
- License stored in `SystemSettings` key `LICENSE_KEY`; `public.key` file in working directory.