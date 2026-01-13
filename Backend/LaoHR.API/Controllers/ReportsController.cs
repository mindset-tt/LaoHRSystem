using Microsoft.AspNetCore.Mvc;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly NssfReportService _nssfService;
    private readonly PdfFormService _pdfFormService;

    public ReportsController(NssfReportService nssfService, PdfFormService pdfFormService)
    {
        _nssfService = nssfService;
        _pdfFormService = pdfFormService;
    }

    [HttpGet("nssf/{periodId}")]
    public async Task<IActionResult> DownloadNssfReport(int periodId)
    {
        try
        {
            // Use the new Form Filling Service
            var pdfBytes = await _pdfFormService.FillNssfForm(periodId);
            return File(pdfBytes, "application/pdf", $"NSSF_Payment_Form_{periodId}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
