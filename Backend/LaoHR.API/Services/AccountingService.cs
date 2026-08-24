using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4B — accounting service. Enforces double-entry invariants and posted
/// journal immutability. The General Ledger is DERIVED from POSTED journal lines
/// (no mutable GL balance table as sole truth).
///
/// Invariants:
///   - SUM(DEBIT) == SUM(CREDIT) for every POSTED journal (no tolerance).
///   - A journal line must not have Debit > 0 AND Credit > 0, nor both zero.
///   - Journal lines must target posting accounts (IsPostingAccount == true).
///   - Posted journals are immutable (correction via reversal + new journal).
///   - Posting to a CLOSED/LOCKED period is rejected.
/// </summary>
public interface IAccountingService
{
    /// <summary>Posts a draft journal (validates balance, period, accounts).</summary>
    Task<JournalEntry> PostAsync(int journalEntryId, int postedByEmployeeId, CancellationToken ct = default);

    /// <summary>Reverses a posted journal (creates a new entry with inverted lines).</summary>
    Task<JournalEntry> ReverseAsync(int journalEntryId, int postedByEmployeeId, CancellationToken ct = default);

    /// <summary>Returns the account balance (derived from posted journal lines).</summary>
    Task<decimal> GetAccountBalanceAsync(int accountId, CancellationToken ct = default);

    /// <summary>Returns a trial balance (opening/debit/credit/closing per account).</summary>
    Task<List<TrialBalanceRow>> GetTrialBalanceAsync(int fiscalPeriodId, CancellationToken ct = default);

    /// <summary>Returns the general ledger (posted journal lines) for a fiscal period.</summary>
    Task<List<GeneralLedgerRow>> GetGeneralLedgerAsync(int fiscalPeriodId, CancellationToken ct = default);
}

public sealed class TrialBalanceRow
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

public sealed class GeneralLedgerRow
{
    public int JournalLineId { get; set; }
    public int JournalEntryId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public DateTime PostingDate { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int? SourceId { get; set; }
    public string? Description { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int? CostCenterId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
}

public sealed class AccountingService : IAccountingService
{
    private readonly LaoHRDbContext _context;
    private readonly INumberSequenceService _numbers;

    public AccountingService(LaoHRDbContext context, INumberSequenceService numbers)
    {
        _context = context;
        _numbers = numbers;
    }

    public async Task<JournalEntry> PostAsync(int journalEntryId, int postedByEmployeeId, CancellationToken ct = default)
    {
        // Only begin a transaction if one is not already active (PostingService
        // wraps auto-posting in its own transaction; nested transactions are not
        // supported by Npgsql).
        var relational = _context.Database.IsRelational();
        var alreadyInTransaction = relational && _context.Database.CurrentTransaction != null;
        await using var tx = (relational && !alreadyInTransaction)
            ? await _context.Database.BeginTransactionAsync(ct)
            : null;

        try
        {
            // Lock the journal entry row (FOR UPDATE on PostgreSQL) so two
            // concurrent POST actions serialize and only one transitions DRAFT → POSTED.
            var entry = await LockJournalAsync(journalEntryId, ct)
                ?? throw new InvalidOperationException("Journal entry not found.");

            if (entry.Status != "DRAFT")
                throw new InvalidOperationException($"Cannot post a journal in '{entry.Status}' state.");

            // Load lines (after the row lock is held).
            await _context.Entry(entry).Collection(e => e.Lines).LoadAsync(ct);

            // Period must be OPEN.
            var period = await _context.FiscalPeriods
                .FirstOrDefaultAsync(p => p.FiscalPeriodId == entry.FiscalPeriodId, ct)
                ?? throw new InvalidOperationException("Fiscal period not found.");
            if (period.Status != "OPEN")
                throw new InvalidOperationException($"Cannot post to a '{period.Status}' period.");

            // Double-entry invariant.
            var totalDebit = entry.Lines.Sum(l => l.Debit);
            var totalCredit = entry.Lines.Sum(l => l.Credit);
            if (totalDebit != totalCredit)
                throw new InvalidOperationException(
                    $"Journal is unbalanced: debit {totalDebit}, credit {totalCredit}.");

            if (entry.Lines.Count == 0)
                throw new InvalidOperationException("Journal has no lines.");

            // Line invariants + posting account validation.
            foreach (var line in entry.Lines)
            {
                if (line.Debit > 0 && line.Credit > 0)
                    throw new InvalidOperationException("A journal line cannot have both debit and credit.");
                if (line.Debit <= 0 && line.Credit <= 0)
                    throw new InvalidOperationException("A journal line must have a non-zero debit or credit.");

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.AccountId == line.AccountId, ct)
                    ?? throw new InvalidOperationException($"Account {line.AccountId} not found.");
                if (!account.IsPostingAccount)
                    throw new InvalidOperationException($"Account '{account.AccountCode}' is not a posting account.");
            }

            entry.Status = "POSTED";
            entry.PostedByEmployeeId = postedByEmployeeId;
            entry.PostedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
            return entry;
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>Locks the journal entry row (FOR UPDATE on PostgreSQL) for the transaction.</summary>
    private async Task<JournalEntry?> LockJournalAsync(int journalEntryId, CancellationToken ct)
    {
        if (_context.Database.IsRelational())
        {
            return await _context.JournalEntries
                .FromSqlRaw("SELECT * FROM \"JournalEntries\" WHERE \"JournalEntryId\" = {0} FOR UPDATE", journalEntryId)
                .FirstOrDefaultAsync(ct);
        }
        return await _context.JournalEntries
            .FirstOrDefaultAsync(e => e.JournalEntryId == journalEntryId, ct);
    }

    public async Task<JournalEntry> ReverseAsync(int journalEntryId, int postedByEmployeeId, CancellationToken ct = default)
    {
        var original = await _context.JournalEntries
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.JournalEntryId == journalEntryId, ct)
            ?? throw new InvalidOperationException("Journal entry not found.");

        if (original.Status != "POSTED")
            throw new InvalidOperationException("Only POSTED journals can be reversed.");

        var reversal = new JournalEntry
        {
            JournalNumber = await _numbers.NextAsync("JE"),
            PostingDate = DateTime.UtcNow,
            FiscalPeriodId = original.FiscalPeriodId,
            SourceType = "MANUAL_JOURNAL",
            Description = $"Reversal of {original.JournalNumber}",
            Status = "DRAFT",
            Currency = original.Currency,
            CreatedByEmployeeId = postedByEmployeeId,
            ReversesJournalEntryId = original.JournalEntryId,
        };

        foreach (var line in original.Lines)
        {
            reversal.Lines.Add(new JournalLine
            {
                AccountId = line.AccountId,
                Description = line.Description,
                Debit = line.Credit,   // inverted
                Credit = line.Debit,   // inverted
                CostCenterId = line.CostCenterId,
                DepartmentId = line.DepartmentId,
                ProjectId = line.ProjectId,
                Currency = line.Currency,
                ExchangeRate = line.ExchangeRate,
                Reference = line.Reference,
            });
        }

        _context.JournalEntries.Add(reversal);
        await _context.SaveChangesAsync(ct);

        // Post the reversal immediately.
        return await PostAsync(reversal.JournalEntryId, postedByEmployeeId, ct);
    }

    public async Task<decimal> GetAccountBalanceAsync(int accountId, CancellationToken ct = default)
    {
        var debits = await _context.JournalLines
            .Where(l => l.AccountId == accountId && l.JournalEntry!.Status == "POSTED")
            .SumAsync(l => (decimal?)l.Debit, ct) ?? 0m;
        var credits = await _context.JournalLines
            .Where(l => l.AccountId == accountId && l.JournalEntry!.Status == "POSTED")
            .SumAsync(l => (decimal?)l.Credit, ct) ?? 0m;
        return debits - credits;
    }

    public async Task<List<TrialBalanceRow>> GetTrialBalanceAsync(int fiscalPeriodId, CancellationToken ct = default)
    {
        var lines = await _context.JournalLines
            .Where(l => l.JournalEntry!.FiscalPeriodId == fiscalPeriodId && l.JournalEntry!.Status == "POSTED")
            .Select(l => new { l.AccountId, l.Debit, l.Credit })
            .ToListAsync(ct);

        var accounts = await _context.Accounts.AsNoTracking()
            .ToDictionaryAsync(a => a.AccountId, ct);

        return lines
            .GroupBy(l => l.AccountId)
            .Select(g => new TrialBalanceRow
            {
                AccountId = g.Key,
                AccountCode = accounts.TryGetValue(g.Key, out var a) ? a.AccountCode : "?",
                AccountName = accounts.TryGetValue(g.Key, out var a2) ? a2.Name : "?",
                AccountType = accounts.TryGetValue(g.Key, out var a3) ? a3.AccountType : "?",
                Debit = g.Sum(x => x.Debit),
                Credit = g.Sum(x => x.Credit),
                Balance = g.Sum(x => x.Debit) - g.Sum(x => x.Credit),
            })
            .OrderBy(r => r.AccountCode)
            .ToList();
    }

    public async Task<List<GeneralLedgerRow>> GetGeneralLedgerAsync(int fiscalPeriodId, CancellationToken ct = default)
    {
        var rows = await _context.JournalLines
            .Where(l => l.JournalEntry!.FiscalPeriodId == fiscalPeriodId && l.JournalEntry!.Status == "POSTED")
            .Select(l => new GeneralLedgerRow
            {
                JournalLineId = l.JournalLineId,
                JournalEntryId = l.JournalEntryId,
                JournalNumber = l.JournalEntry!.JournalNumber,
                PostingDate = l.JournalEntry!.PostingDate,
                SourceType = l.JournalEntry!.SourceType,
                SourceId = l.JournalEntry!.SourceId,
                Description = l.Description ?? l.JournalEntry!.Description,
                AccountId = l.AccountId,
                AccountCode = l.Account != null ? l.Account.AccountCode : "?",
                AccountName = l.Account != null ? l.Account.Name : "?",
                Debit = l.Debit,
                Credit = l.Credit,
                CostCenterId = l.CostCenterId,
                DepartmentId = l.DepartmentId,
                ProjectId = l.ProjectId,
            })
            .OrderBy(r => r.PostingDate).ThenBy(r => r.JournalEntryId).ThenBy(r => r.JournalLineId)
            .ToListAsync(ct);

        return rows;
    }
}
