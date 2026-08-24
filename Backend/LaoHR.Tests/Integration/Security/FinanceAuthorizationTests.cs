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
/// Phase 4B — Finance authorization + IDOR + segregation-of-duties tests.
/// Proves that HR and ordinary Employees cannot access finance/accounting
/// surfaces (AP, payments, journals, bank accounts, COA).
/// </summary>
public class FinanceAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public FinanceAuthorizationTests(CustomWebApplicationFactory factory)
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

    private async Task SeedHrUserAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
        if (!db.Users.Any(u => u.Username == "hrfinance"))
        {
            db.Users.Add(new AppUser
            {
                Username = "hrfinance",
                PasswordHash = PasswordHasher.HashPassword("pass123"),
                PasswordHashVersion = 2,
                Role = "HR",
                DisplayName = "HR Finance Test",
                IsActive = true,
            });
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Hr_CannotListSupplierInvoices()
    {
        await SeedHrUserAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrfinance", "pass123");

        var resp = await client.GetAsync("/api/supplier-invoices");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Hr_CannotListPayments()
    {
        await SeedHrUserAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrfinance", "pass123");

        var resp = await client.GetAsync("/api/payments");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Hr_CannotListBankAccounts()
    {
        await SeedHrUserAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrfinance", "pass123");

        var resp = await client.GetAsync("/api/bank-accounts");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Hr_CannotListAccounts()
    {
        await SeedHrUserAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrfinance", "pass123");

        var resp = await client.GetAsync("/api/accounts");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Hr_CannotListJournals()
    {
        await SeedHrUserAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrfinance", "pass123");

        var resp = await client.GetAsync("/api/journals");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListSupplierInvoices()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/supplier-invoices");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListBankAccounts()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/bank-accounts");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task FinanceRole_CanListSupplierInvoices()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            if (!db.Users.Any(u => u.Username == "finuser"))
            {
                db.Users.Add(new AppUser
                {
                    Username = "finuser",
                    PasswordHash = PasswordHasher.HashPassword("pass123"),
                    PasswordHashVersion = 2,
                    Role = "Finance",
                    DisplayName = "Finance User",
                    IsActive = true,
                });
                await db.SaveChangesAsync();
            }
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "finuser", "pass123");

        var resp = await client.GetAsync("/api/supplier-invoices");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FinanceRole_CannotCreateEmployee()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            if (!db.Users.Any(u => u.Username == "finuser2"))
            {
                db.Users.Add(new AppUser
                {
                    Username = "finuser2",
                    PasswordHash = PasswordHasher.HashPassword("pass123"),
                    PasswordHashVersion = 2,
                    Role = "Finance",
                    DisplayName = "Finance User 2",
                    IsActive = true,
                });
                await db.SaveChangesAsync();
            }
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "finuser2", "pass123");

        // Finance role should NOT have HR/employee administration (write) access.
        var resp = await client.PostAsJsonAsync("/api/employees", new
        {
            EmployeeCode = "FIN-TEST",
            LaoName = "Finance Test",
            IsActive = true,
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
