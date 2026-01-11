using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;
using Microsoft.Extensions.Configuration;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    
    public LeaveController(LaoHRDbContext context, IEmailService emailService, IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }
    
    /// <summary>
    /// Get all leave requests
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequests(
        [FromQuery] string? status = null,
        [FromQuery] int? employeeId = null)
    {
        var query = _context.LeaveRequests
            .Include(l => l.Employee)
            .AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(l => l.Status == status);
        
        if (employeeId.HasValue)
            query = query.Where(l => l.EmployeeId == employeeId.Value);
        
        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
    }
    
    /// <summary>
    /// Create leave request
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<LeaveRequest>> CreateLeaveRequest(LeaveRequest request)
    {
        // Calculate total days
        request.TotalDays = (int)(request.EndDate - request.StartDate).TotalDays + 1;
        request.Status = "PENDING";
        request.CreatedAt = DateTime.UtcNow;
        
        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();
        
        // Notify HR
        var employee = await _context.Employees.FindAsync(request.EmployeeId);
        var hrEmail = _configuration["SmtpSettings:HrEmail"];
        if (!string.IsNullOrEmpty(hrEmail) && employee != null)
        {
            var subject = $"New Leave Request: {employee.LaoName}";
            var body = $@"
                <h3>New Leave Request</h3>
                <p><strong>Employee:</strong> {employee.LaoName} ({employee.EmployeeCode})</p>
                <p><strong>Type:</strong> {request.LeaveType}</p>
                <p><strong>Dates:</strong> {request.StartDate:dd/MM/yyyy} - {request.EndDate:dd/MM/yyyy} ({request.TotalDays} days)</p>
                <p><strong>Reason:</strong> {request.Reason}</p>
                <p>Please login to review.</p>";
            
            await _emailService.SendEmailAsync(hrEmail, subject, body);
        }
        
        return CreatedAtAction(nameof(GetLeaveRequests), request);
    }
    
    /// <summary>
    /// Approve leave request
    /// </summary>
    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveLeave(int id, [FromBody] ApproveRequest request)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.LeaveId == id);
            
        if (leave == null) return NotFound();
        
        leave.Status = "APPROVED";
        leave.ApprovedById = request.ApprovedById;
        leave.ApprovedAt = DateTime.UtcNow;
        leave.ApproverNotes = request.Notes;
        
        await _context.SaveChangesAsync();
        
        // Notify Employee
        if (!string.IsNullOrEmpty(leave.Employee.Email))
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
    /// Reject leave request
    /// </summary>
    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectLeave(int id, [FromBody] ApproveRequest request)
    {
        var leave = await _context.LeaveRequests
            .Include(l => l.Employee)
            .FirstOrDefaultAsync(l => l.LeaveId == id);
            
        if (leave == null) return NotFound();
        
        leave.Status = "REJECTED";
        leave.ApprovedById = request.ApprovedById;
        leave.ApprovedAt = DateTime.UtcNow;
        leave.ApproverNotes = request.Notes;
        
        await _context.SaveChangesAsync();
        
        // Notify Employee
        if (!string.IsNullOrEmpty(leave.Employee.Email))
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
}

public class ApproveRequest
{
    public int? ApprovedById { get; set; }
    public string? Notes { get; set; }
}


