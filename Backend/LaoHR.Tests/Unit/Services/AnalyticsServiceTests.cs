using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Moq;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 3C3 — analytics KPI validation with small known datasets.
/// </summary>
public class AnalyticsServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);

        // Scope: privileged (Admin) so all employees are visible.
        var httpContext = new DefaultHttpContext();
        httpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin"),
                new System.Security.Claims.Claim("EmployeeId", "1"),
            }, "test"));
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext);
        var scope = new DataScopeService(_context, accessor.Object);

        _service = new AnalyticsService(_context, scope);
    }

    private async Task SeedHeadcountAsync()
    {
        // 3 employees: 2 active, 1 inactive.
        _context.Employees.AddRange(
            new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "A", IsActive = true, HireDate = new DateTime(2026, 1, 1) },
            new Employee { EmployeeId = 2, EmployeeCode = "E2", LaoName = "B", IsActive = true, HireDate = new DateTime(2026, 2, 1) },
            new Employee { EmployeeId = 3, EmployeeCode = "E3", LaoName = "C", IsActive = false, HireDate = new DateTime(2026, 3, 1) });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task Hr_HeadcountActive_CountsOnlyActive()
    {
        await SeedHeadcountAsync();

        var result = await _service.GetHrAsync();

        var activeKpi = result.Kpis.First(k => k.Id == "HR-HEADCOUNT-ACTIVE");
        activeKpi.Value.Should().Be(2);
        var inactiveKpi = result.Kpis.First(k => k.Id == "HR-HEADCOUNT-INACTIVE");
        inactiveKpi.Value.Should().Be(1);
    }

    [Fact]
    public async Task Executive_ActiveHeadcount_Matches()
    {
        await SeedHeadcountAsync();

        var result = await _service.GetExecutiveAsync();

        result.Kpis.First(k => k.Id == "HR-HEADCOUNT-ACTIVE").Value.Should().Be(2);
    }

    [Fact]
    public async Task Pm_ProjectStatus_CountsByStatus()
    {
        _context.Projects.AddRange(
            new Project { ProjectId = 1, Code = "P1", Name = "One", Status = "ACTIVE", OwnerId = 1 },
            new Project { ProjectId = 2, Code = "P2", Name = "Two", Status = "ACTIVE", OwnerId = 1 },
            new Project { ProjectId = 3, Code = "P3", Name = "Three", Status = "COMPLETED", OwnerId = 1 });
        await _context.SaveChangesAsync();

        var result = await _service.GetPmAsync();

        result.Kpis.First(k => k.Id == "PM-PROJECT-ACTIVE").Value.Should().Be(2);
        result.Kpis.First(k => k.Id == "PM-PROJECT-COMPLETED").Value.Should().Be(1);
    }

    [Fact]
    public async Task Payroll_GrossTotal_SumsSlips()
    {
        _context.Employees.Add(new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "A", IsActive = true });
        _context.PayrollPeriods.Add(new PayrollPeriod { PeriodId = 1, Year = 2026, Month = 1, PeriodName = "Jan", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31) });
        _context.SalarySlips.AddRange(
            new SalarySlip { SlipId = 1, EmployeeId = 1, PeriodId = 1, GrossIncome = 1000, NetSalary = 900 },
            new SalarySlip { SlipId = 2, EmployeeId = 1, PeriodId = 1, GrossIncome = 2000, NetSalary = 1800 });
        await _context.SaveChangesAsync();

        var result = await _service.GetPayrollAsync();

        result.Kpis.First(k => k.Id == "PAY-GROSS-TOTAL").Value.Should().Be(3000);
        result.Kpis.First(k => k.Id == "PAY-NET-TOTAL").Value.Should().Be(2700);
    }

    [Fact]
    public async Task Finance_ExpenseApproved_SumsAmountLak()
    {
        _context.Employees.Add(new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "A", IsActive = true });
        _context.Expenses.AddRange(
            new Expense { ExpenseId = 1, ExpenseNumber = "X1", EmployeeId = 1, CategoryId = 1, Title = "a", ExpenseDate = DateTime.UtcNow, Currency = "LAK", Amount = 100, AmountLak = 100, Status = "APPROVED" },
            new Expense { ExpenseId = 2, ExpenseNumber = "X2", EmployeeId = 1, CategoryId = 1, Title = "b", ExpenseDate = DateTime.UtcNow, Currency = "LAK", Amount = 50, AmountLak = 50, Status = "SUBMITTED" });
        await _context.SaveChangesAsync();

        var result = await _service.GetFinanceAsync(null, null, null);

        result.Kpis.First(k => k.Id == "FIN-EXPENSE-APPROVED").Value.Should().Be(100);
        result.Kpis.First(k => k.Id == "FIN-EXPENSE-SUBMITTED").Value.Should().Be(50);
    }
}
