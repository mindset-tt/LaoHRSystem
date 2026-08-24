using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class PurchaseOrderItemDto
{
    public int PurchaseOrderItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class PurchaseOrderListItem
{
    public int PurchaseOrderId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? RequestId { get; set; }
    public string Currency { get; set; } = "LAK";
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PurchaseOrderDetail
{
    public int PurchaseOrderId { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? RequestId { get; set; }
    public string Currency { get; set; } = "LAK";
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public string? PaymentTerms { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItem
{
    public string Description { get; set; } = string.Empty;
    public int? ItemId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxRate { get; set; } = 0;
    public string? Notes { get; set; }
}

public class CreatePurchaseOrderRequest
{
    public int SupplierId { get; set; }
    public int? RequestId { get; set; }
    public int? BudgetId { get; set; }
    public string Currency { get; set; } = "LAK";
    public DateTime? ExpectedDate { get; set; }
    public string? PaymentTerms { get; set; }
    public List<CreatePurchaseOrderItem> Items { get; set; } = new();
}

[Authorize]
[ApiController]
[Route("api/purchase-orders")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly INumberSequenceService _numbers;
    private readonly IBudgetService _budget;

    public PurchaseOrdersController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        INumberSequenceService numbers,
        IBudgetService budget)
    {
        _context = context;
        _access = access;
        _numbers = numbers;
        _budget = budget;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PurchaseOrderListItem>>> GetOrders(
        [FromQuery] string? status = null,
        [FromQuery] int? supplierId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewProcurement())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.PurchaseOrders.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);
        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.PONumber.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PurchaseOrderListItem
            {
                PurchaseOrderId = p.PurchaseOrderId,
                PONumber = p.PONumber,
                SupplierId = p.SupplierId,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                RequestId = p.RequestId,
                Currency = p.Currency,
                OrderDate = p.OrderDate,
                ExpectedDate = p.ExpectedDate,
                Status = p.Status,
                Total = p.Total,
                CreatedAt = p.CreatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<PurchaseOrderListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PurchaseOrderDetail>> GetOrder(int id)
    {
        if (!_access.CanViewProcurement())
            return Forbid();

        var p = await _context.PurchaseOrders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PurchaseOrderId == id);
        if (p == null) return NotFound();

        var items = await _context.PurchaseOrderItems.AsNoTracking()
            .Where(i => i.PurchaseOrderId == id)
            .Select(i => new PurchaseOrderItemDto
            {
                PurchaseOrderItemId = i.PurchaseOrderItemId,
                Description = i.Description,
                ItemId = i.ItemId,
                Quantity = i.Quantity,
                Unit = i.Unit,
                UnitPrice = i.UnitPrice,
                TaxRate = i.TaxRate,
                LineTotal = i.LineTotal,
                Notes = i.Notes,
            })
            .ToListAsync();

        var supplierName = await _context.Suppliers
            .Where(s => s.SupplierId == p.SupplierId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync();

        return new PurchaseOrderDetail
        {
            PurchaseOrderId = p.PurchaseOrderId,
            PONumber = p.PONumber,
            SupplierId = p.SupplierId,
            SupplierName = supplierName,
            RequestId = p.RequestId,
            Currency = p.Currency,
            OrderDate = p.OrderDate,
            ExpectedDate = p.ExpectedDate,
            PaymentTerms = p.PaymentTerms,
            Status = p.Status,
            Subtotal = p.Subtotal,
            Tax = p.Tax,
            Total = p.Total,
            Items = items,
        };
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseOrder>> CreateOrder([FromBody] CreatePurchaseOrderRequest request)
    {
        if (!_access.CanManageProcurement())
            return Forbid();

        if (request.Items == null || request.Items.Count == 0)
            return BadRequest("At least one line is required.");

        var supplier = await _context.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(s => s.SupplierId == request.SupplierId);
        if (supplier == null) return BadRequest("Supplier not found.");
        if (supplier.Status == "BLOCKED")
            return BadRequest("Supplier is blocked.");

        // If converting from a request, it must be APPROVED.
        int? budgetId = request.BudgetId;
        if (request.RequestId.HasValue)
        {
            var pr = await _context.PurchaseRequests.AsNoTracking()
                .FirstOrDefaultAsync(r => r.PurchaseRequestId == request.RequestId.Value);
            if (pr == null) return BadRequest("Purchase request not found.");
            if (pr.Status != "APPROVED")
                return BadRequest("Cannot create a PO from a request that is not APPROVED.");
            // Inherit the request's budget if not explicitly overridden.
            budgetId ??= pr.BudgetId;
        }

        var number = await _numbers.NextAsync("PO");

        var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
        var tax = request.Items.Sum(i => i.Quantity * i.UnitPrice * i.TaxRate);
        var total = subtotal + tax;

        // Budget enforcement: commit against the linked budget (converts the
        // PR reservation to a commitment). Throws on overspend.
        if (budgetId.HasValue)
        {
            try
            {
                await _budget.CommitAsync(budgetId.Value, total);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

        var po = new PurchaseOrder
        {
            PONumber = number,
            SupplierId = request.SupplierId,
            RequestId = request.RequestId,
            BudgetId = budgetId,
            Currency = request.Currency,
            OrderDate = DateTime.UtcNow,
            ExpectedDate = request.ExpectedDate,
            PaymentTerms = request.PaymentTerms,
            Status = "DRAFT",
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
        };

        foreach (var item in request.Items)
        {
            po.Items.Add(new PurchaseOrderItem
            {
                Description = item.Description,
                ItemId = item.ItemId,
                Quantity = item.Quantity,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice,
                TaxRate = item.TaxRate,
                LineTotal = item.Quantity * item.UnitPrice * (1 + item.TaxRate),
                Notes = item.Notes,
            });
        }

        _context.PurchaseOrders.Add(po);
        await _context.SaveChangesAsync();

        // Mark the source request as CONVERTED.
        if (request.RequestId.HasValue)
        {
            var pr = await _context.PurchaseRequests
                .FirstOrDefaultAsync(r => r.PurchaseRequestId == request.RequestId.Value);
            if (pr != null)
            {
                pr.Status = "CONVERTED";
                pr.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        return CreatedAtAction(nameof(GetOrder), new { id = po.PurchaseOrderId }, po);
    }

    [HttpPost("{id:int}/send")]
    public async Task<IActionResult> Send(int id)
    {
        if (!_access.CanManageProcurement())
            return Forbid();

        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(x => x.PurchaseOrderId == id);
        if (po == null) return NotFound();
        if (po.Status != "DRAFT" && po.Status != "APPROVED")
            return BadRequest($"Cannot send a PO in '{po.Status}' state.");

        po.Status = "SENT";
        po.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        if (!_access.CanManageProcurement())
            return Forbid();

        var po = await _context.PurchaseOrders.FirstOrDefaultAsync(x => x.PurchaseOrderId == id);
        if (po == null) return NotFound();
        if (po.Status is "RECEIVED" or "CLOSED" or "PARTIALLY_RECEIVED")
            return BadRequest($"Cannot cancel a PO in '{po.Status}' state.");

        po.Status = "CANCELLED";
        po.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
