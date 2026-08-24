using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LaoHR.API.Data;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using LaoHR.API.Services;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using LaoHR.API.Metrics;

// Phase 6a — Serilog as the host logger.
// Configuration is sourced from `Serilog` section in appsettings; a fallback
// in-code setup guarantees structured logs even when configuration is missing
// (e.g. first boot, mis-configured env). File sink writes daily rolling JSON
// files under `Logs/` and is opt-in for non-Development via the
// `Serilog:WriteToFile` flag.
// Phase 3B — use CreateLogger() (not CreateBootstrapLogger()) so the static
// Log.Logger is a plain Logger, not a frozen ReloadableLogger. This avoids
// "The logger is already frozen" when WebApplicationFactory re-runs the entry
// point during integration testing.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    // Migration compatibility (Phase 4D.1): the existing migration chain embeds
    // seed data with DateTimeKind.Unspecified literals (e.g. holidays), which
    // Npgsql rejects for timestamptz outside legacy mode. Operators set
    // NPGSQL_LEGACY_TIMESTAMP=1 so RUNTIME startup migration can apply the same
    // chain `dotnet ef database update` applies (see LaoHRDbContextFactory and
    // scripts/validate-postgres.ps1). Must be set before any Npgsql use.
    if (string.Equals(
            Environment.GetEnvironmentVariable("NPGSQL_LEGACY_TIMESTAMP"), "1",
            StringComparison.OrdinalIgnoreCase))
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, lc) =>
{
    lc
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithProperty("Application", "LaoHR.API")
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .WriteTo.Console(new RenderedCompactJsonFormatter());

    // Optional file sink — daily rolling JSON, lightweight (no Seq / no Elasticsearch
    // dependency). Operators can tail `Logs/laohr-*.json` or ship via Filebeat.
    if (ctx.Configuration.GetValue("Serilog:WriteToFile", false))
    {
        lc.WriteTo.File(
            formatter: new CompactJsonFormatter(),
            path: "Logs/laohr-.json",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            shared: true);
    }

    // OTLP exporter — opt-in. When `OpenTelemetry:Otlp:Endpoint` is set, traces
    // ship there. When unset, the OTel SDK is still wired but only exports to
    // the console for local debugging.
    var otlpEndpoint = ctx.Configuration["OpenTelemetry:Otlp:Endpoint"];
    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
    {
        // Traces are exported via the OpenTelemetry pipeline; Serilog stays
        // focused on logs. Keep this branch lightweight and side-effect free.
    }
});

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// JWT Configuration
// Phase 3A — fail-fast: in Production, the JWT key MUST be supplied via configuration.
// The hardcoded fallback is only acceptable in Development/Testing.
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LaoHRServer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LaoHRClient";

if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
    {
        throw new InvalidOperationException(
            "JWT signing key (Jwt:Key) is missing. " +
            "Set it via environment variable 'Jwt__Key' (minimum 64 characters). " +
            "Production/Staging MUST NOT use the hardcoded fallback key.");
    }
    // Development/Testing fallback only — never use in production
    jwtKey = "LaoHRSystemSecretKey2024VeryLongKeyForSecurity!";
}

// Phase 4D — production startup config validation (fail fast).
if (builder.Environment.IsProduction() || builder.Environment.IsStaging())
{
    if (jwtKey.Length < 64)
        throw new InvalidOperationException(
            "JWT signing key (Jwt:Key) must be at least 64 characters in Production/Staging.");

    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is missing. Set it via environment variable 'ConnectionStrings__DefaultConnection'.");

    var origins = builder.Configuration["Cors:AllowedOrigins"];
    if (string.IsNullOrWhiteSpace(origins))
        throw new InvalidOperationException(
            "Cors:AllowedOrigins is missing. Set it to a comma-separated allow-list of trusted origins.");
}

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

// FluentValidation
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Default-deny: every controller/action without an explicit [Authorize] or
// [AllowAnonymous] is still required to be opted-in. We achieve this by
// registering a fallback policy and applying it to all controller endpoints.
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Rate limiter on /api/auth/login: 5 attempts per 60 seconds per IP, sliding window.
// Disabled in Testing so integration tests (which share one IP) are not throttled.
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Phase 4D.1 — standard 429 metadata: advertise when the client may retry.
        // Built-in limiters don't populate RetryAfter metadata on rejection;
        // fall back to the configured window length (worst-case wait).
        options.OnRejected = static (context, _) =>
        {
            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter =
                    ((TimeSpan)retryAfter).TotalSeconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
            }
            return ValueTask.CompletedTask;
        };

        // Login: 5 attempts / 60s per IP (sliding window).
        options.AddPolicy("auth-login", context =>
        {
            var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromSeconds(60),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
        });

        // Refresh: 30 attempts / 60s per IP (higher than login; rotation is frequent).
        options.AddPolicy("auth-refresh", context =>
        {
            var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromSeconds(60),
                SegmentsPerWindow = 6,
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                AutoReplenishment = true
            });
        });
    });
}

// License key cache (5 minutes) — the license middleware reads this instead of
// hitting the DB on every request.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LaoHR.API.Services.ILicenseKeyCache, LaoHR.API.Services.LicenseKeyCache>();

// Phase 6g — Health checks for liveness/readiness probes.
// /health/live  = process is up (no dependencies)
// /health/ready = DB connection is usable (Postgres)
builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionStringFactory: _ => builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty,
        name: "postgres",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: new[] { "db", "ready" });

// Phase 6b — OpenTelemetry tracing.
// ASP.NET Core + EF Core + outbound HttpClient instrumented. Exporter:
// OTLP when `OpenTelemetry:Otlp:Endpoint` is configured; console otherwise.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(rb => rb.AddService("LaoHR.API"))
    .WithTracing(tb =>
    {
        tb.AddAspNetCoreInstrumentation(opts =>
            {
                // Don't trace health checks — high-frequency, low-value.
                opts.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/health");
            })
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation();

        var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tb.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
    })
    // Phase 4D.1 — metrics. HTTP request count/duration, runtime (GC/threads/
    // memory), and process CPU/memory via built-in meters; LaoHR operational
    // counters via AppMetrics. Exported on a local Prometheus pull endpoint
    // (/metrics). The endpoint is network-internal: the API container port is
    // not published to the host in the production compose topology.
    .WithMetrics(mb =>
    {
        // ASP.NET Core request metrics are emitted by the framework itself
        // ("Microsoft.AspNetCore.Hosting" / "...Kestrel"); no extra call needed.
        mb.AddRuntimeInstrumentation()
          .AddProcessInstrumentation()
          .AddMeter("Microsoft.AspNetCore.Hosting")
          .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
          .AddMeter(AppMetrics.MeterName);

        mb.AddPrometheusExporter();
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo { Title = "Lao HR API", Version = "v1" });
    
    // Authorization
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

// Database - using SQL Server
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<LaoHR.API.Data.IAuditLogChannel, LaoHR.API.Data.AuditLogChannel>();
builder.Services.AddScoped<AuditLogInterceptor>();
builder.Services.AddHostedService<LaoHR.API.Data.AuditLogWriter>();

// Database Configuration
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<LaoHRDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDbForTesting"));
}
else
{
    builder.Services.AddDbContext<LaoHRDbContext>((sp, options) => {
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("LaoHR.API"))
           .AddInterceptors(sp.GetRequiredService<AuditLogInterceptor>());
    });
}

// Custom services
builder.Services.AddScoped<PayrollService>();
builder.Services.AddScoped<IComplianceRuleService, ComplianceRuleService>();
builder.Services.AddScoped<IOrganizationHierarchyService, OrganizationHierarchyService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ICurrentEmployeeService, CurrentEmployeeService>();
builder.Services.AddScoped<IDataScopeService, DataScopeService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IProjectAccessService, ProjectAccessService>();
builder.Services.AddScoped<IPmPlanningService, PmPlanningService>();
builder.Services.AddScoped<IRecruitmentAccessService, RecruitmentAccessService>();
builder.Services.AddScoped<IHireConversionService, HireConversionService>();
builder.Services.AddScoped<IPerformanceAccessService, PerformanceAccessService>();
builder.Services.AddScoped<PayslipPdfService>();
builder.Services.AddScoped<IBankTransferService, BankTransferService>();
builder.Services.AddScoped<NssfReportService>();
builder.Services.AddScoped<PdfFormService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICompanySettingsService, CompanySettingsService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IWorkDayService, WorkDayService>();

// Phase 4A — Back Office services
builder.Services.AddScoped<INumberSequenceService, NumberSequenceService>();
builder.Services.AddScoped<IBackOfficeAccessService, BackOfficeAccessService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();

// Phase 4B — Finance + Accounting services
builder.Services.AddScoped<IFinanceAccessService, FinanceAccessService>();
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<IAccountsPayableService, AccountsPayableService>();

// Phase 4B.1 — Accounting configuration + posting
builder.Services.AddScoped<IAccountingConfigurationService, AccountingConfigurationService>();
builder.Services.AddScoped<IPostingService, PostingService>();
builder.Services.AddScoped<ISegregationOfDutiesService, SegregationOfDutiesService>();

// Phase 4B.2 — Finance report export (CSV + Excel)
builder.Services.AddScoped<IFinanceExportService, FinanceExportService>();

// Phase 4C — Corporate Operations
builder.Services.AddScoped<ICorporateOperationsAccessService, CorporateOperationsAccessService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IContractLifecycleService, ContractLifecycleService>();
builder.Services.AddScoped<IFleetService, FleetService>();

// Phase 6c — Refresh-token rotation service.
builder.Services.AddScoped<LaoHR.API.Services.IRefreshTokenService, LaoHR.API.Services.RefreshTokenService>();

// Background Jobs
builder.Services.AddHostedService<LaoHR.API.Jobs.LeaveScheduledJobsService>();
builder.Services.AddHostedService<LaoHR.API.Jobs.RetentionService>();


// CORS for frontend — locked to a known-origin allow-list.
// In Development any origin is allowed for convenience; in non-Development the
// allow-list is sourced from configuration (comma-separated), so misconfiguration
// defaults to "no origins" instead of "any origin".
var corsOriginsConfig = builder.Configuration["Cors:AllowedOrigins"] ?? string.Empty;
var corsOrigins = corsOriginsConfig
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // Dev convenience: reflect any localhost / 127.0.0.1 origin. Do NOT
            // blanket-allow arbitrary hosts even in dev.
            policy.SetIsOriginAllowed(origin =>
            {
                if (string.IsNullOrEmpty(origin)) return false;
                if (origin.StartsWith("http://localhost:", StringComparison.OrdinalIgnoreCase)) return true;
                if (origin.StartsWith("http://127.0.0.1:", StringComparison.OrdinalIgnoreCase)) return true;
                return corsOrigins.Contains(origin);
            });
        }
        else
        {
            policy.WithOrigins(corsOrigins.ToArray());
        }

        policy.AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
              .WithExposedHeaders("Content-Disposition");
    });
});

// Phase 3A — global exception handling with RFC 7807 ProblemDetails
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<LaoHR.API.Middleware.GlobalExceptionHandler>();

var app = builder.Build();

// Configure pipeline
// Phase 3A — global exception handler (must be early in pipeline)
app.UseExceptionHandler();

// Phase 4D — forwarded headers (behind a TLS-terminating reverse proxy).
// Only trust the configured known proxy/network; do not blindly trust arbitrary
// X-Forwarded-* headers from any client.
var knownProxy = builder.Configuration["ForwardedHeaders:KnownProxy"];
if (!string.IsNullOrWhiteSpace(knownProxy))
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        KnownProxies = { System.Net.IPAddress.Parse(knownProxy) },
    });
}

// Phase 4D — HTTPS redirection (only meaningful when not already behind a proxy
// that terminates TLS; harmless otherwise).
if (!builder.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Phase 4D — security headers.
app.UseMiddleware<LaoHR.API.Middleware.SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Lao HR API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowFrontend");
if (!builder.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

// Phase 6a — Serilog request logging with structured enrichment.
// One log line per HTTP request; never logs Authorization headers.
app.UseSerilogRequestLogging(opts =>
{
    opts.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    opts.EnrichDiagnosticContext = (diag, http) =>
    {
        diag.Set("RequestHost", http.Request.Host.Value);
        diag.Set("RequestScheme", http.Request.Scheme);
        diag.Set("UserAgent", http.Request.Headers.UserAgent.ToString());
        diag.Set("ClientIP", http.Connection.RemoteIpAddress?.ToString() ?? "unknown");
    };
});

// License check BEFORE authentication: an expired/invalid license should not
// let a request get a JWT issued.
if (!builder.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<LaoHR.API.Middleware.LicenseMiddleware>();
}

// Phase 4D.1 — Prometheus scrape endpoint (/metrics). Network-restricted by
// topology (API port not published to the host; reverse proxy does not route
// /metrics). Operators must keep it off untrusted interfaces.
if (!builder.Environment.IsEnvironment("Testing"))
{
    app.MapPrometheusScrapingEndpoint("/metrics").AllowAnonymous();
}

// Authenticate & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
// Phase 6g — liveness vs readiness split.
// Liveness: process is up (no deps). Readiness: DB reachable.
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // no checks → always healthy if the process is alive
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
}).AllowAnonymous();

// Seed / migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
    var isTesting = builder.Environment.IsEnvironment("Testing");
    var isDevelopment = builder.Environment.IsDevelopment();

    if (isTesting)
    {
        // Testing uses InMemory — EnsureCreated builds the schema from the model.
        db.Database.EnsureCreated();
    }
    else
    {
        // Phase 3B — production/development use EF Core migrations (Npgsql).
        // No EnsureCreated fallback: a migration failure must surface loudly,
        // not silently rebuild the schema and risk data loss.
        db.Database.Migrate();
    }

    // Demo users (admin/admin123, hradmin/hr123, employee/emp123) and other sample
    // data are seeded only in Development or Testing — never in Production.
    if (isDevelopment || isTesting)
    {
        DbSeeder.Seed(db);
    }

    // Provider-agnostic performance indexes (IF NOT EXISTS — safe to run on every boot).
    try
    {
        PerformanceIndexes.Apply(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️  PerformanceIndexes.Apply failed: {ex.Message}");
    }
}

Console.WriteLine("🚀 Lao HR System API running at http://localhost:5000");
if (builder.Environment.IsDevelopment())
{
    Console.WriteLine("� Swagger UI: http://localhost:5000");
    Console.WriteLine("🔐 Default users: admin/admin123, hradmin/hr123, employee/emp123 (Development only)");
}

app.Run();
}
catch (Exception ex)
{
    // Phase 6a — fatal-startup log: emitted before the host logger takes over.
    Log.Fatal(ex, "LaoHR.API host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
