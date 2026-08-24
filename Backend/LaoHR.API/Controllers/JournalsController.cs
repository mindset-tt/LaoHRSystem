using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class JournalLineDto
{
    public int JournalLineId { get; set; }
    public int AccountId { get; set; }
    public string? AccountCode { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int? CostCenterId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
}

public class JournalEntryDto
{
    public int JournalEntryId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public DateTime PostingDate { get; set; }
    public int FiscalPeriodId { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public int? SourceId { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = "LAK";
    public int? ReversesJournalEntryId { get; set; }
    public List<JournalLineDto> Lines { get; set; } = new();
}

public class CreateJournalLineRequest
{
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public int? CostCenterId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
}

public class CreateJournalEntryRequest
{
    public DateTime PostingDate { get; set; }
    public int FiscalPeriodId { get; set; }
    public string SourceType { get; set; } = "MANUAL_JOURNAL";
    public int? SourceId { get; set; }
    public string? Description { get; set; }
    public string Currency { get; set; } = "LAK";
    public List<CreateJournalLineRequest> Lines { get; set; } = new();
}

[Authorize]
[ApiController]
[Route("api/journals")]
public class JournalsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;
    private readonly IAccountingService _accounting;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly INumberSequenceService _numbers;
    private readonly ISegregationOfDutiesService _sod;

    public JournalsController(
        LaoHRDbContext context,
        IFinanceAccessService access,
        IAccountingService accounting,
        ICurrentEmployeeService currentEmployee,
        INumberSequenceService numbers,
        ISegregationOfDutiesService sod)
    {
        _context = context;
        _access = access;
        _accounting = accounting;
        _currentEmployee = currentEmployee;
        _numbers = numbers;
        _sod = sod;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<JournalEntryDto>>> GetJournals(
        [FromQuery] string? status = null,
        [FromQuery] int? fiscalPeriodId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewAccounting())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.JournalEntries.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(j => j.Status == status);
        if (fiscalPeriodId.HasValue)
            query = query.Where(j => j.FiscalPeriodId == fiscalPeriodId.Value);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(j => j.PostingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(j => new JournalEntryDto
            {
                JournalEntryId = j.JournalEntryId,
                JournalNumber = j.JournalNumber,
                PostingDate = j.PostingDate,
                FiscalPeriodId = j.FiscalPeriodId,
                SourceType = j.SourceType,
                SourceId = j.SourceId,
                Description = j.Description,
                Status = j.Status,
                Currency = j.Currency,
                ReversesJournalEntryId = j.ReversesJournalEntryId,
            })
            .ToListAsync();

        return new PaginatedResponse<JournalEntryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JournalEntryDto>> GetJournal(int id)
    {
        if (!_access.CanViewAccounting())
            return Forbid();

        var j = await _context.JournalEntries.AsNoTracking()
            .FirstOrDefaultAsync(x => x.JournalEntryId == id);
        if (j == null) return NotFound();

        var lines = await _context.JournalLines.AsNoTracking()
            .Where(l => l.JournalEntryId == id)
            .Select(l => new JournalLineDto
            {
                JournalLineId = l.JournalLineId,
                AccountId = l.AccountId,
                AccountCode = l.Account != null ? l.Account.AccountCode : null,
                Description = l.Description,
                Debit = l.Debit,
                Credit = l.Credit,
                CostCenterId = l.CostCenterId,
                DepartmentId = l.DepartmentId,
                ProjectId = l.ProjectId,
            })
            .ToListAsync();

        return new JournalEntryDto
        {
            JournalEntryId = j.JournalEntryId,
            JournalNumber = j.JournalNumber,
            PostingDate = j.PostingDate,
            FiscalPeriodId = j.FiscalPeriodId,
            SourceType = j.SourceType,
            SourceId = j.SourceId,
            Description = j.Description,
            Status = j.Status,
            Currency = j.Currency,
            ReversesJournalEntryId = j.ReversesJournalEntryId,
            Lines = lines,
        };
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntry>> CreateJournal([FromBody] CreateJournalEntryRequest request)
    {
        if (!_access.CanPostAccounting())
            return Forbid();

        if (request.Lines == null || request.Lines.Count == 0)
            return BadRequest("At least one line is required.");

        var number = await _numbers.NextAsync("JE");

        var entry = new JournalEntry
        {
            JournalNumber = number,
            PostingDate = request.PostingDate,
            FiscalPeriodId = request.FiscalPeriodId,
            SourceType = request.SourceType,
            SourceId = request.SourceId,
            Description = request.Description,
            Status = "DRAFT",
            Currency = request.Currency,
            CreatedByEmployeeId = _currentEmployee.GetCurrentEmployeeId(),
        };

        foreach (var line in request.Lines)
        {
            entry.Lines.Add(new JournalLine
            {
                AccountId = line.AccountId,
                Description = line.Description,
                Debit = line.Debit,
                Credit = line.Credit,
                CostCenterId = line.CostCenterId,
                DepartmentId = line.DepartmentId,
                ProjectId = line.ProjectId,
            });
        }

        _context.JournalEntries.Add(entry);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetJournal), new { id = entry.JournalEntryId }, entry);
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> Post(int id)
    {
        if (!_access.CanPostAccounting())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            await _sod.EnforceJournalPostAsync(id, actor.Value);
            await _accounting.PostAsync(id, actor.Value);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id:int}/reverse")]
    public async Task<IActionResult> Reverse(int id)
    {
        if (!_access.CanPostAccounting())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            var reversal = await _accounting.ReverseAsync(id, actor.Value);
            return Ok(reversal);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpGet("trial-balance")]
    public async Task<ActionResult<List<TrialBalanceRow>>> GetTrialBalance([FromQuery] int fiscalPeriodId)
    {
        if (!_access.CanViewAccounting())
            return Forbid();

        return await _accounting.GetTrialBalanceAsync(fiscalPeriodId);
    }

    [HttpGet("trial-balance/export")]
    public async Task<IActionResult> ExportTrialBalance([FromQuery] int fiscalPeriodId)
    {
        if (!_access.CanExportFinance())
            return Forbid();

        var rows = await _accounting.GetTrialBalanceAsync(fiscalPeriodId);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("AccountCode,AccountName,AccountType,Debit,Credit,Balance");
        foreach (var r in rows)
        {
            sb.AppendLine($"{EscapeCsv(r.AccountCode)},{EscapeCsv(r.AccountName)},{EscapeCsv(r.AccountType)},{r.Debit},{r.Credit},{r.Balance}");
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"trial-balance-{fiscalPeriodId}.csv");
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
