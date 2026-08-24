using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using LaoHR.API.Controllers;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.Tests.Helpers;

public class TestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory _factory;
    protected readonly HttpClient _client;

    public TestBase(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Authenticate as the seeded admin user and set the Bearer token on the client.
    /// </summary>
    protected async Task AuthenticateAsync(string username = "admin", string password = "admin123")
    {
        var request = new LoginRequest { Username = username, Password = password };
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result!.Token);
    }

    protected void ReseedDatabase()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
    }
}
