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
/// Phase 3C4 — PM authorization + IDOR tests. Proves a non-member cannot view
/// or edit another project's board/timeline/health/dependencies.
/// </summary>
public class PmAuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PmAuthorizationTests(CustomWebApplicationFactory factory)
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

    private async Task<(int projectId, int outsiderEmpId)> SeedProjectAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();

        var owner = new Employee { EmployeeCode = "PMOWN", LaoName = "Owner", IsActive = true };
        var outsider = new Employee { EmployeeCode = "PMOUT", LaoName = "Outsider", IsActive = true };
        db.Employees.AddRange(owner, outsider);
        await db.SaveChangesAsync();

        var project = new Project { Code = "PM-P1", Name = "Secret Project", Status = "ACTIVE", OwnerId = owner.EmployeeId };
        db.Projects.Add(project);
        await db.SaveChangesAsync();

        db.Users.Add(new AppUser { Username = "pmoutsider", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", EmployeeId = outsider.EmployeeId, IsActive = true });
        await db.SaveChangesAsync();

        return (project.ProjectId, outsider.EmployeeId);
    }

    [Fact]
    public async Task NonMember_CannotViewBoard()
    {
        var (projectId, _) = await SeedProjectAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "pmoutsider", "pass123");

        var resp = await client.GetAsync($"/api/pm/projects/{projectId}/board");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NonMember_CannotViewTimeline()
    {
        var (projectId, _) = await SeedProjectAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "pmoutsider", "pass123");

        var resp = await client.GetAsync($"/api/pm/projects/{projectId}/timeline");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NonMember_CannotViewHealth()
    {
        var (projectId, _) = await SeedProjectAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "pmoutsider", "pass123");

        var resp = await client.GetAsync($"/api/pm/projects/{projectId}/health");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task NonMember_CannotAddDependency()
    {
        var (projectId, _) = await SeedProjectAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "pmoutsider", "pass123");

        var resp = await client.PostAsJsonAsync($"/api/pm/projects/{projectId}/dependencies", new { PredecessorTaskId = 1, SuccessorTaskId = 2 });
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
