using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class ExpenseListItem
{
    public int ExpenseId { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime ExpenseDate { get; set; }
    public string Currency { get; set; } = "LAK";
    public decimal Amount { get; set; }
    public decimal AmountLak { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ExpenseDetail
{
    public int ExpenseId { get; set; }
    public string ExpenseNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public int? PayrollPeriodId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string Currency { get; set; } = "LAK";
    public decimal Amount { get; set; }
    public decimal ExchangeRateUsed { get; set; }
    public decimal AmountLak { get; set; }
    public string? ReceiptPath { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateExpenseRequest
{
    public int EmployeeId { get; set; }
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime ExpenseDate { get; set; }
    public string Currency { get; set; } = "LAK";
    public decimal Amount { get; set; }
    public string? ReceiptPath { get; set; }
}

public class UpdateExpenseRequest
{
    public int? CategoryId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpenseDate { get; set; }
    public string? Currency { get; set; }
    public decimal? Amount { get; set; }
    public string? ReceiptPath { get; set; }
    public string? Status { get; set; }
    public string? ApproverNotes { get; set; }
}

public class ExpenseCategoryItem
{
    public int ExpenseCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public bool RequiresReceipt { get; set; }
    public decimal? DefaultLimit { get; set; }
    public bool IsActive { get; set; }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IApprovalService _approval;
    private readonly INotificationService _notifications;
    private readonly IDataScopeService _scope;
    private readonly IFinanceAccessService _financeAccess;
    private readonly IPostingService _posting;

    public ExpensesController(
        LaoHRDbContext context,
        ICurrentEmployeeService currentEmployee,
        IApprovalService approval,
        INotificationService notifications,
        IDataScopeService scope,
        IFinanceAccessService financeAccess,
        IPostingService posting)
    {
        _context = context;
        _currentEmployee = currentEmployee;
        _approval = approval;
        _notifications = notifications;
        _scope = scope;
        _financeAccess = financeAccess;
        _posting = posting;
    }

    [HttpGet("categories")]
    public async Task<ActionResult<List<ExpenseCategoryItem>>> GetCategories()
    {
        var items = await _context.ExpenseCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new ExpenseCategoryItem
            {
                ExpenseCategoryId = c.ExpenseCategoryId,
                Code = c.Code,
                Name = c.Name,
                NameLao = c.NameLao,
                RequiresReceipt = c.RequiresReceipt,
                DefaultLimit = c.DefaultLimit,
                IsActive = c.IsActive,
            })
            .ToListAsync();
        return items;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<ExpenseListItem>>> GetExpenses(
        [FromQuery] string? status = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] bool mineOnly = false,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        // Phase 3C3 — read-path authorization: intersect with visible scope.
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync();

        var query = _context.Expenses.AsNoTracking()
            .Where(e => visibleIds.Contains(e.EmployeeId))
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(e => e.Status == status);
        if (employeeId.HasValue)
            query = query.Where(e => e.EmployeeId == employeeId.Value);
        if (categoryId.HasValue)
            query = query.Where(e => e.CategoryId == categoryId.Value);
        if (mineOnly)
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId.HasValue)
                query = query.Where(e => e.EmployeeId == currentEmployeeId.Value);
        }
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(e =>
                e.ExpenseNumber.ToLower().Contains(s) ||
                e.Title.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();

        var items = await query
            .OrderByDescending(e => e.ExpenseDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ExpenseListItem
            {
                ExpenseId = e.ExpenseId,
                ExpenseNumber = e.ExpenseNumber,
                EmployeeId = e.EmployeeId,
                EmployeeName = e.Employee != null ? (e.Employee.EnglishName ?? e.Employee.LaoName) : null,
                CategoryId = e.CategoryId,
                CategoryName = e.Category != null ? e.Category.Name : null,
                Title = e.Title,
                ExpenseDate = e.ExpenseDate,
                Currency = e.Currency,
                Amount = e.Amount,
                AmountLak = e.AmountLak,
                Status = e.Status,
                ApproverId = e.ApproverId,
                ApproverName = e.Approver != null ? (e.Approver.EnglishName ?? e.Approver.LaoName) : null,
                CreatedAt = e.CreatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<ExpenseListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseDetail>> GetExpense(int id)
    {
        var e = await _context.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.ExpenseId == id);
        if (e == null) return NotFound();

        // Phase 3C3 — detail IDOR: only visible employees' expenses.
        if (!await _scope.CanViewEmployeeAsync(e.EmployeeId))
            return Forbid();

        var employee = await _context.Employees
            .Where(emp => emp.EmployeeId == e.EmployeeId)
            .Select(emp => emp.EnglishName ?? emp.LaoName)
            .FirstOrDefaultAsync();

        var category = await _context.ExpenseCategories
            .Where(c => c.ExpenseCategoryId == e.CategoryId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync();

        string? approverName = null;
        if (e.ApproverId.HasValue)
        {
            approverName = await _context.Employees
                .Where(emp => emp.EmployeeId == e.ApproverId.Value)
                .Select(emp => emp.EnglishName ?? emp.LaoName)
                .FirstOrDefaultAsync();
        }

        return new ExpenseDetail
        {
            ExpenseId = e.ExpenseId,
            ExpenseNumber = e.ExpenseNumber,
            EmployeeId = e.EmployeeId,
            EmployeeName = employee,
            CategoryId = e.CategoryId,
            CategoryName = category,
            PayrollPeriodId = e.PayrollPeriodId,
            Title = e.Title,
            Description = e.Description,
            ExpenseDate = e.ExpenseDate,
            Currency = e.Currency,
            Amount = e.Amount,
            ExchangeRateUsed = e.ExchangeRateUsed,
            AmountLak = e.AmountLak,
            ReceiptPath = e.ReceiptPath,
            Status = e.Status,
            ApproverId = e.ApproverId,
            ApproverName = approverName,
            ApprovedAt = e.ApprovedAt,
            ApproverNotes = e.ApproverNotes,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
        };
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense([FromBody] CreateExpenseRequest request)
    {
        if (request.Amount <= 0)
            return BadRequest("Amount must be positive");

        // Phase 3C2 — resolve the submitter from the authenticated user (IDOR fix).
        var requesterEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (requesterEmployeeId == null)
            return Unauthorized("No linked employee profile.");

        var (rate, error) = await ResolveExchangeRate(request.Currency);
        if (error != null) return BadRequest(error);

        var nextNumber = await NextExpenseNumber();

        var expense = new Expense
        {
            ExpenseNumber = nextNumber,
            EmployeeId = requesterEmployeeId.Value,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            ExpenseDate = request.ExpenseDate,
            Currency = request.Currency,
            Amount = request.Amount,
            ExchangeRateUsed = rate,
            AmountLak = Math.Round(request.Amount * rate, 2),
            ReceiptPath = request.ReceiptPath,
            Status = "SUBMITTED",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        // Phase 3C2 — create the approval request.
        var requester = await _context.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == requesterEmployeeId.Value);
        var steps = new List<ApprovalStepDefinition>();
        if (requester?.ManagerId != null)
            steps.Add(new ApprovalStepDefinition { ResolverType = "DIRECT_MANAGER" });
        else
            steps.Add(new ApprovalStepDefinition { ResolverType = "ROLE", RoleName = "HR" });
        await _approval.CreateRequestAsync("EXPENSE", expense.ExpenseId, requesterEmployeeId.Value, steps);

        if (requester?.ManagerId != null)
        {
            await _notifications.NotifyEmployeeAsync(
                requester.ManagerId.Value,
                "APPROVAL_REQUESTED",
                "New expense request",
                $"{requester.EnglishName ?? requester.LaoName} submitted an expense of {expense.AmountLak:N0} LAK.",
                "EXPENSE",
                expense.ExpenseId);
        }

        return CreatedAtAction(nameof(GetExpense), new { id = expense.ExpenseId }, expense);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
    {
        var e = await _context.Expenses.FirstOrDefaultAsync(x => x.ExpenseId == id);
        if (e == null) return NotFound();

        // Only the submitter can edit DRAFT/SUBMITTED; afterwards only status transitions remain.
        if (e.Status != "DRAFT" && e.Status != "SUBMITTED")
            return BadRequest($"Cannot edit expense in '{e.Status}' state");

        if (request.CategoryId.HasValue) e.CategoryId = request.CategoryId.Value;
        if (request.Title != null) e.Title = request.Title;
        if (request.Description != null) e.Description = request.Description;
        if (request.ExpenseDate.HasValue) e.ExpenseDate = request.ExpenseDate.Value;
        if (request.Currency != null)
        {
            e.Currency = request.Currency;
            var (rate, _) = await ResolveExchangeRate(request.Currency);
            e.ExchangeRateUsed = rate;
        }
        if (request.Amount.HasValue)
        {
            e.Amount = request.Amount.Value;
            e.AmountLak = Math.Round(request.Amount.Value * e.ExchangeRateUsed, 2);
        }
        if (request.ReceiptPath != null) e.ReceiptPath = request.ReceiptPath;

        if (request.Status != null && IsValidStatusTransition(e.Status, request.Status))
        {
            e.Status = request.Status;
            if (request.Status == "APPROVED" || request.Status == "REJECTED")
            {
                e.ApproverId = GetCurrentEmployeeId();
                e.ApprovedAt = DateTime.UtcNow;
            }
            if (request.ApproverNotes != null) e.ApproverNotes = request.ApproverNotes;
        }

        e.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveExpense(int id, [FromBody] ApproveActionRequest? body = null)
    {
        return await SetApprovalState(id, "APPROVED", body?.Notes);
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> RejectExpense(int id, [FromBody] ApproveActionRequest? body = null)
    {
        return await SetApprovalState(id, "REJECTED", body?.Notes);
    }

    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> MarkPaid(int id)
    {
        var e = await _context.Expenses.FirstOrDefaultAsync(x => x.ExpenseId == id);
        if (e == null) return NotFound();
        if (e.Status != "APPROVED")
            return BadRequest($"Only APPROVED expenses can be marked paid");
        e.Status = "PAID";
        e.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/post-to-gl")]
    public async Task<IActionResult> PostToGl(int id)
    {
        if (!_financeAccess.CanPostAccounting())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            var entry = await _posting.PostExpenseAsync(id, actor.Value);
            return Ok(entry);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var e = await _context.Expenses.FirstOrDefaultAsync(x => x.ExpenseId == id);
        if (e == null) return NotFound();
        if (e.Status != "DRAFT" && e.Status != "REJECTED")
            return BadRequest("Only DRAFT or REJECTED expenses can be deleted");
        _context.Expenses.Remove(e);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<IActionResult> SetApprovalState(int id, string newStatus, string? notes)
    {
        var actorEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (actorEmployeeId == null)
            return Unauthorized("No linked employee profile.");

        var e = await _context.Expenses.FirstOrDefaultAsync(x => x.ExpenseId == id);
        if (e == null) return NotFound();
        if (e.Status != "SUBMITTED")
            return BadRequest($"Only SUBMITTED expenses can be {newStatus.ToLower()}");

        var approval = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.RequestType == "EXPENSE" && r.EntityId == id && r.Status == "PENDING");
        if (approval == null)
            return BadRequest("No pending approval for this expense.");

        try
        {
            if (newStatus == "APPROVED")
            {
                var result = await _approval.ApproveAsync(approval.ApprovalRequestId, actorEmployeeId.Value, notes);
                if (result.Status == "APPROVED")
                {
                    e.Status = "APPROVED";
                    e.ApproverId = actorEmployeeId.Value;
                    e.ApprovedAt = DateTime.UtcNow;
                    if (notes != null) e.ApproverNotes = notes;
                    e.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await _notifications.NotifyEmployeeAsync(
                        e.EmployeeId,
                        "APPROVAL_APPROVED",
                        "Expense approved",
                        $"Your expense {e.ExpenseNumber} was approved.",
                        "EXPENSE",
                        e.ExpenseId);
                }
            }
            else
            {
                await _approval.RejectAsync(approval.ApprovalRequestId, actorEmployeeId.Value, notes);
                e.Status = "REJECTED";
                e.ApproverId = actorEmployeeId.Value;
                e.ApprovedAt = DateTime.UtcNow;
                if (notes != null) e.ApproverNotes = notes;
                e.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _notifications.NotifyEmployeeAsync(
                    e.EmployeeId,
                    "APPROVAL_REJECTED",
                    "Expense rejected",
                    $"Your expense {e.ExpenseNumber} was rejected.",
                    "EXPENSE",
                    e.ExpenseId);
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        return NoContent();
    }

    private static bool IsValidStatusTransition(string from, string to)
    {
        if (from == "DRAFT" && to == "SUBMITTED") return true;
        if (from == "SUBMITTED" && (to == "APPROVED" || to == "REJECTED")) return true;
        if (from == "APPROVED" && to == "PAID") return true;
        return false;
    }

    private async Task<string> NextExpenseNumber()
    {
        var today = DateTime.UtcNow;
        var prefix = $"EXP-{today:yyyyMM}-";
        var last = await _context.Expenses
            .Where(e => e.ExpenseNumber.StartsWith(prefix))
            .OrderByDescending(e => e.ExpenseNumber)
            .Select(e => e.ExpenseNumber)
            .FirstOrDefaultAsync();
        var nextSeq = 1;
        if (last != null && int.TryParse(last.Substring(prefix.Length), out var n)) nextSeq = n + 1;
        return $"{prefix}{nextSeq:000}";
    }

    private async Task<(decimal rate, string? error)> ResolveExchangeRate(string currency)
    {
        if (string.Equals(currency, "LAK", StringComparison.OrdinalIgnoreCase))
            return (1m, null);
        var today = DateTime.UtcNow.Date;
        var rate = await _context.ConversionRates
            .Where(r => r.FromCurrency == currency && r.ToCurrency == "LAK" && r.IsActive
                        && r.EffectiveDate <= today
                        && (r.ExpiryDate == null || r.ExpiryDate >= today))
            .OrderByDescending(r => r.EffectiveDate)
            .Select(r => r.Rate)
            .FirstOrDefaultAsync();
        if (rate <= 0)
            return (0, $"No active exchange rate for {currency} → LAK");
        return (rate, null);
    }

    private int? GetCurrentEmployeeId()
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return null;
        return _context.Users
            .Where(u => u.Username == username)
            .Select(u => u.EmployeeId)
            .FirstOrDefault();
    }
}

public class ApproveActionRequest
{
    public string? Notes { get; set; }
}