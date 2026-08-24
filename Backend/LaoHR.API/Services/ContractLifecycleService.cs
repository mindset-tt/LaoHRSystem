using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4C — contract lifecycle (renewal/amendment/termination with history).
///
/// History invariant: prior terms are preserved in <see cref="ContractHistory"/>
/// and never silently overwritten.
/// </summary>
public interface IContractLifecycleService
{
    /// <summary>Renews a contract, preserving prior term in history.</summary>
    Task<Contract> RenewAsync(int contractId, DateTime newStartDate, DateTime? newEndDate,
        decimal? newAmount, int changedByEmployeeId, string? notes, CancellationToken ct = default);

    /// <summary>Terminates a contract, preserving prior status in history.</summary>
    Task<Contract> TerminateAsync(int contractId, int changedByEmployeeId, string? notes, CancellationToken ct = default);
}

public sealed class ContractLifecycleService : IContractLifecycleService
{
    private readonly LaoHRDbContext _context;

    public ContractLifecycleService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<Contract> RenewAsync(int contractId, DateTime newStartDate, DateTime? newEndDate,
        decimal? newAmount, int changedByEmployeeId, string? notes, CancellationToken ct = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.ContractId == contractId, ct)
            ?? throw new InvalidOperationException("Contract not found.");

        if (newEndDate.HasValue && newEndDate.Value < newStartDate)
            throw new InvalidOperationException("Contract end date must be on or after start date.");

        _context.ContractHistories.Add(new ContractHistory
        {
            ContractId = contractId,
            ChangeType = "RENEWAL",
            Notes = notes,
            PreviousStartDate = contract.StartDate,
            PreviousEndDate = contract.EndDate,
            PreviousAmount = contract.Amount,
            PreviousStatus = contract.Status,
            NewStartDate = newStartDate,
            NewEndDate = newEndDate,
            NewAmount = newAmount,
            NewStatus = "ACTIVE",
            ChangedByEmployeeId = changedByEmployeeId,
        });

        contract.StartDate = newStartDate;
        contract.EndDate = newEndDate;
        contract.Amount = newAmount;
        contract.Status = "ACTIVE";
        contract.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return contract;
    }

    public async Task<Contract> TerminateAsync(int contractId, int changedByEmployeeId, string? notes, CancellationToken ct = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.ContractId == contractId, ct)
            ?? throw new InvalidOperationException("Contract not found.");

        _context.ContractHistories.Add(new ContractHistory
        {
            ContractId = contractId,
            ChangeType = "TERMINATION",
            Notes = notes,
            PreviousStartDate = contract.StartDate,
            PreviousEndDate = contract.EndDate,
            PreviousAmount = contract.Amount,
            PreviousStatus = contract.Status,
            NewStatus = "TERMINATED",
            ChangedByEmployeeId = changedByEmployeeId,
        });

        contract.Status = "TERMINATED";
        contract.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return contract;
    }
}
