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
/// Phase 4B.2 — Finance export authorization.
///
/// Proves server-side enforcement of finance report/export endpoints:
///   - Employee → 403
///   - HR       → 403 (HR has procurement/warehouse capabilities but NOT finance)
///   - Finance  → 200
///   - Admin    → 200 (superset)
///
/// Note: "Warehouse" and "Procurement" are not distinct roles in this system —
/// they are capabilities granted to Admin/HR via IBackOfficeAccessService. HR
/// (which holds those capabilities) is proven forbidden from finance exports,
/// which transitively proves Warehouse/Procurement cannot export finance.
/// </summary>
public class FinanceExportAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FinanceExportAuthorizationTests(CustomWebApplicationFactory factory)
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

    private async Task SeedUserAsync(string username, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
        if (!db.Users.Any(u => u.Username == username))
        {
            db.Users.Add(new AppUser
            {
                Username = username,
                PasswordHash = PasswordHasher.HashPassword("pass123"),
                PasswordHashVersion = 2,
                Role = role,
                DisplayName = $"{role} Export Test",
                IsActive = true,
            });
            await db.SaveChangesAsync();
        }
    }

    private static readonly string[] ExportEndpoints =
    {
        "/api/finance/reports/supplier-invoices/export",
        "/api/finance/reports/ap-aging/export",
        "/api/finance/reports/payments/export",
        "/api/finance/reports/general-ledger/export?fiscalPeriodId=1",
        "/api/finance/reports/trial-balance/export?fiscalPeriodId=1",
        "/api/finance/reports/expense-summary/export",
        "/api/finance/reports/budget-utilization/export",
        "/api/journals/trial-balance/export?fiscalPeriodId=1",
    };

    private static readonly string[] ReportEndpoints =
    {
        "/api/finance/reports/supplier-invoices",
        "/api/finance/reports/ap-aging",
        "/api/finance/reports/payments",
        "/api/finance/reports/general-ledger?fiscalPeriodId=1",
        "/api/finance/reports/trial-balance?fiscalPeriodId=1",
        "/api/finance/reports/expense-summary",
        "/api/finance/reports/budget-utilization",
    };

    [Fact]
    public async Task Employee_CannotExportFinance()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        foreach (var endpoint in ExportEndpoints)
        {
            var resp = await client.GetAsync(endpoint);
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden, $"Employee should be forbidden from {endpoint}");
        }
    }

    [Fact]
    public async Task Employee_CannotViewFinanceReports()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        foreach (var endpoint in ReportEndpoints)
        {
            var resp = await client.GetAsync(endpoint);
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden, $"Employee should be forbidden from {endpoint}");
        }
    }

    [Fact]
    public async Task Hr_CannotExportFinance()
    {
        await SeedUserAsync("hrexport", "HR");
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrexport", "pass123");

        foreach (var endpoint in ExportEndpoints)
        {
            var resp = await client.GetAsync(endpoint);
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden, $"HR should be forbidden from {endpoint}");
        }
    }

    [Fact]
    public async Task Hr_CannotViewFinanceReports()
    {
        await SeedUserAsync("hrexport2", "HR");
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrexport2", "pass123");

        foreach (var endpoint in ReportEndpoints)
        {
            var resp = await client.GetAsync(endpoint);
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden, $"HR should be forbidden from {endpoint}");
        }
    }

    [Fact]
    public async Task Finance_CanViewFinanceReports()
    {
        await SeedUserAsync("finexport", "Finance");
        var client = _factory.CreateClient();
        await LoginAsync(client, "finexport", "pass123");

        foreach (var endpoint in ReportEndpoints)
        {
            var resp = await client.GetAsync(endpoint);
            resp.StatusCode.Should().Be(HttpStatusCode.OK, $"Finance should be allowed to view {endpoint}");
        }
    }

    [Fact]
    public async Task Finance_CanExportFinance()
    {
        await SeedUserAsync("finexport2", "Finance");
        var client = _factory.CreateClient();
        await LoginAsync(client, "finexport2", "pass123");

        foreach (var endpoint in ExportEndpoints)
        {
            var resp = await client.GetAsync(endpoint);
            resp.StatusCode.Should().Be(HttpStatusCode.OK, $"Finance should be allowed to export {endpoint}");
        }
    }

    [Fact]
    public async Task Admin_CanExportFinance()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "admin", "admin123");

        foreach (var endpoint in ExportEndpoints)
        {
            var resp = await client.GetAsync(endpoint);
            resp.StatusCode.Should().Be(HttpStatusCode.OK, $"Admin should be allowed to export {endpoint}");
        }
    }
}
