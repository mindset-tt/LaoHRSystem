using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class OrganizationHierarchyServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly OrganizationHierarchyService _service;

    public OrganizationHierarchyServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new OrganizationHierarchyService(_context);
    }

    private async Task SeedOrgAsync()
    {
        _context.Departments.AddRange(
            new Department { DepartmentId = 1, DepartmentName = "Operations", DepartmentCode = "OPS" },
            new Department { DepartmentId = 2, DepartmentName = "HR", DepartmentCode = "HR", ParentDepartmentId = 1 },
            new Department { DepartmentId = 3, DepartmentName = "Recruiting", DepartmentCode = "REC", ParentDepartmentId = 2 });

        _context.Employees.AddRange(
            new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "CEO", DepartmentId = 1 },
            new Employee { EmployeeId = 2, EmployeeCode = "E2", LaoName = "HR Director", DepartmentId = 2, ManagerId = 1 },
            new Employee { EmployeeId = 3, EmployeeCode = "E3", LaoName = "Recruiter", DepartmentId = 3, ManagerId = 2 });

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetDepartmentTree_ReturnsNestedHierarchy()
    {
        await SeedOrgAsync();

        var tree = await _service.GetDepartmentTreeAsync();

        tree.Should().HaveCount(1); // Operations is the only root
        tree[0].DepartmentName.Should().Be("Operations");
        tree[0].Children.Should().HaveCount(1);
        tree[0].Children[0].DepartmentName.Should().Be("HR");
        tree[0].Children[0].Children.Should().HaveCount(1);
        tree[0].Children[0].Children[0].DepartmentName.Should().Be("Recruiting");
    }

    [Fact]
    public async Task GetDirectReports_ReturnsOnlyDirectReports()
    {
        await SeedOrgAsync();

        var reports = await _service.GetDirectReportsAsync(1); // CEO

        reports.Should().HaveCount(1);
        reports[0].EmployeeCode.Should().Be("E2"); // HR Director only (not Recruiter)
    }

    [Fact]
    public async Task GetManagerChain_ReturnsChainInOrder()
    {
        await SeedOrgAsync();

        var chain = await _service.GetManagerChainAsync(3); // Recruiter

        chain.Should().HaveCount(2);
        chain[0].EmployeeCode.Should().Be("E2"); // HR Director
        chain[1].EmployeeCode.Should().Be("E1"); // CEO
    }

    [Fact]
    public async Task WouldCreateDepartmentCycle_SelfParent_ReturnsTrue()
    {
        await SeedOrgAsync();

        var wouldCycle = await _service.WouldCreateDepartmentCycleAsync(1, 1);

        wouldCycle.Should().BeTrue();
    }

    [Fact]
    public async Task WouldCreateDepartmentCycle_DeepCycle_ReturnsTrue()
    {
        await SeedOrgAsync();

        // Operations (1) → HR (2) → Recruiting (3). Reparenting Operations under Recruiting = cycle.
        var wouldCycle = await _service.WouldCreateDepartmentCycleAsync(1, 3);

        wouldCycle.Should().BeTrue();
    }

    [Fact]
    public async Task WouldCreateDepartmentCycle_ValidReparent_ReturnsFalse()
    {
        await SeedOrgAsync();

        // Reparent Recruiting (3) directly under Operations (1) — no cycle.
        var wouldCycle = await _service.WouldCreateDepartmentCycleAsync(3, 1);

        wouldCycle.Should().BeFalse();
    }

    [Fact]
    public async Task WouldCreateReportingCycle_SelfManager_ReturnsTrue()
    {
        await SeedOrgAsync();

        var wouldCycle = await _service.WouldCreateReportingCycleAsync(1, 1);

        wouldCycle.Should().BeTrue();
    }

    [Fact]
    public async Task WouldCreateReportingCycle_DeepCycle_ReturnsTrue()
    {
        await SeedOrgAsync();

        // CEO (1) → HR Director (2) → Recruiter (3). Making CEO report to Recruiter = cycle.
        var wouldCycle = await _service.WouldCreateReportingCycleAsync(1, 3);

        wouldCycle.Should().BeTrue();
    }

    [Fact]
    public async Task WouldCreateReportingCycle_ValidManager_ReturnsFalse()
    {
        await SeedOrgAsync();

        // Recruiter (3) reporting to CEO (1) — no cycle.
        var wouldCycle = await _service.WouldCreateReportingCycleAsync(3, 1);

        wouldCycle.Should().BeFalse();
    }
}