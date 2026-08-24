using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class ContractLifecycleServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly ContractLifecycleService _service;

    public ContractLifecycleServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new ContractLifecycleService(_context);
    }

    private async Task<int> SeedContractAsync()
    {
        var contract = new Contract
        {
            ContractNumber = "CTR-1",
            Title = "Service Agreement",
            OwnerEmployeeId = 1,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 12, 31),
            Amount = 1000,
            Status = "ACTIVE",
        };
        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();
        return contract.ContractId;
    }

    [Fact]
    public async Task Renew_PreservesPriorTermInHistory()
    {
        var id = await SeedContractAsync();

        await _service.RenewAsync(id, new DateTime(2027, 1, 1), new DateTime(2027, 12, 31), 1200, 1, "Renewed");

        var contract = await _context.Contracts.FindAsync(id);
        contract!.StartDate.Should().Be(new DateTime(2027, 1, 1));
        contract.Amount.Should().Be(1200);
        contract.Status.Should().Be("ACTIVE");

        var history = await _context.ContractHistories.SingleAsync();
        history.ChangeType.Should().Be("RENEWAL");
        history.PreviousStartDate.Should().Be(new DateTime(2026, 1, 1));
        history.PreviousAmount.Should().Be(1000);
        history.NewStartDate.Should().Be(new DateTime(2027, 1, 1));
        history.NewAmount.Should().Be(1200);
    }

    [Fact]
    public async Task Renew_EndBeforeStart_Throws()
    {
        var id = await SeedContractAsync();
        var act = () => _service.RenewAsync(id, new DateTime(2027, 1, 1), new DateTime(2026, 1, 1), 1200, 1, null);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Terminate_PreservesPriorStatusInHistory()
    {
        var id = await SeedContractAsync();

        await _service.TerminateAsync(id, 1, "Ended");

        var contract = await _context.Contracts.FindAsync(id);
        contract!.Status.Should().Be("TERMINATED");

        var history = await _context.ContractHistories.SingleAsync();
        history.ChangeType.Should().Be("TERMINATION");
        history.PreviousStatus.Should().Be("ACTIVE");
        history.NewStatus.Should().Be("TERMINATED");
    }
}
