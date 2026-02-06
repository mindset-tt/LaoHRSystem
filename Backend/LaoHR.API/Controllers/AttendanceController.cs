using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    
    public AttendanceController(LaoHRDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Get attendance records
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendance(
        [FromQuery] DateTime? date = null,
        [FromQuery] int? employeeId = null)
    {
        var query = _context.Attendances
            .Include(a => a.Employee)
            .AsQueryable();
        
        if (date.HasValue)
            query = query.Where(a => a.AttendanceDate.Date == date.Value.Date);
        
        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        
        return await query.OrderByDescending(a => a.AttendanceDate).Take(100).ToListAsync();
    }
    
    /// <summary>
    /// Get today's attendance for current user
    /// </summary>
    [HttpGet("today")]
    public async Task<ActionResult<Attendance>> GetToday()
    {
        var employeeId = GetCurrentEmployeeId();
        var nowUtc = DateTime.UtcNow;
        var laoTimeZone = TimeZoneInfo.CreateCustomTimeZone("LaoTime", TimeSpan.FromHours(7), "Lao Time", "Lao Time");
        var today = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, laoTimeZone).Date;
        
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == today);
            
        return Ok(attendance);
    }

    /// <summary>
    /// Clock in
    /// </summary>
    [HttpPost("clock-in")]
    public async Task<ActionResult<Attendance>> ClockIn(ClockRequest request)
    {
        var employeeId = GetCurrentEmployeeId();
        var nowUtc = DateTime.UtcNow;
        var laoTimeZone = TimeZoneInfo.CreateCustomTimeZone("LaoTime", TimeSpan.FromHours(7), "Lao Time", "Lao Time");
        var today = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, laoTimeZone).Date;
        
        // Check if already clocked in
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == today);
        
        if (existing?.ClockIn != null)
            return BadRequest("Already clocked in today");
        
        // Get work schedule settings for late threshold
        var workSchedule = await _context.WorkSchedules.FirstOrDefaultAsync();
        var workStart = workSchedule?.WorkStartTime ?? new TimeSpan(8, 30, 0);
        var lateThreshold = workSchedule?.LateThresholdMinutes ?? 15;
        var nowLao = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, laoTimeZone);        
        var isLate = nowLao.TimeOfDay > workStart.Add(TimeSpan.FromMinutes(lateThreshold));
        
        var attendance = existing ?? new Attendance
        {
            EmployeeId = employeeId,
            AttendanceDate = today
        };
        
        attendance.ClockIn = nowLao;
        attendance.ClockInLatitude = request.Latitude;
        attendance.ClockInLongitude = request.Longitude;
        attendance.ClockInMethod = request.Method ?? "WEB";
        attendance.IsLate = isLate;
        attendance.Status = isLate ? "LATE" : "PRESENT";
        
        if (existing == null)
            _context.Attendances.Add(attendance);
        
        await _context.SaveChangesAsync();
        
        return Ok(attendance);
    }
    
    /// <summary>
    /// Clock out
    /// </summary>
    [HttpPost("clock-out")]
    public async Task<ActionResult<Attendance>> ClockOut(ClockRequest request)
    {
        var employeeId = GetCurrentEmployeeId();

        var nowUtc = DateTime.UtcNow;
        var laoTimeZone = TimeZoneInfo.CreateCustomTimeZone("LaoTime", TimeSpan.FromHours(7), "Lao Time", "Lao Time");
        var nowLao = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, laoTimeZone);
        
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AttendanceDate == nowLao.Date);
        
        if (attendance == null)
            return BadRequest("Not clocked in today");
        
        if (attendance.ClockOut != null)
            return BadRequest("Already clocked out today");
        
        attendance.ClockOut = nowLao;
        attendance.ClockOutLatitude = request.Latitude;
        attendance.ClockOutLongitude = request.Longitude;
        attendance.ClockOutMethod = request.Method ?? "WEB";
        
        // Calculate work hours
        if (attendance.ClockIn != null)
        {   
            var diff = nowLao - attendance.ClockIn.Value;
            attendance.WorkHours = Math.Round((decimal)diff.TotalHours, 2);
        }
        
        // Get work schedule for early leave detection
        var workSchedule = await _context.WorkSchedules.FirstOrDefaultAsync();
        var workEnd = workSchedule?.WorkEndTime ?? new TimeSpan(17, 30, 0);
        attendance.IsEarlyLeave = nowLao.TimeOfDay < workEnd;
        
        await _context.SaveChangesAsync();
        
        return Ok(attendance);
    }

    private int GetCurrentEmployeeId()
    {
        // Get Employee ID from Claims (added by AuthController)
        var empIdStr = User.FindFirst("EmployeeId")?.Value;
        
        if (int.TryParse(empIdStr, out var empId))
        {
            return empId;
        }
        
        // Fallback or Error Case
        // If logged in as "admin" without an Employee record, this might fail or return 0
        // Ideally should throw or handle gracefully
        
        // Fallback for demo/legacy admin login that might not be linked yet:
        var username = User.Identity?.Name?.ToLower();
        if (username == "admin") return 1; // Temporary fallback
        
        // If we can't identify the employee, throw exception to be caught
        throw new InvalidOperationException("Current user is not linked to an Employee record.");
    }
    
    /// <summary>
    /// Manual attendance entry
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<ActionResult<Attendance>> CreateAttendance(Attendance attendance)
    {
        // Check for existing
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == attendance.EmployeeId && 
                                      a.AttendanceDate == attendance.AttendanceDate);
        
        if (existing != null)
        {
            // Update existing
            existing.ClockIn = attendance.ClockIn;
            existing.ClockOut = attendance.ClockOut;
            existing.Status = attendance.Status;
            existing.Notes = attendance.Notes;
        }
        else
        {
            _context.Attendances.Add(attendance);
        }
        
        await _context.SaveChangesAsync();
        return Ok(existing ?? attendance);
    }
}

public class ClockRequest
{
    public int EmployeeId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? Method { get; set; }
}
