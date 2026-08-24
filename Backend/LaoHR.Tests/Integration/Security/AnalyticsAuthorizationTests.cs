using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LaoHR.Tests.Integration.Security;

/// <summary>
/// Phase 3C3 — analytics authorization tests. Proves role-based dashboard
/// visibility and manager scope.
/// </summary>
public class AnalyticsAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AnalyticsAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task<string> LoginAsync(HttpClient client, string username, string password)
    {
        var resp = await client.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
        resp.EnsureSuccessStatusCode();
        var token = (await resp.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>()).GetProperty("token").GetString();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return token!;
    }

    [Fact]
    public async Task Employee_CannotAccessExecutiveDashboard()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/analytics/executive");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotAccessHrDashboard()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/analytics/hr");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotAccessPayrollAnalytics()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/analytics/payroll");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_CanAccessExecutiveDashboard()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "admin", "admin123");

        var resp = await client.GetAsync("/api/analytics/executive");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Employee_CanAccessManagerDashboard()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/analytics/my-team");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
