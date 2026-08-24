using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4A.1 — budget consumption with concurrency safety.
///
/// Semantics (each amount resides in exactly ONE state — no double counting):
///   Reserved  → PR approved (reservation)
///   Committed → PO created (reservation converted, or direct commitment)
///   Actual    → expense/AP posted (Phase 4B)
///
/// Available = Approved - Reserved - Committed - Actual.
///
/// Concurrency: consumption uses a PostgreSQL row lock (SELECT ... FOR UPDATE)
/// inside a transaction so two simultaneous approvals cannot both consume the
/// same remaining budget. Overspend is rejected unless AllowBudgetOverrun=true.
/// </summary>
public interface IBudgetService
{
    /// <summary>Reserves an amount against a budget (PR approval). Throws on overspend.</summary>
    Task ReserveAsync(int budgetId, decimal amount, CancellationToken ct = default);

    /// <summary>Converts a reservation to a commitment (PO creation).</summary>
    Task CommitAsync(int budgetId, decimal amount, CancellationToken ct = default);

    /// <summary>Releases a reservation (PR rejected/cancelled).</summary>
    Task ReleaseReservationAsync(int budgetId, decimal amount, CancellationToken ct = default);

    /// <summary>Releases a commitment (PO cancelled).</summary>
    Task ReleaseCommitmentAsync(int budgetId, decimal amount, CancellationToken ct = default);

    /// <summary>
    /// Recognizes an actual (posted invoice/expense): reduces commitment and
    /// increases actual, so the amount moves from Committed → Actual (no double count).
    /// </summary>
    Task RecognizeActualAsync(int budgetId, decimal amount, CancellationToken ct = default);

    /// <summary>Returns the current available amount for a budget.</summary>
    Task<decimal> GetAvailableAsync(int budgetId, CancellationToken ct = default);
}

public sealed class BudgetService : IBudgetService
{
    private readonly LaoHRDbContext _context;

    public BudgetService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task ReserveAsync(int budgetId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) return;

        var relational = _context.Database.IsRelational();
        var alreadyInTransaction = relational && _context.Database.CurrentTransaction != null;
        await using var tx = (relational && !alreadyInTransaction) ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var budget = await LockBudgetAsync(budgetId, ct);
            var available = budget.ApprovedAmount - budget.ReservedAmount - budget.CommittedAmount - budget.ActualAmount;
            if (available < amount && !await AllowOverrunAsync(ct))
                throw new InvalidOperationException(
                    $"Insufficient budget: available {available}, requested {amount}.");

            budget.ReservedAmount += amount;
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task CommitAsync(int budgetId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) return;

        var relational = _context.Database.IsRelational();
        var alreadyInTransaction = relational && _context.Database.CurrentTransaction != null;
        await using var tx = (relational && !alreadyInTransaction) ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var budget = await LockBudgetAsync(budgetId, ct);

            // Convert reservation → commitment. If the PO amount exceeds the
            // reservation (e.g. price changed), the delta is a new commitment.
            var delta = amount - budget.ReservedAmount;
            if (delta > 0)
            {
                var available = budget.ApprovedAmount - budget.ReservedAmount - budget.CommittedAmount - budget.ActualAmount;
                if (available < delta && !await AllowOverrunAsync(ct))
                    throw new InvalidOperationException(
                        $"Insufficient budget: available {available}, additional commitment {delta}.");
                budget.CommittedAmount += delta;
            }

            // Move the reserved portion into committed.
            var reservedToConvert = Math.Min(budget.ReservedAmount, amount);
            budget.ReservedAmount -= reservedToConvert;
            budget.CommittedAmount += reservedToConvert;

            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ReleaseReservationAsync(int budgetId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) return;

        var relational = _context.Database.IsRelational();
        var alreadyInTransaction = relational && _context.Database.CurrentTransaction != null;
        await using var tx = (relational && !alreadyInTransaction) ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var budget = await LockBudgetAsync(budgetId, ct);
            budget.ReservedAmount = Math.Max(0, budget.ReservedAmount - amount);
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task ReleaseCommitmentAsync(int budgetId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) return;

        var relational = _context.Database.IsRelational();
        var alreadyInTransaction = relational && _context.Database.CurrentTransaction != null;
        await using var tx = (relational && !alreadyInTransaction) ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var budget = await LockBudgetAsync(budgetId, ct);
            budget.CommittedAmount = Math.Max(0, budget.CommittedAmount - amount);
            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task RecognizeActualAsync(int budgetId, decimal amount, CancellationToken ct = default)
    {
        if (amount <= 0) return;

        var relational = _context.Database.IsRelational();
        var alreadyInTransaction = relational && _context.Database.CurrentTransaction != null;
        await using var tx = (relational && !alreadyInTransaction) ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var budget = await LockBudgetAsync(budgetId, ct);

            // Move the amount from Committed → Actual (no double count).
            var fromCommitment = Math.Min(budget.CommittedAmount, amount);
            budget.CommittedAmount -= fromCommitment;
            budget.ActualAmount += amount;

            budget.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<decimal> GetAvailableAsync(int budgetId, CancellationToken ct = default)
    {
        var budget = await _context.Budgets.AsNoTracking()
            .FirstOrDefaultAsync(b => b.BudgetId == budgetId, ct);
        if (budget == null) return 0;
        return budget.ApprovedAmount - budget.ReservedAmount - budget.CommittedAmount - budget.ActualAmount;
    }

    /// <summary>Locks the budget row for the duration of the transaction.</summary>
    private async Task<Budget> LockBudgetAsync(int budgetId, CancellationToken ct)
    {
        Budget? budget;
        if (_context.Database.IsRelational())
        {
            // PostgreSQL row lock via FOR UPDATE (serializes concurrent consumption).
            budget = await _context.Budgets
                .FromSqlRaw("SELECT * FROM \"Budgets\" WHERE \"BudgetId\" = {0} FOR UPDATE", budgetId)
                .FirstOrDefaultAsync(ct);
        }
        else
        {
            // InMemory (tests) has no row locking; the transaction still serializes
            // within a single test's synchronous flow.
            budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == budgetId, ct);
        }

        if (budget == null)
            throw new InvalidOperationException($"Budget {budgetId} not found.");
        return budget;
    }

    private async Task<bool> AllowOverrunAsync(CancellationToken ct)
    {
        var setting = await _context.SystemSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingKey == "ALLOW_BUDGET_OVERRUN", ct);
        return setting != null && string.Equals(setting.SettingValue, "true", StringComparison.OrdinalIgnoreCase);
    }
}
