using LaoHR.API.Services;
using LaoHR.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 3C2 — unified approval inbox. The current user's pending approvals
/// (as approver) and their own submitted requests.
/// </summary>
[Authorize]
[ApiController]
[Route("api/approvals")]
public class ApprovalsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IApprovalService _approval;

    public ApprovalsController(
        LaoHRDbContext context,
        ICurrentEmployeeService currentEmployee,
        IApprovalService approval)
    {
        _context = context;
        _currentEmployee = currentEmployee;
        _approval = approval;
    }

    /// <summary>Pending approvals where the current user is the resolved approver.</summary>
    [HttpGet("my-pending")]
    public async Task<ActionResult<List<ApprovalInboxItem>>> GetMyPending()
    {
        var employeeId = _currentEmployee.GetCurrentEmployeeId();
        if (employeeId == null)
            return Ok(new List<ApprovalInboxItem>());

        var items = await _context.ApprovalSteps
            .AsNoTracking()
            .Where(s => s.ApproverEmployeeId == employeeId.Value && s.Status == "PENDING")
            .Join(_context.ApprovalRequests,
                s => s.ApprovalRequestId,
                r => r.ApprovalRequestId,
                (s, r) => new { Step = s, Request = r })
            .Where(x => x.Request.Status == "PENDING")
            .Select(x => new ApprovalInboxItem
            {
                ApprovalRequestId = x.Request.ApprovalRequestId,
                RequestType = x.Request.RequestType,
                EntityId = x.Request.EntityId,
                RequesterEmployeeId = x.Request.RequesterEmployeeId,
                RequesterName = x.Request.Requester != null ? (x.Request.Requester.EnglishName ?? x.Request.Requester.LaoName) : null,
                Status = x.Request.Status,
                CreatedAt = x.Request.CreatedAt
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Pending approval count (for navigation badge).</summary>
    [HttpGet("my-pending/count")]
    public async Task<ActionResult<int>> GetMyPendingCount()
    {
        var employeeId = _currentEmployee.GetCurrentEmployeeId();
        if (employeeId == null)
            return Ok(0);

        var count = await _context.ApprovalSteps
            .AsNoTracking()
            .Where(s => s.ApproverEmployeeId == employeeId.Value && s.Status == "PENDING")
            .Join(_context.ApprovalRequests,
                s => s.ApprovalRequestId,
                r => r.ApprovalRequestId,
                (s, r) => r)
            .Where(r => r.Status == "PENDING")
            .CountAsync();

        return Ok(count);
    }

    /// <summary>Approval detail with steps and history.</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApprovalDetail>> GetApproval(int id)
    {
        var request = await _context.ApprovalRequests
            .AsNoTracking()
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.ApprovalRequestId == id);

        if (request == null) return NotFound();

        // Phase 3C2B — authorization: only the requester, a current approver,
        // or an HR/Admin may view the detail. Unrelated users get 403.
        var currentEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        var isPrivileged = User.IsInRole("Admin") || User.IsInRole("HR");
        var isRequester = currentEmployeeId.HasValue && request.RequesterEmployeeId == currentEmployeeId.Value;
        var isApprover = currentEmployeeId.HasValue && request.Steps.Any(s => s.ApproverEmployeeId == currentEmployeeId.Value);

        if (!isPrivileged && !isRequester && !isApprover)
            return Forbid();

        var history = await _context.ApprovalActions
            .AsNoTracking()
            .Where(a => a.ApprovalRequestId == id)
            .OrderBy(a => a.ActedAt)
            .ToListAsync();

        return Ok(new ApprovalDetail
        {
            ApprovalRequestId = request.ApprovalRequestId,
            RequestType = request.RequestType,
            EntityId = request.EntityId,
            RequesterEmployeeId = request.RequesterEmployeeId,
            Status = request.Status,
            CurrentStepIndex = request.CurrentStepIndex,
            CreatedAt = request.CreatedAt,
            CompletedAt = request.CompletedAt,
            Steps = request.Steps.OrderBy(s => s.StepOrder).Select(s => new ApprovalStepDto
            {
                StepOrder = s.StepOrder,
                ResolverType = s.ResolverType,
                ApproverEmployeeId = s.ApproverEmployeeId,
                Status = s.Status,
                ActedAt = s.ActedAt,
                Comment = s.Comment
            }).ToList(),
            History = history.Select(a => new ApprovalActionDto
            {
                ActorEmployeeId = a.ActorEmployeeId,
                Action = a.Action,
                Comment = a.Comment,
                ActedAt = a.ActedAt
            }).ToList()
        });
    }

    /// <summary>My submitted requests (requester view) — consolidated ESS "My Requests".</summary>
    [HttpGet("mine")]
    public async Task<ActionResult<List<ApprovalInboxItem>>> GetMine()
    {
        var employeeId = _currentEmployee.GetCurrentEmployeeId();
        if (employeeId == null)
            return Ok(new List<ApprovalInboxItem>());

        var items = await _context.ApprovalRequests
            .AsNoTracking()
            .Where(r => r.RequesterEmployeeId == employeeId.Value)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new ApprovalInboxItem
            {
                ApprovalRequestId = r.ApprovalRequestId,
                RequestType = r.RequestType,
                EntityId = r.EntityId,
                RequesterEmployeeId = r.RequesterEmployeeId,
                RequesterName = r.Requester != null ? (r.Requester.EnglishName ?? r.Requester.LaoName) : null,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Approve the current step (identity from authenticated user).</summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApprovalActionRequest body)
    {
        var employeeId = _currentEmployee.GetCurrentEmployeeId();
        if (employeeId == null)
            return Unauthorized("No linked employee profile.");

        try
        {
            var result = await _approval.ApproveAsync(id, employeeId.Value, body.Comment);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Reject the current step (identity from authenticated user).</summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ApprovalActionRequest body)
    {
        var employeeId = _currentEmployee.GetCurrentEmployeeId();
        if (employeeId == null)
            return Unauthorized("No linked employee profile.");

        try
        {
            var result = await _approval.RejectAsync(id, employeeId.Value, body.Comment);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}

public sealed class ApprovalInboxItem
{
    public int ApprovalRequestId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int RequesterEmployeeId { get; set; }
    public string? RequesterName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class ApprovalDetail
{
    public int ApprovalRequestId { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public int RequesterEmployeeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CurrentStepIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<ApprovalStepDto> Steps { get; set; } = new();
    public List<ApprovalActionDto> History { get; set; } = new();
}

public sealed class ApprovalStepDto
{
    public int StepOrder { get; set; }
    public string ResolverType { get; set; } = string.Empty;
    public int? ApproverEmployeeId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? ActedAt { get; set; }
    public string? Comment { get; set; }
}

public sealed class ApprovalActionDto
{
    public int ActorEmployeeId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime ActedAt { get; set; }
}

public sealed class ApprovalActionRequest
{
    public string? Comment { get; set; }
}