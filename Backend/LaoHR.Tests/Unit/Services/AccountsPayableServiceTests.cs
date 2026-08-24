using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class AccountsPayableServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly AccountsPayableService _service;

    public AccountsPayableServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new AccountsPayableService(_context);
    }

    private async Task<int> SeedInvoiceAsync(decimal total, string status = "APPROVED")
    {
        var supplier = new Supplier { SupplierCode = "SUP-AP", Name = "AP Supplier", Status = "ACTIVE" };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = $"INV-{Guid.NewGuid():N}",
            SupplierId = supplier.SupplierId,
            InvoiceDate = DateTime.UtcNow,
            Currency = "LAK",
            Subtotal = total,
            TotalAmount = total,
            RemainingAmount = total,
            Status = status,
        };
        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice.SupplierInvoiceId;
    }

    private async Task<Payment> CreatePaymentAsync(int invoiceId, decimal amount)
    {
        var payment = new Payment
        {
            PaymentNumber = $"PAY-{Guid.NewGuid():N}",
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = "BANK_TRANSFER",
            Currency = "LAK",
            Amount = amount,
            Status = "DRAFT",
        };
        payment.Allocations.Add(new PaymentAllocation { SupplierInvoiceId = invoiceId, Amount = amount });
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    [Fact]
    public async Task PostPayment_FullPayment_MarksPaid()
    {
        var invoiceId = await SeedInvoiceAsync(100);
        var payment = await CreatePaymentAsync(invoiceId, 100);

        await _service.PostPaymentAsync(payment.PaymentId, 1);

        var invoice = await _context.SupplierInvoices.FindAsync(invoiceId);
        invoice!.Status.Should().Be("PAID");
        invoice.RemainingAmount.Should().Be(0);
    }

    [Fact]
    public async Task PostPayment_PartialPayment_MarksPartiallyPaid()
    {
        var invoiceId = await SeedInvoiceAsync(100);
        var payment = await CreatePaymentAsync(invoiceId, 40);

        await _service.PostPaymentAsync(payment.PaymentId, 1);

        var invoice = await _context.SupplierInvoices.FindAsync(invoiceId);
        invoice!.Status.Should().Be("PARTIALLY_PAID");
        invoice.RemainingAmount.Should().Be(60);
    }

    [Fact]
    public async Task PostPayment_Overpayment_Throws()
    {
        var invoiceId = await SeedInvoiceAsync(100);
        var payment = await CreatePaymentAsync(invoiceId, 150);

        var act = () => _service.PostPaymentAsync(payment.PaymentId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PostPayment_UnapprovedInvoice_Throws()
    {
        var invoiceId = await SeedInvoiceAsync(100, "DRAFT");
        var payment = await CreatePaymentAsync(invoiceId, 100);

        var act = () => _service.PostPaymentAsync(payment.PaymentId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task PostPayment_Twice_ThrowsOnSecond()
    {
        var invoiceId = await SeedInvoiceAsync(100);
        var payment = await CreatePaymentAsync(invoiceId, 100);

        await _service.PostPaymentAsync(payment.PaymentId, 1);

        // Second post must be rejected (already POSTED).
        var act = () => _service.PostPaymentAsync(payment.PaymentId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Match_NonPoInvoice_ReturnsNotApplicable()
    {
        var invoiceId = await SeedInvoiceAsync(100);
        var result = await _service.MatchAsync(invoiceId);
        result.Should().Be("NOT_APPLICABLE");
    }

    [Fact]
    public async Task PostPayment_Overpayment_LeavesInvoiceUnchanged()
    {
        // Atomicity: a rejected overpayment must not partially mutate the invoice.
        var invoiceId = await SeedInvoiceAsync(100);
        var payment = await CreatePaymentAsync(invoiceId, 150);

        var act = () => _service.PostPaymentAsync(payment.PaymentId, 1);
        await act.Should().ThrowAsync<InvalidOperationException>();

        var invoice = await _context.SupplierInvoices.FindAsync(invoiceId);
        invoice!.PaidAmount.Should().Be(0);
        invoice.RemainingAmount.Should().Be(100);
        invoice.Status.Should().Be("APPROVED");
    }
}
