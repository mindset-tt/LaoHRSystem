using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class AssetListItem
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? SerialNumber { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public string? Currency { get; set; }
    public int? CustodianEmployeeId { get; set; }
    public string? CustodianName { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class AssetDetail
{
    public int AssetId { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? SerialNumber { get; set; }
    public int? PurchaseOrderItemId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public string? Currency { get; set; }
    public int? WorkLocationId { get; set; }
    public int? CustodianEmployeeId { get; set; }
    public string? CustodianName { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<AssetAssignmentDto> Assignments { get; set; } = new();
}

public class AssetAssignmentDto
{
    public int AssetAssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public int AssignedByEmployeeId { get; set; }
    public string? ConditionAtAssignment { get; set; }
    public string? ConditionAtReturn { get; set; }
}

public class CreateAssetRequest
{
    public string Name { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public string? SerialNumber { get; set; }
    public int? PurchaseOrderItemId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public string? Currency { get; set; }
    public int? WorkLocationId { get; set; }
    public int? CustodianEmployeeId { get; set; }
}

public class AssignAssetRequest
{
    public int EmployeeId { get; set; }
    public string? ConditionAtAssignment { get; set; }
}

[Authorize]
[ApiController]
[Route("api/assets")]
public class AssetsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly INumberSequenceService _numbers;

    public AssetsController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        ICurrentEmployeeService currentEmployee,
        INumberSequenceService numbers)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _numbers = numbers;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AssetListItem>>> GetAssets(
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewAssets())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Assets.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(a => a.Name.ToLower().Contains(s) || a.AssetCode.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();
        var items = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AssetListItem
            {
                AssetId = a.AssetId,
                AssetCode = a.AssetCode,
                Name = a.Name,
                CategoryId = a.CategoryId,
                SerialNumber = a.SerialNumber,
                AcquisitionCost = a.AcquisitionCost,
                Currency = a.Currency,
                CustodianEmployeeId = a.CustodianEmployeeId,
                CustodianName = a.Custodian != null ? (a.Custodian.EnglishName ?? a.Custodian.LaoName) : null,
                Status = a.Status,
            })
            .ToListAsync();

        return new PaginatedResponse<AssetListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AssetDetail>> GetAsset(int id)
    {
        if (!_access.CanViewAssets())
            return Forbid();

        var a = await _context.Assets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.AssetId == id);
        if (a == null) return NotFound();

        var assignments = await _context.AssetAssignments.AsNoTracking()
            .Where(x => x.AssetId == id)
            .OrderByDescending(x => x.AssignedAt)
            .Select(x => new AssetAssignmentDto
            {
                AssetAssignmentId = x.AssetAssignmentId,
                EmployeeId = x.EmployeeId,
                EmployeeName = x.Employee != null ? (x.Employee.EnglishName ?? x.Employee.LaoName) : null,
                AssignedAt = x.AssignedAt,
                ReturnedAt = x.ReturnedAt,
                AssignedByEmployeeId = x.AssignedByEmployeeId,
                ConditionAtAssignment = x.ConditionAtAssignment,
                ConditionAtReturn = x.ConditionAtReturn,
            })
            .ToListAsync();

        var custodianName = a.CustodianEmployeeId.HasValue
            ? await _context.Employees
                .Where(e => e.EmployeeId == a.CustodianEmployeeId.Value)
                .Select(e => e.EnglishName ?? e.LaoName)
                .FirstOrDefaultAsync()
            : null;

        return new AssetDetail
        {
            AssetId = a.AssetId,
            AssetCode = a.AssetCode,
            Name = a.Name,
            CategoryId = a.CategoryId,
            SerialNumber = a.SerialNumber,
            PurchaseOrderItemId = a.PurchaseOrderItemId,
            PurchaseDate = a.PurchaseDate,
            AcquisitionCost = a.AcquisitionCost,
            Currency = a.Currency,
            WorkLocationId = a.WorkLocationId,
            CustodianEmployeeId = a.CustodianEmployeeId,
            CustodianName = custodianName,
            Status = a.Status,
            Assignments = assignments,
        };
    }

    [HttpPost]
    public async Task<ActionResult<Asset>> CreateAsset([FromBody] CreateAssetRequest request)
    {
        if (!_access.CanManageAssets())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var code = await _numbers.NextAsync("AST");

        var asset = new Asset
        {
            AssetCode = code,
            Name = request.Name,
            CategoryId = request.CategoryId,
            SerialNumber = request.SerialNumber,
            PurchaseOrderItemId = request.PurchaseOrderItemId,
            PurchaseDate = request.PurchaseDate,
            AcquisitionCost = request.AcquisitionCost,
            Currency = request.Currency,
            WorkLocationId = request.WorkLocationId,
            CustodianEmployeeId = request.CustodianEmployeeId,
            Status = "AVAILABLE",
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAsset), new { id = asset.AssetId }, asset);
    }

    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> Assign(int id, [FromBody] AssignAssetRequest request)
    {
        if (!_access.CanAssignAssets())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        var asset = await _context.Assets.FirstOrDefaultAsync(x => x.AssetId == id);
        if (asset == null) return NotFound();

        if (asset.Status is "DISPOSED" or "RETIRED")
            return BadRequest($"Cannot assign a {asset.Status} asset.");

        // Invariant: no two active assignments to different employees.
        var activeAssignment = await _context.AssetAssignments
            .AnyAsync(a => a.AssetId == id && a.ReturnedAt == null);
        if (activeAssignment)
            return BadRequest("Asset is already assigned.");

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.EmployeeId == request.EmployeeId);
        if (employee == null) return BadRequest("Employee not found.");

        _context.AssetAssignments.Add(new AssetAssignment
        {
            AssetId = id,
            EmployeeId = request.EmployeeId,
            AssignedAt = DateTime.UtcNow,
            AssignedByEmployeeId = actor.Value,
            ConditionAtAssignment = request.ConditionAtAssignment,
        });

        asset.Status = "ASSIGNED";
        asset.CustodianEmployeeId = request.EmployeeId;
        asset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/return")]
    public async Task<IActionResult> Return(int id, [FromBody] ApproveActionRequest? body = null)
    {
        if (!_access.CanAssignAssets())
            return Forbid();

        var asset = await _context.Assets.FirstOrDefaultAsync(x => x.AssetId == id);
        if (asset == null) return NotFound();

        var assignment = await _context.AssetAssignments
            .Where(a => a.AssetId == id && a.ReturnedAt == null)
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefaultAsync();
        if (assignment == null)
            return BadRequest("No active assignment to return.");

        assignment.ReturnedAt = DateTime.UtcNow;
        assignment.ConditionAtReturn = body?.Notes;

        asset.Status = "AVAILABLE";
        asset.CustodianEmployeeId = null;
        asset.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/dispose")]
    public async Task<IActionResult> Dispose(int id)
    {
        if (!_access.CanManageAssets())
            return Forbid();

        var asset = await _context.Assets.FirstOrDefaultAsync(x => x.AssetId == id);
        if (asset == null) return NotFound();

        var activeAssignment = await _context.AssetAssignments
            .AnyAsync(a => a.AssetId == id && a.ReturnedAt == null);
        if (activeAssignment)
            return BadRequest("Return the asset before disposal.");

        asset.Status = "DISPOSED";
        asset.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
