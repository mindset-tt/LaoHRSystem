using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class InventoryItemDto
{
    public int InventoryItemId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string ItemType { get; set; } = "STOCK";
    public bool TrackInventory { get; set; }
    public decimal? ReorderLevel { get; set; }
    public bool IsActive { get; set; }
}

public class CreateInventoryItemRequest
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public string? UnitOfMeasure { get; set; }
    public string ItemType { get; set; } = "STOCK";
    public bool TrackInventory { get; set; } = true;
    public decimal? ReorderLevel { get; set; }
}

public class InventoryCategoryDto
{
    public int InventoryCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public int? ParentCategoryId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class WarehouseDto
{
    public int WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public int? WorkLocationId { get; set; }
    public string? Address { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class StockBalanceDto
{
    public int ItemId { get; set; }
    public int WarehouseId { get; set; }
    public decimal OnHand { get; set; }
}

public class StockMovementDto
{
    public int StockMovementId { get; set; }
    public int ItemId { get; set; }
    public string? ItemName { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int PerformedByEmployeeId { get; set; }
    public string? Notes { get; set; }
}

public class RecordMovementRequest
{
    public int ItemId { get; set; }
    public int WarehouseId { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
}

public class TransferRequest
{
    public int ItemId { get; set; }
    public int FromWarehouseId { get; set; }
    public int ToWarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class AdjustmentRequest
{
    public int ItemId { get; set; }
    public int WarehouseId { get; set; }
    public decimal Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

[Authorize]
[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly IInventoryService _inventory;
    private readonly ICurrentEmployeeService _currentEmployee;

    public InventoryController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        IInventoryService inventory,
        ICurrentEmployeeService currentEmployee)
    {
        _context = context;
        _access = access;
        _inventory = inventory;
        _currentEmployee = currentEmployee;
    }

    // ---- Items ----
    [HttpGet("items")]
    public async Task<ActionResult<PaginatedResponse<InventoryItemDto>>> GetItems(
        [FromQuery] string? itemType = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewInventory())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.InventoryItems.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(itemType))
            query = query.Where(i => i.ItemType == itemType);
        if (categoryId.HasValue)
            query = query.Where(i => i.CategoryId == categoryId.Value);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(s) || i.SKU.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();
        var items = await query
            .OrderBy(i => i.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new InventoryItemDto
            {
                InventoryItemId = i.InventoryItemId,
                SKU = i.SKU,
                Name = i.Name,
                NameLao = i.NameLao,
                Description = i.Description,
                CategoryId = i.CategoryId,
                UnitOfMeasure = i.UnitOfMeasure,
                ItemType = i.ItemType,
                TrackInventory = i.TrackInventory,
                ReorderLevel = i.ReorderLevel,
                IsActive = i.IsActive,
            })
            .ToListAsync();

        return new PaginatedResponse<InventoryItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("items/{id:int}")]
    public async Task<ActionResult<InventoryItemDto>> GetItem(int id)
    {
        if (!_access.CanViewInventory())
            return Forbid();

        var i = await _context.InventoryItems.AsNoTracking()
            .FirstOrDefaultAsync(x => x.InventoryItemId == id);
        if (i == null) return NotFound();

        return new InventoryItemDto
        {
            InventoryItemId = i.InventoryItemId,
            SKU = i.SKU,
            Name = i.Name,
            NameLao = i.NameLao,
            Description = i.Description,
            CategoryId = i.CategoryId,
            UnitOfMeasure = i.UnitOfMeasure,
            ItemType = i.ItemType,
            TrackInventory = i.TrackInventory,
            ReorderLevel = i.ReorderLevel,
            IsActive = i.IsActive,
        };
    }

    [HttpPost("items")]
    public async Task<ActionResult<InventoryItem>> CreateItem([FromBody] CreateInventoryItemRequest request)
    {
        if (!_access.CanManageInventory())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.SKU) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("SKU and Name are required.");

        if (request.ItemType is not ("STOCK" or "CONSUMABLE" or "SERVICE" or "ASSET"))
            return BadRequest("Invalid item type.");

        var item = new InventoryItem
        {
            SKU = request.SKU,
            Name = request.Name,
            NameLao = request.NameLao,
            Description = request.Description,
            CategoryId = request.CategoryId,
            UnitOfMeasure = request.UnitOfMeasure,
            ItemType = request.ItemType,
            TrackInventory = request.TrackInventory,
            ReorderLevel = request.ReorderLevel,
            IsActive = true,
        };
        _context.InventoryItems.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetItem), new { id = item.InventoryItemId }, item);
    }

    [HttpPut("items/{id:int}")]
    public async Task<IActionResult> UpdateItem(int id, [FromBody] CreateInventoryItemRequest request)
    {
        if (!_access.CanManageInventory())
            return Forbid();

        var item = await _context.InventoryItems.FirstOrDefaultAsync(x => x.InventoryItemId == id);
        if (item == null) return NotFound();

        if (request.SKU != null) item.SKU = request.SKU;
        if (request.Name != null) item.Name = request.Name;
        if (request.NameLao != null) item.NameLao = request.NameLao;
        if (request.Description != null) item.Description = request.Description;
        if (request.CategoryId.HasValue) item.CategoryId = request.CategoryId;
        if (request.UnitOfMeasure != null) item.UnitOfMeasure = request.UnitOfMeasure;
        if (request.ItemType != null) item.ItemType = request.ItemType;
        item.TrackInventory = request.TrackInventory;
        if (request.ReorderLevel.HasValue) item.ReorderLevel = request.ReorderLevel;

        item.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---- Categories ----
    [HttpGet("categories")]
    public async Task<ActionResult<List<InventoryCategoryDto>>> GetCategories()
    {
        if (!_access.CanViewInventory())
            return Forbid();

        return await _context.InventoryCategories.AsNoTracking()
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name)
            .Select(c => new InventoryCategoryDto
            {
                InventoryCategoryId = c.InventoryCategoryId,
                Code = c.Code,
                Name = c.Name,
                NameLao = c.NameLao,
                ParentCategoryId = c.ParentCategoryId,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
            })
            .ToListAsync();
    }

    [HttpPost("categories")]
    public async Task<ActionResult<InventoryCategory>> CreateCategory([FromBody] InventoryCategoryDto request)
    {
        if (!_access.CanManageInventory())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Code and Name are required.");

        // Prevent cycles: parent must not be self or a descendant.
        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _context.InventoryCategories
                .FirstOrDefaultAsync(c => c.InventoryCategoryId == request.ParentCategoryId.Value);
            if (parent == null) return BadRequest("Parent category not found.");
        }

        var category = new InventoryCategory
        {
            Code = request.Code,
            Name = request.Name,
            NameLao = request.NameLao,
            ParentCategoryId = request.ParentCategoryId,
            SortOrder = request.SortOrder,
            IsActive = true,
        };
        _context.InventoryCategories.Add(category);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCategories), new { }, category);
    }

    // ---- Warehouses ----
    [HttpGet("warehouses")]
    public async Task<ActionResult<List<WarehouseDto>>> GetWarehouses()
    {
        if (!_access.CanViewInventory())
            return Forbid();

        return await _context.Warehouses.AsNoTracking()
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseDto
            {
                WarehouseId = w.WarehouseId,
                Code = w.Code,
                Name = w.Name,
                NameLao = w.NameLao,
                WorkLocationId = w.WorkLocationId,
                Address = w.Address,
                ManagerEmployeeId = w.ManagerEmployeeId,
                Status = w.Status,
            })
            .ToListAsync();
    }

    [HttpPost("warehouses")]
    public async Task<ActionResult<Warehouse>> CreateWarehouse([FromBody] WarehouseDto request)
    {
        if (!_access.CanManageInventory())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Code and Name are required.");

        var warehouse = new Warehouse
        {
            Code = request.Code,
            Name = request.Name,
            NameLao = request.NameLao,
            WorkLocationId = request.WorkLocationId,
            Address = request.Address,
            ManagerEmployeeId = request.ManagerEmployeeId,
            Status = "ACTIVE",
        };
        _context.Warehouses.Add(warehouse);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetWarehouses), new { }, warehouse);
    }

    // ---- Stock ----
    [HttpGet("stock")]
    public async Task<ActionResult<List<StockBalanceDto>>> GetStockBalances(
        [FromQuery] int? itemId = null,
        [FromQuery] int? warehouseId = null)
    {
        if (!_access.CanViewInventory())
            return Forbid();

        var items = await _context.InventoryItems.AsNoTracking()
            .Where(i => i.TrackInventory)
            .Select(i => i.InventoryItemId)
            .ToListAsync();

        var warehouses = await _context.Warehouses.AsNoTracking()
            .Select(w => w.WarehouseId)
            .ToListAsync();

        var result = new List<StockBalanceDto>();
        foreach (var iid in items)
        {
            if (itemId.HasValue && itemId.Value != iid) continue;
            foreach (var wid in warehouses)
            {
                if (warehouseId.HasValue && warehouseId.Value != wid) continue;
                var onHand = await _inventory.GetBalanceAsync(iid, wid);
                if (onHand != 0)
                    result.Add(new StockBalanceDto { ItemId = iid, WarehouseId = wid, OnHand = onHand });
            }
        }
        return result;
    }

    [HttpGet("stock/movements")]
    public async Task<ActionResult<PaginatedResponse<StockMovementDto>>> GetMovements(
        [FromQuery] int? itemId = null,
        [FromQuery] int? warehouseId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewInventory())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.StockMovements.AsNoTracking().AsQueryable();
        if (itemId.HasValue) query = query.Where(m => m.ItemId == itemId.Value);
        if (warehouseId.HasValue) query = query.Where(m => m.WarehouseId == warehouseId.Value);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(m => m.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new StockMovementDto
            {
                StockMovementId = m.StockMovementId,
                ItemId = m.ItemId,
                ItemName = m.Item != null ? m.Item.Name : null,
                WarehouseId = m.WarehouseId,
                WarehouseName = m.Warehouse != null ? m.Warehouse.Name : null,
                MovementType = m.MovementType,
                Quantity = m.Quantity,
                ReferenceType = m.ReferenceType,
                ReferenceId = m.ReferenceId,
                OccurredAt = m.OccurredAt,
                PerformedByEmployeeId = m.PerformedByEmployeeId,
                Notes = m.Notes,
            })
            .ToListAsync();

        return new PaginatedResponse<StockMovementDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpPost("stock/movements")]
    public async Task<ActionResult<StockMovement>> RecordMovement([FromBody] RecordMovementRequest request)
    {
        if (!_access.CanManageInventory())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        if (request.MovementType is not ("RECEIPT" or "ISSUE" or "ADJUSTMENT_IN" or "ADJUSTMENT_OUT" or "RETURN"))
            return BadRequest("Invalid movement type.");

        try
        {
            var movement = await _inventory.RecordMovementAsync(
                request.ItemId, request.WarehouseId, request.MovementType, request.Quantity,
                actor.Value, request.ReferenceType, request.ReferenceId, request.Notes);
            return Ok(movement);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("stock/transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request)
    {
        if (!_access.CanManageInventory())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            await _inventory.TransferAsync(
                request.ItemId, request.FromWarehouseId, request.ToWarehouseId,
                request.Quantity, actor.Value, request.Notes);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("stock/adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustmentRequest request)
    {
        if (!_access.CanAdjustInventory())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Reason))
            return BadRequest("A reason is required for stock adjustments.");

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        var movementType = request.Quantity >= 0 ? "ADJUSTMENT_IN" : "ADJUSTMENT_OUT";
        var qty = Math.Abs(request.Quantity);

        try
        {
            await _inventory.RecordMovementAsync(
                request.ItemId, request.WarehouseId, movementType, qty,
                actor.Value, "ADJUSTMENT", null, request.Reason);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
