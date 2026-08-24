using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class GoodsReceiptItemDto
{
    public int GoodsReceiptItemId { get; set; }
    public int PurchaseOrderItemId { get; set; }
    public string? Description { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class GoodsReceiptListItem
{
    public int GoodsReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public string? PONumber { get; set; }
    public int ReceivedByEmployeeId { get; set; }
    public string? ReceivedByName { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int? WarehouseId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class GoodsReceiptDetail
{
    public int GoodsReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public string? PONumber { get; set; }
    public int ReceivedByEmployeeId { get; set; }
    public string? ReceivedByName { get; set; }
    public DateTime ReceivedDate { get; set; }
    public int? WarehouseId { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<GoodsReceiptItemDto> Items { get; set; } = new();
}

public class CreateGoodsReceiptItem
{
    public int PurchaseOrderItemId { get; set; }
    public decimal QuantityReceived { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string? Notes { get; set; }
}

public class CreateGoodsReceiptRequest
{
    public int PurchaseOrderId { get; set; }
    public int? WarehouseId { get; set; }
    public string? Notes { get; set; }
    public List<CreateGoodsReceiptItem> Items { get; set; } = new();
}

[Authorize]
[ApiController]
[Route("api/goods-receipts")]
public class GoodsReceiptsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IInventoryService _inventory;
    private readonly INumberSequenceService _numbers;

    public GoodsReceiptsController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        ICurrentEmployeeService currentEmployee,
        IInventoryService inventory,
        INumberSequenceService numbers)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _inventory = inventory;
        _numbers = numbers;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<GoodsReceiptListItem>>> GetReceipts(
        [FromQuery] int? purchaseOrderId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewProcurement())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.GoodsReceipts.AsNoTracking().AsQueryable();
        if (purchaseOrderId.HasValue)
            query = query.Where(g => g.PurchaseOrderId == purchaseOrderId.Value);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(g => g.ReceivedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GoodsReceiptListItem
            {
                GoodsReceiptId = g.GoodsReceiptId,
                ReceiptNumber = g.ReceiptNumber,
                PurchaseOrderId = g.PurchaseOrderId,
                PONumber = g.PurchaseOrder != null ? g.PurchaseOrder.PONumber : null,
                ReceivedByEmployeeId = g.ReceivedByEmployeeId,
                ReceivedByName = g.ReceivedBy != null ? (g.ReceivedBy.EnglishName ?? g.ReceivedBy.LaoName) : null,
                ReceivedDate = g.ReceivedDate,
                WarehouseId = g.WarehouseId,
                Status = g.Status,
            })
            .ToListAsync();

        return new PaginatedResponse<GoodsReceiptListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GoodsReceiptDetail>> GetReceipt(int id)
    {
        if (!_access.CanViewProcurement())
            return Forbid();

        var g = await _context.GoodsReceipts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.GoodsReceiptId == id);
        if (g == null) return NotFound();

        var items = await _context.GoodsReceiptItems.AsNoTracking()
            .Where(i => i.GoodsReceiptId == id)
            .Select(i => new GoodsReceiptItemDto
            {
                GoodsReceiptItemId = i.GoodsReceiptItemId,
                PurchaseOrderItemId = i.PurchaseOrderItemId,
                Description = i.PurchaseOrderItem != null ? i.PurchaseOrderItem.Description : null,
                QuantityReceived = i.QuantityReceived,
                AcceptedQuantity = i.AcceptedQuantity,
                RejectedQuantity = i.RejectedQuantity,
                Notes = i.Notes,
            })
            .ToListAsync();

        var poNumber = await _context.PurchaseOrders
            .Where(p => p.PurchaseOrderId == g.PurchaseOrderId)
            .Select(p => p.PONumber)
            .FirstOrDefaultAsync();

        var receivedByName = await _context.Employees
            .Where(e => e.EmployeeId == g.ReceivedByEmployeeId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        return new GoodsReceiptDetail
        {
            GoodsReceiptId = g.GoodsReceiptId,
            ReceiptNumber = g.ReceiptNumber,
            PurchaseOrderId = g.PurchaseOrderId,
            PONumber = poNumber,
            ReceivedByEmployeeId = g.ReceivedByEmployeeId,
            ReceivedByName = receivedByName,
            ReceivedDate = g.ReceivedDate,
            WarehouseId = g.WarehouseId,
            Notes = g.Notes,
            Status = g.Status,
            Items = items,
        };
    }

    [HttpPost]
    public async Task<ActionResult<GoodsReceipt>> CreateReceipt([FromBody] CreateGoodsReceiptRequest request)
    {
        if (!_access.CanManageProcurement())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        if (request.Items == null || request.Items.Count == 0)
            return BadRequest("At least one receipt line is required.");

        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == request.PurchaseOrderId);
        if (po == null) return NotFound("Purchase order not found.");
        if (po.Status is "CANCELLED" or "CLOSED")
            return BadRequest($"Cannot receive against a PO in '{po.Status}' state.");

        var number = await _numbers.NextAsync("GRN");

        var receipt = new GoodsReceipt
        {
            ReceiptNumber = number,
            PurchaseOrderId = request.PurchaseOrderId,
            ReceivedByEmployeeId = actor.Value,
            ReceivedDate = DateTime.UtcNow,
            WarehouseId = request.WarehouseId,
            Notes = request.Notes,
            Status = "DRAFT",
        };

        foreach (var item in request.Items)
        {
            var poLine = po.Items.FirstOrDefault(l => l.PurchaseOrderItemId == item.PurchaseOrderItemId);
            if (poLine == null) return BadRequest($"PO line {item.PurchaseOrderItemId} not found.");

            // Cannot receive more than ordered (unless configured — not yet).
            var alreadyReceived = await _context.GoodsReceiptItems
                .Where(gi => gi.PurchaseOrderItemId == item.PurchaseOrderItemId
                             && gi.GoodsReceipt!.Status == "POSTED")
                .SumAsync(gi => (decimal?)gi.AcceptedQuantity) ?? 0m;

            if (alreadyReceived + item.AcceptedQuantity > poLine.Quantity)
                return BadRequest($"Receiving more than ordered for line '{poLine.Description}'.");

            receipt.Items.Add(new GoodsReceiptItem
            {
                PurchaseOrderItemId = item.PurchaseOrderItemId,
                QuantityReceived = item.QuantityReceived,
                AcceptedQuantity = item.AcceptedQuantity,
                RejectedQuantity = item.RejectedQuantity,
                Notes = item.Notes,
            });
        }

        _context.GoodsReceipts.Add(receipt);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReceipt), new { id = receipt.GoodsReceiptId }, receipt);
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> Post(int id)
    {
        if (!_access.CanManageProcurement())
            return Forbid();

        var receipt = await _context.GoodsReceipts
            .Include(g => g.Items)
            .FirstOrDefaultAsync(g => g.GoodsReceiptId == id);
        if (receipt == null) return NotFound();
        if (receipt.Status != "DRAFT")
            return BadRequest($"Cannot post a receipt in '{receipt.Status}' state.");

        var po = await _context.PurchaseOrders
            .Include(p => p.Items)
            .FirstOrDefaultAsync(p => p.PurchaseOrderId == receipt.PurchaseOrderId);
        if (po == null) return NotFound("Purchase order not found.");

        // Idempotency guard: posting twice must not double-count stock.
        if (receipt.Status == "POSTED")
            return BadRequest("Receipt already posted.");

        var relational = _context.Database.IsRelational();
        await using var tx = relational ? await _context.Database.BeginTransactionAsync() : null;

        try
        {
            foreach (var item in receipt.Items)
            {
                var poLine = po.Items.FirstOrDefault(l => l.PurchaseOrderItemId == item.PurchaseOrderItemId);
                if (poLine == null) continue;

                // Only stocked items generate stock movements.
                if (poLine.ItemId.HasValue && receipt.WarehouseId.HasValue)
                {
                    var invItem = await _context.InventoryItems
                        .FirstOrDefaultAsync(i => i.InventoryItemId == poLine.ItemId.Value);
                    if (invItem != null && invItem.TrackInventory && item.AcceptedQuantity > 0)
                    {
                        await _inventory.RecordMovementAsync(
                            poLine.ItemId.Value, receipt.WarehouseId.Value, "RECEIPT",
                            item.AcceptedQuantity, receipt.ReceivedByEmployeeId,
                            "GOODS_RECEIPT", receipt.GoodsReceiptId, null);
                    }

                    // Asset auto-generation: ItemType == ASSET → one asset per accepted unit.
                    if (invItem != null && invItem.ItemType == "ASSET" && item.AcceptedQuantity > 0)
                    {
                        await GenerateAssetsAsync(po, poLine, item, invItem);
                    }
                }
            }

            receipt.Status = "POSTED";
            await _context.SaveChangesAsync();

            // Update PO status based on received quantities.
            await UpdatePoStatusAsync(po);

            if (tx != null) await tx.CommitAsync();
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync();
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Creates one fixed asset per accepted unit for ASSET-type items. Idempotent:
    /// assets already generated for this receipt line are not duplicated.
    /// </summary>
    private async Task GenerateAssetsAsync(
        PurchaseOrder po, PurchaseOrderItem poLine, GoodsReceiptItem receiptItem, InventoryItem invItem)
    {
        var existingCount = await _context.Assets
            .CountAsync(a => a.GoodsReceiptItemId == receiptItem.GoodsReceiptItemId);

        var toCreate = (int)receiptItem.AcceptedQuantity - existingCount;
        for (var i = 0; i < toCreate; i++)
        {
            var code = await _numbers.NextAsync("AST");
            _context.Assets.Add(new Asset
            {
                AssetCode = code,
                Name = invItem.Name,
                CategoryId = invItem.CategoryId,
                PurchaseOrderItemId = poLine.PurchaseOrderItemId,
                GoodsReceiptItemId = receiptItem.GoodsReceiptItemId,
                PurchaseDate = po.OrderDate,
                AcquisitionCost = poLine.UnitPrice,
                Currency = po.Currency,
                Status = "AVAILABLE",
            });
        }
        await _context.SaveChangesAsync();
    }

    private async Task UpdatePoStatusAsync(PurchaseOrder po)
    {
        var allLines = po.Items;
        var fullyReceived = true;
        var anyReceived = false;

        foreach (var line in allLines)
        {
            var received = await _context.GoodsReceiptItems
                .Where(gi => gi.PurchaseOrderItemId == line.PurchaseOrderItemId
                             && gi.GoodsReceipt!.Status == "POSTED")
                .SumAsync(gi => (decimal?)gi.AcceptedQuantity) ?? 0m;

            if (received > 0) anyReceived = true;
            if (received < line.Quantity) fullyReceived = false;
        }

        if (fullyReceived && anyReceived)
            po.Status = "RECEIVED";
        else if (anyReceived)
            po.Status = "PARTIALLY_RECEIVED";

        po.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
