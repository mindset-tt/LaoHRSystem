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
        var request = new ClockRequest { EmployeeId = 1, Latitude = 17.9m, Longitude = 102.6m, Method = "WEB" };
        
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
        var response = await _client.PostAsJsonAsync("/api/attendance/clock-in", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
