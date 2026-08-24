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
/// Phase 4A — Back Office authorization + IDOR regression tests.
///
/// Proves the domain boundaries of the new modules:
///   - A normal Employee cannot view/manage procurement, inventory, assets,
///     contracts, or finance (these are Admin/HR-gated).
///   - An Employee CAN create and view their OWN purchase requests and service
///     requests (self-service), but cannot read another employee's requests.
///   - Supplier bank/tax fields are not exposed to non-privileged callers.
/// </summary>
public class BackOfficeAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public BackOfficeAuthorizationTests(CustomWebApplicationFactory factory)
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

    private async Task<(int empA, int empB)> SeedTwoEmployeesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();

        var empA = new Employee { EmployeeCode = "BOA", LaoName = "BackOffice A", IsActive = true };
        var empB = new Employee { EmployeeCode = "BOB", LaoName = "BackOffice B", IsActive = true };
        db.Employees.AddRange(empA, empB);
        await db.SaveChangesAsync();

        db.Users.AddRange(
            new AppUser { Username = "boa", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", EmployeeId = empA.EmployeeId, IsActive = true },
            new AppUser { Username = "bob", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", EmployeeId = empB.EmployeeId, IsActive = true });
        await db.SaveChangesAsync();

        return (empA.EmployeeId, empB.EmployeeId);
    }

    // ---- Procurement / Inventory / Assets / Contracts / Finance are Admin/HR-gated ----

    [Fact]
    public async Task Employee_CannotListSuppliers()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync("/api/suppliers");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListPurchaseOrders()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync("/api/purchase-orders");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListInventoryItems()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync("/api/inventory/items");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListWarehouses()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync("/api/inventory/warehouses");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListAssets()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync("/api/assets");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListContracts()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync("/api/contracts");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotListBudgets()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync("/api/budgets");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- Purchase request self-service + IDOR ----

    [Fact]
    public async Task Employee_CanCreateOwnPurchaseRequest()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.PostAsJsonAsync("/api/purchase-requests", new
        {
            Currency = "LAK",
            Items = new[]
            {
                new { Description = "Laptop", Quantity = 1m, EstimatedUnitPrice = 1000m }
            }
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task EmployeeA_CannotReadEmployeeB_PurchaseRequest()
    {
        var (_, empB) = await SeedTwoEmployeesAsync();

        int prId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var pr = new PurchaseRequest
            {
                RequestNumber = "PR-IDOR-1",
                RequestedByEmployeeId = empB,
                Status = "DRAFT",
                Currency = "LAK",
                TotalEstimatedAmount = 100,
            };
            db.PurchaseRequests.Add(pr);
            await db.SaveChangesAsync();
            prId = pr.PurchaseRequestId;
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync($"/api/purchase-requests/{prId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotApprovePurchaseRequest()
    {
        var (empA, _) = await SeedTwoEmployeesAsync();

        int prId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var pr = new PurchaseRequest
            {
                RequestNumber = "PR-IDOR-2",
                RequestedByEmployeeId = empA,
                Status = "PENDING_APPROVAL",
                Currency = "LAK",
                TotalEstimatedAmount = 100,
            };
            db.PurchaseRequests.Add(pr);
            await db.SaveChangesAsync();
            prId = pr.PurchaseRequestId;
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.PostAsJsonAsync($"/api/purchase-requests/{prId}/approve", new { });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- Service request self-service + IDOR ----

    [Fact]
    public async Task Employee_CanCreateOwnServiceRequest()
    {
        await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.PostAsJsonAsync("/api/service-requests", new
        {
            CategoryId = 1,
            Subject = "Need a new monitor",
            Priority = "MEDIUM"
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task EmployeeA_CannotReadEmployeeB_ServiceRequest()
    {
        var (_, empB) = await SeedTwoEmployeesAsync();

        int srId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var sr = new ServiceRequest
            {
                RequestNumber = "SR-IDOR-1",
                RequesterEmployeeId = empB,
                CategoryId = 1,
                Subject = "B's request",
                Priority = "MEDIUM",
                Status = "OPEN",
            };
            db.ServiceRequests.Add(sr);
            await db.SaveChangesAsync();
            srId = sr.ServiceRequestId;
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.GetAsync($"/api/service-requests/{srId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotUpdateServiceRequest()
    {
        var (empA, _) = await SeedTwoEmployeesAsync();

        int srId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var sr = new ServiceRequest
            {
                RequestNumber = "SR-IDOR-2",
                RequesterEmployeeId = empA,
                CategoryId = 1,
                Subject = "A's request",
                Priority = "MEDIUM",
                Status = "OPEN",
            };
            db.ServiceRequests.Add(sr);
            await db.SaveChangesAsync();
            srId = sr.ServiceRequestId;
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "boa", "pass123");

        var resp = await client.PutAsJsonAsync($"/api/service-requests/{srId}", new { Status = "RESOLVED" });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
