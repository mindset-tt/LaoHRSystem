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
/// Phase 3C5 — recruitment IDOR tests. Proves an unrelated employee cannot
/// access candidate/application/offer/onboarding data.
/// </summary>
public class RecruitmentIdorTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public RecruitmentIdorTests(CustomWebApplicationFactory factory)
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

    private async Task<int> SeedCandidateAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
        var candidate = new Candidate { FirstName = "Secret", LastName = "Candidate", Email = "secret@example.com", Status = "ACTIVE" };
        db.Candidates.Add(candidate);
        await db.SaveChangesAsync();
        return candidate.CandidateId;
    }

    [Fact]
    public async Task UnrelatedEmployee_CannotViewCandidate()
    {
        var candidateId = await SeedCandidateAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync($"/api/recruitment/candidates/{candidateId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnrelatedEmployee_CannotViewOffer()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
        var offer = new Offer { ApplicationId = 1, PositionId = 1, Salary = 100, Status = "DRAFT" };
        db.Offers.Add(offer);
        await db.SaveChangesAsync();
        var offerId = offer.OfferId;

        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.GetAsync($"/api/recruitment/offers/{offerId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UnrelatedEmployee_CannotHire()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
        var app = new Application { CandidateId = 1, OpeningId = 1, CurrentStage = "OFFER", Status = "ACTIVE" };
        db.Applications.Add(app);
        await db.SaveChangesAsync();
        var appId = app.ApplicationId;

        var client = _factory.CreateClient();
        await LoginAsync(client, "employee", "emp123");

        var resp = await client.PostAsync($"/api/recruitment/applications/{appId}/hire", null);
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
