using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Controllers;

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
    /// Clock in
    /// </summary>
    [HttpPost("clock-in")]
    public async Task<ActionResult<Attendance>> ClockIn(ClockRequest request)
    {
        var today = DateTime.UtcNow.Date;
        
        // Check if already clocked in
        var existing = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && a.AttendanceDate == today);
        
        if (existing?.ClockIn != null)
            return BadRequest("Already clocked in today");
        
        var workStart = TimeSpan.Parse("08:30");
        var now = DateTime.UtcNow;
        var isLate = now.TimeOfDay > workStart;
        
        var attendance = existing ?? new Attendance
        {
            EmployeeId = request.EmployeeId,
            AttendanceDate = today
        };
        
        attendance.ClockIn = now;
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
        var today = DateTime.UtcNow.Date;
        
        var attendance = await _context.Attendances
            .FirstOrDefaultAsync(a => a.EmployeeId == request.EmployeeId && a.AttendanceDate == today);
        
        if (attendance == null)
            return BadRequest("Not clocked in today");
        
        if (attendance.ClockOut != null)
            return BadRequest("Already clocked out today");
        
        var now = DateTime.UtcNow;
        attendance.ClockOut = now;
        attendance.ClockOutLatitude = request.Latitude;
        attendance.ClockOutLongitude = request.Longitude;
        attendance.ClockOutMethod = request.Method ?? "WEB";
        
        // Calculate work hours
        if (attendance.ClockIn != null)
        {
            var diff = now - attendance.ClockIn.Value;
            attendance.WorkHours = Math.Round((decimal)diff.TotalHours, 2);
        }
        
        var workEnd = TimeSpan.Parse("17:30");
        attendance.IsEarlyLeave = now.TimeOfDay < workEnd;
        
        await _context.SaveChangesAsync();
        
        return Ok(attendance);
    }
    
    /// <summary>
    /// Manual attendance entry
    /// </summary>
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
