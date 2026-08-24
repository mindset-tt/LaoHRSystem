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
            // Phase 4D.1 — was an N+1 loop (2 SUM queries per item, ~400 round
            // trips for 200 items, measured p50 ~767 ms). Now a single grouped
            // aggregate; EXPLAIN ANALYZE shows ~11 ms total.
            var tracked = await _context.InventoryItems
                .Where(i => i.TrackInventory)
                .Select(i => new { i.InventoryItemId, i.ReorderLevel })
                .ToListAsync();
            var onHand = await CountOnHandByItemAsync();
            dto.LowStockItems = tracked.Count(i =>
                i.ReorderLevel != null && onHand.GetValueOrDefault(i.InventoryItemId) <= i.ReorderLevel.Value);
            dto.OutOfStockItems = tracked.Count(i => onHand.GetValueOrDefault(i.InventoryItemId) <= 0m);
        }

        return dto;
    }

    /// <summary>
    /// Phase 4D.1 — net on-hand quantity per tracked inventory item in ONE
    /// query (replaces the per-item SUM round trips). IN-type movements add,
    /// OUT-type movements subtract, unknown types count zero (matches the
    /// previous per-item semantics exactly).
    /// </summary>
    private async Task<Dictionary<int, decimal>> CountOnHandByItemAsync()
    {
        var rows = await _context.InventoryItems
            .Where(i => i.TrackInventory)
            .Select(i => new
            {
                i.InventoryItemId,
                OnHand = _context.StockMovements
                    .Where(m => m.ItemId == i.InventoryItemId)
                    .Sum(m =>
                        (m.MovementType == "RECEIPT" || m.MovementType == "TRANSFER_IN" ||
                         m.MovementType == "ADJUSTMENT_IN" || m.MovementType == "RETURN")
                            ? m.Quantity
                        : (m.MovementType == "ISSUE" || m.MovementType == "TRANSFER_OUT" ||
                           m.MovementType == "ADJUSTMENT_OUT")
                            ? -m.Quantity
                        : 0m)
            })
            .ToListAsync();

        return rows.ToDictionary(r => r.InventoryItemId, r => r.OnHand);
    }
}
