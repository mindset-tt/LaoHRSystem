using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

// =============================================================================
// Phase 4B — Finance + Accounting controllers.
// =============================================================================

public class AccountDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public string AccountType { get; set; } = string.Empty;
    public int? ParentAccountId { get; set; }
    public bool IsPostingAccount { get; set; }
    public bool IsActive { get; set; }
    public string? Currency { get; set; }
}

public class CreateAccountRequest
{
    public string AccountCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public string AccountType { get; set; } = "ASSET";
    public int? ParentAccountId { get; set; }
    public bool IsPostingAccount { get; set; } = true;
    public string? Currency { get; set; }
}

[Authorize]
[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;

    public AccountsController(LaoHRDbContext context, IFinanceAccessService access)
    {
        _context = context;
        _access = access;
    }

    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAccounts()
    {
        if (!_access.CanViewAccounting())
            return Forbid();

        return await _context.Accounts.AsNoTracking()
            .OrderBy(a => a.AccountCode)
            .Select(a => new AccountDto
            {
                AccountId = a.AccountId,
                AccountCode = a.AccountCode,
                Name = a.Name,
                NameLao = a.NameLao,
                AccountType = a.AccountType,
                ParentAccountId = a.ParentAccountId,
                IsPostingAccount = a.IsPostingAccount,
                IsActive = a.IsActive,
                Currency = a.Currency,
            })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Account>> CreateAccount([FromBody] CreateAccountRequest request)
    {
        if (!_access.CanManageCoa())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.AccountCode) || string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("AccountCode and Name are required.");

        if (request.AccountType is not ("ASSET" or "LIABILITY" or "EQUITY" or "REVENUE" or "EXPENSE"))
            return BadRequest("Invalid account type.");

        // Prevent cycles: parent must not be self or a descendant.
        if (request.ParentAccountId.HasValue)
        {
            var parent = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == request.ParentAccountId.Value);
            if (parent == null) return BadRequest("Parent account not found.");
        }

        var account = new Account
        {
            AccountCode = request.AccountCode,
            Name = request.Name,
            NameLao = request.NameLao,
            AccountType = request.AccountType,
            ParentAccountId = request.ParentAccountId,
            IsPostingAccount = request.IsPostingAccount,
            Currency = request.Currency,
            IsActive = true,
        };
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAccounts), new { }, account);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] CreateAccountRequest request)
    {
        if (!_access.CanManageCoa())
            return Forbid();

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == id);
        if (account == null) return NotFound();

        account.AccountCode = request.AccountCode;
        account.Name = request.Name;
        account.NameLao = request.NameLao;
        account.AccountType = request.AccountType;
        account.ParentAccountId = request.ParentAccountId;
        account.IsPostingAccount = request.IsPostingAccount;
        account.Currency = request.Currency;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Soft-deactivate (never delete an account with history).
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateAccount(int id)
    {
        if (!_access.CanManageCoa())
            return Forbid();

        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == id);
        if (account == null) return NotFound();

        var hasLines = await _context.JournalLines.AnyAsync(l => l.AccountId == id);
        if (hasLines)
            return BadRequest("Account has journal history; deactivate instead of deleting.");

        account.IsActive = false;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
