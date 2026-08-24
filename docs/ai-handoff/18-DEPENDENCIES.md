# 18 — Dependencies

> `VERIFIED` from `.csproj` + `package.json`.

## Backend dependencies (LaoHR.API)

| Package | Version | Purpose | Status | Notes |
|---|---|---|---|---|
| ClosedXML | 0.105.0 | Excel export | Active | Payroll export |
| System.IO.Packaging | 9.0.2 | OOXML support for ClosedXML | Active | |
| FluentValidation.AspNetCore | 11.3.1 | Input validation | Partial | Only LoginRequestValidator exists |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.1 | JWT auth | Active | |
| Microsoft.EntityFrameworkCore.Design | 10.0.1 | EF tooling | Active | |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.1 | Test DB | Active | Testing env |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.1 | Legacy migrations | ⚠️ Legacy | Runtime uses Npgsql; Bridge apps still use it |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.1 | PostgreSQL provider | Active | Runtime |
| Microsoft.EntityFrameworkCore.Tools | 10.0.1 | EF CLI | Active | |
| Newtonsoft.Json | 13.0.4 | JSON serialization | Active | AuditLog + license |
| QuestPDF | 2025.12.1 | PDF generation | Active | Community license |
| itext7 | 7.2.5 | PDF form fill | Active | ⚠️ Older; check CVEs |
| Swashbuckle.AspNetCore | 6.4.0 | Swagger/OpenAPI | Active | ⚠️ Older; Dev only |
| Serilog.AspNetCore | 9.0.0 | Logging | Active | |
| Serilog.Sinks.Console | 6.0.0 | Console sink | Active | |
| Serilog.Sinks.File | 6.0.0 | File sink | Active | Optional |
| Serilog.Formatting.Compact | 3.0.0 | JSON formatter | Active | |
| Serilog.Enrichers.Environment | 3.0.1 | MachineName | Active | |
| Serilog.Enrichers.Thread | 4.0.0 | ThreadId | Active | |
| AspNetCore.HealthChecks.NpgSql | 9.0.0 | Postgres health | Active | |
| OpenTelemetry.Extensions.Hosting | 1.9.0 | Telemetry host | Active | |
| OpenTelemetry.Instrumentation.AspNetCore | 1.9.0 | HTTP tracing | Active | |
| OpenTelemetry.Instrumentation.Http | 1.9.0 | HttpClient tracing | Active | |
| OpenTelemetry.Instrumentation.EntityFrameworkCore | 1.0.0-beta.12 | EF tracing | Active | Beta version |
| OpenTelemetry.Exporter.OpenTelemetryProtocol | 1.9.0 | OTLP export | Active | Optional |

## Backend dependencies (LaoHR.Shared)

| Package | Version | Purpose | Notes |
|---|---|---|---|
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.1 | DbContext base | ⚠️ Runtime uses Npgsql in API; Shared references SqlServer |
| Newtonsoft.Json | 13.0.4 | JSON | AuditLog/license |

## Backend dependencies (Bridge.Service / Bridge.Config)

| Package | Version | Purpose | Notes |
|---|---|---|---|
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.1 | DB access | ⚠️ Not migrated to Npgsql |
| Microsoft.Extensions.Hosting | 10.0.1 | Worker host | Bridge.Service |
| Microsoft.Extensions.Hosting.WindowsServices | 10.0.1 | Windows Service | Bridge.Service |

## Backend dependencies (LaoHR.Tests)

| Package | Version | Purpose |
|---|---|---|
| xunit | 2.9.3 | Test framework |
| xunit.runner.visualstudio | 3.1.4 | Test runner |
| Microsoft.NET.Test.Sdk | 17.14.1 | Test SDK |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.1 | Integration testing |
| Moq | 4.20.72 | Mocking |
| FluentAssertions | 8.8.0 | Assertions |
| Bogus | 35.6.5 | Fake data |
| coverlet.collector / coverlet.msbuild | 6.0.4 | Coverage |

## Frontend dependencies

| Package | Version | Purpose | Status |
|---|---|---|---|
| next | 16.1.1 | Framework | Active |
| react / react-dom | 19.2.3 | UI runtime | Active |
| react-hook-form | ^7.85.0 | Forms | Active |
| @hookform/resolvers | ^3.10.0 | Form resolvers | Active |
| zod | ^3.25.76 | Validation | Active |
| tailwindcss | ^4 | Styling | Active (CSS-based config) |
| @tailwindcss/postcss | ^4 | PostCSS plugin | Active |
| typescript | ^5 | Type safety | Active |
| eslint / eslint-config-next | ^9 / 16.1.1 | Linting | Active |
| babel-plugin-react-compiler | 1.0.0 | React Compiler | Active |
| @types/node / @types/react / @types/react-dom | ^20 / ^19 / ^19 | Types | Active |

## Dependency observations

- **Duplicated DB providers**: both `SqlServer` and `Npgsql` referenced. SqlServer is legacy (migrations + Bridge apps). `VERIFIED`.
- **No axios** — native fetch. `VERIFIED`.
- **No UI library, no chart library, no state library, no SWR/React Query, no date library, no virtualization library** — deliberate lightweight stance. `VERIFIED`.
- **itext7 7.2.5 + Swashbuckle 6.4.0** are older — `INFERRED` check for CVEs.
- **OpenTelemetry.Instrumentation.EntityFrameworkCore 1.0.0-beta.12** — beta version. `VERIFIED`.
- **Vendor lock-in**: ZKTeco SDK (hardware-specific), QuestPDF (Community license), ClosedXML, iText7. All replaceable but non-trivial.
- **No outdated architecture** detected — .NET 10 + Next 16 + React 19 are current. `VERIFIED`.

## Apparently unused

- `Class1.cs` in `LaoHR.Shared` — empty placeholder. `VERIFIED` (audit noted).
- `UnitTest1.cs` in `LaoHR.Tests` — coverage booster placeholder. `VERIFIED`.
- `frontend/public/*` default SVGs — not app-specific. `VERIFIED`.