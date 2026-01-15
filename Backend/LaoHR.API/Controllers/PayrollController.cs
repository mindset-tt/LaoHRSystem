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
    public async Task<ActionResult<PayrollPeriod>> CreatePeriod([FromBody] CreatePeriodRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var exists = await _context.PayrollPeriods
            .AnyAsync(p => p.Year == request.Year && p.Month == request.Month);
        
        if (exists)
            return BadRequest(new { message = $"Period {request.Month}/{request.Year} already exists" });
        
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
    
    /// <summary>
    /// Export payroll period to Excel
    /// </summary>
    [HttpGet("periods/{periodId}/export")]
    public async Task<IActionResult> ExportPayroll(int periodId)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId);
        if (period == null) return NotFound("Period not found");

        var slips = await _context.SalarySlips
            .Include(s => s.Employee)
            .ThenInclude(e => e.Department)
            .Where(s => s.PeriodId == periodId)
            .OrderBy(s => s.Employee.EmployeeCode)
            .ToListAsync();

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Payroll");

        // Headers
        worksheet.Cell(1, 1).Value = "Employee Code";
        worksheet.Cell(1, 2).Value = "Name (Lao)";
        worksheet.Cell(1, 3).Value = "Name (English)";
        worksheet.Cell(1, 4).Value = "Department";
        worksheet.Cell(1, 5).Value = "Base Salary";
        worksheet.Cell(1, 6).Value = "Contract Currency";
        worksheet.Cell(1, 7).Value = "Exchange Rate";
        worksheet.Cell(1, 8).Value = "Base Salary (LAK)";
        worksheet.Cell(1, 9).Value = "Overtime";
        worksheet.Cell(1, 10).Value = "Allowances";
        worksheet.Cell(1, 11).Value = "Gross Income";
        worksheet.Cell(1, 12).Value = "NSSF (Emp)";
        worksheet.Cell(1, 13).Value = "Tax";
        worksheet.Cell(1, 14).Value = "Other Deductions";
        worksheet.Cell(1, 15).Value = "Net Salary (LAK)";
        worksheet.Cell(1, 16).Value = "Net Salary (Original)";
        worksheet.Cell(1, 17).Value = "Payment Currency";
        worksheet.Cell(1, 18).Value = "Status";

        // Style Header
        var headerRange = worksheet.Range(1, 1, 1, 18);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;

        // Data
        int row = 2;
        foreach (var slip in slips)
        {
            worksheet.Cell(row, 1).Value = slip.Employee.EmployeeCode;
            worksheet.Cell(row, 2).Value = slip.Employee.LaoName;
            worksheet.Cell(row, 3).Value = slip.Employee.EnglishName;
            worksheet.Cell(row, 4).Value = slip.Employee.Department?.DepartmentName ?? "-";
            worksheet.Cell(row, 5).Value = slip.BaseSalaryOriginal;
            worksheet.Cell(row, 6).Value = slip.ContractCurrency ?? "LAK";
            worksheet.Cell(row, 7).Value = slip.ExchangeRateUsed;
            worksheet.Cell(row, 8).Value = slip.BaseSalary;
            worksheet.Cell(row, 9).Value = slip.OvertimePay;
            worksheet.Cell(row, 10).Value = slip.Allowances;
            worksheet.Cell(row, 11).Value = slip.GrossIncome;
            worksheet.Cell(row, 12).Value = slip.NssfEmployeeDeduction;
            worksheet.Cell(row, 13).Value = slip.TaxDeduction;
            worksheet.Cell(row, 14).Value = slip.OtherDeductions;
            worksheet.Cell(row, 15).Value = slip.NetSalary;
            worksheet.Cell(row, 16).Value = slip.NetSalaryOriginal;
            worksheet.Cell(row, 17).Value = slip.PaymentCurrency ?? "LAK";
            worksheet.Cell(row, 18).Value = slip.Status;
            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Payroll_{period.Year}_{period.Month:D2}.xlsx");
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
