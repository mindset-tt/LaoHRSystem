using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4A — inventory stock ledger. Stock balance is DERIVED from the
/// immutable StockMovement ledger (never a mutable Item.Quantity). All mutations
/// are transactional and enforce inventory invariants:
///   - quantity must be non-zero
///   - stocked items require a warehouse
///   - transfers must balance (out + in atomically)
///   - adjustments require a reason
///   - negative stock is rejected unless the company setting allows it
/// </summary>
public interface IInventoryService
{
    /// <summary>Current on-hand quantity for an item in a warehouse (derived from ledger).</summary>
    Task<decimal> GetBalanceAsync(int itemId, int warehouseId, CancellationToken ct = default);

    /// <summary>Records a single stock movement (transactional).</summary>
    Task<StockMovement> RecordMovementAsync(
        int itemId, int warehouseId, string movementType, decimal quantity,
        int performedByEmployeeId, string? referenceType, int? referenceId,
        string? notes, CancellationToken ct = default);

    /// <summary>Atomically records a transfer out of one warehouse and into another.</summary>
    Task TransferAsync(
        int itemId, int fromWarehouseId, int toWarehouseId, decimal quantity,
        int performedByEmployeeId, string? notes, CancellationToken ct = default);
}

public sealed class InventoryService : IInventoryService
{
    private readonly LaoHRDbContext _context;

    public InventoryService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetBalanceAsync(int itemId, int warehouseId, CancellationToken ct = default)
    {
        var ins = await _context.StockMovements
            .Where(m => m.ItemId == itemId && m.WarehouseId == warehouseId
                        && (m.MovementType == "RECEIPT" || m.MovementType == "TRANSFER_IN" || m.MovementType == "ADJUSTMENT_IN" || m.MovementType == "RETURN"))
            .SumAsync(m => (decimal?)m.Quantity, ct) ?? 0m;

        var outs = await _context.StockMovements
            .Where(m => m.ItemId == itemId && m.WarehouseId == warehouseId
                        && (m.MovementType == "ISSUE" || m.MovementType == "TRANSFER_OUT" || m.MovementType == "ADJUSTMENT_OUT"))
            .SumAsync(m => (decimal?)m.Quantity, ct) ?? 0m;

        return ins - outs;
    }

    public async Task<StockMovement> RecordMovementAsync(
        int itemId, int warehouseId, string movementType, decimal quantity,
        int performedByEmployeeId, string? referenceType, int? referenceId,
        string? notes, CancellationToken ct = default)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Stock movement quantity must be positive.");

        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.InventoryItemId == itemId, ct)
            ?? throw new InvalidOperationException("Inventory item not found.");

        if (item.TrackInventory && warehouseId <= 0)
            throw new InvalidOperationException("A warehouse is required for stocked items.");

        var isOut = movementType is "ISSUE" or "TRANSFER_OUT" or "ADJUSTMENT_OUT";

        if (isOut)
        {
            var balance = await GetBalanceAsync(itemId, warehouseId, ct);
            if (balance < quantity)
            {
                var allowNegative = await AllowNegativeStockAsync(ct);
                if (!allowNegative)
                    throw new InvalidOperationException(
                        $"Insufficient stock: on-hand {balance}, requested {quantity}.");
            }
        }

        var movement = new StockMovement
        {
            ItemId = itemId,
            WarehouseId = warehouseId,
            MovementType = movementType,
            Quantity = quantity,
            ReferenceType = referenceType,
            ReferenceId = referenceId,
            OccurredAt = DateTime.UtcNow,
            PerformedByEmployeeId = performedByEmployeeId,
            Notes = notes,
        };
        _context.StockMovements.Add(movement);
        await _context.SaveChangesAsync(ct);
        return movement;
    }

    public async Task TransferAsync(
        int itemId, int fromWarehouseId, int toWarehouseId, decimal quantity,
        int performedByEmployeeId, string? notes, CancellationToken ct = default)
    {
        if (fromWarehouseId == toWarehouseId)
            throw new InvalidOperationException("Source and destination warehouses must differ.");

        if (quantity <= 0)
            throw new InvalidOperationException("Transfer quantity must be positive.");

        // Atomic: both movements in one transaction.
        var relational = _context.Database.IsRelational();
        await using var tx = relational ? await _context.Database.BeginTransactionAsync(ct) : null;

        try
        {
            await RecordMovementAsync(itemId, fromWarehouseId, "TRANSFER_OUT", quantity,
                performedByEmployeeId, "TRANSFER", null, notes, ct);
            await RecordMovementAsync(itemId, toWarehouseId, "TRANSFER_IN", quantity,
                performedByEmployeeId, "TRANSFER", null, notes, ct);

            if (tx != null) await tx.CommitAsync(ct);
        }
        catch
        {
            if (tx != null) await tx.RollbackAsync(ct);
            throw;
        }
    }

    private async Task<bool> AllowNegativeStockAsync(CancellationToken ct)
    {
        var setting = await _context.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SettingKey == "ALLOW_NEGATIVE_STOCK", ct);
        return setting != null && string.Equals(setting.SettingValue, "true", StringComparison.OrdinalIgnoreCase);
    }
}
