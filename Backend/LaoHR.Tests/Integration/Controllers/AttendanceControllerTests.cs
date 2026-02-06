using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using LaoHR.API.Controllers;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class AttendanceControllerTests : TestBase
{
    public AttendanceControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task ClockIn_ValidRequest_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        
        // NUCLEAR CLEANUP: Target IDs 1 AND 2 to be absolutely safe
        // This ensures no matter who the controller thinks we are, the data is gone.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var history = db.Attendances.Where(a => a.EmployeeId == 1 || a.EmployeeId == 2);
            db.Attendances.RemoveRange(history);
            await db.SaveChangesAsync();
        }

        // 1. LOGIN
        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new { Username = "employee", Password = "emp123" });
        loginResp.EnsureSuccessStatusCode();
        var token = (await loginResp.Content.ReadFromJsonAsync<dynamic>()).GetProperty("token").GetString();
        
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Prepare Request
        // The Controller ignores the ID sent here and uses the Token ID (1), but we send it for completeness.
        var request = new ClockRequest 
        { 
            EmployeeId = 1, 
            Latitude = 17.9m, 
            Longitude = 102.6m, 
            Method = "WEB" 
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/attendance/clock-in", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"ClockIn Failed: {response.StatusCode} - {error}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}