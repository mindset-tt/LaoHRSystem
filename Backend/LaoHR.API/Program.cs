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

var builder = WebApplication.CreateBuilder(args);

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "LaoHRSystemSecretKey2024VeryLongKeyForSecurity!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LaoHRSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LaoHRFrontend";

// Add services
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
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

builder.Services.AddDbContext<LaoHRDbContext>((sp, options) => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("LaoHR.API"))
           .AddInterceptors(sp.GetRequiredService<AuditLogInterceptor>());
});

// Custom services
builder.Services.AddScoped<PayrollService>();
builder.Services.AddScoped<PayslipPdfService>();
builder.Services.AddScoped<IBankTransferService, BankTransferService>();
builder.Services.AddScoped<NssfReportService>();
builder.Services.AddScoped<IEmailService, EmailService>();


// CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
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

// Authentication & Authorization middleware
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<LaoHR.API.Middleware.LicenseMiddleware>();

app.MapControllers();

// Seed database on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
    // Apply any pending migrations
    db.Database.Migrate();
}

Console.WriteLine("🚀 Lao HR System API running at http://localhost:5000");
Console.WriteLine("📚 Swagger UI: http://localhost:5000");
Console.WriteLine("🔐 Default users: admin/admin123, hr/hr123, employee/emp123");

app.Run();
