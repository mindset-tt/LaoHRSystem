using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class BankAccountDto
{
    public int BankAccountId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "LAK";
    public string? Branch { get; set; }
    public string? Swift { get; set; }
    public bool IsActive { get; set; }
    public decimal? OpeningBalance { get; set; }
    public int? GLAccountId { get; set; }
}

public class CreateBankAccountRequest
{
    public string BankName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string Currency { get; set; } = "LAK";
    public string? Branch { get; set; }
    public string? Swift { get; set; }
    public decimal? OpeningBalance { get; set; }
    public int? GLAccountId { get; set; }
}

[Authorize]
[ApiController]
[Route("api/bank-accounts")]
public class BankAccountsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;

    public BankAccountsController(LaoHRDbContext context, IFinanceAccessService access)
    {
        _context = context;
        _access = access;
    }

    [HttpGet]
    public async Task<ActionResult<List<BankAccountDto>>> GetBankAccounts()
    {
        if (!_access.CanViewBankAccounts())
            return Forbid();

        // Full account number only for BANK_MANAGE; viewers get a masked value.
        var canSeeFull = _access.CanManageBankAccounts();

        var accounts = await _context.BankAccounts.AsNoTracking()
            .OrderBy(b => b.BankName)
            .ToListAsync();

        return accounts.Select(b => new BankAccountDto
        {
            BankAccountId = b.BankAccountId,
            BankName = b.BankName,
            AccountName = b.AccountName,
            AccountNumber = canSeeFull ? b.AccountNumber : MaskAccountNumber(b.AccountNumber),
            Currency = b.Currency,
            Branch = b.Branch,
            Swift = canSeeFull ? b.Swift : null,
            IsActive = b.IsActive,
            OpeningBalance = canSeeFull ? b.OpeningBalance : null,
            GLAccountId = b.GLAccountId,
        }).ToList();
    }

    private static string MaskAccountNumber(string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length <= 4)
            return "****";
        return "****" + accountNumber.Substring(accountNumber.Length - 4);
    }

    [HttpPost]
    public async Task<ActionResult<BankAccount>> CreateBankAccount([FromBody] CreateBankAccountRequest request)
    {
        if (!_access.CanManageBankAccounts())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.BankName) || string.IsNullOrWhiteSpace(request.AccountNumber))
            return BadRequest("BankName and AccountNumber are required.");

        var account = new BankAccount
        {
            BankName = request.BankName,
            AccountName = request.AccountName,
            AccountNumber = request.AccountNumber,
            Currency = request.Currency,
            Branch = request.Branch,
            Swift = request.Swift,
            OpeningBalance = request.OpeningBalance,
            GLAccountId = request.GLAccountId,
            IsActive = true,
        };
        _context.BankAccounts.Add(account);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetBankAccounts), new { }, account);
    }
}
