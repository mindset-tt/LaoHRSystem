# 05 — Runtime and Data Flow

> `VERIFIED` from `Program.cs` (startup) + controller/service code (flows).

## Startup sequence (backend)

```mermaid
flowchart TD
    Start[dotnet run / Docker] --> SerilogBoot[Bootstrap Serilog logger]
    SerilogBoot --> Builder[CreateBuilder]
    Builder --> AppCtx[AppContext.SetSwitch Npgsql legacy timestamp behavior]
    AppCtx --> Config[Load appsettings.json + env vars]
    Config --> SerilogCfg[Configure Serilog from config]
    SerilogCfg --> DI[DI registrations]
    DI --> AuthDI[AddAuthentication JwtBearer + FallbackPolicy default-deny]
    AuthDI --> RateLimit[AddRateLimiter auth-login policy]
    RateLimit --> Cors[AddCors AllowFrontend dev=any localhost / prod=allow-list]
    Cors --> Health[AddHealthChecks AddNpgSql]
    Health --> Otel[AddOpenTelemetry tracing HTTP/EF + OTLP optional]
    Otel --> Swagger[AddSwaggerGen v1 + JWT security def]
    Swagger --> QuestPDF[QuestPDF Community license]
    QuestPDF --> ServicesDI[Register services: PayrollService, PayslipPdfService, BankTransfer, NssfReport, PdfForm, Email, CompanySettings, Address, Leave, WorkDay, RefreshToken, LicenseCache, AuditChannel, AuditWriter, LeaveJobs, Retention]
    ServicesDI --> Build[Build app]
    Build --> Pipe[Middleware pipeline]
    Pipe --> P1[Swagger Dev only]
    P1 --> P2[UseCors AllowFrontend]
    P2 --> P3[UseRateLimiter]
    P3 --> P4[UseSerilogRequestLogging]
    P4 --> P5[UseMiddleware LicenseMiddleware skip in Testing]
    P5 --> P6[UseAuthentication]
    P6 --> P7[UseAuthorization]
    P7 --> P8[MapControllers]
    P8 --> P9[MapHealthChecks /health/live /health/ready]
    P9 --> Init[DB initialization]
    Init --> InitTest{Testing?}
    InitTest -->|yes| Ensure[EnsureCreated + DbSeeder.Seed]
    InitTest -->|no| Migrate[Migrate fallback EnsureCreated + DbSeeder.Seed dev/test + PerformanceIndexes.Apply]
    Ensure --> Run[Run on http://localhost:5000 or ASPNETCORE_URLS]
    Migrate --> Run
```

## Startup sequence (frontend)

```mermaid
flowchart TD
    Start[npm run dev / node server.js] --> Next[Next.js bootstrap]
    Next --> Layout[Root layout: ThemeProvider → LanguageProvider → ToastProvider → AuthProvider]
    Layout --> Route{Route}
    Route -->|/login| LoginPage[Login page]
    Route -->|/(dashboard)/*| DashLayout[(dashboard) layout useRequireAuth]
    DashLayout -->|not authed| RedirectLogin[Redirect to /login]
    DashLayout -->|authed| Page[Render page]
    Page --> Fetch[useEffect → endpoint module → apiClient → backend]
```

`AuthProvider` on mount: checks `hasValidToken()` from `localStorage`; if invalid, attempts one refresh; then fetches `/api/auth/me` for user info.

## Major user-flow traces

### Login flow

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Frontend (login page)
    participant API as /api/auth/login
    participant DB as PostgreSQL
    U->>FE: submit username/password
    FE->>API: POST {username, password}
    Note over API: Rate-limited 5/60s per IP<br/>LoginRequestValidator validates
    API->>DB: find AppUser by username
    API->>DB: PasswordHasher.Verify (SHA-256)
    API->>DB: issue RefreshToken (SHA-256 hash, 14d)
    API-->>FE: {token, expiresAt, refreshToken, refreshExpiresAt, username, role, displayName}
    FE->>FE: store tokens in localStorage + AuthContext
    FE-->>U: redirect to /
```

### Clock-in (attendance)

```mermaid
sequenceDiagram
    participant E as Employee
    participant FE as Attendance page
    participant API as /api/attendance/clock-in
    participant WS as WorkDayService
    participant DB
    E->>FE: click Clock In (geolocation)
    FE->>API: POST {lat, long, method}
    API->>DB: load WorkSchedule + today's Holiday
    API->>WS: IsWorkDay(today) / IsHoliday
    API->>DB: check existing attendance today (unique idx)
    API->>DB: create Attendance (ClockIn, lat/long, IsLate vs threshold)
    API-->>FE: 200 attendance record
```

### Payroll run

```mermaid
sequenceDiagram
    participant HR as HR/Admin
    participant FE as Payroll page
    participant API as /api/payroll/periods/{id}/run
    participant PS as PayrollService
    participant DB
    HR->>FE: click Run Payroll
    FE->>API: POST periods/{id}/run
    API->>PS: ProcessPayrollAsync(periodId)
    PS->>DB: load employees, ConversionRates, TaxBrackets, WorkSchedule, NSSF settings, PayrollAdjustments
    loop per active employee
        PS->>PS: convert salary to LAK via rate
        PS->>PS: compute NSSF employee + employer
        PS->>PS: progressive tax via brackets
        PS->>PS: apply adjustments
        PS->>PS: compute NetSalary
    end
    PS->>DB: upsert SalarySlips (status CALCULATED)
    PS->>DB: AuditLog via interceptor → channel
    API-->>FE: 200 period updated
```

### Leave request + approval

```mermaid
sequenceDiagram
    participant E as Employee
    participant FE as Leave page
    participant API as /api/leave
    participant LS as LeaveService
    participant DB
    E->>FE: submit leave request (multipart form + attachment)
    FE->>API: POST /api/leave
    API->>DB: create LeaveRequest (status PENDING)
    API-->>FE: 201
    Note over FE: HR sees in Approvals tab
    FE->>API: POST /api/leave/{id}/approve
    API->>DB: update status APPROVED + ApprovedById + ApprovedAt
    API->>LS: update LeaveBalance.UsedDays
    API->>DB: AuditLog via interceptor → channel
    API-->>FE: 200
```

### Create project task

```mermaid
sequenceDiagram
    participant U as User
    participant FE as Project detail page
    participant API as /api/projects/{projectId}/projecttasks
    participant DB
    U->>FE: create task (board/list tab)
    FE->>API: POST {title, status, priority, assigneeIds, dueDate}
    API->>DB: create ProjectTask + TaskAssignees
    API->>DB: create ActivityLog entry
    API->>DB: AuditLog via interceptor → channel
    API-->>FE: 201 task
    FE->>FE: refresh task list/board
```

### Token refresh

```mermaid
sequenceDiagram
    participant FE as apiClient
    participant API as /api/auth/refresh
    participant RTS as RefreshTokenService
    participant DB
    FE->>API: POST {refreshToken}
    API->>RTS: rotate
    RTS->>DB: find token by SHA-256 hash
    alt token valid & not revoked & not expired
        RTS->>DB: revoke old token + issue new (ReplacedByHash)
        RTS-->>API: new access + refresh
        API-->>FE: 200 {token, refreshToken, ...}
    else replay detected (already revoked + has ReplacedByHash)
        RTS->>DB: revoke entire family
        API-->>FE: 401
    else invalid/expired
        API-->>FE: 401
    end
```

## Shutdown behavior

- Backend: Serilog `Log.CloseAndFlush()` in finally block around host run. `VERIFIED`.
- Background services (`LeaveScheduledJobsService`, `RetentionService`, `AuditLogWriter`) use default `BackgroundService` cancellation via host shutdown token. `VERIFIED`.