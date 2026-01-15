using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/settings")]
public class WorkScheduleController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IWorkDayService _workDayService;

    public WorkScheduleController(LaoHRDbContext context, IWorkDayService workDayService)
    {
        _context = context;
        _workDayService = workDayService;
    }

    /// <summary>
    /// Get the work schedule
    /// </summary>
    [HttpGet("work-schedule")]
    public async Task<ActionResult<WorkSchedule>> GetWorkSchedule()
    {
        return await _workDayService.GetWorkScheduleAsync();
    }

    /// <summary>
    /// Update or create work schedule
    /// </summary>
    [HttpPut("work-schedule")]
    public async Task<ActionResult<WorkSchedule>> UpdateWorkSchedule([FromBody] UpdateWorkScheduleDto dto)
    {
        var schedule = await _context.WorkSchedules.FirstOrDefaultAsync();
        
        if (schedule == null)
        {
            schedule = new WorkSchedule();
            _context.WorkSchedules.Add(schedule);
        }
        
        schedule.Monday = dto.Monday;
        schedule.Tuesday = dto.Tuesday;
        schedule.Wednesday = dto.Wednesday;
        schedule.Thursday = dto.Thursday;
        schedule.Friday = dto.Friday;
        schedule.Saturday = dto.Saturday;
        schedule.Sunday = dto.Sunday;
        schedule.WorkStartTime = TimeSpan.Parse(dto.WorkStartTime);
        schedule.WorkEndTime = TimeSpan.Parse(dto.WorkEndTime);
        schedule.BreakStartTime = TimeSpan.Parse(dto.BreakStartTime);
        schedule.BreakEndTime = TimeSpan.Parse(dto.BreakEndTime);
        schedule.LateThresholdMinutes = dto.LateThresholdMinutes;
        
        // Saturday configuration
        schedule.SaturdayWorkType = dto.SaturdayWorkType;
        schedule.SaturdayHours = dto.SaturdayHours;
        schedule.SaturdayWeeks = dto.SaturdayWeeks;
        if (!string.IsNullOrEmpty(dto.SaturdayStartTime))
            schedule.SaturdayStartTime = TimeSpan.Parse(dto.SaturdayStartTime);
        if (!string.IsNullOrEmpty(dto.SaturdayEndTime))
            schedule.SaturdayEndTime = TimeSpan.Parse(dto.SaturdayEndTime);
        
        // Laos law standard
        schedule.StandardMonthlyHours = dto.StandardMonthlyHours;
        schedule.DailyWorkHours = dto.DailyWorkHours;
        
        schedule.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return schedule;
    }

    /// <summary>
    /// Calculate work days between two dates
    /// </summary>
    [HttpGet("workdays/calculate")]
    public async Task<ActionResult<WorkDayBreakdown>> CalculateWorkDays(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        return await _workDayService.GetWorkDayBreakdownAsync(startDate, endDate);
    }
}

public class UpdateWorkScheduleDto
{
    public bool Monday { get; set; } = true;
    public bool Tuesday { get; set; } = true;
    public bool Wednesday { get; set; } = true;
    public bool Thursday { get; set; } = true;
    public bool Friday { get; set; } = true;
    public bool Saturday { get; set; } = false;
    public bool Sunday { get; set; } = false;
    public string WorkStartTime { get; set; } = "08:00:00";
    public string WorkEndTime { get; set; } = "17:00:00";
    public string BreakStartTime { get; set; } = "12:00:00";
    public string BreakEndTime { get; set; } = "13:00:00";
    public int LateThresholdMinutes { get; set; } = 15;
    
    // Saturday configuration
    public string SaturdayWorkType { get; set; } = "NONE";
    public decimal SaturdayHours { get; set; } = 0;
    public string SaturdayWeeks { get; set; } = "";
    public string? SaturdayStartTime { get; set; }
    public string? SaturdayEndTime { get; set; }
    
    // Laos law standard
    public decimal StandardMonthlyHours { get; set; } = 160;
    public decimal DailyWorkHours { get; set; } = 8;
}

