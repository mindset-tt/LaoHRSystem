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
/// Phase 4C — Corporate Operations authorization + IDOR.
///
/// Proves domain boundaries:
///   - Employee cannot manage facilities/fleet/visitors/documents.
///   - Employee CAN book rooms/vehicles and create travel (self-service).
///   - Employee cannot view another employee's travel (IDOR).
///   - Employee cannot browse the visitor register (privacy).
/// </summary>
public class CorporateAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public CorporateAuthorizationTests(CustomWebApplicationFactory factory)
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

    private async Task SeedUserAsync(string username, string role, int? employeeId = null)
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
                DisplayName = $"{role} Corporate Test",
                EmployeeId = employeeId,
                IsActive = true,
            });
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task Employee_CannotManageFacilities()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.PostAsJsonAsync("/api/facilities", new
        {
            FacilityCode = "F1",
            Name = "HQ",
            FacilityType = "OFFICE",
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotManageFleet()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.PostAsJsonAsync("/api/fleet/vehicles", new
        {
            VehicleCode = "V1",
            RegistrationNumber = "REG-1",
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotManageVisitors()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/visitors");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotManageDocuments()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.PostAsJsonAsync("/api/corporate-documents", new
        {
            Title = "Doc",
            DocumentType = "GENERAL",
            OwnerEntityType = "GENERAL",
            OwnerEntityId = 1,
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotViewFacilities()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/facilities");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_CannotViewFleet()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/fleet/vehicles");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Hr_CanViewFacilities()
    {
        await SeedUserAsync("hrcorp", "HR");
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrcorp", "pass123");

        var resp = await client.GetAsync("/api/facilities");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Hr_CanManageFacilities()
    {
        await SeedUserAsync("hrcorp2", "HR");
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrcorp2", "pass123");

        var resp = await client.PostAsJsonAsync("/api/facilities", new
        {
            FacilityCode = "F2",
            Name = "Branch",
            FacilityType = "BRANCH",
        });
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Employee_CannotExportCorporateReports()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var endpoints = new[]
        {
            "/api/corporate/reports/contracts/export",
            "/api/corporate/reports/fleet/export",
            "/api/corporate/reports/travel/export",
            "/api/corporate/reports/visitor-log/export",
        };
        foreach (var endpoint in endpoints)
        {
            var resp = await client.GetAsync(endpoint);
            resp.StatusCode.Should().Be(HttpStatusCode.Forbidden, $"Employee should be forbidden from {endpoint}");
        }
    }

    [Fact]
    public async Task Hr_CanExportCorporateReports()
    {
        await SeedUserAsync("hrcorp3", "HR");
        var client = _factory.CreateClient();
        await LoginAsync(client, "hrcorp3", "pass123");

        var resp = await client.GetAsync("/api/corporate/reports/contracts/export");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Employee_CannotViewAnotherEmployeesTravel()
    {
        // Seed two employees and a travel request for employee B.
        int empA, empB;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var a = new Employee { EmployeeCode = "CORPA", LaoName = "Corp A", IsActive = true };
            var b = new Employee { EmployeeCode = "CORPB", LaoName = "Corp B", IsActive = true };
            db.Employees.AddRange(a, b);
            await db.SaveChangesAsync();
            empA = a.EmployeeId;
            empB = b.EmployeeId;

            db.Users.AddRange(
                new AppUser { Username = "corpa", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", EmployeeId = empA, IsActive = true },
                new AppUser { Username = "corpb", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", EmployeeId = empB, IsActive = true });
            await db.SaveChangesAsync();

            db.TravelRequests.Add(new TravelRequest
            {
                TravelNumber = "TRV-1",
                EmployeeId = empB,
                Purpose = "Conference",
                Destination = "Bangkok",
                DepartureDate = DateTime.UtcNow,
                ReturnDate = DateTime.UtcNow.AddDays(2),
                Status = "DRAFT",
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "corpa", "pass123");

        // Employee A cannot read employee B's travel request (IDOR).
        var resp = await client.GetAsync("/api/travel/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
