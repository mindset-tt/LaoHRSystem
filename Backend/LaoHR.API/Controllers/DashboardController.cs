using LaoHR.API.Data;
using LaoHR.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public DashboardController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStats>> GetStats()
    {
        var totalEmployees = await _context.Employees.CountAsync();
        var activeEmployees = await _context.Employees.CountAsync(e => e.IsActive);
        
        var today = DateTime.UtcNow.Date;
        var presentToday = await _context.Attendances
            .CountAsync(a => a.AttendanceDate.Date == today && a.Status == "PRESENT");
            
        var pendingLeave = await _context.LeaveRequests
            .CountAsync(l => l.Status == "PENDING");

        return Ok(new DashboardStats
        {
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            PresentToday = presentToday,
            PendingLeave = pendingLeave
        });
    }
}

public class DashboardStats
{
    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int PresentToday { get; set; }
    public int PendingLeave { get; set; }
}
