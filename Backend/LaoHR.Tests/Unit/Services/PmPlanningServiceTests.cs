using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 3C4 — PM planning service tests: task hierarchy cycles, dependency
/// cycles, capacity/overallocation, project health, portfolio.
/// </summary>
public class PmPlanningServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly PmPlanningService _service;

    public PmPlanningServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new PmPlanningService(_context);
    }

    private async Task SeedTasksAsync()
    {
        _context.Projects.Add(new Project { ProjectId = 1, Code = "P1", Name = "One", Status = "ACTIVE", OwnerId = 1 });
        _context.ProjectTasks.AddRange(
            new ProjectTask { TaskId = 1, ProjectId = 1, Title = "A", Status = "TODO" },
            new ProjectTask { TaskId = 2, ProjectId = 1, Title = "B", Status = "TODO" },
            new ProjectTask { TaskId = 3, ProjectId = 1, Title = "C", Status = "TODO" });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task TaskCycle_SelfParent_Detected()
    {
        await SeedTasksAsync();
        (await _service.WouldCreateTaskCycleAsync(1, 1)).Should().BeTrue();
    }

    [Fact]
    public async Task TaskCycle_DeepCycle_Detected()
    {
        await SeedTasksAsync();
        // 1 -> 2 -> 3, then try 3 -> 1 (cycle).
        _context.ProjectTasks.Find(2)!.ParentTaskId = 1;
        _context.ProjectTasks.Find(3)!.ParentTaskId = 2;
        await _context.SaveChangesAsync();

        (await _service.WouldCreateTaskCycleAsync(1, 3)).Should().BeTrue();
    }

    [Fact]
    public async Task TaskCycle_NoCycle_Allowed()
    {
        await SeedTasksAsync();
        (await _service.WouldCreateTaskCycleAsync(1, 2)).Should().BeFalse();
    }

    [Fact]
    public async Task DependencyCycle_Self_Detected()
    {
        await SeedTasksAsync();
        (await _service.WouldCreateDependencyCycleAsync(1, 1, 1)).Should().BeTrue();
    }

    [Fact]
    public async Task DependencyCycle_DeepCycle_Detected()
    {
        await SeedTasksAsync();
        // Existing: 1 -> 2, 2 -> 3. Adding 3 -> 1 creates a cycle.
        _context.TaskDependencies.AddRange(
            new TaskDependency { ProjectId = 1, PredecessorTaskId = 1, SuccessorTaskId = 2 },
            new TaskDependency { ProjectId = 1, PredecessorTaskId = 2, SuccessorTaskId = 3 });
        await _context.SaveChangesAsync();

        (await _service.WouldCreateDependencyCycleAsync(1, 3, 1)).Should().BeTrue();
    }

    [Fact]
    public async Task DependencyCycle_NoCycle_Allowed()
    {
        await SeedTasksAsync();
        _context.TaskDependencies.Add(new TaskDependency { ProjectId = 1, PredecessorTaskId = 1, SuccessorTaskId = 2 });
        await _context.SaveChangesAsync();

        (await _service.WouldCreateDependencyCycleAsync(1, 2, 3)).Should().BeFalse();
    }

    [Fact]
    public async Task Workload_Overallocated_Detected()
    {
        _context.Employees.Add(new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "A", IsActive = true });
        _context.Projects.Add(new Project { ProjectId = 1, Code = "P1", Name = "One", Status = "ACTIVE", OwnerId = 1 });
        _context.Resources.AddRange(
            new Resource { ProjectId = 1, EmployeeId = 1, AllocationPercent = 60 },
            new Resource { ProjectId = 1, EmployeeId = 1, AllocationPercent = 70 });
        await _context.SaveChangesAsync();

        var result = await _service.GetResourceWorkloadAsync();

        result.Should().HaveCount(1);
        result[0].TotalAllocationPercent.Should().Be(130);
        result[0].IsOverallocated.Should().BeTrue();
    }

    [Fact]
    public async Task Health_OverdueTasks_AtRisk()
    {
        _context.Projects.Add(new Project { ProjectId = 1, Code = "P1", Name = "One", Status = "ACTIVE", OwnerId = 1 });
        _context.ProjectTasks.Add(new ProjectTask { TaskId = 1, ProjectId = 1, Title = "A", Status = "TODO", DueDate = DateTime.UtcNow.AddDays(-1) });
        await _context.SaveChangesAsync();

        var result = await _service.GetProjectHealthAsync(1);

        result.ScheduleHealth.Should().Be("AT_RISK");
        result.OverdueTaskCount.Should().Be(1);
    }

    [Fact]
    public async Task Health_CriticalRisk_Red()
    {
        _context.Projects.Add(new Project { ProjectId = 1, Code = "P1", Name = "One", Status = "ACTIVE", OwnerId = 1 });
        _context.Risks.Add(new Risk { ProjectId = 1, Title = "R", Status = "OPEN", Priority = "CRITICAL" });
        await _context.SaveChangesAsync();

        var result = await _service.GetProjectHealthAsync(1);

        result.RiskHealth.Should().Be("RED");
        result.OverallHealth.Should().Be("RED");
    }

    [Fact]
    public async Task Portfolio_ComputesProgressAndHealth()
    {
        _context.Projects.Add(new Project { ProjectId = 1, Code = "P1", Name = "One", Status = "ACTIVE", OwnerId = 1 });
        _context.ProjectTasks.AddRange(
            new ProjectTask { TaskId = 1, ProjectId = 1, Title = "A", Status = "DONE" },
            new ProjectTask { TaskId = 2, ProjectId = 1, Title = "B", Status = "TODO" });
        await _context.SaveChangesAsync();

        var result = await _service.GetPortfolioAsync();

        result.Should().HaveCount(1);
        result[0].ProgressPercent.Should().Be(50);
    }
}
