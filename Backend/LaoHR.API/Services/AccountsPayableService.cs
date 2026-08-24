using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4B — Accounts Payable service. Handles 3-way match (PO ↔ Goods Receipt
/// ↔ Supplier Invoice) and payment allocation with concurrency safety.
///
/// Invariants:
///   - Cannot pay a DRAFT/PENDING/MATCH_EXCEPTION invoice.
///   - Cannot overpay an invoice (no credit/prepayment model yet).
///   - Payment allocation is idempotent (double-posting prevented).
///   - AP balance derives from approved invoices and payments (no mutable summary).
/// </summary>
public interface IAccountsPayableService
{
    /// <summary>Runs 3-way match for a PO-backed invoice; returns the match status.</summary>
    Task<string> MatchAsync(int supplierInvoiceId, CancellationToken ct = default);

    /// <summary>Posts a payment against one or more invoices (transactional, concurrency-safe).</summary>
    Task<Payment> PostPaymentAsync(
        int paymentId, int postedByEmployeeId, CancellationToken ct = default);

    /// <summary>Returns AP aging buckets for approved/partially-paid invoices.</summary>
    Task<ApAgingDto> GetAgingAsync(CancellationToken ct = default);
}

public sealed class ApAgingDto
{
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90 { get; set; }
    public decimal Total { get; set; }
}

public sealed class AccountsPayableService : IAccountsPayableService
{
    private readonly LaoHRDbContext _context;

    public AccountsPayableService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<string> MatchAsync(int supplierInvoiceId, CancellationToken ct = default)
    {
        var invoice = await _context.SupplierInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.SupplierInvoiceId == supplierInvoiceId, ct)
            ?? throw new InvalidOperationException("Supplier invoice not found.");

        if (invoice.PurchaseOrderId == null)
            return "NOT_APPLICABLE";

        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == invoice.PurchaseOrderId, ct)
            ?? throw new InvalidOperationException("Purchase order not found.");

        var quantityVariance = false;
        var priceVariance = false;

        foreach (var line in invoice.Lines)
        {
            if (line.PurchaseOrderItemId == null) continue;
            var poLine = po.Items.FirstOrDefault(l => l.PurchaseOrderItemId == line.PurchaseOrderItemId);
            if (poLine == null) continue;

            // Received quantity for this PO line.
            var received = await _context.GoodsReceiptItems
                .Where(gi => gi.PurchaseOrderItemId == poLine.PurchaseOrderItemId
                             && gi.GoodsReceipt!.Status == "POSTED")
                .SumAsync(gi => (decimal?)gi.AcceptedQuantity, ct) ?? 0m;

            if (line.Quantity != received) quantityVariance = true;
            if (line.UnitPrice != poLine.UnitPrice) priceVariance = true;
        }

        if (quantityVariance && priceVariance) return "QUANTITY_AND_PRICE_VARIANCE";
        if (quantityVariance) return "QUANTITY_VARIANCE";
        if (priceVariance) return "PRICE_VARIANCE";
        return "MATCHED";
    }

    public async Task<Payment> PostPaymentAsync(int paymentId, int postedByEmployeeId, CancellationToken ct = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Allocations)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct)
            ?? throw new InvalidOperationException("Payment not found.");

        if (payment.Status != "DRAFT" && payment.Status != "APPROVED")
            throw new InvalidOperationException($"Cannot post a payment in '{payment.Status}' state.");

        if (payment.Allocations.Count == 0)
            throw new InvalidOperationException("Payment has no allocations.");

        var relational = _context.Database.IsRelational();
        await using var tx = relational ? await _context.Database.BeginTransactionAsync(ct) : null;

        try
        {
            foreach (var alloc in payment.Allocations)
            {
                // Lock the invoice row (FOR UPDATE on PostgreSQL) so concurrent
                // payments serialize on the same invoice and cannot both read the
                // same RemainingAmount (prevents overpayment race).
                var invoice = await LockInvoiceAsync(alloc.SupplierInvoiceId, ct)
                    ?? throw new InvalidOperationException($"Invoice {alloc.SupplierInvoiceId} not found.");

                if (invoice.Status is "DRAFT" or "PENDING_MATCH" or "MATCH_EXCEPTION" or "PENDING_APPROVAL")
                    throw new InvalidOperationException($"Cannot pay invoice '{invoice.InvoiceNumber}' in '{invoice.Status}' state.");

                if (alloc.Amount > invoice.RemainingAmount)
                    throw new InvalidOperationException(
                        $"Overpayment: invoice '{invoice.InvoiceNumber}' remaining {invoice.RemainingAmount}, allocation {alloc.Amount}.");

                invoice.PaidAmount += alloc.Amount;
                invoice.RemainingAmount -= alloc.Amount;
                invoice.Status = invoice.RemainingAmount <= 0 ? "PAID" : "PARTIALLY_PAID";
                invoice.UpdatedAt = DateTime.UtcNow;
            }

            payment.Status = "POSTED";
            await _context.SaveChangesAsync(ct);
            if (tx != null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }

        return payment;
    }

    /// <summary>Locks the invoice row (FOR UPDATE on PostgreSQL) for the transaction.</summary>
    private async Task<SupplierInvoice?> LockInvoiceAsync(int supplierInvoiceId, CancellationToken ct)
    {
        if (_context.Database.IsRelational())
        {
            return await _context.SupplierInvoices
                .FromSqlRaw("SELECT * FROM \"SupplierInvoices\" WHERE \"SupplierInvoiceId\" = {0} FOR UPDATE", supplierInvoiceId)
                .FirstOrDefaultAsync(ct);
        }
        return await _context.SupplierInvoices
            .FirstOrDefaultAsync(i => i.SupplierInvoiceId == supplierInvoiceId, ct);
    }

    public async Task<ApAgingDto> GetAgingAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var invoices = await _context.SupplierInvoices
            .Where(i => i.Status == "APPROVED" || i.Status == "PARTIALLY_PAID")
            .Select(i => new { i.RemainingAmount, i.DueDate })
            .ToListAsync(ct);

        var dto = new ApAgingDto();
        foreach (var inv in invoices)
        {
            var due = inv.DueDate?.Date ?? today;
            var days = (due - today).Days;
            if (days <= 0) dto.Current += inv.RemainingAmount;
            else if (days <= 30) dto.Days1To30 += inv.RemainingAmount;
            else if (days <= 60) dto.Days31To60 += inv.RemainingAmount;
            else if (days <= 90) dto.Days61To90 += inv.RemainingAmount;
            else dto.Over90 += inv.RemainingAmount;
        }
        dto.Total = dto.Current + dto.Days1To30 + dto.Days31To60 + dto.Days61To90 + dto.Over90;
        return dto;
    }
}
