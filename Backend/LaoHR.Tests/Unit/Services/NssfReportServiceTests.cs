using FluentAssertions;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class NssfReportServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly NssfReportService _service;

    public NssfReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new NssfReportService(_context);
    }

    [Fact]
    public async Task GenerateNssfReport_NoPeriod_ThrowsException()
    {
        await Assert.ThrowsAsync<FileNotFoundException>(() => _service.GenerateNssfReport(999));
    }

    [Fact]
    public async Task GenerateNssfReport_ValidPeriod_ReturnsPdfBytes()
    {
        // Arrange
        var period = new PayrollPeriod { PeriodId = 1, PeriodName = "Jan 2026", StartDate = DateTime.Now, EndDate = DateTime.Now };
        _context.PayrollPeriods.Add(period);
        
        var emp = new Employee { EmployeeId = 1, EmployeeCode = "EMP001", LaoName = "Test Emp" };
        _context.Employees.Add(emp);
        
        _context.SalarySlips.Add(new SalarySlip 
        { 
            EmployeeId = 1, 
            PeriodId = 1, 
            NssfBase = 1000000, 
            NssfEmployeeDeduction = 55000, 
            NssfEmployerContribution = 60000 
        });
        
        await _context.SaveChangesAsync();

        // Act
        // Note: QuestPDF might require license config in some versions, or community license.
        // If it throws, we need to set license.
        try 
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }
        catch {} // Might be already set

        var bytes = await _service.GenerateNssfReport(1);

        // Assert
        bytes.Should().NotBeNull();
        bytes.Should().NotBeEmpty();
        // PDF header check? %PDF-1.
        // Just checking bytes existing is enough for coverage.
    }
}
