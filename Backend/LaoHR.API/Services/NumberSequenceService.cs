using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4A — concurrency-safe document numbering. Replaces the inline
/// "StartsWith + OrderByDescending + parse" pattern (which is not safe under
/// concurrent requests) with a DB-backed sequence row per (prefix, year).
///
/// Concurrency safety: the sequence row is locked (SELECT ... FOR UPDATE on
/// PostgreSQL) inside a transaction, so two concurrent requests cannot read the
/// same LastValue and produce duplicate numbers.
///
/// Callers SHOULD invoke this inside the same transaction as the entity insert
/// so that a rollback also rolls back the sequence increment (no gaps on failure).
/// </summary>
public interface INumberSequenceService
{
    /// <summary>
    /// Returns the next number formatted as "{prefix}-{yyyy}-{seq:000000}".
    /// </summary>
    Task<string> NextAsync(string prefix, CancellationToken ct = default);
}

public sealed class NumberSequenceService : INumberSequenceService
{
    private readonly LaoHRDbContext _context;

    public NumberSequenceService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<string> NextAsync(string prefix, CancellationToken ct = default)
    {
        var year = DateTime.UtcNow.Year;

        var relational = _context.Database.IsRelational();
        var alreadyInTransaction = relational && _context.Database.CurrentTransaction != null;
        await using var tx = (relational && !alreadyInTransaction)
            ? await _context.Database.BeginTransactionAsync(ct)
            : null;
        try
        {
            var seq = await LockSequenceAsync(prefix, year, ct);

            if (seq == null)
            {
                seq = new NumberSequence { Prefix = prefix, Year = year, LastValue = 1 };
                _context.NumberSequences.Add(seq);
            }
            else
            {
                seq.LastValue += 1;
                seq.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
            return $"{prefix}-{year}-{seq.LastValue:000000}";
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>Locks the sequence row (FOR UPDATE on PostgreSQL) for the transaction.</summary>
    private async Task<NumberSequence?> LockSequenceAsync(string prefix, int year, CancellationToken ct)
    {
        if (_context.Database.IsRelational())
        {
            return await _context.NumberSequences
                .FromSqlRaw("SELECT * FROM \"NumberSequences\" WHERE \"Prefix\" = {0} AND \"Year\" = {1} FOR UPDATE", prefix, year)
                .FirstOrDefaultAsync(ct);
        }

        // InMemory (tests) has no row locking; the transaction still serializes
        // within a single synchronous flow.
        return await _context.NumberSequences
            .FirstOrDefaultAsync(n => n.Prefix == prefix && n.Year == year, ct);
    }
}
