using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;
using Microsoft.Extensions.Configuration;
using ClosedXML.Excel;

namespace LaoHR.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly IApprovalService _approval;
    private readonly INotificationService _notifications;
    private readonly IDataScopeService _scope;
    
    public LeaveController(
        LaoHRDbContext context, 
        IEmailService emailService, 
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ICurrentEmployeeService currentEmployee,
        IApprovalService approval,
        INotificationService notifications,
        IDataScopeService scope)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _environment = environment;
        _currentEmployee = currentEmployee;
        _approval = approval;
        _notifications = notifications;
        _scope = scope;
    }
    
    #region Leave Requests
    
    /// <summary>
    /// Get leave requests (paged + projected).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<LeaveRequestListItem>>> GetLeaveRequests(
        [FromQuery] string? status = null,
        [FromQuery] int? employeeId = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        // Phase 3C3 — read-path authorization: intersect the requested filter
        // with the current user's visible employee scope. A non-privileged user
        // can only see their own (and, for managers, direct reports') leave.
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync();

        var query = _context.LeaveRequests
            .AsNoTracking()
            .Where(l => visibleIds.Contains(l.EmployeeId))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(l => l.Status == status);

        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);

        if (year.HasValue)
            query = query.Where(l => l.StartDate.Year == year.Value);

        if (month.HasValue)
            query = query.Where(l => l.StartDate.Month == month.Value);

        var total = await query.LongCountAsync();

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new LeaveRequestListItem
            {
                LeaveRequestId = l.LeaveId,
                EmployeeId = l.EmployeeId,
                LeaveType = l.LeaveType,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                Reason = l.Reason,
                Status = l.Status,
                ApprovedById = l.ApprovedById,
                ApprovedAt = l.ApprovedAt,
                CreatedAt = l.CreatedAt,
                EmployeeCode = l.Employee != null ? l.Employee.EmployeeCode : null,
                EmployeeName = l.Employee != null ? l.Employee.EnglishName ?? l.Employee.LaoName : null
            })
            .ToListAsync();

        return new PaginatedResponse<LeaveRequestListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }
    
    /// <summary>
    /// Get leave balance for an employee
    /// </summary>
    [HttpGet("balance")]
    public async Task<ActionResult<IEnumerable<LeaveBalanceDto>>> GetLeaveBalance([FromQuery] int? employeeId = null)
    {
        var currentYear = DateTime.UtcNow.Year;

        // Phase 3C3 — non-privileged users may only see their own balance.
        if (!_scope.IsPrivileged())
        {
            var selfId = _scope.GetCurrentEmployeeId();
            if (selfId == null) return Ok(new List<LeaveBalanceDto>());
            employeeId = selfId.Value;
        }
        
        // Get all leave policies
        var policies = await _context.LeavePolicies.Where(p => p.IsActive).ToListAsync();
        
        // Get approved leave requests for the current year
        var query = _context.LeaveRequests
            .Where(l => l.Status == "APPROVED" && l.StartDate.Year == currentYear);
        
        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);
        
        var approvedLeaves = await query.ToListAsync();
        
        // Calculate used days per leave type
        var usedDays = approvedLeaves
            .GroupBy(l => l.LeaveType)
            .ToDictionary(g => g.Key, g => g.Sum(l => l.TotalDays));
        
        // Build balance response from policies
        var balances = policies.Select(policy => new LeaveBalanceDto
        {
            LeaveType = policy.LeaveType,
            LeaveTypeLao = policy.LeaveTypeLao,
            Total = policy.AnnualQuota,
            Used = usedDays.TryGetValue(policy.LeaveType, out var used) ? used : 0,
            Remaining = policy.AnnualQuota - (usedDays.TryGetValue(policy.LeaveType, out var usedVal) ? usedVal : 0),
            AllowHalfDay = policy.AllowHalfDay,
            RequiresAttachment = policy.RequiresAttachment,
            MinDaysForAttachment = policy.MinDaysForAttachment
        }).ToList();
        
        return Ok(balances);
    }
    
    /// <summary>
    /// Get leave calendar data for a month
    /// </summary>
    [HttpGet("calendar")]
    public async Task<ActionResult<IEnumerable<LeaveCalendarItem>>> GetLeaveCalendar(
        [FromQuery] int year,
        [FromQuery] int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Phase 3C3 — scope calendar to the current user's visible employees.
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync();
        
        var leaves = await _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => visibleIds.Contains(l.EmployeeId) &&
                        (l.Status == "APPROVED" || l.Status == "PENDING") &&
                        l.StartDate <= endDate && l.EndDate >= startDate)
            .Select(l => new LeaveCalendarItem
            {
                LeaveId = l.LeaveId,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee!.EnglishName ?? l.Employee.LaoName,
                LeaveType = l.LeaveType,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                IsHalfDay = l.IsHalfDay,
                HalfDayType = l.HalfDayType,
                Status = l.Status
            })
            .ToListAsync();
        
        return Ok(leaves);
    }
    
    /// <summary>
    /// Create leave request with half-day and attachment support
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LeaveRequest>> CreateLeaveRequest([FromForm] CreateLeaveRequestDto request)
    {
        // Phase 3C2 — resolve the requester from the authenticated user, never
        // from the client. This closes the IDOR where any user could submit a
        // leave request on behalf of another employee.
        var requesterEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (requesterEmployeeId == null)
            return Unauthorized("No linked employee profile.");

        // Get policy for this leave type
        var policy = await _context.LeavePolicies.FirstOrDefaultAsync(p => p.LeaveType == request.LeaveType);
        if (policy == null)
            return BadRequest($"Unknown leave type: {request.LeaveType}");
        
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = requesterEmployeeId.Value,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsHalfDay = request.IsHalfDay,
            HalfDayType = request.HalfDayType,
            Reason = request.Reason,
            Status = "PENDING",
            CreatedAt = DateTime.UtcNow
        };
        
        // Calculate total days
        if (request.IsHalfDay)
        {
            if (!policy.AllowHalfDay)
                return BadRequest($"{request.LeaveType} leave does not allow half-day requests");
            
            leaveRequest.TotalDays = 0.5m;
            leaveRequest.EndDate = leaveRequest.StartDate; // Half-day is always single day
        }
        else
        {
            leaveRequest.TotalDays = (int)(request.EndDate - request.StartDate).TotalDays + 1;
        }
        
        // Validate attachment requirement
        if (policy.RequiresAttachment && leaveRequest.TotalDays >= policy.MinDaysForAttachment)
        {
            if (request.Attachment == null || request.Attachment.Length == 0)
                return BadRequest($"{request.LeaveType} leave requires attachment for {policy.MinDaysForAttachment}+ days");
        }
        
        // Handle attachment upload
        if (request.Attachment != null && request.Attachment.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "LeaveAttachments");
            Directory.CreateDirectory(uploadsFolder);
            
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Attachment.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.Attachment.CopyToAsync(stream);
            }
            
            leaveRequest.AttachmentPath = $"/Uploads/LeaveAttachments/{fileName}";
        }
        
        // Validate leave balance
        var currentYear = DateTime.UtcNow.Year;
        var usedDays = await _context.LeaveRequests
            .Where(l => l.EmployeeId == requesterEmployeeId.Value 
                     && l.LeaveType == request.LeaveType
                     && (l.Status == "APPROVED" || l.Status == "PENDING")
                     && l.StartDate.Year == currentYear)
            .SumAsync(l => l.TotalDays);
        
        var remaining = policy.AnnualQuota - usedDays;
        if (leaveRequest.TotalDays > remaining)
        {
            return BadRequest($"Insufficient {request.LeaveType.ToLower()} leave balance. Requested: {leaveRequest.TotalDays} days, Remaining: {remaining} days");
        }
        
        _context.LeaveRequests.Add(leaveRequest);
        await _context.SaveChangesAsync();
        
        // Phase 3C2 — create the approval request (DIRECT_MANAGER step).
        // If the employee has no manager, fall back to an HR role step.
        var requester = await _context.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.EmployeeId == requesterEmployeeId.Value);
        var steps = new List<ApprovalStepDefinition>();
        if (requester?.ManagerId != null)
        {
            steps.Add(new ApprovalStepDefinition { ResolverType = "DIRECT_MANAGER" });
        }
        else
        {
            steps.Add(new ApprovalStepDefinition { ResolverType = "ROLE", RoleName = "HR" });
        }
        await _approval.CreateRequestAsync("LEAVE", leaveRequest.LeaveId, requesterEmployeeId.Value, steps);

        // Notify the approver (best-effort).
        var approverEmployeeId = requester?.ManagerId;
        if (approverEmployeeId != null)
        {
            await _notifications.NotifyEmployeeAsync(
                approverEmployeeId.Value,
                "APPROVAL_REQUESTED",
                "New leave request",
                $"{requester?.EnglishName ?? requester?.LaoName} requested {leaveRequest.TotalDays} day(s) of {leaveRequest.LeaveType} leave.",
                "LEAVE",
                leaveRequest.LeaveId);
        }

        // Notify HR
        var employee = await _context.Employees.FindAsync(requesterEmployeeId.Value);
        var hrEmail = _configuration["SmtpSettings:HrEmail"];
        if (!string.IsNullOrEmpty(hrEmail) && employee != null)
        {
            var subject = $"New Leave Request: {employee.LaoName}";
            var halfDayInfo = leaveRequest.IsHalfDay ? $" ({leaveRequest.HalfDayType})" : "";
            var body = $@"
                <h3>New Leave Request</h3>
                <p><strong>Employee:</strong> {employee.LaoName} ({employee.EmployeeCode})</p>
                <p><strong>Type:</strong> {leaveRequest.LeaveType}{halfDayInfo}</p>
                <p><strong>Dates:</strong> {leaveRequest.StartDate:dd/MM/yyyy} - {leaveRequest.EndDate:dd/MM/yyyy} ({leaveRequest.TotalDays} days)</p>
                <p><strong>Reason:</strong> {leaveRequest.Reason}</p>
                <p>Please login to review.</p>";
            
            await _emailService.SendEmailAsync(hrEmail, subject, body);
        }
        
        return CreatedAtAction(nameof(GetLeaveRequests), leaveRequest);
    }
    
    /// <summary>
    /// Approve leave request (Phase 3C2 — routed through the approval engine).
    /// Approver identity is resolved server-side; the client-supplied
    /// <c>ApprovedById</c> is ignored.
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveLeave(int id, [FromBody] ApproveRequest request)
    {
        var actorEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (actorEmployeeId == null)
            return Unauthorized("No linked employee profile.");

        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.LeaveId == id);
            
        if (leave == null) return NotFound();

        var approval = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.RequestType == "LEAVE" && r.EntityId == id && r.Status == "PENDING");
        if (approval == null)
            return BadRequest("No pending approval for this leave request.");

        try
        {
            var result = await _approval.ApproveAsync(approval.ApprovalRequestId, actorEmployeeId.Value, request.Notes);

            // Only finalize the leave when the whole chain is approved.
            if (result.Status == "APPROVED")
            {
                leave.Status = "APPROVED";
                leave.ApprovedById = actorEmployeeId.Value;
                leave.ApprovedAt = DateTime.UtcNow;
                leave.ApproverNotes = request.Notes;
                await _context.SaveChangesAsync();

                await _notifications.NotifyEmployeeAsync(
                    leave.EmployeeId,
                    "APPROVAL_APPROVED",
                    "Leave request approved",
                    $"Your {leave.LeaveType} leave request was approved.",
                    "LEAVE",
                    leave.LeaveId);
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

        // Notify Employee (email, best-effort)
        if (!string.IsNullOrEmpty(leave.Employee?.Email))
        {
            var subject = "Leave Request Approved";
            var body = $@"
                <h3>Leave Request Approved</h3>
                <p>Your leave request for <strong>{leave.StartDate:dd/MM/yyyy} - {leave.EndDate:dd/MM/yyyy}</strong> has been approved.</p>
                <p><strong>Notes:</strong> {request.Notes ?? "-"}</p>";
                
            await _emailService.SendEmailAsync(leave.Employee.Email, subject, body);
        }
        
        return Ok(leave);
    }
    
    /// <summary>
    /// Reject leave request (Phase 3C2 — routed through the approval engine).
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectLeave(int id, [FromBody] ApproveRequest request)
    {
        var actorEmployeeId = _currentEmployee.GetCurrentEmployeeId();
        if (actorEmployeeId == null)
            return Unauthorized("No linked employee profile.");

        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.LeaveId == id);
            
        if (leave == null) return NotFound();

        var approval = await _context.ApprovalRequests
            .FirstOrDefaultAsync(r => r.RequestType == "LEAVE" && r.EntityId == id && r.Status == "PENDING");
        if (approval == null)
            return BadRequest("No pending approval for this leave request.");

        try
        {
            await _approval.RejectAsync(approval.ApprovalRequestId, actorEmployeeId.Value, request.Notes);

            leave.Status = "REJECTED";
            leave.ApprovedById = actorEmployeeId.Value;
            leave.ApprovedAt = DateTime.UtcNow;
            leave.ApproverNotes = request.Notes;
            await _context.SaveChangesAsync();

            await _notifications.NotifyEmployeeAsync(
                leave.EmployeeId,
                "APPROVAL_REJECTED",
                "Leave request rejected",
                $"Your {leave.LeaveType} leave request was rejected.",
                "LEAVE",
                leave.LeaveId);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        // Notify Employee (email, best-effort)
        if (!string.IsNullOrEmpty(leave.Employee?.Email))
        {
            var subject = "Leave Request Rejected";
            var body = $@"
                <h3>Leave Request Rejected</h3>
                <p>Your leave request for <strong>{leave.StartDate:dd/MM/yyyy} - {leave.EndDate:dd/MM/yyyy}</strong> has been rejected.</p>
                <p><strong>Reason:</strong> {request.Notes ?? "-"}</p>";
                
            await _emailService.SendEmailAsync(leave.Employee.Email, subject, body);
        }
        
        return Ok(leave);
    }
    
    #endregion
    
    #region Leave Policies
    
    /// <summary>
    /// Get all leave policies
    /// </summary>
    [HttpGet("policies")]
    public async Task<ActionResult<IEnumerable<LeavePolicy>>> GetLeavePolicies()
    {
        return await _context.LeavePolicies.OrderBy(p => p.LeavePolicyId).ToListAsync();
    }
    
    /// <summary>
    /// Update a leave policy (Admin only)
    /// </summary>
    [HttpPut("policies/{id}")]
    public async Task<IActionResult> UpdateLeavePolicy(int id, [FromBody] UpdateLeavePolicyDto dto)
    {
        var policy = await _context.LeavePolicies.FindAsync(id);
        if (policy == null) return NotFound();
        
        policy.AnnualQuota = dto.AnnualQuota;
        policy.MaxCarryOver = dto.MaxCarryOver;
        policy.AccrualPerMonth = dto.AccrualPerMonth;
        policy.RequiresAttachment = dto.RequiresAttachment;
        policy.MinDaysForAttachment = dto.MinDaysForAttachment;
        policy.AllowHalfDay = dto.AllowHalfDay;
        policy.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return Ok(policy);
    }
    
    #endregion
    
    #region Export
    
    /// <summary>
    /// Export leave history to Excel
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportLeaveHistory(
        [FromQuery] int? year = null,
        [FromQuery] int? employeeId = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;

        // Phase 3C3 — export must use the same scope as the dashboard/list.
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync();
        
        var query = _context.LeaveRequests
            .Include(l => l.Employee)
            .Where(l => visibleIds.Contains(l.EmployeeId) && l.StartDate.Year == targetYear);
        
        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);
        
        var leaves = await query.OrderBy(l => l.StartDate).ToListAsync();
        
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Leave History");
        
        // Header
        worksheet.Cell(1, 1).Value = "Employee Code";
        worksheet.Cell(1, 2).Value = "Employee Name";
        worksheet.Cell(1, 3).Value = "Leave Type";
        worksheet.Cell(1, 4).Value = "Start Date";
        worksheet.Cell(1, 5).Value = "End Date";
        worksheet.Cell(1, 6).Value = "Days";
        worksheet.Cell(1, 7).Value = "Half Day";
        worksheet.Cell(1, 8).Value = "Status";
        worksheet.Cell(1, 9).Value = "Reason";
        
        // Style header
        var headerRange = worksheet.Range(1, 1, 1, 9);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        // Data
        var row = 2;
        foreach (var leave in leaves)
        {
            worksheet.Cell(row, 1).Value = leave.Employee?.EmployeeCode ?? "";
            worksheet.Cell(row, 2).Value = leave.Employee?.EnglishName ?? leave.Employee?.LaoName ?? "";
            worksheet.Cell(row, 3).Value = leave.LeaveType;
            worksheet.Cell(row, 4).Value = leave.StartDate.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 5).Value = leave.EndDate.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 6).Value = leave.TotalDays;
            worksheet.Cell(row, 7).Value = leave.IsHalfDay ? leave.HalfDayType : "-";
            worksheet.Cell(row, 8).Value = leave.Status;
            worksheet.Cell(row, 9).Value = leave.Reason ?? "";
            row++;
        }
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        
        return File(
            stream.ToArray(), 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"LeaveHistory_{targetYear}.xlsx");
    }
    
    #endregion
    
    #region Admin Jobs
    
    /// <summary>
    /// Manually trigger monthly accrual (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("admin/run-accrual")]
    public async Task<IActionResult> RunMonthlyAccrual(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromServices] ILeaveService leaveService = null!)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;
        
        await leaveService.InitializeYearlyBalancesAsync(targetYear);
        await leaveService.ProcessMonthlyAccrualAsync(targetYear, targetMonth);
        
        return Ok(new { message = $"Monthly accrual processed for {targetYear}-{targetMonth}" });
    }
    
    /// <summary>
    /// Manually trigger year-end carry-over (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("admin/run-carryover")]
    public async Task<IActionResult> RunYearEndCarryOver(
        [FromQuery] int? fromYear = null,
        [FromQuery] int? toYear = null,
        [FromServices] ILeaveService leaveService = null!)
    {
        var from = fromYear ?? DateTime.UtcNow.Year - 1;
        var to = toYear ?? DateTime.UtcNow.Year;
        
        await leaveService.ProcessYearEndCarryOverAsync(from, to);
        await leaveService.InitializeYearlyBalancesAsync(to);
        
        return Ok(new { message = $"Carry-over processed from {from} to {to}" });
    }
    
    /// <summary>
    /// Initialize leave balances for a year (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("admin/initialize-balances")]
    public async Task<IActionResult> InitializeBalances(
        [FromQuery] int? year = null,
        [FromServices] ILeaveService leaveService = null!)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        
        await leaveService.InitializeYearlyBalancesAsync(targetYear);
        
        return Ok(new { message = $"Leave balances initialized for {targetYear}" });
    }
    
    #endregion
}

#region DTOs

public class ApproveRequest
{
    public int? ApprovedById { get; set; }
    public string? Notes { get; set; }
}

public class LeaveBalanceDto
{
    public string LeaveType { get; set; } = string.Empty;
    public string? LeaveTypeLao { get; set; }
    public int Total { get; set; }
    public decimal Used { get; set; }
    public decimal Remaining { get; set; }
    public bool AllowHalfDay { get; set; }
    public bool RequiresAttachment { get; set; }
    public int MinDaysForAttachment { get; set; }
}

public class LeaveCalendarItem
{
    public int LeaveId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public bool IsHalfDay { get; set; }
    public string? HalfDayType { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateLeaveRequestDto
{
    public int EmployeeId { get; set; }
    public string LeaveType { get; set; } = "ANNUAL";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsHalfDay { get; set; }
    public string? HalfDayType { get; set; }
    public string? Reason { get; set; }
    public IFormFile? Attachment { get; set; }
}

public class UpdateLeavePolicyDto
{
    public int AnnualQuota { get; set; }
    public int MaxCarryOver { get; set; }
    public decimal AccrualPerMonth { get; set; }
    public bool RequiresAttachment { get; set; }
    public int MinDaysForAttachment { get; set; }
    public bool AllowHalfDay { get; set; }
}

#endregion

public sealed class LeaveRequestListItem
{
    public int LeaveRequestId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
