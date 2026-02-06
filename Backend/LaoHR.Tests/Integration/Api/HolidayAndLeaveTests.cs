using System.Net;
using System.Net.Http.Json;
using System.Text.Json; 
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore; 
using LaoHR.Shared.Data;
using FluentAssertions;
using LaoHR.Shared.Models;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Api;

public class HolidayAndLeaveTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions;

    public HolidayAndLeaveTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
    }

    private async Task AuthenticateAsync()
    {
        var login = new { Username = "admin", Password = "admin123" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", login);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = result.GetProperty("token").GetString();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task HolidayLifecycle_ShouldWork()
    {
        await AuthenticateAsync();

        int year = new Random().Next(2030, 2050);

        // 1. Create
        var newHoliday = new Holiday 
        { 
            Date = new DateTime(year, 12, 25), 
            Name = $"Xmas {year}", 
            NameLao = "ວັນຄຣິດສະມາດ",
            Year = year,
            IsRecurring = true
        };
        
        var createResp = await _client.PostAsJsonAsync("/api/holidays", newHoliday);
        createResp.IsSuccessStatusCode.Should().BeTrue();
        
        var created = await createResp.Content.ReadFromJsonAsync<Holiday>(_jsonOptions);
        int id = created!.HolidayId;

        // 2. Get All
        var listResp = await _client.GetAsync($"/api/holidays?year={year}");
        listResp.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // 3. Update
        created.Name = "Christmas Day Updated";
        var updateResp = await _client.PutAsJsonAsync($"/api/holidays/{id}", created);
        updateResp.IsSuccessStatusCode.Should().BeTrue();

        // 4. Delete
        var delResp = await _client.DeleteAsync($"/api/holidays/{id}");
        delResp.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task LeaveLifecycle_ShouldWork()
    {
        var client = _factory.CreateClient();
        
        int requesterId = 1; 
        int approverId = 2;  
        int currentYear = DateTime.Now.Year;
        
        // UNIQUE TAG: This is our tracking number.
        string uniqueReason = $"TEST-{Guid.NewGuid()}";

        // PRE-FLIGHT: Ensure Data State
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            
            // 1. HIRE STAFF
            var emp1 = await db.Employees.FindAsync(requesterId);
            if (emp1 == null) db.Employees.Add(new Employee { EmployeeId = requesterId, EmployeeCode = "EMP001", LaoName = "Requester", IsActive = true });
            else { emp1.IsActive = true; }

            var emp2 = await db.Employees.FindAsync(approverId);
            if (emp2 == null) db.Employees.Add(new Employee { EmployeeId = approverId, EmployeeCode = "HR001", LaoName = "Approver", IsActive = true });
            else { emp2.IsActive = true; }

            await db.SaveChangesAsync();

            // 2. NUCLEAR CLEANUP
            var oldLeaves = db.LeaveRequests.Where(l => l.EmployeeId == requesterId || l.EmployeeId == approverId);
            db.LeaveRequests.RemoveRange(oldLeaves);

            // 3. FUND ACCOUNTS
            var balance = await db.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == requesterId && b.Year == currentYear && b.LeaveType == "SICK");

            if (balance == null)
            {
                db.LeaveBalances.Add(new LeaveBalance
                {
                    EmployeeId = requesterId,
                    Year = currentYear,
                    LeaveType = "SICK",
                    TotalDays = 10,
                    UsedDays = 0
                });
            }
            else
            {
                balance.TotalDays = 10;
                balance.UsedDays = 0;
            }

            await db.SaveChangesAsync();
        }

        // STEP 1: EMPLOYEE REQUESTS LEAVE
        var empLogin = await client.PostAsJsonAsync("/api/auth/login", new { Username = "employee", Password = "emp123" });
        empLogin.EnsureSuccessStatusCode();
        var empToken = (await empLogin.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();
        
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", empToken);

        // Use form data since controller expects [FromForm]
        var formContent = new MultipartFormDataContent();
        formContent.Add(new StringContent(requesterId.ToString()), "EmployeeId");
        formContent.Add(new StringContent("SICK"), "LeaveType");
        formContent.Add(new StringContent(DateTime.Today.AddDays(1).ToString("yyyy-MM-dd")), "StartDate");
        formContent.Add(new StringContent(DateTime.Today.AddDays(2).ToString("yyyy-MM-dd")), "EndDate");
        formContent.Add(new StringContent(uniqueReason), "Reason");
        formContent.Add(new StringContent("false"), "IsHalfDay");

        var postResp = await client.PostAsync("/api/leave", formContent);
        if (!postResp.IsSuccessStatusCode)
        {
            var err = await postResp.Content.ReadAsStringAsync();
            throw new Exception($"Create Leave Failed: {postResp.StatusCode} - {err}");
        }

        // FAIL-SAFE FINDER: Go directly to DB to get the ID for this Unique Reason
        int leaveId;
        using (var scope = _factory.Services.CreateScope())
        {
             var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
             // Find the specific record we just made
             var myLeave = await db.LeaveRequests.FirstOrDefaultAsync(l => l.Reason == uniqueReason);
             
             if (myLeave == null)
             {
                 throw new Exception($"Leave created via API with Reason='{uniqueReason}' but not found in DB.");
             }
             leaveId = myLeave.LeaveId;
        }

        // STEP 2: ADMIN APPROVES LEAVE
        var hrLogin = await client.PostAsJsonAsync("/api/auth/login", new { Username = "admin", Password = "admin123" });
        hrLogin.EnsureSuccessStatusCode();
        var hrToken = (await hrLogin.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);

        var approval = new { ApprovedById = approverId, Notes = "Approved by Test" };
        var approveResp = await client.PostAsJsonAsync($"/api/leave/{leaveId}/approve", approval);
        
        if (!approveResp.IsSuccessStatusCode)
        {
            var err = await approveResp.Content.ReadAsStringAsync();
            throw new Exception($"Approve Leave #{leaveId} Failed: {approveResp.StatusCode} - {err}");
        }
        
        approveResp.IsSuccessStatusCode.Should().BeTrue();
        
        // STEP 3: CLEANUP
        await client.DeleteAsync($"/api/leave/{leaveId}");
    }
}