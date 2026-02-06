using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly NssfReportService _nssfService;
    private readonly PdfFormService _pdfFormService;
    private readonly LaoHR.Shared.Data.LaoHRDbContext _context;

    public ReportsController(NssfReportService nssfService, PdfFormService pdfFormService, LaoHR.Shared.Data.LaoHRDbContext context)
    {
        _nssfService = nssfService;
        _pdfFormService = pdfFormService;
        _context = context;
    }

    [HttpGet("nssf/{periodId}")]
    public async Task<IActionResult> DownloadNssfReport(int periodId)
    {
        try
        {
            var period = await _context.PayrollPeriods.FindAsync(periodId);
            var company = await _context.CompanySettings.FirstOrDefaultAsync();
            
            var companyName = company?.CompanyNameEn?.Replace(" ", "_") ?? "Company";
            var periodStr = period != null ? $"{period.Month:D2}_{period.Year}" : $"{periodId}";

            // Use the new Form Filling Service
            var pdfBytes = await _pdfFormService.FillNssfForm(periodId);
            return File(pdfBytes, "application/pdf", $"NSSF_Payment_Form_{companyName}_{periodStr}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("nssf/zip/{periodId}")]
    public async Task<IActionResult> DownloadNssfPackage(int periodId)
    {
        try
        {
            var period = await _context.PayrollPeriods.FindAsync(periodId);
            var company = await _context.CompanySettings.FirstOrDefaultAsync();
            
            var companyName = company?.CompanyNameEn?.Replace(" ", "_") ?? "Company";
            var periodStr = period != null ? $"{period.Month:D2}_{period.Year}" : $"{periodId}";

            var pdfFormBytes = await _pdfFormService.FillNssfForm(periodId);
            var nssfReportBytes = await _nssfService.GenerateNssfReport(periodId);

            using var memoryStream = new MemoryStream();
            using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true))
            {
                // Add NSSF Payment Form
                var formEntry = archive.CreateEntry($"NSSF_Payment_Form_{companyName}_{periodStr}.pdf");
                using (var entryStream = formEntry.Open())
                {
                    await entryStream.WriteAsync(pdfFormBytes, 0, pdfFormBytes.Length);
                }

                // Add NSSF Report
                var reportEntry = archive.CreateEntry($"NSSF_Report_{companyName}_{periodStr}.pdf");
                using (var entryStream = reportEntry.Open())
                {
                    await entryStream.WriteAsync(nssfReportBytes, 0, nssfReportBytes.Length);
                }
            }

            memoryStream.Position = 0;
            return File(memoryStream.ToArray(), "application/zip", $"NSSF_Package_{companyName}_{periodStr}.zip");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
