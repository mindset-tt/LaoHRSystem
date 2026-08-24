using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class FiscalYearDto
{
    public int FiscalYearId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class FiscalPeriodDto
{
    public int FiscalPeriodId { get; set; }
    public int FiscalYearId { get; set; }
    public int PeriodNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateFiscalYearRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class CreateFiscalPeriodRequest
{
    public int FiscalYearId { get; set; }
    public int PeriodNumber { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

[Authorize]
[ApiController]
[Route("api/fiscal-periods")]
public class FiscalPeriodsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;

    public FiscalPeriodsController(LaoHRDbContext context, IFinanceAccessService access)
    {
        _context = context;
        _access = access;
    }

    [HttpGet("years")]
    public async Task<ActionResult<List<FiscalYearDto>>> GetYears()
    {
        if (!_access.CanViewAccounting())
            return Forbid();

        return await _context.FiscalYears.AsNoTracking()
            .OrderByDescending(y => y.StartDate)
            .Select(y => new FiscalYearDto
            {
                FiscalYearId = y.FiscalYearId,
                Name = y.Name,
                StartDate = y.StartDate,
                EndDate = y.EndDate,
                Status = y.Status,
            })
            .ToListAsync();
    }

    [HttpPost("years")]
    public async Task<ActionResult<FiscalYear>> CreateYear([FromBody] CreateFiscalYearRequest request)
    {
        if (!_access.CanManagePeriods())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");
        if (request.EndDate <= request.StartDate)
            return BadRequest("End date must be after start date.");

        var year = new FiscalYear
        {
            Name = request.Name,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "OPEN",
        };
        _context.FiscalYears.Add(year);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetYears), new { }, year);
    }

    [HttpGet]
    public async Task<ActionResult<List<FiscalPeriodDto>>> GetPeriods([FromQuery] int? fiscalYearId = null)
    {
        if (!_access.CanViewAccounting())
            return Forbid();

        var query = _context.FiscalPeriods.AsNoTracking().AsQueryable();
        if (fiscalYearId.HasValue)
            query = query.Where(p => p.FiscalYearId == fiscalYearId.Value);

        return await query
            .OrderBy(p => p.FiscalYearId).ThenBy(p => p.PeriodNumber)
            .Select(p => new FiscalPeriodDto
            {
                FiscalPeriodId = p.FiscalPeriodId,
                FiscalYearId = p.FiscalYearId,
                PeriodNumber = p.PeriodNumber,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Status = p.Status,
            })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<FiscalPeriod>> CreatePeriod([FromBody] CreateFiscalPeriodRequest request)
    {
        if (!_access.CanManagePeriods())
            return Forbid();

        if (request.EndDate <= request.StartDate)
            return BadRequest("End date must be after start date.");

        var period = new FiscalPeriod
        {
            FiscalYearId = request.FiscalYearId,
            PeriodNumber = request.PeriodNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = "OPEN",
        };
        _context.FiscalPeriods.Add(period);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPeriods), new { }, period);
    }

    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> ClosePeriod(int id)
    {
        if (!_access.CanManagePeriods())
            return Forbid();

        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.FiscalPeriodId == id);
        if (period == null) return NotFound();

        period.Status = "CLOSED";
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/lock")]
    public async Task<IActionResult> LockPeriod(int id)
    {
        if (!_access.CanManagePeriods())
            return Forbid();

        var period = await _context.FiscalPeriods.FirstOrDefaultAsync(p => p.FiscalPeriodId == id);
        if (period == null) return NotFound();

        period.Status = "LOCKED";
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
