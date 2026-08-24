using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Security;

/// <summary>
/// Phase 4D — security headers verification.
///
/// Proves the SecurityHeadersMiddleware emits the expected defense-in-depth
/// response headers on API responses.
/// </summary>
public class SecurityHeadersTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SecurityHeadersTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ApiResponse_IncludesSecurityHeaders()
    {
        var client = _factory.CreateClient();

        // Login to get a token, then hit an authenticated endpoint.
        var login = await client.PostAsJsonAsync("/api/auth/login", new { Username = "admin", Password = "admin123" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>()).GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var resp = await client.GetAsync("/api/employees");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        resp.Headers.TryGetValues("X-Content-Type-Options", out var nosniff).Should().BeTrue();
        nosniff.Should().Contain("nosniff");

        resp.Headers.TryGetValues("X-Frame-Options", out var frame).Should().BeTrue();
        frame.Should().Contain("DENY");

        resp.Headers.TryGetValues("Referrer-Policy", out var referrer).Should().BeTrue();
        referrer.Should().Contain("strict-origin-when-cross-origin");
    }

    [Fact]
    public async Task UnauthenticatedProtectedApi_Returns401()
    {
        var client = _factory.CreateClient();
        var resp = await client.GetAsync("/api/employees");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
