using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 4B.2 — Finance report endpoints + CSV/Excel export.
///
/// Every endpoint enforces finance authorization server-side via
/// <see cref="IFinanceAccessService"/>. Sensitive bulk exports (General Ledger,
/// Trial Balance, AP Aging) also write a persisted audit record.
/// </summary>
[Authorize]
[ApiController]
[Route("api/finance/reports")]
public class FinanceReportsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;
    private readonly IAccountingService _accounting;
    private readonly IAccountsPayableService _ap;
    private readonly IFinanceExportService _export;

    public FinanceReportsController(
        LaoHRDbContext context,
        IFinanceAccessService access,
        IAccountingService accounting,
        IAccountsPayableService ap,
        IFinanceExportService export)
    {
        _context = context;
        _access = access;
        _accounting = accounting;
        _ap = ap;
        _export = export;
    }

    // ---- Supplier Invoice Register ----

    [HttpGet("supplier-invoices")]
    public async Task<ActionResult<List<SupplierInvoiceRegisterRow>>> SupplierInvoiceRegister(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] string? status = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] int? costCenterId = null,
        [FromQuery] int? projectId = null)
    {
        if (!_access.CanViewFinanceReports())
            return Forbid();

        var query = _context.SupplierInvoices.AsNoTracking().AsQueryable();
        if (from.HasValue) query = query.Where(i => i.InvoiceDate >= from.Value);
        if (to.HasValue) query = query.Where(i => i.InvoiceDate <= to.Value);
        if (supplierId.HasValue) query = query.Where(i => i.SupplierId == supplierId.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(i => i.Status == status);
        if (departmentId.HasValue) query = query.Where(i => i.DepartmentId == departmentId.Value);
        if (costCenterId.HasValue) query = query.Where(i => i.CostCenterId == costCenterId.Value);
        if (projectId.HasValue) query = query.Where(i => i.ProjectId == projectId.Value);

        var rows = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new SupplierInvoiceRegisterRow
            {
                InvoiceNumber = i.InvoiceNumber,
                SupplierName = i.Supplier != null ? i.Supplier.Name : null,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Currency = i.Currency,
                TotalAmount = i.TotalAmount,
                PaidAmount = i.PaidAmount,
                RemainingAmount = i.RemainingAmount,
                Status = i.Status,
                MatchStatus = i.MatchStatus,
            })
            .ToListAsync();

        return rows;
    }

    [HttpGet("supplier-invoices/export")]
    public async Task<IActionResult> ExportSupplierInvoiceRegister(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] string? status = null)
    {
        if (!_access.CanExportFinance())
            return Forbid();

        var rows = await SupplierInvoiceRegister(from, to, supplierId, status);
        var list = (rows.Result as OkObjectResult)?.Value as List<SupplierInvoiceRegisterRow> ?? new();

        var headers = new[] { "InvoiceNumber", "Supplier", "InvoiceDate", "DueDate", "Currency", "Total", "Paid", "Remaining", "Status", "MatchStatus" };
        var data = list.Select(r => new object?[]
        {
            r.InvoiceNumber, r.SupplierName, r.InvoiceDate, r.DueDate, r.Currency,
            r.TotalAmount, r.PaidAmount, r.RemainingAmount, r.Status, r.MatchStatus,
        });

        await RecordExportAuditAsync("SUPPLIER_INVOICE_REGISTER");
        return File(_export.BuildCsv(headers, data), "text/csv", "supplier-invoice-register.csv");
    }

    // ---- AP Aging ----

    [HttpGet("ap-aging")]
    public async Task<ActionResult<ApAgingDto>> ApAging()
    {
        if (!_access.CanViewFinanceReports())
            return Forbid();

        return await _ap.GetAgingAsync();
    }

    [HttpGet("ap-aging/export")]
    public async Task<IActionResult> ExportApAging()
    {
        if (!_access.CanExportFinance())
            return Forbid();

        var aging = await _ap.GetAgingAsync();
        var headers = new[] { "Bucket", "Amount" };
        var data = new List<object?[]>
        {
            new object?[] { "Current", aging.Current },
            new object?[] { "1-30 days", aging.Days1To30 },
            new object?[] { "31-60 days", aging.Days31To60 },
            new object?[] { "61-90 days", aging.Days61To90 },
            new object?[] { "Over 90 days", aging.Over90 },
            new object?[] { "Total", aging.Total },
        };

        await RecordExportAuditAsync("AP_AGING");
        return File(_export.BuildCsv(headers, data), "text/csv", "ap-aging.csv");
    }

    // ---- Payment Register ----

    [HttpGet("payments")]
    public async Task<ActionResult<List<PaymentRegisterRow>>> PaymentRegister(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? status = null)
    {
        if (!_access.CanViewFinanceReports())
            return Forbid();

        var query = _context.Payments.AsNoTracking().AsQueryable();
        if (from.HasValue) query = query.Where(p => p.PaymentDate >= from.Value);
        if (to.HasValue) query = query.Where(p => p.PaymentDate <= to.Value);
        if (!string.IsNullOrEmpty(status)) query = query.Where(p => p.Status == status);

        var rows = await query
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentRegisterRow
            {
                PaymentNumber = p.PaymentNumber,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Currency = p.Currency,
                Amount = p.Amount,
                Status = p.Status,
                ReferenceNumber = p.ReferenceNumber,
                BankAccountId = p.BankAccountId,
            })
            .ToListAsync();

        return rows;
    }

    [HttpGet("payments/export")]
    public async Task<IActionResult> ExportPaymentRegister(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? status = null)
    {
        if (!_access.CanExportFinance())
            return Forbid();

        var rows = await PaymentRegister(from, to, status);
        var list = (rows.Result as OkObjectResult)?.Value as List<PaymentRegisterRow> ?? new();

        var headers = new[] { "PaymentNumber", "Date", "Method", "Currency", "Amount", "Status", "Reference" };
        var data = list.Select(r => new object?[]
        {
            r.PaymentNumber, r.PaymentDate, r.PaymentMethod, r.Currency, r.Amount, r.Status, r.ReferenceNumber,
        });

        await RecordExportAuditAsync("PAYMENT_REGISTER");
        return File(_export.BuildCsv(headers, data), "text/csv", "payment-register.csv");
    }

    // ---- General Ledger ----

    [HttpGet("general-ledger")]
    public async Task<ActionResult<List<GeneralLedgerRow>>> GeneralLedger([FromQuery] int fiscalPeriodId)
    {
        if (!_access.CanViewFinanceReports())
            return Forbid();

        return await _accounting.GetGeneralLedgerAsync(fiscalPeriodId);
    }

    [HttpGet("general-ledger/export")]
    public async Task<IActionResult> ExportGeneralLedger([FromQuery] int fiscalPeriodId, [FromQuery] string format = "csv")
    {
        if (!_access.CanExportFinance())
            return Forbid();

        var rows = await _accounting.GetGeneralLedgerAsync(fiscalPeriodId);
        var headers = new[] { "JournalNumber", "PostingDate", "SourceType", "SourceId", "Description", "AccountCode", "AccountName", "Debit", "Credit" };
        var data = rows.Select(r => new object?[]
        {
            r.JournalNumber, r.PostingDate, r.SourceType, r.SourceId, r.Description,
            r.AccountCode, r.AccountName, r.Debit, r.Credit,
        });

        await RecordExportAuditAsync("GENERAL_LEDGER");

        if (string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return File(_export.BuildExcel("General Ledger", headers, data),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"general-ledger-{fiscalPeriodId}.xlsx");
        }

        return File(_export.BuildCsv(headers, data), "text/csv", $"general-ledger-{fiscalPeriodId}.csv");
    }

    // ---- Trial Balance ----

    [HttpGet("trial-balance")]
    public async Task<ActionResult<List<TrialBalanceRow>>> TrialBalance([FromQuery] int fiscalPeriodId)
    {
        if (!_access.CanViewFinanceReports())
            return Forbid();

        return await _accounting.GetTrialBalanceAsync(fiscalPeriodId);
    }

    [HttpGet("trial-balance/export")]
    public async Task<IActionResult> ExportTrialBalance([FromQuery] int fiscalPeriodId, [FromQuery] string format = "csv")
    {
        if (!_access.CanExportFinance())
            return Forbid();

        var rows = await _accounting.GetTrialBalanceAsync(fiscalPeriodId);
        var headers = new[] { "AccountCode", "AccountName", "AccountType", "Debit", "Credit", "Balance" };
        var data = rows.Select(r => new object?[]
        {
            r.AccountCode, r.AccountName, r.AccountType, r.Debit, r.Credit, r.Balance,
        });

        await RecordExportAuditAsync("TRIAL_BALANCE");

        if (string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return File(_export.BuildExcel("Trial Balance", headers, data),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"trial-balance-{fiscalPeriodId}.xlsx");
        }

        return File(_export.BuildCsv(headers, data), "text/csv", $"trial-balance-{fiscalPeriodId}.csv");
    }

    // ---- Expense Summary ----

    [HttpGet("expense-summary")]
    public async Task<ActionResult<List<ExpenseSummaryRow>>> ExpenseSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (!_access.CanViewFinanceReports())
            return Forbid();

        var query = _context.Expenses.AsNoTracking().AsQueryable();
        if (from.HasValue) query = query.Where(e => e.ExpenseDate >= from.Value);
        if (to.HasValue) query = query.Where(e => e.ExpenseDate <= to.Value);

        var rows = await query
            .GroupBy(e => e.CategoryId)
            .Select(g => new ExpenseSummaryRow
            {
                CategoryName = g.First().Category != null ? g.First().Category!.Name : "?",
                Count = g.Count(),
                TotalAmountLak = g.Sum(e => e.AmountLak),
            })
            .OrderByDescending(r => r.TotalAmountLak)
            .ToListAsync();

        return rows;
    }

    [HttpGet("expense-summary/export")]
    public async Task<IActionResult> ExportExpenseSummary(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        if (!_access.CanExportFinance())
            return Forbid();

        var rows = await ExpenseSummary(from, to);
        var list = (rows.Result as OkObjectResult)?.Value as List<ExpenseSummaryRow> ?? new();

        var headers = new[] { "Category", "Count", "TotalAmountLak" };
        var data = list.Select(r => new object?[] { r.CategoryName, r.Count, r.TotalAmountLak });

        await RecordExportAuditAsync("EXPENSE_SUMMARY");
        return File(_export.BuildCsv(headers, data), "text/csv", "expense-summary.csv");
    }

    // ---- Budget Utilization ----

    [HttpGet("budget-utilization")]
    public async Task<ActionResult<List<BudgetUtilizationRow>>> BudgetUtilization([FromQuery] int? fiscalYear = null)
    {
        if (!_access.CanViewFinanceReports())
            return Forbid();

        var query = _context.Budgets.AsNoTracking().AsQueryable();
        if (fiscalYear.HasValue) query = query.Where(b => b.FiscalYear == fiscalYear.Value);

        var rows = await query
            .OrderByDescending(b => b.FiscalYear).ThenBy(b => b.Category)
            .Select(b => new BudgetUtilizationRow
            {
                FiscalYear = b.FiscalYear,
                Category = b.Category,
                Currency = b.Currency,
                Approved = b.ApprovedAmount,
                Reserved = b.ReservedAmount,
                Committed = b.CommittedAmount,
                Actual = b.ActualAmount,
                Available = b.ApprovedAmount - b.ReservedAmount - b.CommittedAmount - b.ActualAmount,
            })
            .ToListAsync();

        return rows;
    }

    [HttpGet("budget-utilization/export")]
    public async Task<IActionResult> ExportBudgetUtilization([FromQuery] int? fiscalYear = null)
    {
        if (!_access.CanExportFinance())
            return Forbid();

        var rows = await BudgetUtilization(fiscalYear);
        var list = (rows.Result as OkObjectResult)?.Value as List<BudgetUtilizationRow> ?? new();

        var headers = new[] { "FiscalYear", "Category", "Currency", "Approved", "Reserved", "Committed", "Actual", "Available" };
        var data = list.Select(r => new object?[]
        {
            r.FiscalYear, r.Category, r.Currency, r.Approved, r.Reserved, r.Committed, r.Actual, r.Available,
        });

        await RecordExportAuditAsync("BUDGET_UTILIZATION");
        return File(_export.BuildCsv(headers, data), "text/csv", "budget-utilization.csv");
    }

    // ---- Export audit (reuses the existing AuditLog entity) ----

    private async Task RecordExportAuditAsync(string reportName)
    {
        var username = User.Identity?.Name ?? "SYSTEM";
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = username,
            EntityName = "FINANCE_EXPORT",
            Action = "EXPORT",
            KeyValues = System.Text.Json.JsonSerializer.Serialize(new { Report = reportName }),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { Report = reportName, At = DateTime.UtcNow }),
            Timestamp = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
    }
}

public class SupplierInvoiceRegisterRow
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string Currency { get; set; } = "LAK";
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MatchStatus { get; set; }
}

public class PaymentRegisterRow
{
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Currency { get; set; } = "LAK";
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public int? BankAccountId { get; set; }
}

public class ExpenseSummaryRow
{
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmountLak { get; set; }
}

public class BudgetUtilizationRow
{
    public int FiscalYear { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Currency { get; set; } = "LAK";
    public decimal Approved { get; set; }
    public decimal Reserved { get; set; }
    public decimal Committed { get; set; }
    public decimal Actual { get; set; }
    public decimal Available { get; set; }
}
