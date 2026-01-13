using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Api;

public class HolidayAndLeaveTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HolidayAndLeaveTests(CustomWebApplicationFactory factory)
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
    
    private class LoginResponse { public string Token { get; set; } = ""; }

    [Fact]
    public async Task HolidayLifecycle_ShouldWork()
    {
        await AuthenticateAsync();

        // 1. Create
        var newHoliday = new Holiday 
        { 
            Date = new DateTime(2026, 12, 25), 
            Name = "Xmas 2026", 
            NameEn = "Christmas",
            Year = 2026,
            IsRecurring = true
        };
        
        var createResp = await _client.PostAsJsonAsync("/api/holidays", newHoliday);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        
        var created = await createResp.Content.ReadFromJsonAsync<Holiday>();
        created.Should().NotBeNull();
        int id = created!.HolidayId;

        // 2. Get All
        var listResp = await _client.GetAsync("/api/holidays?year=2026");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // 3. Update
        created.Name = "Christmas Day";
        var updateResp = await _client.PutAsJsonAsync($"/api/holidays/{id}", created);
        updateResp.IsSuccessStatusCode.Should().BeTrue(); // 200 or 204

        // 4. Delete
        var delResp = await _client.DeleteAsync($"/api/holidays/{id}");
        delResp.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task LeaveLifecycle_ShouldWork()
    {
        await AuthenticateAsync();

        // Need an employee first
        var empFn = new Employee { EmployeeCode = "LVE001", LaoName = "Leave Guy", DepartmentId = 1, BaseSalary = 1000 };
        // Assuming Department 1 exists from previous tests or seed. If not, it might fail inside.
        // But let's assume seed exists or try to ensure logic.
        // Actually, create dept to be safe.
        await _client.PostAsJsonAsync("/api/departments", new Department { DepartmentName = "Leave Dept" });
        
        var empResp = await _client.PostAsJsonAsync("/api/employees", empFn);
        // If fail, maybe duplicate code. Use random code.
        if (!empResp.IsSuccessStatusCode)
        {
             empFn.EmployeeCode = "LVE" + new Random().Next(1000,9999);
             empResp = await _client.PostAsJsonAsync("/api/employees", empFn);
        }
        var empObj = await empResp.Content.ReadFromJsonAsync<Employee>();
        int empId = empObj!.EmployeeId;

        // 1. Request Leave
        var request = new LeaveRequest
        {
            EmployeeId = empId,
            LeaveType = "SICK",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Reason = "Flu",
            TotalDays = 2
        };

        var postResp = await _client.PostAsJsonAsync("/api/leave", request);
        postResp.IsSuccessStatusCode.Should().BeTrue();
        var createdLeave = await postResp.Content.ReadFromJsonAsync<LeaveRequest>();
        int leaveId = createdLeave!.LeaveId;

        // 2. Get Pending
        var getResp = await _client.GetAsync("/api/leave?status=PENDING");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Approve
        var approval = new { ApprovedById = 999, Notes = "Have fun" };
        var approveResp = await _client.PostAsJsonAsync($"/api/leave/{leaveId}/approve", approval);
        approveResp.IsSuccessStatusCode.Should().BeTrue();
        
        // 4. Update (maybe change dates)
        // Usually can't update after approve, but let's try standard update endpoint if exists
        request.Reason = "Updated Flu";
        // var putResp = await _client.PutAsJsonAsync($"/api/leave/{leaveId}", request);
        
        // 5. Delete (Cancel)
        var delResp = await _client.DeleteAsync($"/api/leave/{leaveId}");
        delResp.IsSuccessStatusCode.Should().BeTrue();
    }
}
