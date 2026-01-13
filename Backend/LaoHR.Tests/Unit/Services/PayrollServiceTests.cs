using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class PayrollServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly PayrollService _service;

    public PayrollServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new LaoHRDbContext(options);
        _service = new PayrollService(_context);
        
        SeedSettings();
    }

    private void SeedSettings()
    {
        _context.SystemSettings.Add(new SystemSetting { SettingKey = "NSSF_CEILING_BASE", SettingValue = "4500000" });
        _context.SystemSettings.Add(new SystemSetting { SettingKey = "NSSF_EMPLOYEE_RATE", SettingValue = "0.055" });
        _context.SystemSettings.Add(new SystemSetting { SettingKey = "EX_RATE_USD", SettingValue = "22000" });
        
        _context.TaxBrackets.AddRange(
            new TaxBracket { MinIncome = 0, MaxIncome = 1300000, TaxRate = 0, SortOrder = 1, IsActive = true },
            new TaxBracket { MinIncome = 1300001, MaxIncome = 5000000, TaxRate = 0.05m, SortOrder = 2, IsActive = true },
            new TaxBracket { MinIncome = 5000001, MaxIncome = 15000000, TaxRate = 0.10m, SortOrder = 3, IsActive = true }
        );
        
        _context.SaveChanges();
    }

    [Fact]
    public async Task CalculateSalary_WithHighIncome_ShouldCapNSSF()
    {
        // Arrange
        await _service.LoadSettingsAsync();
        decimal salary = 10000000m; // 10M

        // Act
        var result = _service.CalculateSalary(salary);

        // Assert
        // Ceiling is 4.5M. 5.5% of 4.5M = 247,500
        result.NssfBase.Should().Be(4500000m);
        result.NssfEmployeeDeduction.Should().Be(247500m);
    }
    
    [Fact]
    public async Task CalculateSalary_TaxLogic_ShouldBeCorrect()
    {
        // Arrange
        await _service.LoadSettingsAsync();
        // 6,000,000 Gross
        // NSSF Base = Min(6M, 4.5M) = 4.5M
        // NSSF Employee = 4.5M * 5.5% = 247,500
        // Taxable Before Family = 6M - 247,500 = 5,752,500
        // Family Deduction (0 deps) = 0
        // Taxable = 5,752,500
        
        // Tax Calculation:
        // 0 - 1.3M (1.3M) @ 0% = 0
        // 1.3M - 5M (3.7M) @ 5% = 185,000
        // 5M - 15M (Remaining 752,500) @ 10% = 75,250
        // Total Tax = 260,250
        
        decimal salary = 6000000m;

        // Act
        var result = _service.CalculateSalary(salary);

        // Assert
        result.GrossIncome.Should().Be(6000000m);
        result.NssfEmployeeDeduction.Should().Be(247500m);
        result.TaxDeduction.Should().BeApproximately(260250m, 1.0m);
        result.NetSalary.Should().BeApproximately(6000000m - 247500m - 260250m, 1.0m);
    }
}
