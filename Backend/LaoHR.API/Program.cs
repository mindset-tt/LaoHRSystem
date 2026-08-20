using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

var builder = WebApplication.CreateBuilder(args);

// QuestPDF License
QuestPDF.Settings.License = LicenseType.Community;

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "LaoHRSystemSecretKey2024VeryLongKeyForSecurity!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LaoHRSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LaoHRFrontend";

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
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
});

// License key cache (5 minutes) — the license middleware reads this instead of
// hitting the DB on every request.
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<LaoHR.API.Services.ILicenseKeyCache, LaoHR.API.Services.LicenseKeyCache>();

// Health checks for liveness/readiness probes.
builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Lao HR API", Version = "v1" });
    
    // Authorization
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
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
builder.Services.AddScoped<PayslipPdfService>();
builder.Services.AddScoped<IBankTransferService, BankTransferService>();
builder.Services.AddScoped<NssfReportService>();
builder.Services.AddScoped<PdfFormService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICompanySettingsService, CompanySettingsService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<ILeaveService, LeaveService>();
builder.Services.AddScoped<IWorkDayService, WorkDayService>();

// Background Jobs
builder.Services.AddHostedService<LaoHR.API.Jobs.LeaveScheduledJobsService>();


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

var app = builder.Build();

// Configure pipeline
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
app.UseRateLimiter();

// License check BEFORE authentication: an expired/invalid license should not
// let a request get a JWT issued.
if (!builder.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<LaoHR.API.Middleware.LicenseMiddleware>();
}

// Authenticate & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Seed / migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
    var isTesting = builder.Environment.IsEnvironment("Testing");
    var isDevelopment = builder.Environment.IsDevelopment();

    if (isTesting)
    {
        db.Database.EnsureCreated();
    }
    else
    {
        // Try Migrate() first. If it fails (e.g. on a fresh DB where the
        // migration history table doesn't exist yet, or against a provider
        // mismatch), fall back to EnsureCreated. Once the bootstrap has run
        // and the schema is in place, future boots will succeed with Migrate().
        try
        {
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Migrate() failed: {ex.Message}. Falling back to EnsureCreated().");
            db.Database.EnsureCreated();
        }
    }

    // Demo users (admin/admin123, hr/hr123, employee/emp123) and other sample
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
    Console.WriteLine("🔐 Default users: admin/admin123, hr/hr123, employee/emp123 (Development only)");
}

app.Run();

public partial class Program { }
