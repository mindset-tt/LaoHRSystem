using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

builder.Services.AddAuthorization();

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
builder.Services.AddScoped<AuditLogInterceptor>();

// Database Configuration
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<LaoHRDbContext>(options =>
        options.UseInMemoryDatabase("InMemoryDbForTesting"));
}
else
{
    builder.Services.AddDbContext<LaoHRDbContext>((sp, options) => {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
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


// CORS for frontend - allow all origins for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Allow any origin
              .AllowAnyMethod()
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

// Authenticate & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

if (!builder.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<LaoHR.API.Middleware.LicenseMiddleware>();
}

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
    // Apply any pending migrations
    if (!builder.Environment.IsEnvironment("Testing"))
    {
        // RESET DATABASE to apply new mock data (Wipe old data)
        // Remove this line later if you want to keep data between restarts
        // db.Database.EnsureDeleted();   
        
        db.Database.Migrate();
        DbSeeder.Seed(db);
    }
    else
    {
        db.Database.EnsureCreated();
        DbSeeder.Seed(db);
    }
}

Console.WriteLine("🚀 Lao HR System API running at http://localhost:5000");
Console.WriteLine("📚 Swagger UI: http://localhost:5000");
Console.WriteLine("🔐 Default users: admin/admin123, hr/hr123, employee/emp123");

app.Run();

public partial class Program { }
