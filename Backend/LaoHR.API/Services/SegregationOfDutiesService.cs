using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4B.1 — segregation of duties. Lightweight, configurable policy (NOT a
/// generic rules engine). Server-side enforcement: frontend hiding is not
/// sufficient.
///
/// Policies (configurable via SystemSettings, conservative PRODUCT defaults):
///   PreventInvoiceSelfApproval  (default true)
///   PreventPaymentSelfApproval  (default true)
///   PreventJournalSelfPost       (default true)
///
/// These are PRODUCT defaults, NOT Lao legal requirements.
/// </summary>
public interface ISegregationOfDutiesService
{
    /// <summary>Throws if the invoice creator is the same as the approver (when policy enabled).</summary>
    Task EnforceInvoiceApprovalAsync(int invoiceId, int approverEmployeeId, CancellationToken ct = default);

    /// <summary>Throws if the payment creator is the same as the approver (when policy enabled).</summary>
    Task EnforcePaymentApprovalAsync(int paymentId, int approverEmployeeId, CancellationToken ct = default);

    /// <summary>Throws if the journal creator is the same as the poster (when policy enabled).</summary>
    Task EnforceJournalPostAsync(int journalEntryId, int posterEmployeeId, CancellationToken ct = default);
}

public sealed class SegregationOfDutiesService : ISegregationOfDutiesService
{
    private readonly LaoHRDbContext _context;

    public SegregationOfDutiesService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task EnforceInvoiceApprovalAsync(int invoiceId, int approverEmployeeId, CancellationToken ct = default)
    {
        if (!await IsEnabledAsync("SOD_PREVENT_INVOICE_SELF_APPROVAL", ct)) return;

        var invoice = await _context.SupplierInvoices
            .FirstOrDefaultAsync(i => i.SupplierInvoiceId == invoiceId, ct);
        if (invoice?.CreatedByEmployeeId == approverEmployeeId)
            throw new InvalidOperationException(
                "Segregation of duties: the invoice creator cannot approve their own invoice.");
    }

    public async Task EnforcePaymentApprovalAsync(int paymentId, int approverEmployeeId, CancellationToken ct = default)
    {
        if (!await IsEnabledAsync("SOD_PREVENT_PAYMENT_SELF_APPROVAL", ct)) return;

        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct);
        if (payment?.CreatedByEmployeeId == approverEmployeeId)
            throw new InvalidOperationException(
                "Segregation of duties: the payment creator cannot approve their own payment.");
    }

    public async Task EnforceJournalPostAsync(int journalEntryId, int posterEmployeeId, CancellationToken ct = default)
    {
        if (!await IsEnabledAsync("SOD_PREVENT_JOURNAL_SELF_POST", ct)) return;

        var journal = await _context.JournalEntries
            .FirstOrDefaultAsync(j => j.JournalEntryId == journalEntryId, ct);
        if (journal?.CreatedByEmployeeId == posterEmployeeId)
            throw new InvalidOperationException(
                "Segregation of duties: the journal creator cannot post their own journal.");
    }

    private async Task<bool> IsEnabledAsync(string key, CancellationToken ct)
    {
        var setting = await _context.SystemSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key, ct);
        // Conservative default: enabled when the setting is absent.
        if (setting == null) return true;
        return !string.Equals(setting.SettingValue, "false", StringComparison.OrdinalIgnoreCase);
    }
}
