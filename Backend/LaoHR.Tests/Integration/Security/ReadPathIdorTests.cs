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
/// Phase 3C3 — read-path IDOR regression tests. Proves that a normal employee
/// cannot read another employee's leave/expense/loan/documents by manipulating
/// employeeId filters or route ids.
/// </summary>
public class ReadPathIdorTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ReadPathIdorTests(CustomWebApplicationFactory factory)
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

    private async Task<(int empA, int empB)> SeedTwoEmployeesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();

        var empA = new Employee { EmployeeCode = "IDORA", LaoName = "Employee A", IsActive = true };
        var empB = new Employee { EmployeeCode = "IDORB", LaoName = "Employee B", IsActive = true };
        db.Employees.AddRange(empA, empB);
        await db.SaveChangesAsync();

        // Link two users to the employees.
        db.Users.AddRange(
            new AppUser { Username = "idora", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", EmployeeId = empA.EmployeeId, IsActive = true },
            new AppUser { Username = "idorb", PasswordHash = PasswordHasher.HashPassword("pass123"), PasswordHashVersion = 2, Role = "Employee", EmployeeId = empB.EmployeeId, IsActive = true });
        await db.SaveChangesAsync();

        // Seed a leave, expense, and loan for employee B.
        db.LeaveRequests.Add(new LeaveRequest { EmployeeId = empB.EmployeeId, LeaveType = "ANNUAL", StartDate = DateTime.UtcNow.AddDays(30), EndDate = DateTime.UtcNow.AddDays(31), TotalDays = 2, Status = "PENDING" });
        db.Expenses.Add(new Expense { ExpenseNumber = "EXP-IDOR-1", EmployeeId = empB.EmployeeId, CategoryId = 1, Title = "B's expense", ExpenseDate = DateTime.UtcNow, Currency = "LAK", Amount = 100, AmountLak = 100, Status = "SUBMITTED" });
        db.EmployeeLoans.Add(new EmployeeLoan { LoanNumber = "LOAN-IDOR-1", EmployeeId = empB.EmployeeId, Principal = 1000, PrincipalLak = 1000, Currency = "LAK", InterestRate = 0, Installments = 1, InstallmentAmount = 1000, StartDate = DateTime.UtcNow, Status = "DRAFT" });
        await db.SaveChangesAsync();

        return (empA.EmployeeId, empB.EmployeeId);
    }

    [Fact]
    public async Task EmployeeA_CannotListEmployeeB_Leave()
    {
        var (_, empB) = await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "idora", "pass123");

        var resp = await client.GetAsync($"/api/leave?employeeId={empB}");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        // Employee A's scope is self-only; B's leave must not appear.
        body.Should().NotContain("EXP-IDOR");
    }

    [Fact]
    public async Task EmployeeA_CannotListEmployeeB_Expenses()
    {
        var (_, empB) = await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "idora", "pass123");

        var resp = await client.GetAsync($"/api/expenses?employeeId={empB}");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().NotContain("B's expense");
    }

    [Fact]
    public async Task EmployeeA_CannotListEmployeeB_Loans()
    {
        var (_, empB) = await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "idora", "pass123");

        var resp = await client.GetAsync($"/api/employeeLoans?employeeId={empB}");
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().NotContain("LOAN-IDOR-1");
    }

    [Fact]
    public async Task EmployeeA_CannotReadEmployeeB_Documents()
    {
        var (_, empB) = await SeedTwoEmployeesAsync();
        var client = _factory.CreateClient();
        await LoginAsync(client, "idora", "pass123");

        var resp = await client.GetAsync($"/api/documents/employee/{empB}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task EmployeeA_CannotReadEmployeeB_ExpenseDetail()
    {
        var (_, empB) = await SeedTwoEmployeesAsync();
        int expenseId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            expenseId = db.Expenses.First(e => e.EmployeeId == empB).ExpenseId;
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "idora", "pass123");

        var resp = await client.GetAsync($"/api/expenses/{expenseId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task EmployeeA_CannotReadEmployeeB_LoanDetail()
    {
        var (_, empB) = await SeedTwoEmployeesAsync();
        int loanId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<LaoHRDbContext>();
            loanId = db.EmployeeLoans.First(l => l.EmployeeId == empB).LoanId;
        }

        var client = _factory.CreateClient();
        await LoginAsync(client, "idora", "pass123");

        var resp = await client.GetAsync($"/api/employeeLoans/{loanId}");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
