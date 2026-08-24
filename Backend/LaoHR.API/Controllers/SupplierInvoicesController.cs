using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class SupplierInvoiceLineDto
{
    public int SupplierInvoiceLineId { get; set; }
    public int? PurchaseOrderItemId { get; set; }
    public int? InventoryItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public int? AccountId { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
}

public class SupplierInvoiceListItem
{
    public int SupplierInvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? PurchaseOrderId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string Currency { get; set; } = "LAK";
    public decimal TotalAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? MatchStatus { get; set; }
}

public class SupplierInvoiceDetail : SupplierInvoiceListItem
{
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
    public int? DepartmentId { get; set; }
    public List<SupplierInvoiceLineDto> Lines { get; set; } = new();
}

public class CreateSupplierInvoiceLine
{
    public int? PurchaseOrderItemId { get; set; }
    public int? InventoryItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public int? AccountId { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
}

public class CreateSupplierInvoiceRequest
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public int? PurchaseOrderId { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public string Currency { get; set; } = "LAK";
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
    public int? DepartmentId { get; set; }
    public List<CreateSupplierInvoiceLine> Lines { get; set; } = new();
}

[Authorize]
[ApiController]
[Route("api/supplier-invoices")]
public class SupplierInvoicesController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;
    private readonly IAccountsPayableService _ap;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IPostingService _posting;
    private readonly ISegregationOfDutiesService _sod;
    private readonly INotificationService _notifications;

    public SupplierInvoicesController(
        LaoHRDbContext context,
        IFinanceAccessService access,
        IAccountsPayableService ap,
        ICurrentEmployeeService currentEmployee,
        IPostingService posting,
        ISegregationOfDutiesService sod,
        INotificationService notifications)
    {
        _context = context;
        _access = access;
        _ap = ap;
        _currentEmployee = currentEmployee;
        _posting = posting;
        _sod = sod;
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<SupplierInvoiceListItem>>> GetInvoices(
        [FromQuery] string? status = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewAp())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.SupplierInvoices.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);
        if (supplierId.HasValue)
            query = query.Where(i => i.SupplierId == supplierId.Value);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new SupplierInvoiceListItem
            {
                SupplierInvoiceId = i.SupplierInvoiceId,
                InvoiceNumber = i.InvoiceNumber,
                SupplierId = i.SupplierId,
                SupplierName = i.Supplier != null ? i.Supplier.Name : null,
                PurchaseOrderId = i.PurchaseOrderId,
                InvoiceDate = i.InvoiceDate,
                DueDate = i.DueDate,
                Currency = i.Currency,
                TotalAmount = i.TotalAmount,
                RemainingAmount = i.RemainingAmount,
                Status = i.Status,
                MatchStatus = i.MatchStatus,
            })
            .ToListAsync();

        return new PaginatedResponse<SupplierInvoiceListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierInvoiceDetail>> GetInvoice(int id)
    {
        if (!_access.CanViewAp())
            return Forbid();

        var i = await _context.SupplierInvoices.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SupplierInvoiceId == id);
        if (i == null) return NotFound();

        var lines = await _context.SupplierInvoiceLines.AsNoTracking()
            .Where(l => l.SupplierInvoiceId == id)
            .Select(l => new SupplierInvoiceLineDto
            {
                SupplierInvoiceLineId = l.SupplierInvoiceLineId,
                PurchaseOrderItemId = l.PurchaseOrderItemId,
                InventoryItemId = l.InventoryItemId,
                Description = l.Description,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Subtotal = l.Subtotal,
                TaxAmount = l.TaxAmount,
                AccountId = l.AccountId,
                CostCenterId = l.CostCenterId,
                ProjectId = l.ProjectId,
            })
            .ToListAsync();

        var supplierName = await _context.Suppliers
            .Where(s => s.SupplierId == i.SupplierId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync();

        return new SupplierInvoiceDetail
        {
            SupplierInvoiceId = i.SupplierInvoiceId,
            InvoiceNumber = i.InvoiceNumber,
            SupplierId = i.SupplierId,
            SupplierName = supplierName,
            PurchaseOrderId = i.PurchaseOrderId,
            InvoiceDate = i.InvoiceDate,
            DueDate = i.DueDate,
            Currency = i.Currency,
            Subtotal = i.Subtotal,
            TaxAmount = i.TaxAmount,
            TotalAmount = i.TotalAmount,
            PaidAmount = i.PaidAmount,
            RemainingAmount = i.RemainingAmount,
            Status = i.Status,
            MatchStatus = i.MatchStatus,
            CostCenterId = i.CostCenterId,
            ProjectId = i.ProjectId,
            DepartmentId = i.DepartmentId,
            Lines = lines,
        };
    }

    [HttpPost]
    public async Task<ActionResult<SupplierInvoice>> CreateInvoice([FromBody] CreateSupplierInvoiceRequest request)
    {
        if (!_access.CanCreateAp())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
            return BadRequest("InvoiceNumber is required.");
        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest("At least one line is required.");

        // Duplicate prevention: SupplierId + normalized InvoiceNumber.
        var normalized = request.InvoiceNumber.Trim();
        var duplicate = await _context.SupplierInvoices
            .AnyAsync(i => i.SupplierId == request.SupplierId && i.InvoiceNumber == normalized);
        if (duplicate)
            return Conflict("A supplier invoice with this number already exists for this supplier.");

        var subtotal = request.Lines.Sum(l => l.Quantity * l.UnitPrice);
        var tax = request.Lines.Sum(l => l.TaxAmount);
        var total = subtotal + tax;

        var invoice = new SupplierInvoice
        {
            InvoiceNumber = normalized,
            SupplierId = request.SupplierId,
            PurchaseOrderId = request.PurchaseOrderId,
            InvoiceDate = request.InvoiceDate,
            DueDate = request.DueDate,
            Currency = request.Currency,
            Subtotal = subtotal,
            TaxAmount = tax,
            TotalAmount = total,
            RemainingAmount = total,
            Status = "DRAFT",
            CostCenterId = request.CostCenterId,
            ProjectId = request.ProjectId,
            DepartmentId = request.DepartmentId,
            CreatedByEmployeeId = _currentEmployee.GetCurrentEmployeeId(),
        };

        foreach (var line in request.Lines)
        {
            invoice.Lines.Add(new SupplierInvoiceLine
            {
                PurchaseOrderItemId = line.PurchaseOrderItemId,
                InventoryItemId = line.InventoryItemId,
                Description = line.Description,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Subtotal = line.Quantity * line.UnitPrice,
                TaxAmount = line.TaxAmount,
                AccountId = line.AccountId,
                CostCenterId = line.CostCenterId,
                ProjectId = line.ProjectId,
            });
        }

        _context.SupplierInvoices.Add(invoice);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.SupplierInvoiceId }, invoice);
    }

    [HttpPost("{id:int}/match")]
    public async Task<IActionResult> Match(int id)
    {
        if (!_access.CanCreateAp())
            return Forbid();

        var invoice = await _context.SupplierInvoices
            .FirstOrDefaultAsync(i => i.SupplierInvoiceId == id);
        if (invoice == null) return NotFound();

        var matchStatus = await _ap.MatchAsync(id);
        invoice.MatchStatus = matchStatus;
        invoice.Status = matchStatus == "MATCHED" ? "PENDING_APPROVAL" : "MATCH_EXCEPTION";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Notify the invoice creator of a match exception (best-effort).
        if (matchStatus != "MATCHED" && invoice.CreatedByEmployeeId.HasValue)
        {
            await _notifications.NotifyEmployeeAsync(
                invoice.CreatedByEmployeeId.Value,
                "SUPPLIER_INVOICE_MATCH_EXCEPTION",
                "Invoice match exception",
                $"Supplier invoice {invoice.InvoiceNumber} has a match exception ({matchStatus}).",
                "SUPPLIER_INVOICE",
                invoice.SupplierInvoiceId);
        }

        return Ok(new { MatchStatus = matchStatus, Status = invoice.Status });
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        if (!_access.CanApproveAp())
            return Forbid();

        var invoice = await _context.SupplierInvoices
            .FirstOrDefaultAsync(i => i.SupplierInvoiceId == id);
        if (invoice == null) return NotFound();

        if (invoice.Status is "DRAFT" or "PENDING_MATCH" or "MATCH_EXCEPTION")
            return BadRequest($"Cannot approve an invoice in '{invoice.Status}' state.");

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            await _sod.EnforceInvoiceApprovalAsync(id, actor.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        invoice.Status = "APPROVED";
        invoice.ApprovedByEmployeeId = actor.Value;
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Notify the invoice creator of approval (best-effort).
        if (invoice.CreatedByEmployeeId.HasValue)
        {
            await _notifications.NotifyEmployeeAsync(
                invoice.CreatedByEmployeeId.Value,
                "SUPPLIER_INVOICE_APPROVED",
                "Supplier invoice approved",
                $"Supplier invoice {invoice.InvoiceNumber} was approved.",
                "SUPPLIER_INVOICE",
                invoice.SupplierInvoiceId);
        }

        return NoContent();
    }

    [HttpPost("{id:int}/void")]
    public async Task<IActionResult> Void(int id)
    {
        if (!_access.CanCreateAp())
            return Forbid();

        var invoice = await _context.SupplierInvoices
            .FirstOrDefaultAsync(i => i.SupplierInvoiceId == id);
        if (invoice == null) return NotFound();

        if (invoice.Status == "PAID" || invoice.Status == "PARTIALLY_PAID")
            return BadRequest("Cannot void a paid invoice.");

        invoice.Status = "VOID";
        invoice.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> Post(int id)
    {
        if (!_access.CanPostAccounting())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            var entry = await _posting.PostSupplierInvoiceAsync(id, actor.Value);
            return Ok(entry);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("aging")]
    public async Task<ActionResult<ApAgingDto>> GetAging()
    {
        if (!_access.CanViewAp())
            return Forbid();

        return await _ap.GetAgingAsync();
    }
}
