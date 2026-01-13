using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class LeaveControllerTests : TestBase
{
    public LeaveControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateLeave_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = new LeaveRequest 
        { 
            EmployeeId = 1, 
            LeaveType = "ANNUAL", 
            StartDate = DateTime.Today, 
            EndDate = DateTime.Today.AddDays(2), 
            Reason = "Vacation" 
        };
        
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            if (!await db.Employees.AnyAsync(e => e.EmployeeId == 1))
            {
                db.Employees.Add(new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "Test", IsActive = true });
                await db.SaveChangesAsync();
            }
        }

        // Act
        var response = await _client.PostAsJsonAsync("/api/leave", request);

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
    }
}
