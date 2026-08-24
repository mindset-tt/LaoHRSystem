using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class BudgetServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly BudgetService _service;

    public BudgetServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new BudgetService(_context);
    }

    private async Task<int> SeedBudgetAsync(decimal approved)
    {
        var budget = new Budget
        {
            FiscalYear = 2026,
            Category = "IT",
            Currency = "LAK",
            ApprovedAmount = approved,
            Status = "APPROVED",
        };
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return budget.BudgetId;
    }

    [Fact]
    public async Task Reserve_WithinBudget_Succeeds()
    {
        var id = await SeedBudgetAsync(100);

        await _service.ReserveAsync(id, 70);

        var available = await _service.GetAvailableAsync(id);
        available.Should().Be(30);
    }

    [Fact]
    public async Task Reserve_OverBudget_Throws()
    {
        var id = await SeedBudgetAsync(100);

        var act = () => _service.ReserveAsync(id, 150);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Commit_ConvertsReservationToCommitment()
    {
        var id = await SeedBudgetAsync(100);

        await _service.ReserveAsync(id, 70);
        await _service.CommitAsync(id, 70);

        var budget = await _context.Budgets.FindAsync(id);
        budget!.ReservedAmount.Should().Be(0);
        budget.CommittedAmount.Should().Be(70);
        (await _service.GetAvailableAsync(id)).Should().Be(30);
    }

    [Fact]
    public async Task Commit_ExceedingReservation_ConsumesAdditionalBudget()
    {
        var id = await SeedBudgetAsync(100);

        await _service.ReserveAsync(id, 50);
        await _service.CommitAsync(id, 80); // 30 more than reserved

        var budget = await _context.Budgets.FindAsync(id);
        budget!.ReservedAmount.Should().Be(0);
        budget.CommittedAmount.Should().Be(80);
        (await _service.GetAvailableAsync(id)).Should().Be(20);
    }

    [Fact]
    public async Task ReleaseReservation_FreesBudget()
    {
        var id = await SeedBudgetAsync(100);

        await _service.ReserveAsync(id, 70);
        await _service.ReleaseReservationAsync(id, 70);

        (await _service.GetAvailableAsync(id)).Should().Be(100);
    }

    [Fact]
    public async Task SequentialReservations_RejectOverspend()
    {
        // Budget = 100. Reserve 70 (ok), then reserve 50 (would exceed → throws).
        // Total consumed stays at 70, never 120.
        var id = await SeedBudgetAsync(100);

        await _service.ReserveAsync(id, 70);

        var act = () => _service.ReserveAsync(id, 50);
        await act.Should().ThrowAsync<InvalidOperationException>();

        var budget = await _context.Budgets.FindAsync(id);
        budget!.ReservedAmount.Should().Be(70);
    }

    [Fact]
    public async Task RecognizeActual_MovesCommitmentToActual_NoDoubleCount()
    {
        var id = await SeedBudgetAsync(100);

        await _service.ReserveAsync(id, 100);
        await _service.CommitAsync(id, 100);

        // Recognize 40 actual: commitment 100 → 60, actual 0 → 40.
        await _service.RecognizeActualAsync(id, 40);

        var budget = await _context.Budgets.FindAsync(id);
        budget!.CommittedAmount.Should().Be(60);
        budget.ActualAmount.Should().Be(40);
        // Available = 100 - 0 - 60 - 40 = 0 (no double count).
        (await _service.GetAvailableAsync(id)).Should().Be(0);
    }

    [Fact]
    public async Task ReleaseCommitment_FreesBudget()
    {
        var id = await SeedBudgetAsync(100);

        await _service.ReserveAsync(id, 100);
        await _service.CommitAsync(id, 100);
        await _service.ReleaseCommitmentAsync(id, 100);

        var budget = await _context.Budgets.FindAsync(id);
        budget!.CommittedAmount.Should().Be(0);
        (await _service.GetAvailableAsync(id)).Should().Be(100);
    }

    [Fact]
    public async Task FullLifecycle_ReserveCommitActual_ReconcilesCorrectly()
    {
        // Budget = 1000.
        var id = await SeedBudgetAsync(1000);

        // PR = 600 → Reserved = 600.
        await _service.ReserveAsync(id, 600);
        var b = await _context.Budgets.FindAsync(id);
        b!.ReservedAmount.Should().Be(600);

        // PO = 600 → Reserved = 0, Committed = 600.
        await _service.CommitAsync(id, 600);
        b = await _context.Budgets.FindAsync(id);
        b!.ReservedAmount.Should().Be(0);
        b.CommittedAmount.Should().Be(600);

        // Invoice A = 400 → Committed = 200, Actual = 400.
        await _service.RecognizeActualAsync(id, 400);
        b = await _context.Budgets.FindAsync(id);
        b!.CommittedAmount.Should().Be(200);
        b.ActualAmount.Should().Be(400);

        // Invoice B = 200 → Committed = 0, Actual = 600.
        await _service.RecognizeActualAsync(id, 200);
        b = await _context.Budgets.FindAsync(id);
        b!.CommittedAmount.Should().Be(0);
        b.ActualAmount.Should().Be(600);

        // Available = 1000 - 0 - 0 - 600 = 400.
        (await _service.GetAvailableAsync(id)).Should().Be(400);
    }
}
