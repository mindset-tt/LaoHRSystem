using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Middleware;

public class LicenseMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LicenseMiddleware> _logger;
    private readonly LicenseService _licenseService;
    private readonly ILicenseKeyCache _cache;

    public LicenseMiddleware(
        RequestDelegate next,
        ILogger<LicenseMiddleware> logger,
        ILicenseKeyCache cache)
    {
        _next = next;
        _logger = logger;
        _licenseService = new LicenseService(); // Stateless
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? "";

        // Bypass for:
        // 1. Swagger / OpenAPI
        // 2. Auth (Login / Refresh)
        // 3. License activation endpoints
        // 4. Health checks (load balancers must not require a license)
        if (path.StartsWith("/swagger") ||
            path.StartsWith("/api/auth") ||
            path.StartsWith("/api/license") ||
            path.StartsWith("/health"))
        {
            await _next(context);
            return;
        }

        var entry = await _cache.GetOrLoadAsync(async () =>
        {
            using var scope = context.RequestServices.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var setting = await db.SystemSettings.FindAsync("LICENSE_KEY");
            var licenseKey = setting?.SettingValue ?? string.Empty;
            var verified = _licenseService.VerifyLicense(licenseKey);
            if (verified is null)
            {
                return new LicenseCacheEntry(IsValid: false, Message: "License Invalid or Expired.");
            }
            return new LicenseCacheEntry(IsValid: true, Message: "OK");
        });

        if (entry is null || !entry.IsValid)
        {
            context.Response.StatusCode = 402; // Payment Required
            await context.Response.WriteAsJsonAsync(new { message = entry?.Message ?? "License Invalid or Expired. Please contact support." });
            return;
        }

        await _next(context);
    }
}
