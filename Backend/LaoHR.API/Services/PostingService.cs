using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4B.1 — posting service. Connects operational events to the accounting
/// kernel via controlled, idempotent, atomic auto-posting.
///
/// Each posting:
///   - validates the source state
///   - validates accounting configuration (fails closed if mappings missing)
///   - creates a balanced journal (SourceType + SourceId + PostingPurpose)
///   - posts it atomically
///   - updates the source accounting state
///   - updates budget actual (where applicable)
///
/// Idempotency: a source can only be auto-posted once (unique SourceType+SourceId+PostingPurpose).
/// </summary>
public interface IPostingService
{
    /// <summary>Posts an APPROVED supplier invoice to the GL (Debit expense, Credit AP).</summary>
    Task<JournalEntry> PostSupplierInvoiceAsync(int supplierInvoiceId, int postedByEmployeeId, CancellationToken ct = default);

    /// <summary>Posts a POSTED payment to the GL (Debit AP, Credit bank/cash).</summary>
    Task<JournalEntry> PostPaymentAsync(int paymentId, int postedByEmployeeId, CancellationToken ct = default);

    /// <summary>Posts an APPROVED expense to the GL (Debit expense, Credit employee payable).</summary>
    Task<JournalEntry> PostExpenseAsync(int expenseId, int postedByEmployeeId, CancellationToken ct = default);
}

public sealed class PostingService : IPostingService
{
    private readonly LaoHRDbContext _context;
    private readonly IAccountingConfigurationService _config;
    private readonly IAccountingService _accounting;
    private readonly IBudgetService _budget;
    private readonly INumberSequenceService _numbers;

    public PostingService(
        LaoHRDbContext context,
        IAccountingConfigurationService config,
        IAccountingService accounting,
        IBudgetService budget,
        INumberSequenceService numbers)
    {
        _context = context;
        _config = config;
        _accounting = accounting;
        _budget = budget;
        _numbers = numbers;
    }

    public async Task<JournalEntry> PostSupplierInvoiceAsync(int supplierInvoiceId, int postedByEmployeeId, CancellationToken ct = default)
    {
        var invoice = await _context.SupplierInvoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.SupplierInvoiceId == supplierInvoiceId, ct)
            ?? throw new InvalidOperationException("Supplier invoice not found.");

        if (invoice.Status != "APPROVED")
            throw new InvalidOperationException($"Cannot post an invoice in '{invoice.Status}' state.");

        // Idempotency: already posted?
        if (await HasSourceJournalAsync("SUPPLIER_INVOICE", supplierInvoiceId, "INVOICE", ct))
            throw new InvalidOperationException("Invoice already posted.");

        var apAccount = await _config.GetApControlAccountAsync(ct);
        var periodId = await _config.GetOpenPeriodAsync(invoice.InvoiceDate, ct);

        var relational = _context.Database.IsRelational();
        await using var tx = relational ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var entry = new JournalEntry
            {
                JournalNumber = await _numbers.NextAsync("JE"),
                PostingDate = invoice.InvoiceDate,
                FiscalPeriodId = periodId,
                SourceType = "SUPPLIER_INVOICE",
                SourceId = supplierInvoiceId,
                PostingPurpose = "INVOICE",
                Description = $"Supplier invoice {invoice.InvoiceNumber}",
                Status = "DRAFT",
                Currency = invoice.Currency,
                CreatedByEmployeeId = postedByEmployeeId,
            };

            // Debit expense/account lines, Credit AP control.
            foreach (var line in invoice.Lines)
            {
                var expenseAccount = line.AccountId ?? await _config.GetDefaultExpenseAccountAsync(ct);
                entry.Lines.Add(new JournalLine
                {
                    AccountId = expenseAccount,
                    Description = line.Description,
                    Debit = line.Subtotal + line.TaxAmount,
                    Credit = 0,
                    CostCenterId = line.CostCenterId,
                    ProjectId = line.ProjectId,
                });
            }
            entry.Lines.Add(new JournalLine
            {
                AccountId = apAccount,
                Description = $"AP for {invoice.InvoiceNumber}",
                Debit = 0,
                Credit = invoice.TotalAmount,
            });

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync(ct);

            await _accounting.PostAsync(entry.JournalEntryId, postedByEmployeeId, ct);

            // Budget actual reconciliation (if invoice links to a budget via PO/PR).
            await ReconcileBudgetForInvoiceAsync(invoice, ct);

            if (tx != null) await tx.CommitAsync(ct);
            return entry;
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<JournalEntry> PostPaymentAsync(int paymentId, int postedByEmployeeId, CancellationToken ct = default)
    {
        var payment = await _context.Payments
            .Include(p => p.Allocations)
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId, ct)
            ?? throw new InvalidOperationException("Payment not found.");

        if (payment.Status != "POSTED")
            throw new InvalidOperationException($"Cannot post a payment in '{payment.Status}' state.");

        if (await HasSourceJournalAsync("PAYMENT", paymentId, "PAYMENT", ct))
            throw new InvalidOperationException("Payment already posted.");

        var apAccount = await _config.GetApControlAccountAsync(ct);
        var cashAccount = payment.BankAccountId.HasValue
            ? await GetBankGlAccountAsync(payment.BankAccountId.Value, ct)
            : await _config.GetCashAccountAsync(ct);
        var periodId = await _config.GetOpenPeriodAsync(payment.PaymentDate, ct);

        var relational = _context.Database.IsRelational();
        await using var tx = relational ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var entry = new JournalEntry
            {
                JournalNumber = await _numbers.NextAsync("JE"),
                PostingDate = payment.PaymentDate,
                FiscalPeriodId = periodId,
                SourceType = "PAYMENT",
                SourceId = paymentId,
                PostingPurpose = "PAYMENT",
                Description = $"Payment {payment.PaymentNumber}",
                Status = "DRAFT",
                Currency = payment.Currency,
                CreatedByEmployeeId = postedByEmployeeId,
            };

            // Debit AP, Credit bank/cash.
            entry.Lines.Add(new JournalLine { AccountId = apAccount, Debit = payment.Amount, Credit = 0 });
            entry.Lines.Add(new JournalLine { AccountId = cashAccount, Debit = 0, Credit = payment.Amount });

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync(ct);

            await _accounting.PostAsync(entry.JournalEntryId, postedByEmployeeId, ct);

            if (tx != null) await tx.CommitAsync(ct);
            return entry;
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<JournalEntry> PostExpenseAsync(int expenseId, int postedByEmployeeId, CancellationToken ct = default)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId, ct)
            ?? throw new InvalidOperationException("Expense not found.");

        if (expense.Status != "APPROVED")
            throw new InvalidOperationException($"Cannot post an expense in '{expense.Status}' state.");

        if (await HasSourceJournalAsync("EXPENSE", expenseId, "EXPENSE", ct))
            throw new InvalidOperationException("Expense already posted.");

        var expenseAccount = await GetExpenseCategoryAccountAsync(expense.CategoryId, ct);
        var payableAccount = await _config.GetEmployeePayableAccountAsync(ct);
        var periodId = await _config.GetOpenPeriodAsync(expense.ExpenseDate, ct);

        var relational = _context.Database.IsRelational();
        await using var tx = relational ? await _context.Database.BeginTransactionAsync(ct) : null;
        try
        {
            var entry = new JournalEntry
            {
                JournalNumber = await _numbers.NextAsync("JE"),
                PostingDate = expense.ExpenseDate,
                FiscalPeriodId = periodId,
                SourceType = "EXPENSE",
                SourceId = expenseId,
                PostingPurpose = "EXPENSE",
                Description = $"Expense {expense.ExpenseNumber}",
                Status = "DRAFT",
                Currency = expense.Currency,
                CreatedByEmployeeId = postedByEmployeeId,
            };

            entry.Lines.Add(new JournalLine { AccountId = expenseAccount, Debit = expense.AmountLak, Credit = 0 });
            entry.Lines.Add(new JournalLine { AccountId = payableAccount, Debit = 0, Credit = expense.AmountLak });

            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync(ct);

            await _accounting.PostAsync(entry.JournalEntryId, postedByEmployeeId, ct);

            if (tx != null) await tx.CommitAsync(ct);
            return entry;
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<bool> HasSourceJournalAsync(string sourceType, int sourceId, string purpose, CancellationToken ct)
    {
        return await _context.JournalEntries
            .AnyAsync(j => j.SourceType == sourceType && j.SourceId == sourceId && j.PostingPurpose == purpose, ct);
    }

    private async Task<int> GetBankGlAccountAsync(int bankAccountId, CancellationToken ct)
    {
        var bank = await _context.BankAccounts
            .FirstOrDefaultAsync(b => b.BankAccountId == bankAccountId, ct)
            ?? throw new InvalidOperationException("Bank account not found.");
        if (bank.GLAccountId == null)
            throw new InvalidOperationException(
                $"Bank account '{bank.AccountName}' has no GL account mapping. Configure it before posting payments.");
        return bank.GLAccountId.Value;
    }

    private async Task<int> GetExpenseCategoryAccountAsync(int categoryId, CancellationToken ct)
    {
        var category = await _context.ExpenseCategories
            .FirstOrDefaultAsync(c => c.ExpenseCategoryId == categoryId, ct)
            ?? throw new InvalidOperationException("Expense category not found.");
        return category.AccountId ?? await _config.GetDefaultExpenseAccountAsync(ct);
    }

    private async Task ReconcileBudgetForInvoiceAsync(SupplierInvoice invoice, CancellationToken ct)
    {
        // If the invoice is PO-backed and the PO links to a budget, recognize actual.
        if (invoice.PurchaseOrderId == null) return;

        var po = await _context.PurchaseOrders
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == invoice.PurchaseOrderId, ct);
        if (po?.BudgetId == null) return;

        await _budget.RecognizeActualAsync(po.BudgetId.Value, invoice.TotalAmount, ct);
    }
}
