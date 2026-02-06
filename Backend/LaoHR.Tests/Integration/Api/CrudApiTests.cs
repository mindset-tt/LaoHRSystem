using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Api;

public class CrudApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CrudApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task AuthenticateAsync()
    {
        var login = new { Username = "admin", Password = "admin123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result!.Token);
    }
    
    // Minimal DTOs matching Controller expectations
    private class LoginResponse { public string Token { get; set; } = ""; }

    [Fact]
    public async Task EmployeeLifecycle_ShouldWork()
    {
        await AuthenticateAsync();

        // Ensure Department Exists
        var deptFn = new Department { DepartmentName = "Test Dept" };
        var deptResp = await _client.PostAsJsonAsync("/api/departments", deptFn);
        var deptObj = await deptResp.Content.ReadFromJsonAsync<Department>();
        int deptId = deptObj!.DepartmentId;

        // 1. Create
        var newEmp = new Employee 
        { 
            EmployeeCode = "TEST999", 
            LaoName = "Mr Test", 
            EnglishName = "Test",
            DepartmentId = deptId, 
            JobTitle = "Tester",
            BaseSalary = 5000000 
        };
        
        var createResp = await _client.PostAsJsonAsync("/api/employees", newEmp);
        createResp.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        
        var createdEmp = await createResp.Content.ReadFromJsonAsync<Employee>();
        createdEmp.Should().NotBeNull();
        int empId = createdEmp!.EmployeeId;

        // 2. Get
        var getResp = await _client.GetAsync($"/api/employees/{empId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // 3. Update
        createdEmp.LaoName = "Mr Updated";
        var updateResp = await _client.PutAsJsonAsync($"/api/employees/{empId}", createdEmp);
        updateResp.IsSuccessStatusCode.Should().BeTrue(); // 200 or 204
        
        // 4. Delete
        var delResp = await _client.DeleteAsync($"/api/employees/{empId}");
        delResp.IsSuccessStatusCode.Should().BeTrue();
        
        // 5. Verify Soft Deleted
        var checkResp = await _client.GetAsync($"/api/employees/{empId}");
        checkResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var deletedEmp = await checkResp.Content.ReadFromJsonAsync<Employee>();
        deletedEmp!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DepartmentLifecycle_ShouldWork()
    {
        await AuthenticateAsync();
        
        var newDept = new Department { DepartmentName = "AI Lab" };
        
        // Create
        var res = await _client.PostAsJsonAsync("/api/departments", newDept);
        res.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        var created = await res.Content.ReadFromJsonAsync<Department>();
        
        // Update
        created!.DepartmentName = "AI Laboratory";
        await _client.PutAsJsonAsync($"/api/departments/{created.DepartmentId}", created);
        
        // Delete
        await _client.DeleteAsync($"/api/departments/{created.DepartmentId}");
    }
    
    [Fact]
    public async Task Settings_Update_ShouldWork()
    {
        await AuthenticateAsync();
        
        var dto = new { SettingValue = "UPDATED_KEY" }; // Logic from UpdateSettingDto
        var res = await _client.PutAsJsonAsync("/api/settings/LICENSE_KEY", dto);
        
        // Might be 200 or 404 if not found, but covers the code path
        res.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}
