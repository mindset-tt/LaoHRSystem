using Microsoft.AspNetCore.Mvc;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BankTransferController : ControllerBase
{
    private readonly IBankTransferService _bankService;

    public BankTransferController(IBankTransferService bankService)
    {
        _bankService = bankService;
    }

    [HttpGet("bcel/{periodId}")]
    public async Task<IActionResult> DownloadBcelFile(int periodId)
    {
        try
        {
            var fileBytes = await _bankService.GenerateBcelTransferFile(periodId);
            return File(fileBytes, "text/plain", $"BCEL_Transfer_{periodId}_{DateTime.Now:yyyyMMdd}.txt");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("ldb/{periodId}")]
    public async Task<IActionResult> DownloadLdbFile(int periodId)
    {
        try
        {
            var fileBytes = await _bankService.GenerateLdbTransferFile(periodId);
            return File(fileBytes, "text/csv", $"LDB_Transfer_{periodId}_{DateTime.Now:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
