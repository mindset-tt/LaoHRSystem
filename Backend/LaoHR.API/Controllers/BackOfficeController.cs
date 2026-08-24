using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 4A.1 — Back Office Command Center. Role-scoped "what needs my attention"
/// metrics. Only metrics backed by current data are returned (no fake values).
/// No payroll/salary/loan/performance data is exposed here.
/// </summary>
public class BackOfficeKpiDto
{
    public int PendingApprovals { get; set; }
    public int OpenPurchaseRequests { get; set; }
    public int PendingPurchaseRequests { get; set; }
    public int OpenPurchaseOrders { get; set; }
    public int PendingReceipts { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public int AssetsInMaintenance { get; set; }
    public int ContractsExpiring { get; set; }
    public int OpenServiceRequests { get; set; }
    public int MyOpenPurchaseRequests { get; set; }
    public int MyOpenServiceRequests { get; set; }
}

[Authorize]
[ApiController]
[Route("api/backoffice")]
public class BackOfficeController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;

    public BackOfficeController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        ICurrentEmployeeService currentEmployee)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
    }

    [HttpGet("command-center")]
    public async Task<ActionResult<BackOfficeKpiDto>> GetCommandCenter()
    {
        var dto = new BackOfficeKpiDto();

        // Self-service metrics (all roles).
        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId.HasValue)
        {
            dto.MyOpenPurchaseRequests = await _context.PurchaseRequests
                .CountAsync(p => p.RequestedByEmployeeId == empId.Value
                                 && (p.Status == "DRAFT" || p.Status == "PENDING_APPROVAL"));
            dto.MyOpenServiceRequests = await _context.ServiceRequests
                .CountAsync(s => s.RequesterEmployeeId == empId.Value
                                 && (s.Status == "OPEN" || s.Status == "IN_PROGRESS"));
        }

        // Privileged metrics (Admin/HR).
        if (_access.IsAdmin() || _access.IsHr())
        {
            dto.PendingApprovals = await _context.ApprovalRequests
                .CountAsync(r => r.Status == "PENDING");
            dto.OpenPurchaseRequests = await _context.PurchaseRequests
                .CountAsync(p => p.Status == "APPROVED");
            dto.PendingPurchaseRequests = await _context.PurchaseRequests
                .CountAsync(p => p.Status == "PENDING_APPROVAL");
            dto.OpenPurchaseOrders = await _context.PurchaseOrders
                .CountAsync(p => p.Status == "SENT" || p.Status == "PARTIALLY_RECEIVED");
            dto.PendingReceipts = await _context.PurchaseOrders
                .CountAsync(p => p.Status == "SENT");
            dto.AssetsInMaintenance = await _context.Assets
                .CountAsync(a => a.Status == "IN_MAINTENANCE");
            dto.OpenServiceRequests = await _context.ServiceRequests
                .CountAsync(s => s.Status == "OPEN" || s.Status == "IN_PROGRESS");

            // Contracts expiring within 30 days.
            var soon = DateTime.UtcNow.AddDays(30);
            dto.ContractsExpiring = await _context.Contracts
                .CountAsync(c => c.Status == "ACTIVE" && c.EndDate != null && c.EndDate <= soon);

            // Low stock / out of stock (derived from ledger).
            dto.LowStockItems = await CountLowStockAsync();
            dto.OutOfStockItems = await CountOutOfStockAsync();
        }

        return dto;
    }

    private async Task<int> CountLowStockAsync()
    {
        var items = await _context.InventoryItems
            .Where(i => i.TrackInventory && i.ReorderLevel != null)
            .Select(i => new { i.InventoryItemId, i.ReorderLevel })
            .ToListAsync();

        var count = 0;
        foreach (var item in items)
        {
            var onHand = await GetTotalOnHandAsync(item.InventoryItemId);
            if (onHand <= item.ReorderLevel) count++;
        }
        return count;
    }

    private async Task<int> CountOutOfStockAsync()
    {
        var items = await _context.InventoryItems
            .Where(i => i.TrackInventory)
            .Select(i => i.InventoryItemId)
            .ToListAsync();

        var count = 0;
        foreach (var itemId in items)
        {
            var onHand = await GetTotalOnHandAsync(itemId);
            if (onHand <= 0) count++;
        }
        return count;
    }

    private async Task<decimal> GetTotalOnHandAsync(int itemId)
    {
        var ins = await _context.StockMovements
            .Where(m => m.ItemId == itemId
                        && (m.MovementType == "RECEIPT" || m.MovementType == "TRANSFER_IN" || m.MovementType == "ADJUSTMENT_IN" || m.MovementType == "RETURN"))
            .SumAsync(m => (decimal?)m.Quantity) ?? 0m;
        var outs = await _context.StockMovements
            .Where(m => m.ItemId == itemId
                        && (m.MovementType == "ISSUE" || m.MovementType == "TRANSFER_OUT" || m.MovementType == "ADJUSTMENT_OUT"))
            .SumAsync(m => (decimal?)m.Quantity) ?? 0m;
        return ins - outs;
    }
}
