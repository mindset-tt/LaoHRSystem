using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly PayrollService _payrollService;
    private readonly PayslipPdfService _pdfService;
    
    public PayrollController(LaoHRDbContext context, PayrollService payrollService, PayslipPdfService pdfService)
    {
        _context = context;
        _payrollService = payrollService;
        _pdfService = pdfService;
    }
    
    /// <summary>
    /// Get all payroll periods
    /// </summary>
    [HttpGet("periods")]
    public async Task<ActionResult<IEnumerable<PayrollPeriod>>> GetPeriods()
    {
        return await _context.PayrollPeriods
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .ToListAsync();
    }
    
    /// <summary>
    /// Create new payroll period
    /// </summary>
    [HttpPost("periods")]
    public async Task<ActionResult<PayrollPeriod>> CreatePeriod(CreatePeriodRequest request)
    {
        var exists = await _context.PayrollPeriods
            .AnyAsync(p => p.Year == request.Year && p.Month == request.Month);
        
        if (exists)
            return BadRequest("Period already exists");
        
        var startDate = new DateTime(request.Year, request.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        
        var period = new PayrollPeriod
        {
            Year = request.Year,
            Month = request.Month,
            PeriodName = $"{startDate:MMMM yyyy}",
            StartDate = startDate,
            EndDate = endDate,
            Status = "DRAFT"
        };
        
        _context.PayrollPeriods.Add(period);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetPeriods), period);
    }
    
    /// <summary>
    /// Run payroll calculation for a period
    /// </summary>
    [HttpPost("periods/{periodId}/run")]
    public async Task<ActionResult<IEnumerable<SalarySlip>>> RunPayroll(int periodId)
    {
        try
        {
            var slips = await _payrollService.ProcessPayrollAsync(periodId);
            return Ok(slips);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    /// <summary>
    /// Get salary slips for a period
    /// </summary>
    [HttpGet("periods/{periodId}/slips")]
    public async Task<ActionResult<IEnumerable<SalarySlip>>> GetSlips(int periodId)
    {
        var slips = await _context.SalarySlips
            .Include(s => s.Employee)
            .Where(s => s.PeriodId == periodId)
            .ToListAsync();
        
        return Ok(slips);
    }
    
    /// <summary>
    /// Get single salary slip
    /// </summary>
    [HttpGet("slips/{slipId}")]
    public async Task<ActionResult<SalarySlip>> GetSlip(int slipId)
    {
        var slip = await _context.SalarySlips
            .Include(s => s.Employee)
            .Include(s => s.PayrollPeriod)
            .FirstOrDefaultAsync(s => s.SlipId == slipId);
        
        if (slip == null) return NotFound();
        return Ok(slip);
    }
    
    /// <summary>
    /// Download payslip as PDF
    /// </summary>
    [HttpGet("slips/{slipId}/pdf")]
    public async Task<IActionResult> DownloadPayslipPdf(int slipId)
    {
        var slip = await _context.SalarySlips
            .Include(s => s.Employee)
                .ThenInclude(e => e.Department)
            .Include(s => s.PayrollPeriod)
            .FirstOrDefaultAsync(s => s.SlipId == slipId);
        
        if (slip == null) return NotFound();
        
        var pdfBytes = _pdfService.GeneratePayslip(slip, slip.Employee, slip.PayrollPeriod);
        var fileName = $"Payslip_{slip.Employee.EmployeeCode}_{slip.PayrollPeriod.Year}_{slip.PayrollPeriod.Month:D2}.pdf";
        
        return File(pdfBytes, "application/pdf", fileName);
    }
    
    /// <summary>
    /// Calculate salary preview (without saving)
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<SalaryCalculation>> Calculate(CalculateRequest request)
    {
        await _payrollService.LoadSettingsAsync();
        var result = _payrollService.CalculateSalary(
            request.BaseSalary, 
            request.OvertimePay, 
            request.Allowances, 
            request.OtherDeductions);
        return Ok(result);
    }
}

public class CreatePeriodRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
}

public class CalculateRequest
{
    public decimal BaseSalary { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal Allowances { get; set; }
    public decimal OtherDeductions { get; set; }
}
