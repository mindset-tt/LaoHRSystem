namespace LaoHR.API.Middleware;

/// <summary>
/// Phase 4D — security headers middleware.
///
/// Adds defense-in-depth response headers. HSTS is applied only when the request
/// is already HTTPS (or behind a trusted proxy that sets X-Forwarded-Proto), so
/// we never emit HSTS over a plain-HTTP local connection.
///
/// CSP is intentionally NOT set here for the API (the API returns JSON, not
/// HTML); the frontend (Next.js) is responsible for its own CSP. This middleware
/// sets the headers that are safe and correct for a JSON API.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;
    private readonly bool _emitHsts;

    public SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment env,
        IConfiguration config)
    {
        _next = next;
        _env = env;
        // Phase 4D.2: when a TLS-terminating reverse proxy (e.g. Caddy) already
        // emits HSTS at the edge, set SecurityHeaders__EmitHsts=false on the API
        // to avoid duplicate headers. Default remains ON for direct-HTTPS
        // deployments with no proxy.
        _emitHsts = config.GetValue("SecurityHeaders:EmitHsts", true);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME-type sniffing.
        headers["X-Content-Type-Options"] = "nosniff";

        // Referrer policy: send only origin on cross-origin.
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        // Frame protection (defense-in-depth; CSP frame-ancestors is the modern
        // equivalent but this is harmless for a JSON API).
        headers["X-Frame-Options"] = "DENY";

        // Permissions policy: disable features the API never needs.
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        // HSTS only over HTTPS (or behind a trusted proxy reporting https).
        var isHttps = context.Request.IsHttps
            || string.Equals(context.Request.Headers["X-Forwarded-Proto"], "https", StringComparison.OrdinalIgnoreCase);
        if (isHttps && _emitHsts && !_env.IsDevelopment())
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        await _next(context);
    }
}
