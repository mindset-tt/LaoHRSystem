using FluentAssertions;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LaoHR.Tests.Integration.Services;

public class PayrollServiceOvertimeTests
{
    private readonly LaoHRDbContext _context;
    private readonly PayrollService _service;

    public PayrollServiceOvertimeTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new LaoHRDbContext(options);
        _service = new PayrollService(_context);
        
        // Seed default time settings
        _context.SystemSettings.Add(new SystemSetting { SettingKey = "WORK_START_TIME", SettingValue = "08:30" });
        _context.SystemSettings.Add(new SystemSetting { SettingKey = "WORK_END_TIME", SettingValue = "17:30" });
        _context.SaveChanges();
    }

    [Fact]
    public async Task CalculateOvertime_NormalDay_NoOT_ShouldReturnZero()
    {
        // 8M salary -> Hourly = 8M / 26 / 8 = 38,461.54
        decimal salary = 8000000m;
        
        // Tuesday (Normal Day)
        var date = new DateTime(2026, 6, 2); 
        
        _context.Attendances.Add(new Attendance 
        { 
            EmployeeId = 1, 
            AttendanceDate = date, 
            ClockIn = date.AddHours(8.5), // 08:30
            ClockOut = date.AddHours(17.5) // 17:30
        });
        await _context.SaveChangesAsync();

        var ot = await _service.CalculateOvertimePay(1, date, date, salary);
        
        ot.Should().Be(0);
    }

    [Fact]
    public async Task CalculateOvertime_NormalDay_EveningOT_ShouldCalculate150Percent()
    {
        // Hourly: 100,000 (Example for easy math) -> Salary = 100k * 8 * 26 = 20,800,000
        decimal salary = 20800000m; // Hourly = 100,000
        
        // Wednesday (Normal Day)
        var date = new DateTime(2026, 6, 3);
        
        // Work until 19:30 (2 hours OT)
        // Normal end 17:30.
        // 17:30 - 19:30 is in bucket 17:00-22:00?
        // Wait, logic says bucket starts 06:00 - 22:00 @ 150%. 
        // But normal hours (08:30-17:30) are forcibly deducted.
        // So 17:30-19:30 remains. 2 Hours.
        // 2 Hours * 100k * 1.5 = 300,000
        
        _context.Attendances.Add(new Attendance 
        { 
            EmployeeId = 2, 
            AttendanceDate = date, 
            ClockIn = date.AddHours(8.5), // 08:30
            ClockOut = date.AddHours(19.5) // 19:30
        });
        await _context.SaveChangesAsync();

        var ot = await _service.CalculateOvertimePay(2, date, date, salary);
        
        ot.Should().BeApproximately(300000m, 1.0m);
    }

    [Fact]
    public async Task CalculateOvertime_Weekend_Morning_ShouldCalculate250Percent()
    {
        decimal salary = 20800000m; // Hourly 100k
        
        // Sunday
        var date = new DateTime(2026, 6, 7); 
        
        // Work 08:00 - 12:00 (4 Hours)
        // Weekend Bucket 06:00-16:00 is 250%
        // 4 * 100k * 2.5 = 1,000,000
        
        _context.Attendances.Add(new Attendance 
        { 
            EmployeeId = 3, 
            AttendanceDate = date, 
            ClockIn = date.AddHours(8), 
            ClockOut = date.AddHours(12) 
        });
        await _context.SaveChangesAsync();
        
        var ot = await _service.CalculateOvertimePay(3, date, date, salary);
        
        ot.Should().BeApproximately(1000000m, 1.0m);
    }
    
    [Fact]
    public async Task CalculateOvertime_Holiday_ShouldIdentifyHoliday()
    {
        decimal salary = 20800000m; // Hourly 100k
        
        // Monday (Normal Day usually) but Holiday
        var date = new DateTime(2026, 6, 8); 
        
        _context.Holidays.Add(new Holiday { Date = date, Name = "Test Holiday", Year = 2026 });
        
        _context.Attendances.Add(new Attendance 
        { 
            EmployeeId = 4, 
            AttendanceDate = date, 
            ClockIn = date.AddHours(8), 
            ClockOut = date.AddHours(10) // 2 Hours
        });
        await _context.SaveChangesAsync();

        // 2 Hours * 250% = 5 * 100k = 500,000
        var ot = await _service.CalculateOvertimePay(4, date, date, salary);
        
        ot.Should().BeApproximately(500000m, 1.0m);
    }
}
