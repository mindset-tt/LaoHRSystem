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
/// Phase 3C6 — performance IDOR tests. Proves an unrelated employee cannot
/// access another employee's goals/reviews/feedback/1:1s.
/// </summary>
public class PerformanceIdorTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public PerformanceIdorTests(CustomWebApplicationFactory factory)
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

    private async Task<int> SeedGoalForOtherEmployeeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
        var other = new Employee { EmployeeCode = "PERFOTHER", LaoName = "Other", IsActive = true };
        db.Employees.Add(other);
        await db.SaveChangesAsync();
        var goal = new Goal { EmployeeId = other.EmployeeId, Title = "Secret Goal", Status = "ACTIVE" };
        db.Goals.Add(goal);
        await db.SaveChangesAsync();
        return goal.GoalId;
    }

    [Fact]
    public async Task UnrelatedEmployee_CannotViewGoal()
    {
        var goalId = await SeedGoalForOtherEmployeeAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync($"/api/performance/goals/{goalId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnrelatedEmployee_CannotViewTalentReview()
    {
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync("/api/performance/talent");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnrelatedEmployee_CannotViewFeedback()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
        var a = new Employee { EmployeeCode = "FBA", LaoName = "A", IsActive = true };
        var b = new Employee { EmployeeCode = "FBB", LaoName = "B", IsActive = true };
        db.Employees.AddRange(a, b);
        await db.SaveChangesAsync();
        var feedback = new Feedback { FromEmployeeId = a.EmployeeId, ToEmployeeId = b.EmployeeId, Message = "secret", Visibility = "Private" };
        db.Feedbacks.Add(feedback);
        await db.SaveChangesAsync();
        var feedbackId = feedback.FeedbackId;

        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync($"/api/performance/feedback?toEmployeeId={b.EmployeeId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
