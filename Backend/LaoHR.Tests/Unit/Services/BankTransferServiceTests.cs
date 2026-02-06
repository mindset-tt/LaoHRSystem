using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class BankTransferServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly BankTransferService _service;

    public BankTransferServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new LaoHRDbContext(options);
        _service = new BankTransferService(_context);
    }

    [Fact]
    public async Task GenerateBcelTransferFile_ReturnsCorrectFormat()
    {
        // Arrange
        var period = new PayrollPeriod { PeriodId = 1, Month = 1, Year = 2024, PeriodName = "Jan 2024" };
        _context.PayrollPeriods.Add(period);
        
        var emp = new Employee { EmployeeId = 1, EmployeeCode = "E1", BankAccount = "123456", BankName = "BCEL" };
        _context.Employees.Add(emp);
        
        _context.SalarySlips.Add(new SalarySlip 
        { 
            EmployeeId = 1, 
            PeriodId = 1, 
            NetSalary = 5000000, 
 
        });
        
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GenerateBcelTransferFile(1);
        var content = Encoding.UTF8.GetString(result);

        // Assert
        // H,YYYYMMDD,COMPANY_ACCOUNT,TOTAL_RECORDS,TOTAL_AMOUNT
        content.Should().StartWith("H,");
        content.Should().Contain("1011111111111"); // Company Acc
        content.Should().Contain("5000000"); // Amount
        content.Should().Contain($"D,{emp.BankAccount}"); // Detail
    }
}
