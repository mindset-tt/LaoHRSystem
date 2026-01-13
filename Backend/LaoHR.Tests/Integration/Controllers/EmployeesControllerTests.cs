using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using LaoHR.API.Controllers;
using LaoHR.API.Data;
using LaoHR.Tests.Helpers;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class EmployeesControllerTests : TestBase
{
    public EmployeesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }
    
    private async Task<string> AuthenticateAsync()
    {
        var request = new LoginRequest { Username = "admin", Password = "admin123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    [Fact]
    public async Task GetEmployees_Authorized_ReturnsList()
    {
        // Arrange
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        // Seed DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            if (!await db.Employees.AnyAsync())
            {
                db.Employees.Add(new Employee { EmployeeCode = "EMP001", LaoName = "Test Emp", IsActive = true, CreatedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
            }
        }

        // Act
        var response = await _client.GetAsync("/api/employees");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employees = await response.Content.ReadFromJsonAsync<List<Employee>>();
        employees.Should().NotBeNullOrEmpty();
    }
    
    [Fact]
    public async Task CreateEmployee_ValidData_ReturnsCreated()
    {
        // Arrange
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var newEmployee = new Employee 
        { 
            EmployeeCode = "EMP999", 
            LaoName = "New Guy", 
            IsActive = true,
            BaseSalary = 1000000,
            DependentCount = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/employees", newEmployee);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<Employee>();
        created.Should().NotBeNull();
        created!.EmployeeCode.Should().Be("EMP999");
    }

    [Fact]
    public async Task DeleteEmployee_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        int empId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var emp = new Employee { EmployeeCode = "DEL001", LaoName = "Delete Me", IsActive = true };
            db.Employees.Add(emp);
            await db.SaveChangesAsync();
            empId = emp.EmployeeId;
        }

        // Act
        var response = await _client.DeleteAsync($"/api/employees/{empId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify Soft Delete
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            var deleted = await db.Employees.FindAsync(empId);
            deleted.Should().NotBeNull();
            deleted!.IsActive.Should().BeFalse();
        }
    }
    
    [Fact]
    public async Task GetEmployee_NonExistent_ReturnsNotFound()
    {
        // Arrange
        var token = await AuthenticateAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/employees/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
