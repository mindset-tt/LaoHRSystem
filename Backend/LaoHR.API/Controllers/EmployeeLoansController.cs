using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class LoanListItem
{
    public int LoanId { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? Purpose { get; set; }
    public string Currency { get; set; } = "LAK";
    public decimal PrincipalAmount { get; set; }
    public decimal PrincipalAmountLak { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int Installments { get; set; }
    public int InstallmentsPaid { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? ExpectedEndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LoanDetail
{
    public int LoanId { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string LoanType { get; set; } = "LOAN";
    public string? Purpose { get; set; }
    public string Currency { get; set; } = "LAK";
    public decimal PrincipalAmount { get; set; }
    public decimal ExchangeRateUsed { get; set; }
    public decimal PrincipalAmountLak { get; set; }
    public decimal InterestRate { get; set; }
    public decimal InstallmentAmount { get; set; }
    public int Installments { get; set; }
    public decimal RepaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int InstallmentsPaid { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApproverId { get; set; }
    public string? ApproverName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LoanRepaymentItem> Repayments { get; set; } = new();
}

public class LoanRepaymentItem
{
    public int RepaymentId { get; set; }
    public DateTime RepaidAt { get; set; }
    public decimal AmountLak { get; set; }
    public int? PayrollPeriodId { get; set; }
    public string? Notes { get; set; }
}

public class CreateLoanRequest
{
    public int EmployeeId { get; set; }
    public string LoanType { get; set; } = "LOAN";
    public string? Purpose { get; set; }
    public string Currency { get; set; } = "LAK";
    public decimal PrincipalAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int Installments { get; set; }
    public DateTime StartDate { get; set; }
}

public class UpdateLoanRequest
{
    public string? Purpose { get; set; }
    public decimal? InterestRate { get; set; }
    public int? Installments { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public string? ApproverNotes { get; set; }
}

public class RecordRepaymentRequest
{
    public DateTime RepaidAt { get; set; }
    public decimal AmountLak { get; set; }
    public int? PayrollPeriodId { get; set; }
    public string? Notes { get; set; }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeeLoansController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IApprovalService _approval;
    private readonly INotificationService _notifications;
    private readonly IDataScopeService _scope;

    public EmployeeLoansController(
        LaoHRDbContext context,
        ICurrentEmployeeService currentEmployee,
        IApprovalService approval,
        INotificationService notifications,
        IDataScopeService scope)
    {
        _context = context;
        _currentEmployee = currentEmployee;
        _approval = approval;
        _notifications = notifications;
        _scope = scope;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<LoanListItem>>> GetLoans(
        [FromQuery] string? status = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] bool mineOnly = false,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        // Phase 3C3 — read-path authorization: intersect with visible scope.
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync();

        var query = _context.EmployeeLoans.AsNoTracking()
            .Where(l => visibleIds.Contains(l.EmployeeId))
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(l => l.Status == status);
        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);
        if (mineOnly)
        {
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId.HasValue)
                query = query.Where(l => l.EmployeeId == currentEmployeeId.Value);
        }
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(l =>
                l.LoanNumber.ToLower().Contains(s) ||
                (l.Purpose != null && l.Purpose.ToLower().Contains(s)));
        }

        var total = await query.LongCountAsync();

        // Aggregate paid amounts per loan in a single round-trip.
        var paidAgg = await _context.LoanRepayments
            .AsNoTracking()
            .GroupBy(r => r.LoanId)
            .Select(g => new { LoanId = g.Key, Paid = g.Sum(x => x.AmountLak), Count = g.LongCount() })
            .ToDictionaryAsync(x => x.LoanId, x => (paid: x.Paid, count: x.Count));

        var rows = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new
            {
                Loan = l,
                EmployeeName = l.Employee != null ? (l.Employee.EnglishName ?? l.Employee.LaoName) : null,
                ApproverName = l.Approver != null ? (l.Approver.EnglishName ?? l.Approver.LaoName) : null,
            })
            .ToListAsync();

        var result = rows.Select(x =>
        {
            paidAgg.TryGetValue(x.Loan.LoanId, out var p);
            var remaining = Math.Max(0, x.Loan.PrincipalLak - p.paid);
            var expectedEnd = x.Loan.Installments > 0
                ? x.Loan.StartDate.AddMonths(x.Loan.Installments)
                : (DateTime?)null;
            return new LoanListItem
            {
                LoanId = x.Loan.LoanId,
                LoanNumber = x.Loan.LoanNumber,
                EmployeeId = x.Loan.EmployeeId,
                EmployeeName = x.EmployeeName,
                Purpose = x.Loan.Purpose,
                Currency = x.Loan.Currency,
                PrincipalAmount = x.Loan.Principal,
                PrincipalAmountLak = x.Loan.PrincipalLak,
                RemainingAmount = remaining,
                InstallmentAmount = x.Loan.InstallmentAmount,
                Installments = x.Loan.Installments,
                InstallmentsPaid = (int)p.count,
                StartDate = x.Loan.StartDate,
                ExpectedEndDate = expectedEnd,
                Status = x.Loan.Status,
                ApproverId = x.Loan.ApproverId,
                ApproverName = x.ApproverName,
                CreatedAt = x.Loan.CreatedAt,
            };
        }).ToList();

        return new PaginatedResponse<LoanListItem>
        {
            Items = result,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LoanDetail>> GetLoan(int id)
    {
        var l = await _context.EmployeeLoans
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.LoanId == id);
        if (l == null) return NotFound();

        // Phase 3C3 — detail IDOR: only visible employees' loans.
        if (!await _scope.CanViewEmployeeAsync(l.EmployeeId))
            return Forbid();

        var employee = await _context.Employees
            .Where(e => e.EmployeeId == l.EmployeeId)
            .Select(e => e.EnglishName ?? e.LaoName)
            .FirstOrDefaultAsync();

        string? approverName = null;
        if (l.ApproverId.HasValue)
        {
            approverName = await _context.Employees
                .Where(e => e.EmployeeId == l.ApproverId.Value)
                .Select(e => e.EnglishName ?? e.LaoName)
                .FirstOrDefaultAsync();
        }

        var repayments = await _context.LoanRepayments
            .AsNoTracking()
            .Where(r => r.LoanId == id)
            .OrderBy(r => r.RepaidAt)
            .Select(r => new LoanRepaymentItem
            {
                RepaymentId = r.RepaymentId,
                RepaidAt = r.RepaidAt,
                AmountLak = r.AmountLak,
                PayrollPeriodId = r.PayrollPeriodId,
                Notes = r.Notes,
            })
            .ToListAsync();

        var paid = repayments.Sum(r => r.AmountLak);
        var remaining = Math.Max(0, l.PrincipalLak - paid);

        var expectedEnd = l.Installments > 0
            ? l.StartDate.AddMonths(l.Installments)
            : (DateTime?)null;

        return new LoanDetail
        {
            LoanId = l.LoanId,
            LoanNumber = l.LoanNumber,
            EmployeeId = l.EmployeeId,
            EmployeeName = employee,
            LoanType = l.LoanType,
            Purpose = l.Purpose,
            Currency = l.Currency,
            PrincipalAmount = l.Principal,
            ExchangeRateUsed = l.ExchangeRateUsed,
            PrincipalAmountLak = l.PrincipalLak,
            InterestRate = l.InterestRate,
            InstallmentAmount = l.InstallmentAmount,
            Installments = l.Installments,
            RepaidAmount = paid,
            RemainingAmount = remaining,
            InstallmentsPaid = repayments.Count,
            StartDate = l.StartDate,
            EndDate = l.EndDate ?? expectedEnd,
            Status = l.Status,
            ApproverId = l.ApproverId,
            ApproverName = approverName,
            ApprovedAt = l.ApprovedAt,
            ApproverNotes = l.ApproverNotes,
            CreatedAt = l.CreatedAt,
            UpdatedAt = l.UpdatedAt,
            Repayments = repayments,
        };
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeLoan>> CreateLoan([FromBody] CreateLoanRequest request)
    {
        if (request.PrincipalAmount <= 0) return BadRequest("PrincipalAmount must be positive");
        if (request.Installments <= 0) return BadRequest("Installments must be at least 1");

        // Phase 3C2 — resolve the requester from the authenticated user (IDOR fix).
        var requesterEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (requesterEmployeeId == null)
            return Unauthorized("No linked employee profile.");

        var (rate, error) = await ResolveExchangeRate(request.Currency);
        if (error != null) return BadRequest(error);

        var principalLak = Math.Round(request.PrincipalAmount * rate, 2);
        var installment = Math.Round((principalLak + principalLak * (request.InterestRate / 100m)) / request.Installments, 2);
        var nextNumber = await NextLoanNumber();

        var loan = new EmployeeLoan
        {
            LoanNumber = nextNumber,
            EmployeeId = requesterEmployeeId.Value,
            LoanType = string.IsNullOrEmpty(request.LoanType) ? "LOAN" : request.LoanType,
            Purpose = request.Purpose,
            Currency = request.Currency,
            Principal = request.PrincipalAmount,
            ExchangeRateUsed = rate,
            PrincipalLak = principalLak,
            InterestRate = request.InterestRate,
            InstallmentAmount = installment,
            Installments = request.Installments,
            RepaidAmount = 0,
            StartDate = request.StartDate,
            Status = "DRAFT",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.EmployeeLoans.Add(loan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLoan), new { id = loan.LoanId }, loan);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLoan(int id, [FromBody] UpdateLoanRequest request)
    {
        var l = await _context.EmployeeLoans.FirstOrDefaultAsync(x => x.LoanId == id);
        if (l == null) return NotFound();

        if (l.Status != "DRAFT")
            return BadRequest($"Cannot edit loan in '{l.Status}' state");

        if (request.Purpose != null) l.Purpose = request.Purpose;
        if (request.InterestRate.HasValue) l.InterestRate = request.InterestRate.Value;
        if (request.Installments.HasValue)
        {
            l.Installments = request.Installments.Value;
            l.InstallmentAmount = Math.Round((l.PrincipalLak + l.PrincipalLak * (l.InterestRate / 100m)) / l.Installments, 2);
        }
        if (request.StartDate.HasValue)
        {
            l.StartDate = request.StartDate.Value;
            l.EndDate = l.Installments > 0 ? l.StartDate.AddMonths(l.Installments) : l.EndDate;
        }
        if (request.EndDate.HasValue) l.EndDate = request.EndDate.Value;

        if (request.Status != null && IsValidStatusTransition(l.Status, request.Status))
        {
            l.Status = request.Status;
            if (request.Status == "APPROVED" || request.Status == "REJECTED")
            {
                l.ApproverId = GetCurrentEmployeeId();
                l.ApprovedAt = DateTime.UtcNow;
            }
            if (request.ApproverNotes != null) l.ApproverNotes = request.ApproverNotes;
        }

        l.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> ApproveLoan(int id, [FromBody] ApproveActionRequest? body = null)
    {
        return await SetApprovalState(id, "APPROVED", body?.Notes);
    }

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> RejectLoan(int id, [FromBody] ApproveActionRequest? body = null)
    {
        return await SetApprovalState(id, "REJECTED", body?.Notes);
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> ActivateLoan(int id)
    {
        var l = await _context.EmployeeLoans.FirstOrDefaultAsync(x => x.LoanId == id);
        if (l == null) return NotFound();
        if (l.Status != "APPROVED")
            return BadRequest("Only APPROVED loans can be activated");
        l.Status = "ACTIVE";
        l.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> CancelLoan(int id, [FromBody] ApproveActionRequest? body = null)
    {
        var l = await _context.EmployeeLoans.FirstOrDefaultAsync(x => x.LoanId == id);
        if (l == null) return NotFound();
        if (l.Status != "DRAFT" && l.Status != "APPROVED")
            return BadRequest("Only DRAFT or APPROVED loans can be cancelled");
        l.Status = "CANCELLED";
        if (body?.Notes != null) l.ApproverNotes = body.Notes;
        l.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/repayments")]
    public async Task<ActionResult<List<LoanRepaymentItem>>> GetRepayments(int id)
    {
        var rows = await _context.LoanRepayments
            .AsNoTracking()
            .Where(r => r.LoanId == id)
            .OrderBy(r => r.RepaidAt)
            .Select(r => new LoanRepaymentItem
            {
                RepaymentId = r.RepaymentId,
                RepaidAt = r.RepaidAt,
                AmountLak = r.AmountLak,
                PayrollPeriodId = r.PayrollPeriodId,
                Notes = r.Notes,
            })
            .ToListAsync();
        return rows;
    }

    [HttpPost("{id:int}/repayments")]
    public async Task<ActionResult<LoanRepaymentItem>> RecordRepayment(int id, [FromBody] RecordRepaymentRequest request)
    {
        if (request.AmountLak <= 0) return BadRequest("AmountLak must be positive");

        var loan = await _context.EmployeeLoans.FirstOrDefaultAsync(x => x.LoanId == id);
        if (loan == null) return NotFound();
        if (loan.Status != "ACTIVE" && loan.Status != "SETTLED")
            return BadRequest("Repayments can only be recorded on ACTIVE loans");

        var alreadyPaid = await _context.LoanRepayments
            .Where(r => r.LoanId == id)
            .SumAsync(r => (decimal?)r.AmountLak) ?? 0m;
        var remaining = loan.PrincipalLak - alreadyPaid;
        if (request.AmountLak > remaining + 0.01m)
            return BadRequest($"Repayment ({request.AmountLak:N2}) exceeds remaining balance ({remaining:N2})");

        var repayment = new LoanRepayment
        {
            LoanId = id,
            AmountLak = request.AmountLak,
            RepaidAt = request.RepaidAt,
            PayrollPeriodId = request.PayrollPeriodId,
            Notes = request.Notes,
        };
        _context.LoanRepayments.Add(repayment);
        loan.RepaidAmount = alreadyPaid + request.AmountLak;
        if (loan.RepaidAmount >= loan.PrincipalLak - 0.01m)
        {
            loan.Status = "SETTLED";
        }
        loan.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new LoanRepaymentItem
        {
            RepaymentId = repayment.RepaymentId,
            RepaidAt = repayment.RepaidAt,
            AmountLak = repayment.AmountLak,
            PayrollPeriodId = repayment.PayrollPeriodId,
            Notes = repayment.Notes,
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLoan(int id)
    {
        var l = await _context.EmployeeLoans.FirstOrDefaultAsync(x => x.LoanId == id);
        if (l == null) return NotFound();
        if (l.Status != "DRAFT" && l.Status != "REJECTED" && l.Status != "CANCELLED")
            return BadRequest("Only DRAFT, REJECTED or CANCELLED loans can be deleted");
        var hasRepayments = await _context.LoanRepayments.AnyAsync(r => r.LoanId == id);
        if (hasRepayments)
            return BadRequest("Cannot delete a loan with repayment history");
        _context.EmployeeLoans.Remove(l);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private async Task<IActionResult> SetApprovalState(int id, string newStatus, string? notes)
    {
        var actorEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (actorEmployeeId == null)
            return Unauthorized("No linked employee profile.");

        var l = await _context.EmployeeLoans.FirstOrDefaultAsync(x => x.LoanId == id);
        if (l == null) return NotFound();
        if (l.Status != "DRAFT")
            return BadRequest($"Only DRAFT loans can be {newStatus.ToLower()}");

        // Phase 3C2 — lazily create the approval request on first approval action.
        var approval = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.RequestType == "LOAN" && r.EntityId == id && r.Status == "PENDING");
        if (approval == null)
        {
            var requester = await _context.Employees.AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == l.EmployeeId);
            var steps = new List<ApprovalStepDefinition>();
            if (requester?.ManagerId != null)
                steps.Add(new ApprovalStepDefinition { ResolverType = "DIRECT_MANAGER" });
            else
                steps.Add(new ApprovalStepDefinition { ResolverType = "ROLE", RoleName = "HR" });
            approval = await _approval.CreateRequestAsync("LOAN", l.LoanId, l.EmployeeId, steps);
        }

        try
        {
            if (newStatus == "APPROVED")
            {
                var result = await _approval.ApproveAsync(approval.ApprovalRequestId, actorEmployeeId.Value, notes);
                if (result.Status == "APPROVED")
                {
                    l.Status = "APPROVED";
                    l.ApproverId = actorEmployeeId.Value;
                    l.ApprovedAt = DateTime.UtcNow;
                    if (notes != null) l.ApproverNotes = notes;
                    l.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    await _notifications.NotifyEmployeeAsync(
                        l.EmployeeId,
                        "APPROVAL_APPROVED",
                        "Loan approved",
                        $"Your loan {l.LoanNumber} was approved.",
                        "LOAN",
                        l.LoanId);
                }
            }
            else
            {
                await _approval.RejectAsync(approval.ApprovalRequestId, actorEmployeeId.Value, notes);
                l.Status = "REJECTED";
                l.ApproverId = actorEmployeeId.Value;
                l.ApprovedAt = DateTime.UtcNow;
                if (notes != null) l.ApproverNotes = notes;
                l.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _notifications.NotifyEmployeeAsync(
                    l.EmployeeId,
                    "APPROVAL_REJECTED",
                    "Loan rejected",
                    $"Your loan {l.LoanNumber} was rejected.",
                    "LOAN",
                    l.LoanId);
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
        if (from == "DRAFT" && (to == "APPROVED" || to == "REJECTED")) return true;
        if (from == "APPROVED" && to == "ACTIVE") return true;
        if (from == "ACTIVE" && to == "SETTLED") return true;
        return false;
    }

    private async Task<string> NextLoanNumber()
    {
        var today = DateTime.UtcNow;
        var prefix = $"LN-{today:yyyyMM}-";
        var last = await _context.EmployeeLoans
            .Where(l => l.LoanNumber.StartsWith(prefix))
            .OrderByDescending(l => l.LoanNumber)
            .Select(l => l.LoanNumber)
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