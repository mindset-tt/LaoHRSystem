using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class AccountingServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly AccountingService _service;

    public AccountingServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        var numbers = new NumberSequenceService(_context);
        _service = new AccountingService(_context, numbers);
    }

    private async Task<(int cashAccount, int expenseAccount, int periodId)> SeedAsync()
    {
        var cash = new Account { AccountCode = "1000", Name = "Cash", AccountType = "ASSET", IsPostingAccount = true };
        var expense = new Account { AccountCode = "5000", Name = "Expense", AccountType = "EXPENSE", IsPostingAccount = true };
        _context.Accounts.AddRange(cash, expense);
        await _context.SaveChangesAsync();

        var year = new FiscalYear { Name = "FY2026", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "OPEN" };
        _context.FiscalYears.Add(year);
        await _context.SaveChangesAsync();

        var period = new FiscalPeriod { FiscalYearId = year.FiscalYearId, PeriodNumber = 1, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31), Status = "OPEN" };
        _context.FiscalPeriods.Add(period);
        await _context.SaveChangesAsync();

        return (cash.AccountId, expense.AccountId, period.FiscalPeriodId);
    }

    private async Task<JournalEntry> CreateDraftAsync(int cash, int expense, int period, decimal debit, decimal credit)
    {
        var entry = new JournalEntry
        {
            JournalNumber = $"JE-{Guid.NewGuid():N}",
            PostingDate = DateTime.UtcNow,
            FiscalPeriodId = period,
            SourceType = "MANUAL_JOURNAL",
            Status = "DRAFT",
            Currency = "LAK",
        };
        entry.Lines.Add(new JournalLine { AccountId = expense, Debit = debit, Credit = 0 });
        entry.Lines.Add(new JournalLine { AccountId = cash, Debit = 0, Credit = credit });
        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();
        return entry;
    }

    [Fact]
    public async Task Post_BalancedJournal_Succeeds()
    {
        var (cash, expense, period) = await SeedAsync();
        var entry = await CreateDraftAsync(cash, expense, period, 100, 100);

        var posted = await _service.PostAsync(entry.JournalEntryId, 1);

        posted.Status.Should().Be("POSTED");
    }

    [Fact]
    public async Task Post_UnbalancedJournal_Throws()
    {
        var (cash, expense, period) = await SeedAsync();
        var entry = await CreateDraftAsync(cash, expense, period, 100, 50);

        var act = () => _service.PostAsync(entry.JournalEntryId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Post_LineWithBothDebitAndCredit_Throws()
    {
        var (cash, expense, period) = await SeedAsync();
        var entry = new JournalEntry
        {
            JournalNumber = $"JE-{Guid.NewGuid():N}",
            PostingDate = DateTime.UtcNow,
            FiscalPeriodId = period,
            SourceType = "MANUAL_JOURNAL",
            Status = "DRAFT",
            Currency = "LAK",
        };
        entry.Lines.Add(new JournalLine { AccountId = expense, Debit = 100, Credit = 100 });
        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();

        var act = () => _service.PostAsync(entry.JournalEntryId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Post_ToClosedPeriod_Throws()
    {
        var (cash, expense, period) = await SeedAsync();
        var periodEntity = await _context.FiscalPeriods.FindAsync(period);
        periodEntity!.Status = "CLOSED";
        await _context.SaveChangesAsync();

        var entry = await CreateDraftAsync(cash, expense, period, 100, 100);

        var act = () => _service.PostAsync(entry.JournalEntryId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Post_ToNonPostingAccount_Throws()
    {
        var (_, _, period) = await SeedAsync();
        var header = new Account { AccountCode = "100", Name = "Header", AccountType = "ASSET", IsPostingAccount = false };
        var expense = new Account { AccountCode = "5001", Name = "Expense2", AccountType = "EXPENSE", IsPostingAccount = true };
        _context.Accounts.AddRange(header, expense);
        await _context.SaveChangesAsync();

        var entry = new JournalEntry
        {
            JournalNumber = $"JE-{Guid.NewGuid():N}",
            PostingDate = DateTime.UtcNow,
            FiscalPeriodId = period,
            SourceType = "MANUAL_JOURNAL",
            Status = "DRAFT",
            Currency = "LAK",
        };
        entry.Lines.Add(new JournalLine { AccountId = header.AccountId, Debit = 100, Credit = 0 });
        entry.Lines.Add(new JournalLine { AccountId = expense.AccountId, Debit = 0, Credit = 100 });
        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();

        var act = () => _service.PostAsync(entry.JournalEntryId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Reverse_PostedJournal_CreatesInvertedEntry()
    {
        var (cash, expense, period) = await SeedAsync();
        var entry = await CreateDraftAsync(cash, expense, period, 100, 100);
        await _service.PostAsync(entry.JournalEntryId, 1);

        var reversal = await _service.ReverseAsync(entry.JournalEntryId, 1);

        reversal.Status.Should().Be("POSTED");
        reversal.ReversesJournalEntryId.Should().Be(entry.JournalEntryId);
        // Inverted: original debited expense 100, credited cash 100.
        reversal.Lines.Should().Contain(l => l.AccountId == expense && l.Credit == 100);
        reversal.Lines.Should().Contain(l => l.AccountId == cash && l.Debit == 100);
    }

    [Fact]
    public async Task GetAccountBalance_DerivesFromPostedLines()
    {
        var (cash, expense, period) = await SeedAsync();
        var entry = await CreateDraftAsync(cash, expense, period, 100, 100);
        await _service.PostAsync(entry.JournalEntryId, 1);

        // Expense account: debit 100 → balance +100.
        (await _service.GetAccountBalanceAsync(expense)).Should().Be(100);
        // Cash account: credit 100 → balance -100.
        (await _service.GetAccountBalanceAsync(cash)).Should().Be(-100);
    }
}
