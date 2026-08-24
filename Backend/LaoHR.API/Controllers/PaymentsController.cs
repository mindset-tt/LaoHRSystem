using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class PaymentAllocationDto
{
    public int PaymentAllocationId { get; set; }
    public int SupplierInvoiceId { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentListItem
{
    public int PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Currency { get; set; } = "LAK";
    public decimal Amount { get; set; }
    public int? BankAccountId { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PaymentDetail : PaymentListItem
{
    public string? ReferenceNumber { get; set; }
    public List<PaymentAllocationDto> Allocations { get; set; } = new();
}

public class CreatePaymentAllocation
{
    public int SupplierInvoiceId { get; set; }
    public decimal Amount { get; set; }
}

public class CreatePaymentRequest
{
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = "BANK_TRANSFER";
    public string Currency { get; set; } = "LAK";
    public int? BankAccountId { get; set; }
    public string? ReferenceNumber { get; set; }
    public List<CreatePaymentAllocation> Allocations { get; set; } = new();
}

[Authorize]
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IFinanceAccessService _access;
    private readonly IAccountsPayableService _ap;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly INumberSequenceService _numbers;
    private readonly IPostingService _posting;
    private readonly ISegregationOfDutiesService _sod;
    private readonly INotificationService _notifications;

    public PaymentsController(
        LaoHRDbContext context,
        IFinanceAccessService access,
        IAccountsPayableService ap,
        ICurrentEmployeeService currentEmployee,
        INumberSequenceService numbers,
        IPostingService posting,
        ISegregationOfDutiesService sod,
        INotificationService notifications)
    {
        _context = context;
        _access = access;
        _ap = ap;
        _currentEmployee = currentEmployee;
        _numbers = numbers;
        _posting = posting;
        _sod = sod;
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<PaymentListItem>>> GetPayments(
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewPayments())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Payments.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(p => p.Status == status);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentListItem
            {
                PaymentId = p.PaymentId,
                PaymentNumber = p.PaymentNumber,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod,
                Currency = p.Currency,
                Amount = p.Amount,
                BankAccountId = p.BankAccountId,
                Status = p.Status,
            })
            .ToListAsync();

        return new PaginatedResponse<PaymentListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDetail>> GetPayment(int id)
    {
        if (!_access.CanViewPayments())
            return Forbid();

        var p = await _context.Payments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.PaymentId == id);
        if (p == null) return NotFound();

        var allocations = await _context.PaymentAllocations.AsNoTracking()
            .Where(a => a.PaymentId == id)
            .Select(a => new PaymentAllocationDto
            {
                PaymentAllocationId = a.PaymentAllocationId,
                SupplierInvoiceId = a.SupplierInvoiceId,
                InvoiceNumber = a.SupplierInvoice != null ? a.SupplierInvoice.InvoiceNumber : null,
                Amount = a.Amount,
            })
            .ToListAsync();

        return new PaymentDetail
        {
            PaymentId = p.PaymentId,
            PaymentNumber = p.PaymentNumber,
            PaymentDate = p.PaymentDate,
            PaymentMethod = p.PaymentMethod,
            Currency = p.Currency,
            Amount = p.Amount,
            BankAccountId = p.BankAccountId,
            ReferenceNumber = p.ReferenceNumber,
            Status = p.Status,
            Allocations = allocations,
        };
    }

    [HttpPost]
    public async Task<ActionResult<Payment>> CreatePayment([FromBody] CreatePaymentRequest request)
    {
        if (!_access.CanCreatePayments())
            return Forbid();

        if (request.Allocations == null || request.Allocations.Count == 0)
            return BadRequest("At least one allocation is required.");

        var total = request.Allocations.Sum(a => a.Amount);
        if (total <= 0)
            return BadRequest("Payment amount must be positive.");

        var number = await _numbers.NextAsync("PAY");

        var payment = new Payment
        {
            PaymentNumber = number,
            PaymentDate = request.PaymentDate,
            PaymentMethod = request.PaymentMethod,
            Currency = request.Currency,
            Amount = total,
            BankAccountId = request.BankAccountId,
            ReferenceNumber = request.ReferenceNumber,
            Status = "DRAFT",
            CreatedByEmployeeId = _currentEmployee.GetCurrentEmployeeId(),
        };

        foreach (var alloc in request.Allocations)
        {
            payment.Allocations.Add(new PaymentAllocation
            {
                SupplierInvoiceId = alloc.SupplierInvoiceId,
                Amount = alloc.Amount,
            });
        }

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPayment), new { id = payment.PaymentId }, payment);
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        if (!_access.CanApprovePayments())
            return Forbid();

        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == id);
        if (payment == null) return NotFound();
        if (payment.Status != "DRAFT")
            return BadRequest($"Cannot approve a payment in '{payment.Status}' state.");

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            await _sod.EnforcePaymentApprovalAsync(id, actor.Value);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        payment.Status = "APPROVED";
        payment.ApprovedByEmployeeId = actor.Value;
        await _context.SaveChangesAsync();

        // Notify the payment creator of approval (best-effort).
        if (payment.CreatedByEmployeeId.HasValue)
        {
            await _notifications.NotifyEmployeeAsync(
                payment.CreatedByEmployeeId.Value,
                "PAYMENT_APPROVED",
                "Payment approved",
                $"Payment {payment.PaymentNumber} was approved.",
                "PAYMENT",
                payment.PaymentId);
        }

        return NoContent();
    }

    [HttpPost("{id:int}/post")]
    public async Task<IActionResult> Post(int id)
    {
        if (!_access.CanApprovePayments())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            await _ap.PostPaymentAsync(id, actor.Value);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost("{id:int}/post-to-gl")]
    public async Task<IActionResult> PostToGl(int id)
    {
        if (!_access.CanPostAccounting())
            return Forbid();

        var actor = _currentEmployee.GetCurrentEmployeeId();
        if (actor == null) return Unauthorized("No linked employee profile.");

        try
        {
            var entry = await _posting.PostPaymentAsync(id, actor.Value);
            return Ok(entry);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
