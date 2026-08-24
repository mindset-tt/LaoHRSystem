using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class SegregationOfDutiesServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly SegregationOfDutiesService _service;

    public SegregationOfDutiesServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new SegregationOfDutiesService(_context);
    }

    [Fact]
    public async Task EnforceInvoiceApproval_SameCreator_Throws()
    {
        var supplier = new Supplier { SupplierCode = "S1", Name = "S", Status = "ACTIVE" };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "INV-1",
            SupplierId = supplier.SupplierId,
            InvoiceDate = DateTime.UtcNow,
            Currency = "LAK",
            TotalAmount = 100,
            RemainingAmount = 100,
            Status = "PENDING_APPROVAL",
            CreatedByEmployeeId = 5,
        };
        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        var act = () => _service.EnforceInvoiceApprovalAsync(invoice.SupplierInvoiceId, 5);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task EnforceInvoiceApproval_DifferentApprover_Succeeds()
    {
        var supplier = new Supplier { SupplierCode = "S2", Name = "S", Status = "ACTIVE" };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "INV-2",
            SupplierId = supplier.SupplierId,
            InvoiceDate = DateTime.UtcNow,
            Currency = "LAK",
            TotalAmount = 100,
            RemainingAmount = 100,
            Status = "PENDING_APPROVAL",
            CreatedByEmployeeId = 5,
        };
        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        await _service.EnforceInvoiceApprovalAsync(invoice.SupplierInvoiceId, 6);
    }

    [Fact]
    public async Task EnforcePaymentApproval_SameCreator_Throws()
    {
        var payment = new Payment
        {
            PaymentNumber = "PAY-1",
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "BANK_TRANSFER",
            Currency = "LAK",
            Amount = 100,
            Status = "DRAFT",
            CreatedByEmployeeId = 7,
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var act = () => _service.EnforcePaymentApprovalAsync(payment.PaymentId, 7);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task EnforceJournalPost_SameCreator_Throws()
    {
        var year = new FiscalYear { Name = "FY", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "OPEN" };
        _context.FiscalYears.Add(year);
        await _context.SaveChangesAsync();
        var period = new FiscalPeriod { FiscalYearId = year.FiscalYearId, PeriodNumber = 1, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31), Status = "OPEN" };
        _context.FiscalPeriods.Add(period);
        await _context.SaveChangesAsync();

        var journal = new JournalEntry
        {
            JournalNumber = "JE-1",
            PostingDate = DateTime.UtcNow,
            FiscalPeriodId = period.FiscalPeriodId,
            SourceType = "MANUAL_JOURNAL",
            Status = "DRAFT",
            Currency = "LAK",
            CreatedByEmployeeId = 9,
        };
        _context.JournalEntries.Add(journal);
        await _context.SaveChangesAsync();

        var act = () => _service.EnforceJournalPostAsync(journal.JournalEntryId, 9);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ---- Disabled policy → operation permitted (if otherwise authorized) ----

    [Fact]
    public async Task EnforceInvoiceApproval_WhenPolicyDisabled_SameCreatorPermitted()
    {
        _context.SystemSettings.Add(new SystemSetting
        {
            SettingKey = "SOD_PREVENT_INVOICE_SELF_APPROVAL",
            SettingValue = "false",
        });
        await _context.SaveChangesAsync();

        var supplier = new Supplier { SupplierCode = "S3", Name = "S", Status = "ACTIVE" };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = "INV-3",
            SupplierId = supplier.SupplierId,
            InvoiceDate = DateTime.UtcNow,
            Currency = "LAK",
            TotalAmount = 100,
            RemainingAmount = 100,
            Status = "PENDING_APPROVAL",
            CreatedByEmployeeId = 5,
        };
        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        // Same creator, but policy disabled → no throw.
        await _service.EnforceInvoiceApprovalAsync(invoice.SupplierInvoiceId, 5);
    }

    [Fact]
    public async Task EnforcePaymentApproval_WhenPolicyDisabled_SameCreatorPermitted()
    {
        _context.SystemSettings.Add(new SystemSetting
        {
            SettingKey = "SOD_PREVENT_PAYMENT_SELF_APPROVAL",
            SettingValue = "false",
        });
        await _context.SaveChangesAsync();

        var payment = new Payment
        {
            PaymentNumber = "PAY-2",
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "BANK_TRANSFER",
            Currency = "LAK",
            Amount = 100,
            Status = "DRAFT",
            CreatedByEmployeeId = 7,
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        await _service.EnforcePaymentApprovalAsync(payment.PaymentId, 7);
    }

    [Fact]
    public async Task EnforceJournalPost_WhenPolicyDisabled_SameCreatorPermitted()
    {
        _context.SystemSettings.Add(new SystemSetting
        {
            SettingKey = "SOD_PREVENT_JOURNAL_SELF_POST",
            SettingValue = "false",
        });
        await _context.SaveChangesAsync();

        var year = new FiscalYear { Name = "FY", StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31), Status = "OPEN" };
        _context.FiscalYears.Add(year);
        await _context.SaveChangesAsync();
        var period = new FiscalPeriod { FiscalYearId = year.FiscalYearId, PeriodNumber = 1, StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 1, 31), Status = "OPEN" };
        _context.FiscalPeriods.Add(period);
        await _context.SaveChangesAsync();

        var journal = new JournalEntry
        {
            JournalNumber = "JE-2",
            PostingDate = DateTime.UtcNow,
            FiscalPeriodId = period.FiscalPeriodId,
            SourceType = "MANUAL_JOURNAL",
            Status = "DRAFT",
            Currency = "LAK",
            CreatedByEmployeeId = 9,
        };
        _context.JournalEntries.Add(journal);
        await _context.SaveChangesAsync();

        await _service.EnforceJournalPostAsync(journal.JournalEntryId, 9);
    }
}
