using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LaoHR.API.Services;

/// <summary>
/// Service for calculating working days, respecting work schedule and holidays
/// </summary>
public interface IWorkDayService
{
    /// <summary>
    /// Calculate the number of work days between two dates (inclusive)
    /// Excludes non-work days and holidays
    /// </summary>
    Task<decimal> CalculateWorkDaysAsync(DateTime startDate, DateTime endDate, bool isHalfDay = false);
    
    /// <summary>
    /// Check if a specific date is a work day
    /// </summary>
    Task<bool> IsWorkDayAsync(DateTime date);
    
    /// <summary>
    /// Check if a specific date is a holiday
    /// </summary>
    Task<bool> IsHolidayAsync(DateTime date);
    
    /// <summary>
    /// Get the work schedule
    /// </summary>
    Task<WorkSchedule> GetWorkScheduleAsync();
    
    /// <summary>
    /// Get holidays for a date range
    /// </summary>
    Task<List<Holiday>> GetHolidaysAsync(int year);
    
    /// <summary>
    /// Get detailed breakdown of days in a range
    /// </summary>
    Task<WorkDayBreakdown> GetWorkDayBreakdownAsync(DateTime startDate, DateTime endDate);
}

public class WorkDayBreakdown
{
    public int TotalCalendarDays { get; set; }
    public decimal WorkDays { get; set; }
    public int NonWorkDays { get; set; }
    public int Holidays { get; set; }
    public List<DateTime> HolidayDates { get; set; } = new();
    public List<DateTime> WeekendDates { get; set; } = new();
}

public class WorkDayService : IWorkDayService
{
    private readonly LaoHRDbContext _context;
    private readonly ILogger<WorkDayService> _logger;

    public WorkDayService(LaoHRDbContext context, ILogger<WorkDayService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<WorkSchedule> GetWorkScheduleAsync()
    {
        var schedule = await _context.WorkSchedules.FirstOrDefaultAsync();
        
        // Return default schedule if none exists
        if (schedule == null)
        {
            schedule = new WorkSchedule
            {
                Monday = true,
                Tuesday = true,
                Wednesday = true,
                Thursday = true,
                Friday = true,
                Saturday = false,
                Sunday = false,
                WorkStartTime = new TimeSpan(8, 0, 0),
                WorkEndTime = new TimeSpan(17, 0, 0)
            };
        }
        
        return schedule;
    }

    public async Task<List<Holiday>> GetHolidaysAsync(int year)
    {
        var holidays = await _context.Holidays
            .Where(h => h.IsActive)
            .ToListAsync();
        
        // Filter to holidays that apply to this year
        return holidays.Where(h => 
            h.Date.Year == year || 
            (h.IsRecurring && h.Date.Month >= 1)
        ).ToList();
    }

    public async Task<bool> IsHolidayAsync(DateTime date)
    {
        var dateOnly = date.Date;
        
        return await _context.Holidays.AnyAsync(h => 
            h.IsActive && (
                h.Date.Date == dateOnly ||
                (h.IsRecurring && h.Date.Month == dateOnly.Month && h.Date.Day == dateOnly.Day)
            ));
    }

    public async Task<bool> IsWorkDayAsync(DateTime date)
    {
        var schedule = await GetWorkScheduleAsync();
        
        // Check if it's a work day based on schedule
        if (!schedule.IsWorkDay(date.DayOfWeek))
            return false;
        
        // Check if it's a holiday
        if (await IsHolidayAsync(date))
            return false;
        
        return true;
    }

    public async Task<decimal> CalculateWorkDaysAsync(DateTime startDate, DateTime endDate, bool isHalfDay = false)
    {
        if (isHalfDay)
        {
            // Half day always counts as 0.5 work days if it's a work day
            return await IsWorkDayAsync(startDate) ? 0.5m : 0;
        }
        
        var schedule = await GetWorkScheduleAsync();
        var holidays = await GetHolidaysForRangeAsync(startDate, endDate);
        
        decimal workDays = 0;
        var current = startDate.Date;
        var end = endDate.Date;
        
        while (current <= end)
        {
            // Check if it's a work day based on schedule
            if (schedule.IsWorkDay(current.DayOfWeek))
            {
                // Check if it's not a holiday
                if (!IsHolidayInList(current, holidays))
                {
                    workDays++;
                }
            }
            
            current = current.AddDays(1);
        }
        
        return workDays;
    }

    public async Task<WorkDayBreakdown> GetWorkDayBreakdownAsync(DateTime startDate, DateTime endDate)
    {
        var schedule = await GetWorkScheduleAsync();
        var holidays = await GetHolidaysForRangeAsync(startDate, endDate);
        
        var breakdown = new WorkDayBreakdown();
        var current = startDate.Date;
        var end = endDate.Date;
        
        while (current <= end)
        {
            breakdown.TotalCalendarDays++;
            
            if (!schedule.IsWorkDay(current.DayOfWeek))
            {
                breakdown.NonWorkDays++;
                breakdown.WeekendDates.Add(current);
            }
            else if (IsHolidayInList(current, holidays))
            {
                breakdown.Holidays++;
                breakdown.HolidayDates.Add(current);
            }
            else
            {
                breakdown.WorkDays++;
            }
            
            current = current.AddDays(1);
        }
        
        return breakdown;
    }

    private async Task<List<Holiday>> GetHolidaysForRangeAsync(DateTime startDate, DateTime endDate)
    {
        var holidays = await _context.Holidays
            .Where(h => h.IsActive)
            .ToListAsync();
        
        // Get all years in the range
        var years = Enumerable.Range(startDate.Year, endDate.Year - startDate.Year + 1);
        
        var result = new List<Holiday>();
        foreach (var holiday in holidays)
        {
            if (holiday.IsRecurring)
            {
                // Recurring holiday - check for each year
                foreach (var year in years)
                {
                    var dateThisYear = new DateTime(year, holiday.Date.Month, holiday.Date.Day);
                    if (dateThisYear >= startDate.Date && dateThisYear <= endDate.Date)
                    {
                        result.Add(new Holiday 
                        { 
                            Date = dateThisYear, 
                            Name = holiday.Name, 
                            NameLao = holiday.NameLao 
                        });
                    }
                }
            }
            else
            {
                // One-time holiday
                if (holiday.Date >= startDate.Date && holiday.Date <= endDate.Date)
                {
                    result.Add(holiday);
                }
            }
        }
        
        return result;
    }

    private static bool IsHolidayInList(DateTime date, List<Holiday> holidays)
    {
        return holidays.Any(h => h.Date.Date == date.Date);
    }
}
