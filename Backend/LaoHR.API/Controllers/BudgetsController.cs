using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class BudgetDto
{
    public int BudgetId { get; set; }
    public int FiscalYear { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? ProjectId { get; set; }
    public int? CostCenterId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Currency { get; set; } = "LAK";
    public decimal ApprovedAmount { get; set; }
    public decimal ReservedAmount { get; set; }
    public decimal CommittedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateBudgetRequest
{
    public int FiscalYear { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
    public int? CostCenterId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Currency { get; set; } = "LAK";
    public decimal ApprovedAmount { get; set; }
}

public class CostCenterDto
{
    public int CostCenterId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public int? DepartmentId { get; set; }
    public string Status { get; set; } = string.Empty;
}

[Authorize]
[ApiController]
[Route("api/budgets")]
public class BudgetsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;

    public BudgetsController(LaoHRDbContext context, IFinanceAccessService access)
    {
        _context = context;
        _access = access;
    }

    [HttpGet]
    public async Task<ActionResult<List<BudgetDto>>> GetBudgets([FromQuery] int? fiscalYear = null)
    {
        if (!_access.CanViewFinance())
            return Forbid();

        var query = _context.Budgets.AsNoTracking().AsQueryable();
        if (fiscalYear.HasValue)
            query = query.Where(b => b.FiscalYear == fiscalYear.Value);

        var budgets = await query
            .OrderByDescending(b => b.FiscalYear).ThenBy(b => b.Category)
            .ToListAsync();

        var result = new List<BudgetDto>();
        foreach (var b in budgets)
        {
            var deptName = b.DepartmentId.HasValue
                ? await _context.Departments.Where(d => d.DepartmentId == b.DepartmentId.Value)
                    .Select(d => d.DepartmentName).FirstOrDefaultAsync()
                : null;

            result.Add(new BudgetDto
            {
                BudgetId = b.BudgetId,
                FiscalYear = b.FiscalYear,
                DepartmentId = b.DepartmentId,
                DepartmentName = deptName,
                ProjectId = b.ProjectId,
                CostCenterId = b.CostCenterId,
                Category = b.Category,
                Currency = b.Currency,
                ApprovedAmount = b.ApprovedAmount,
                ReservedAmount = b.ReservedAmount,
                CommittedAmount = b.CommittedAmount,
                ActualAmount = b.ActualAmount,
                AvailableAmount = b.ApprovedAmount - b.ReservedAmount - b.CommittedAmount - b.ActualAmount,
                Status = b.Status,
            });
        }

        return result;
    }

    [HttpPost]
    public async Task<ActionResult<Budget>> CreateBudget([FromBody] CreateBudgetRequest request)
    {
        if (!_access.CanManageFinance())
            return Forbid();

        if (request.FiscalYear <= 0)
            return BadRequest("Fiscal year is required.");
        if (string.IsNullOrWhiteSpace(request.Category))
            return BadRequest("Category is required.");
        if (request.ApprovedAmount < 0)
            return BadRequest("Approved amount cannot be negative.");

        var budget = new Budget
        {
            FiscalYear = request.FiscalYear,
            DepartmentId = request.DepartmentId,
            ProjectId = request.ProjectId,
            CostCenterId = request.CostCenterId,
            Category = request.Category,
            Currency = request.Currency,
            ApprovedAmount = request.ApprovedAmount,
            Status = "APPROVED",
        };
        _context.Budgets.Add(budget);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBudgets), new { }, budget);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBudget(int id, [FromBody] CreateBudgetRequest request)
    {
        if (!_access.CanManageFinance())
            return Forbid();

        var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == id);
        if (budget == null) return NotFound();

        budget.FiscalYear = request.FiscalYear;
        budget.DepartmentId = request.DepartmentId;
        budget.ProjectId = request.ProjectId;
        budget.CostCenterId = request.CostCenterId;
        budget.Category = request.Category;
        budget.Currency = request.Currency;
        budget.ApprovedAmount = request.ApprovedAmount;
        budget.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // ---- Cost centers ----
    [HttpGet("cost-centers")]
    public async Task<ActionResult<List<CostCenterDto>>> GetCostCenters()
    {
        if (!_access.CanViewFinance())
            return Forbid();

        return await _context.CostCenters.AsNoTracking()
            .OrderBy(c => c.Code)
            .Select(c => new CostCenterDto
            {
                CostCenterId = c.CostCenterId,
                Code = c.Code,
                Name = c.Name,
                NameLao = c.NameLao,
                DepartmentId = c.DepartmentId,
                Status = c.Status,
            })
            .ToListAsync();
    }

    [HttpPost("cost-centers")]
    public async Task<ActionResult<CostCenter>> CreateCostCenter([FromBody] CostCenterDto request)
    {
        if (!_access.CanManageFinance())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Code and Name are required.");

        var cc = new CostCenter
        {
            Code = request.Code,
            Name = request.Name,
            NameLao = request.NameLao,
            DepartmentId = request.DepartmentId,
            Status = "ACTIVE",
        };
        _context.CostCenters.Add(cc);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCostCenters), new { }, cc);
    }
}
