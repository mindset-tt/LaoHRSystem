using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 3C2B — manager self-service (MSS) team views. The manager's report
/// scope is always resolved server-side from the authenticated user's linked
/// employee; the client never supplies a manager id.
/// </summary>
[Authorize]
[ApiController]
[Route("api/team")]
public class TeamController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IOrganizationHierarchyService _hierarchy;

    public TeamController(LaoHRDbContext context, IOrganizationHierarchyService hierarchy)
    {
        _context = context;
        _hierarchy = hierarchy;
    }

    /// <summary>Team leave — leave requests for the current manager's direct reports.</summary>
    [HttpGet("leave")]
    public async Task<ActionResult<List<TeamLeaveItem>>> GetTeamLeave(
        [FromQuery] string? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var reportIds = await GetDirectReportIdsAsync();
        if (reportIds.Count == 0)
            return Ok(new List<TeamLeaveItem>());

        var query = _context.LeaveRequests
            .AsNoTracking()
            .Where(l => reportIds.Contains(l.EmployeeId));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(l => l.Status == status);
        if (from.HasValue)
            query = query.Where(l => l.EndDate >= from.Value.Date);
        if (to.HasValue)
            query = query.Where(l => l.StartDate <= to.Value.Date);

        var items = await query
            .OrderByDescending(l => l.StartDate)
            .Select(l => new TeamLeaveItem
            {
                LeaveId = l.LeaveId,
                EmployeeId = l.EmployeeId,
                EmployeeName = l.Employee != null ? (l.Employee.EnglishName ?? l.Employee.LaoName) : null,
                DepartmentName = l.Employee != null && l.Employee.Department != null ? l.Employee.Department.DepartmentName : null,
                JobTitle = l.Employee != null ? l.Employee.JobTitle : null,
                LeaveType = l.LeaveType,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                TotalDays = l.TotalDays,
                Status = l.Status
            })
            .ToListAsync();

        return Ok(items);
    }

    /// <summary>Team attendance — attendance records for the current manager's direct reports.</summary>
    [HttpGet("attendance")]
    public async Task<ActionResult<List<TeamAttendanceItem>>> GetTeamAttendance(
        [FromQuery] DateTime? date = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var reportIds = await GetDirectReportIdsAsync();
        if (reportIds.Count == 0)
            return Ok(new List<TeamAttendanceItem>());

        var query = _context.Attendances
            .AsNoTracking()
            .Where(a => reportIds.Contains(a.EmployeeId));

        if (date.HasValue)
            query = query.Where(a => a.AttendanceDate.Date == date.Value.Date);
        if (from.HasValue)
            query = query.Where(a => a.AttendanceDate >= from.Value.Date);
        if (to.HasValue)
            query = query.Where(a => a.AttendanceDate <= to.Value.Date);

        var items = await query
            .OrderByDescending(a => a.AttendanceDate)
            .Select(a => new TeamAttendanceItem
            {
                AttendanceId = a.AttendanceId,
                EmployeeId = a.EmployeeId,
                EmployeeName = a.Employee != null ? (a.Employee.EnglishName ?? a.Employee.LaoName) : null,
                AttendanceDate = a.AttendanceDate,
                ClockIn = a.ClockIn,
                ClockOut = a.ClockOut,
                Status = a.Status,
                IsLate = a.IsLate,
                IsEarlyLeave = a.IsEarlyLeave,
                WorkHours = a.WorkHours
            })
            .ToListAsync();

        return Ok(items);
    }

    private async Task<List<int>> GetDirectReportIdsAsync()
    {
        var empIdStr = User.FindFirst("EmployeeId")?.Value;
        if (!int.TryParse(empIdStr, out var managerId))
            return new List<int>();

        var reports = await _hierarchy.GetDirectReportsAsync(managerId);
        return reports.Select(r => r.EmployeeId).ToList();
    }
}

public sealed class TeamLeaveItem
{
    public int LeaveId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? DepartmentName { get; set; }
    public string? JobTitle { get; set; }
    public string LeaveType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalDays { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class TeamAttendanceItem
{
    public int AttendanceId { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime AttendanceDate { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsLate { get; set; }
    public bool IsEarlyLeave { get; set; }
    public decimal? WorkHours { get; set; }
}
