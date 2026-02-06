using System.Net;
using System.Net.Http.Json;
using System.Text.Json; 
using FluentAssertions;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class LeaveControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public LeaveControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateLeave_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var client = _factory.CreateClient(); 

        // 1. LOGIN (Get "Badge")
        var loginResp = await client.PostAsJsonAsync("/api/auth/login", new { Username = "employee", Password = "emp123" });
        loginResp.EnsureSuccessStatusCode();
        var token = (await loginResp.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        
        // Attach Badge to Header
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2. Prepare Request
        // Note: Use a date in the future to avoid validation issues
        var request = new LeaveRequest 
        { 
            // EmployeeId 1 because 'employee' maps to 1 in your Controller switch statement
            EmployeeId = 1, 
            LeaveType = "ANNUAL", 
            // Use far future dates to avoid conflicts with other tests
            StartDate = DateTime.UtcNow.AddDays(100), 
            EndDate = DateTime.UtcNow.AddDays(102), 
            Reason = "Vacation" 
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/leave", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
             var error = await response.Content.ReadAsStringAsync();
             // If it fails, print why (likely overlapping dates if you run tests too fast)
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}