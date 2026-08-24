using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class ContractDto
{
    public int ContractId { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public string? ContractType { get; set; }
    public int OwnerEmployeeId { get; set; }
    public string? OwnerName { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? RenewalType { get; set; }
    public DateTime? NoticeDate { get; set; }
}

public class CreateContractRequest
{
    public string Title { get; set; } = string.Empty;
    public int? SupplierId { get; set; }
    public string? ContractType { get; set; }
    public int OwnerEmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Amount { get; set; }
    public string? Currency { get; set; }
    public string? RenewalType { get; set; }
    public DateTime? NoticeDate { get; set; }
}

[Authorize]
[ApiController]
[Route("api/contracts")]
public class ContractsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly INumberSequenceService _numbers;
    private readonly IContractLifecycleService _lifecycle;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IApprovalService _approval;
    private readonly INotificationService _notifications;

    public ContractsController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        INumberSequenceService numbers,
        IContractLifecycleService lifecycle,
        ICurrentEmployeeService currentEmployee,
        IApprovalService approval,
        INotificationService notifications)
    {
        _context = context;
        _access = access;
        _numbers = numbers;
        _lifecycle = lifecycle;
        _currentEmployee = currentEmployee;
        _approval = approval;
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ContractDto>>> GetContracts(
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewContracts())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Contracts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(c => c.Status == status);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(s) || c.ContractNumber.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(c => c.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ContractDto
            {
                ContractId = c.ContractId,
                ContractNumber = c.ContractNumber,
                Title = c.Title,
                SupplierId = c.SupplierId,
                SupplierName = c.Supplier != null ? c.Supplier.Name : null,
                ContractType = c.ContractType,
                OwnerEmployeeId = c.OwnerEmployeeId,
                OwnerName = c.Owner != null ? (c.Owner.EnglishName ?? c.Owner.LaoName) : null,
                DepartmentId = c.DepartmentId,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Amount = c.Amount,
                Currency = c.Currency,
                Status = c.Status,
                RenewalType = c.RenewalType,
                NoticeDate = c.NoticeDate,
            })
            .ToListAsync();

        return new PaginatedResponse<ContractDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContractDto>> GetContract(int id)
    {
        if (!_access.CanViewContracts())
            return Forbid();

        var c = await _context.Contracts.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ContractId == id);
        if (c == null) return NotFound();

        var supplierName = c.SupplierId.HasValue
            ? await _context.Suppliers.Where(s => s.SupplierId == c.SupplierId.Value)
                .Select(s => s.Name).FirstOrDefaultAsync()
            : null;
        var ownerName = await _context.Employees
            .Where(e => e.EmployeeId == c.OwnerEmployeeId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        return new ContractDto
        {
            ContractId = c.ContractId,
            ContractNumber = c.ContractNumber,
            Title = c.Title,
            SupplierId = c.SupplierId,
            SupplierName = supplierName,
            ContractType = c.ContractType,
            OwnerEmployeeId = c.OwnerEmployeeId,
            OwnerName = ownerName,
            DepartmentId = c.DepartmentId,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Amount = c.Amount,
            Currency = c.Currency,
            Status = c.Status,
            RenewalType = c.RenewalType,
            NoticeDate = c.NoticeDate,
        };
    }

    [HttpPost]
    public async Task<ActionResult<Contract>> CreateContract([FromBody] CreateContractRequest request)
    {
        if (!_access.CanManageContracts())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required.");

        var number = await _numbers.NextAsync("CTR");

        var contract = new Contract
        {
            ContractNumber = number,
            Title = request.Title,
            SupplierId = request.SupplierId,
            ContractType = request.ContractType,
            OwnerEmployeeId = request.OwnerEmployeeId,
            DepartmentId = request.DepartmentId,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Amount = request.Amount,
            Currency = request.Currency,
            Status = "ACTIVE",
            RenewalType = request.RenewalType,
            NoticeDate = request.NoticeDate,
        };
        _context.Contracts.Add(contract);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetContract), new { id = contract.ContractId }, contract);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateContract(int id, [FromBody] CreateContractRequest request)
    {
        if (!_access.CanManageContracts())
            return Forbid();

        var c = await _context.Contracts.FirstOrDefaultAsync(x => x.ContractId == id);
        if (c == null) return NotFound();

        c.Title = request.Title;
        c.SupplierId = request.SupplierId;
        c.ContractType = request.ContractType;
        c.OwnerEmployeeId = request.OwnerEmployeeId;
        c.DepartmentId = request.DepartmentId;
        c.StartDate = request.StartDate;
        c.EndDate = request.EndDate;
        c.Amount = request.Amount;
        c.Currency = request.Currency;
        c.RenewalType = request.RenewalType;
        c.NoticeDate = request.NoticeDate;
        c.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/terminate")]
    public async Task<IActionResult> Terminate(int id)
    {
        if (!_access.CanManageContracts())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        try
        {
            await _lifecycle.TerminateAsync(id, empId.Value, "Terminated via API");
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id:int}/renew")]
    public async Task<IActionResult> Renew(int id, [FromBody] RenewContractRequest request)
    {
        if (!_access.CanManageContracts())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        try
        {
            await _lifecycle.RenewAsync(id, request.NewStartDate, request.NewEndDate,
                request.NewAmount, empId.Value, request.Notes);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id:int}/submit-approval")]
    public async Task<IActionResult> SubmitApproval(int id)
    {
        if (!_access.CanManageContracts())
            return Forbid();

        var empId = _currentEmployee.GetCurrentEmployeeId();
        if (empId == null) return Unauthorized("No linked employee profile.");

        var c = await _context.Contracts.FirstOrDefaultAsync(x => x.ContractId == id);
        if (c == null) return NotFound();
        if (c.Status != "DRAFT")
            return BadRequest($"Cannot submit a contract in '{c.Status}' state.");

        await _approval.CreateRequestAsync("CONTRACT", id, empId.Value,
            new List<ApprovalStepDefinition>
            {
                new ApprovalStepDefinition { ResolverType = "DIRECT_MANAGER" },
            });

        c.Status = "PENDING_APPROVAL";
        c.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        if (!_access.CanManageContracts())
            return Forbid();

        var c = await _context.Contracts.FirstOrDefaultAsync(x => x.ContractId == id);
        if (c == null) return NotFound();
        if (c.Status != "PENDING_APPROVAL")
            return BadRequest($"Cannot approve a contract in '{c.Status}' state.");

        c.Status = "ACTIVE";
        c.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _notifications.NotifyEmployeeAsync(
            c.OwnerEmployeeId, "CONTRACT_APPROVED", "Contract approved",
            $"Contract {c.ContractNumber} was approved.",
            "CONTRACT", c.ContractId);

        return NoContent();
    }

    [HttpGet("{id:int}/history")]
    public async Task<ActionResult<List<ContractHistory>>> GetHistory(int id)
    {
        if (!_access.CanViewContracts())
            return Forbid();

        return await _context.ContractHistories.AsNoTracking()
            .Where(h => h.ContractId == id)
            .OrderByDescending(h => h.ChangedAt)
            .ToListAsync();
    }
}

public class RenewContractRequest
{
    public DateTime NewStartDate { get; set; }
    public DateTime? NewEndDate { get; set; }
    public decimal? NewAmount { get; set; }
    public string? Notes { get; set; }
}
