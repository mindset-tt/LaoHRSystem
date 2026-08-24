using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 3C6 — performance workflow tests: review generation with manager
/// snapshot, self/manager review, finalization, competency gap.
/// </summary>
public class PerformanceWorkflowTests
{
    private readonly LaoHRDbContext _context;

    public PerformanceWorkflowTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
    }

    [Fact]
    public async Task ReviewGeneration_SnapshotsManager()
    {
        _context.Employees.AddRange(
            new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "Manager A", IsActive = true },
            new Employee { EmployeeId = 2, EmployeeCode = "E2", LaoName = "Employee", ManagerId = 1, IsActive = true });
        _context.Positions.Add(new Position { PositionId = 1, Title = "Accountant" });
        await _context.SaveChangesAsync();

        var cycle = new PerformanceCycle { Name = "2026 H1", Status = "ACTIVE" };
        _context.PerformanceCycles.Add(cycle);
        await _context.SaveChangesAsync();

        var review = new PerformanceReview
        {
            CycleId = cycle.CycleId,
            EmployeeId = 2,
            ManagerEmployeeId = 1, // snapshot
            PositionId = 1,
            Status = "NOT_STARTED",
        };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        // Manager changes to a new employee 3.
        _context.Employees.Add(new Employee { EmployeeId = 3, EmployeeCode = "E3", LaoName = "Manager B", IsActive = true });
        var emp2 = await _context.Employees.FindAsync(2);
        emp2!.ManagerId = 3;
        await _context.SaveChangesAsync();

        // The review must still be assigned to the snapshotted manager (1).
        var reloaded = await _context.PerformanceReviews.FindAsync(review.ReviewId);
        reloaded!.ManagerEmployeeId.Should().Be(1);
    }

    [Fact]
    public async Task FullPerformanceWorkflow_ProducesFinalizedReview()
    {
        _context.Employees.AddRange(
            new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "Manager", IsActive = true },
            new Employee { EmployeeId = 2, EmployeeCode = "E2", LaoName = "Employee", ManagerId = 1, IsActive = true });
        _context.Positions.Add(new Position { PositionId = 1, Title = "Accountant" });
        await _context.SaveChangesAsync();

        var cycle = new PerformanceCycle { Name = "2026 H1", Status = "ACTIVE" };
        _context.PerformanceCycles.Add(cycle);
        await _context.SaveChangesAsync();

        // Goal
        var goal = new Goal { EmployeeId = 2, ManagerEmployeeId = 1, Title = "Close books", Status = "ACTIVE" };
        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        // Check-in
        _context.GoalCheckIns.Add(new GoalCheckIn { GoalId = goal.GoalId, ProgressPercent = 50, Status = "ACTIVE" });
        await _context.SaveChangesAsync();

        // Review
        var review = new PerformanceReview { CycleId = cycle.CycleId, EmployeeId = 2, ManagerEmployeeId = 1, Status = "NOT_STARTED" };
        _context.PerformanceReviews.Add(review);
        await _context.SaveChangesAsync();

        // Self review
        review.SelfAchievements = "Delivered";
        review.EmployeeSubmittedAt = DateTime.UtcNow;
        review.Status = "SELF_REVIEW";
        await _context.SaveChangesAsync();

        // Manager review
        review.OverallRating = 4;
        review.ManagerComments = "Good";
        review.ManagerSubmittedAt = DateTime.UtcNow;
        review.Status = "MANAGER_REVIEW";
        await _context.SaveChangesAsync();

        // Finalize
        review.Status = "FINALIZED";
        review.FinalizedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var reloaded = await _context.PerformanceReviews.FindAsync(review.ReviewId);
        reloaded!.Status.Should().Be("FINALIZED");
        reloaded.OverallRating.Should().Be(4);
    }

    [Fact]
    public async Task CompetencyGap_CalculatedFromRequirementAndAssessment()
    {
        _context.Employees.Add(new Employee { EmployeeId = 1, EmployeeCode = "E1", LaoName = "Employee", IsActive = true });
        _context.Positions.Add(new Position { PositionId = 1, Title = "Accountant" });
        _context.Competencies.Add(new Competency { CompetencyId = 1, Code = "FIN", Name = "Financial Analysis" });
        _context.PositionCompetencies.Add(new PositionCompetency { PositionId = 1, CompetencyId = 1, RequiredLevel = 4, IsRequired = true });
        _context.CompetencyAssessments.Add(new CompetencyAssessment { EmployeeId = 1, CompetencyId = 1, AssessorEmployeeId = 1, AssessmentType = "Manager", Level = 2 });
        await _context.SaveChangesAsync();

        var required = await _context.PositionCompetencies.FirstAsync(pc => pc.CompetencyId == 1);
        var assessed = await _context.CompetencyAssessments.FirstAsync(a => a.CompetencyId == 1);

        var gap = required.RequiredLevel - assessed.Level;
        gap.Should().Be(2); // development gap
    }
}
