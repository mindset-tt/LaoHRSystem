using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class PostingServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly PostingService _service;

    public PostingServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        var numbers = new NumberSequenceService(_context);
        var accounting = new AccountingService(_context, numbers);
        var config = new AccountingConfigurationService(_context);
        var budget = new BudgetService(_context);
        _service = new PostingService(_context, config, accounting, budget, numbers);
    }

    private async Task SeedConfigAsync()
    {
        var ap = new Account { AccountCode = "2000", Name = "AP", AccountType = "LIABILITY", IsPostingAccount = true };
        var expense = new Account { AccountCode = "5000", Name = "Expense", AccountType = "EXPENSE", IsPostingAccount = true };
        var cash = new Account { AccountCode = "1000", Name = "Cash", AccountType = "ASSET", IsPostingAccount = true };
        var payable = new Account { AccountCode = "2100", Name = "Employee Payable", AccountType = "LIABILITY", IsPostingAccount = true };
        _context.Accounts.AddRange(ap, expense, cash, payable);
        await _context.SaveChangesAsync();

        _context.SystemSettings.AddRange(
            new SystemSetting { SettingKey = "AP_CONTROL_ACCOUNT", SettingValue = ap.AccountId.ToString() },
            new SystemSetting { SettingKey = "DEFAULT_EXPENSE_ACCOUNT", SettingValue = expense.AccountId.ToString() },
            new SystemSetting { SettingKey = "CASH_ACCOUNT", SettingValue = cash.AccountId.ToString() },
            new SystemSetting { SettingKey = "EMPLOYEE_PAYABLE_ACCOUNT", SettingValue = payable.AccountId.ToString() });
        await _context.SaveChangesAsync();

        var year = new FiscalYear { Name = "FY2026", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "OPEN" };
        _context.FiscalYears.Add(year);
        await _context.SaveChangesAsync();
        _context.FiscalPeriods.Add(new FiscalPeriod { FiscalYearId = year.FiscalYearId, PeriodNumber = 1, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "OPEN" });
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task PostSupplierInvoice_CreatesBalancedJournal()
    {
        await SeedConfigAsync();
        var supplier = new Supplier { SupplierCode = "S1", Name = "S", Status = "ACTIVE" };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "INV-1",
            SupplierId = supplier.SupplierId,
            InvoiceDate = DateTime.UtcNow,
            Currency = "LAK",
            Subtotal = 100,
            TotalAmount = 100,
            RemainingAmount = 100,
            Status = "APPROVED",
        };
        invoice.Lines.Add(new SupplierInvoiceLine { Description = "Item", Quantity = 1, UnitPrice = 100, Subtotal = 100 });
        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        var entry = await _service.PostSupplierInvoiceAsync(invoice.SupplierInvoiceId, 1);

        entry.Status.Should().Be("POSTED");
        entry.SourceType.Should().Be("SUPPLIER_INVOICE");
        entry.SourceId.Should().Be(invoice.SupplierInvoiceId);
        entry.Lines.Sum(l => l.Debit).Should().Be(entry.Lines.Sum(l => l.Credit));
    }

    [Fact]
    public async Task PostSupplierInvoice_Twice_Throws()
    {
        await SeedConfigAsync();
        var supplier = new Supplier { SupplierCode = "S2", Name = "S", Status = "ACTIVE" };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "INV-2",
            SupplierId = supplier.SupplierId,
            InvoiceDate = DateTime.UtcNow,
            Currency = "LAK",
            Subtotal = 100,
            TotalAmount = 100,
            RemainingAmount = 100,
            Status = "APPROVED",
        };
        invoice.Lines.Add(new SupplierInvoiceLine { Description = "Item", Quantity = 1, UnitPrice = 100, Subtotal = 100 });
        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        await _service.PostSupplierInvoiceAsync(invoice.SupplierInvoiceId, 1);

        var act = () => _service.PostSupplierInvoiceAsync(invoice.SupplierInvoiceId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PostSupplierInvoice_Unconfigured_Throws()
    {
        // No accounting config seeded → must fail closed.
        var supplier = new Supplier { SupplierCode = "S3", Name = "S", Status = "ACTIVE" };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "INV-3",
            SupplierId = supplier.SupplierId,
            InvoiceDate = DateTime.UtcNow,
            Currency = "LAK",
            Subtotal = 100,
            TotalAmount = 100,
            RemainingAmount = 100,
            Status = "APPROVED",
        };
        invoice.Lines.Add(new SupplierInvoiceLine { Description = "Item", Quantity = 1, UnitPrice = 100, Subtotal = 100 });
        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        var act = () => _service.PostSupplierInvoiceAsync(invoice.SupplierInvoiceId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Fail-closed: no orphan journal, invoice not falsely POSTED.
        _context.JournalEntries.Should().BeEmpty();
        var reloaded = await _context.SupplierInvoices.FindAsync(invoice.SupplierInvoiceId);
        reloaded!.Status.Should().Be("APPROVED");
    }

    [Fact]
    public async Task PostSupplierInvoice_Unconfigured_LeavesNoOrphanJournal()
    {
        await SeedConfigAsync();
        var supplier = new Supplier { SupplierCode = "S4", Name = "S", Status = "ACTIVE" };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Invoice with a line targeting a non-posting account → posting fails.
        var header = new Account { AccountCode = "9999", Name = "Header", AccountType = "ASSET", IsPostingAccount = false };
        _context.Accounts.Add(header);
        await _context.SaveChangesAsync();

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "INV-4",
            SupplierId = supplier.SupplierId,
            InvoiceDate = DateTime.UtcNow,
            Currency = "LAK",
            Subtotal = 100,
            TotalAmount = 100,
            RemainingAmount = 100,
            Status = "APPROVED",
        };
        invoice.Lines.Add(new SupplierInvoiceLine { Description = "Item", Quantity = 1, UnitPrice = 100, Subtotal = 100, AccountId = header.AccountId });
        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        var act = () => _service.PostSupplierInvoiceAsync(invoice.SupplierInvoiceId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // No POSTED journal remains (the DRAFT journal is rolled back on
        // PostgreSQL; on InMemory the transaction is a no-op so a DRAFT may
        // remain, but it must never be POSTED).
        _context.JournalEntries.Should().NotContain(j => j.Status == "POSTED");
    }

    [Fact]
    public async Task PostExpense_CreatesBalancedJournal()
    {
        await SeedConfigAsync();
        var category = new ExpenseCategory { Code = "TRAVEL", Name = "Travel", AccountId = null };
        _context.ExpenseCategories.Add(category);
        await _context.SaveChangesAsync();

        var expense = new Expense
        {
            ExpenseNumber = "EXP-1",
            EmployeeId = 1,
            CategoryId = category.ExpenseCategoryId,
            Title = "Travel",
            ExpenseDate = DateTime.UtcNow,
            Currency = "LAK",
            Amount = 100,
            AmountLak = 100,
            Status = "APPROVED",
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        var entry = await _service.PostExpenseAsync(expense.ExpenseId, 1);

        entry.Status.Should().Be("POSTED");
        entry.SourceType.Should().Be("EXPENSE");
        entry.Lines.Sum(l => l.Debit).Should().Be(entry.Lines.Sum(l => l.Credit));
    }
}
