using Microsoft.AspNetCore.Mvc;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly NssfReportService _nssfService;

    public ReportsController(NssfReportService nssfService)
    {
        _nssfService = nssfService;
    }

    [HttpGet("nssf/{periodId}")]
    public async Task<IActionResult> DownloadNssfReport(int periodId)
    {
        try
        {
            var pdfBytes = await _nssfService.GenerateNssfReport(periodId);
            return File(pdfBytes, "application/pdf", $"NSSF_Report_{periodId}.pdf");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
