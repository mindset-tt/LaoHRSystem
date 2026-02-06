using FluentAssertions;
using LaoHR.API.Services;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class PayslipPdfServiceTests
{
    private readonly PayslipPdfService _service;

    public PayslipPdfServiceTests()
    {
        _service = new PayslipPdfService();
    }

    [Fact]
    public void GeneratePayslip_ValidData_ReturnsPdfBytes()
    {
        // Arrange
        var slip = new SalarySlip { NetSalary = 1000000, BaseSalary = 1000000, GrossIncome = 1000000 };
        var emp = new Employee { 
            LaoName = "Test", 
            EmployeeCode = "E01",
            Department = new Department { DepartmentNameEn = "IT" }
        };
        var period = new PayrollPeriod { PeriodName = "Jan 2024" };

        // Act
        var result = _service.GeneratePayslip(slip, emp, period);

        // Assert
        result.Should().NotBeNull();
        result.Length.Should().BeGreaterThan(0);
        // We verify header signature of PDF if needed, but length > 0 is good enough for "it runs"
        result[0].Should().Be(0x25); // '%'
        result[1].Should().Be(0x50); // 'P'
        result[2].Should().Be(0x44); // 'D'
        result[3].Should().Be(0x46); // 'F'
    }
}
