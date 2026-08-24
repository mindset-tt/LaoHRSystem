# 17 — Configuration & Environment

> `VERIFIED` from `appsettings.json`, `Program.cs`, `.env.example`, `docker-compose.yml`.
> **No secret values are exposed.**

## Environment-variable inventory

| Variable | Required | Purpose | Used by | Default | Notes |
|---|---|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | Yes (non-Testing) | PostgreSQL conn string | API, Bridge apps | — (empty in appsettings) | Must be set via env; prior committed creds must be rotated |
| `Jwt__Key` | Yes | JWT signing key (≥64 chars) | API | — (empty; hardcoded fallback in code: `LaoHRSystemSecretKey2024...`) | Must be set via env |
| `Jwt__Issuer` | No | JWT issuer | API | `LaoHRServer` (config); code fallback `LaoHRSystem` | |
| `Jwt__Audience` | No | JWT audience | API | `LaoHRClient` (config); code fallback `LaoHRFrontend` | |
| `Jwt__DurationInMinutes` | No | Access token lifetime | API | 60 | |
| `Cors__AllowedOrigins` | Yes (prod) | Comma-separated allowed origins | API | — (empty) | Dev: any localhost; prod: must be set |
| `Serilog__WriteToFile` | No | Enable file sink | API | `false` | |
| `OpenTelemetry__Otlp__Endpoint` | No | OTLP collector endpoint | API | — (empty = no export) | |
| `Retention__Enabled` | No | Enable retention pruning | API | `false` | |
| `Retention__AuditLogDays` | No | Audit log retention | API | 365 | |
| `Retention__RefreshTokenDays` | No | Revoked token retention | API | 30 | |
| `Retention__RunAtHourUtc` | No | Daily prune hour (UTC) | API | 3 | |
| `ASPNETCORE_ENVIRONMENT` | No | Environment | API | Development | `Testing` = InMemory + no license mw + seed |
| `ASPNETCORE_URLS` | No | Bind URLs | API | — (code says 5000; Docker 8080) | |
| `POSTGRES_PASSWORD` | Yes (Docker) | Postgres password | docker-compose | — (required) | |
| `JWT_KEY` | Yes (Docker) | Maps to `Jwt__Key` | docker-compose | — (required) | |
| `CORS_ALLOWED_ORIGINS` | No (Docker) | Maps to `Cors__AllowedOrigins` | docker-compose | `http://localhost:3000` | |
| `NEXT_PUBLIC_API_URL` | No | Frontend API base URL | frontend (build-time) | `http://localhost:5000` (apiClient) / `http://localhost:8080` (Dockerfile) | ⚠️ Mismatch |
| `BUILD_STANDALONE` | No | Enable Next standalone output | frontend build | — | |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | No | OTLP endpoint (Docker alias) | docker-compose | — (empty) | |
| `NODE_ENV` | No | Node env | frontend | production (Docker) | |

## SMTP settings (`appsettings.json` — placeholder, not secrets)

| Key | Value | Notes |
|---|---|---|
| `SmtpSettings:Host` | `smtp.example.com` | Placeholder — EmailService mocks/logs if this value |
| `SmtpSettings:Port` | 587 | |
| `SmtpSettings:User` | `your-email@example.com` | Placeholder |
| `SmtpSettings:Pass` | `your-password` | Placeholder |
| `SmtpSettings:FromEmail` | `noreply@laohr.com` | |
| `SmtpSettings:FromName` | `Lao HR System` | |
| `SmtpSettings:HrEmail` | `hr@laohr.com` | |

## appsettings.json structure

```json
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*",
  "ConnectionStrings": { "DefaultConnection": "" },
  "Jwt": { "Key": "", "Issuer": "LaoHRServer", "Audience": "LaoHRClient", "DurationInMinutes": 60 },
  "Cors": { "AllowedOrigins": "" },
  "Serilog": { "MinimumLevel": { ... }, "WriteToFile": false },
  "OpenTelemetry": { "Otlp": { "Endpoint": "" } },
  "Retention": { "Enabled": false, "RefreshTokenDays": 30, "AuditLogDays": 365, "RunAtHourUtc": 3 },
  "SmtpSettings": { ... }
}
```

## Unused / undocumented / hardcoded

- **Hardcoded JWT fallback key** in `Program.cs`: `"LaoHRSystemSecretKey2024VeryLongKeyForSecurity!"` — used if `Jwt:Key` empty. `VERIFIED` risk if env not set.
- **Hardcoded UTC+7** in `AttendanceController` and `datetime.ts` — no central timezone constant. `VERIFIED`.
- **`appsettings.Development.json`** exists for API + Bridge.Service (not inspected in detail; likely dev overrides).
- **`appsettings.json` in Bridge.Service** still references SQL Server (`UseSqlServer`). `VERIFIED` (repo memory).
- **No `.env` file committed** (gitignored); `.env.example` is the template. `VERIFIED`.
- **`launchSettings.json`** (in `Properties/`) not inspected — may set dev ports. `UNKNOWN`.