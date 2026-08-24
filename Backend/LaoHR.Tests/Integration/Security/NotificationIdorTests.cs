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
/// Phase 3C2B — notification + approval authorization (IDOR) regression tests.
/// </summary>
public class NotificationIdorTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public NotificationIdorTests(CustomWebApplicationFactory factory)
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
    public async Task MarkRead_OtherUsersNotification_ReturnsNotFound()
    {
        // Seed two users and a notification for user A only.
        int userAId, userBId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var userA = new AppUser { Username = "usera", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", IsActive = true };
            var userB = new AppUser { Username = "userb", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", IsActive = true };
            db.Users.AddRange(userA, userB);
            await db.SaveChangesAsync();
            userAId = userA.UserId;
            userBId = userB.UserId;

            db.Notifications.Add(new Notification { UserId = userAId, Type = "TEST", Title = "For A only" });
            await db.SaveChangesAsync();
        }

        // User B tries to mark user A's notification as read.
        var clientB = _factory.CreateClient();
        await LoginAsync(clientB, "userb", "pass123");

        var notifId = 0;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            notifId = db.Notifications.First(n => n.UserId == userAId).NotificationId;
        }

        var resp = await clientB.PostAsync($"/api/notifications/{notifId}/read", null);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task List_OnlyReturnsOwnNotifications()
    {
        int userAId, userBId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var userA = new AppUser { Username = "usera2", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", IsActive = true };
            var userB = new AppUser { Username = "userb2", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", IsActive = true };
            db.Users.AddRange(userA, userB);
            await db.SaveChangesAsync();
            userAId = userA.UserId;
            userBId = userB.UserId;

            db.Notifications.AddRange(
                new Notification { UserId = userAId, Type = "TEST", Title = "A's notification" },
                new Notification { UserId = userBId, Type = "TEST", Title = "B's notification" });
            await db.SaveChangesAsync();
        }

        var clientA = _factory.CreateClient();
        await LoginAsync(clientA, "usera2", "pass123");

        var resp = await clientA.GetAsync("/api/notifications");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Contain("A's notification");
        body.Should().NotContain("B's notification");
    }
}
