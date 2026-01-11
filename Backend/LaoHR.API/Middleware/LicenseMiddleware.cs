using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Middleware;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LicenseMiddleware> _logger;
    private readonly LicenseService _licenseService;

    public LicenseMiddleware(RequestDelegate next, ILogger<LicenseMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _licenseService = new LicenseService(); // Stateless
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Bypass for:
        // 1. Swagger
        // 2. Auth (Login)
        // 3. License Activation endpoints
        // 4. Static files (Next.js) if served? No, this is API.
        if (path.StartsWith("/swagger") || 
            path.StartsWith("/api/auth") || 
            path.StartsWith("/api/license"))
        {
            await _next(context);
            return;
        }

        using (var scope = context.RequestServices.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            // Key name in DB: "LICENSE_KEY"
            var setting = await db.SystemSettings.FindAsync("LICENSE_KEY");
            string licenseKey = setting?.SettingValue ?? "";

            var license = _licenseService.VerifyLicense(licenseKey);

            if (license == null)
            {
                context.Response.StatusCode = 402; // Payment Required
                await context.Response.WriteAsJsonAsync(new { message = "License Invalid or Expired. Please contact support." });
                return;
            }
            
            // Optional: Block if Max Employees exceeded? 
            // We can do that in CreateEmployee, not middleware.
        }

        await _next(context);
    }
}
