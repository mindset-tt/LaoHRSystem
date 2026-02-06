using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LaoHR.API.Services;

/// <summary>
/// Service for managing leave balances, accrual, and carry-over
/// </summary>
public interface ILeaveService
{
    /// <summary>
    /// Initialize yearly leave balances for all active employees
    /// Call this at the start of each year or when an employee is hired
    /// </summary>
    Task InitializeYearlyBalancesAsync(int year);
    
    /// <summary>
    /// Process monthly accrual for employees with accrual-based policies
    /// Call this at the start of each month
    /// </summary>
    Task ProcessMonthlyAccrualAsync(int year, int month);
    
    /// <summary>
    /// Process carry-over from previous year
    /// Call this at the start of the new year before initializing balances
    /// </summary>
    Task ProcessYearEndCarryOverAsync(int fromYear, int toYear);
    
    /// <summary>
    /// Get leave balance for a specific employee
    /// </summary>
    Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int year);
}

public class LeaveService : ILeaveService
{
    private readonly LaoHRDbContext _context;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(LaoHRDbContext context, ILogger<LeaveService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeYearlyBalancesAsync(int year)
    {
        _logger.LogInformation("Initializing yearly leave balances for {Year}", year);
        
        var policies = await _context.LeavePolicies.Where(p => p.IsActive).ToListAsync();
        var employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
        
        foreach (var employee in employees)
        {
            foreach (var policy in policies)
            {
                // Check if balance already exists
                var exists = await _context.LeaveBalances.AnyAsync(
                    lb => lb.EmployeeId == employee.EmployeeId 
                       && lb.LeaveType == policy.LeaveType 
                       && lb.Year == year);
                
                if (!exists)
                {
                    var balance = new LeaveBalance
                    {
                        EmployeeId = employee.EmployeeId,
                        LeaveType = policy.LeaveType,
                        Year = year,
                        TotalDays = policy.AnnualQuota,
                        UsedDays = 0,
                        CarriedOverDays = 0
                    };
                    _context.LeaveBalances.Add(balance);
                }
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Initialized leave balances for {Count} employees", employees.Count);
    }

    public async Task ProcessMonthlyAccrualAsync(int year, int month)
    {
        _logger.LogInformation("Processing monthly accrual for {Year}-{Month}", year, month);
        
        var policies = await _context.LeavePolicies
            .Where(p => p.IsActive && p.AccrualPerMonth > 0)
            .ToListAsync();
        
        var employees = await _context.Employees.Where(e => e.IsActive).ToListAsync();
        
        foreach (var policy in policies)
        {
            foreach (var employee in employees)
            {
                // Only accrue if employee was hired before this month
                if (employee.HireDate.HasValue && employee.HireDate.Value <= new DateTime(year, month, 1))
                {
                    var balance = await _context.LeaveBalances.FirstOrDefaultAsync(
                        lb => lb.EmployeeId == employee.EmployeeId 
                           && lb.LeaveType == policy.LeaveType 
                           && lb.Year == year);
                    
                    if (balance != null)
                    {
                        balance.TotalDays += policy.AccrualPerMonth;
                        
                        // Cap at annual quota + carry over
                        var maxDays = policy.AnnualQuota + policy.MaxCarryOver;
                        if (balance.TotalDays > maxDays)
                        {
                            balance.TotalDays = maxDays;
                        }
                    }
                }
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Monthly accrual processed for {Year}-{Month}", year, month);
    }

    public async Task ProcessYearEndCarryOverAsync(int fromYear, int toYear)
    {
        _logger.LogInformation("Processing carry-over from {FromYear} to {ToYear}", fromYear, toYear);
        
        var policies = await _context.LeavePolicies
            .Where(p => p.IsActive && p.MaxCarryOver > 0)
            .ToListAsync();
        
        foreach (var policy in policies)
        {
            // Get all balances for the previous year
            var previousBalances = await _context.LeaveBalances
                .Where(lb => lb.LeaveType == policy.LeaveType && lb.Year == fromYear)
                .ToListAsync();
            
            foreach (var prevBalance in previousBalances)
            {
                var remaining = prevBalance.TotalDays + prevBalance.CarriedOverDays - prevBalance.UsedDays;
                var carryOver = Math.Min((int)remaining, policy.MaxCarryOver);
                
                if (carryOver > 0)
                {
                    // Find or create new year's balance
                    var newBalance = await _context.LeaveBalances.FirstOrDefaultAsync(
                        lb => lb.EmployeeId == prevBalance.EmployeeId 
                           && lb.LeaveType == policy.LeaveType 
                           && lb.Year == toYear);
                    
                    if (newBalance != null)
                    {
                        newBalance.CarriedOverDays = carryOver;
                    }
                    else
                    {
                        // Create new balance with carry-over
                        newBalance = new LeaveBalance
                        {
                            EmployeeId = prevBalance.EmployeeId,
                            LeaveType = policy.LeaveType,
                            Year = toYear,
                            TotalDays = policy.AnnualQuota,
                            UsedDays = 0,
                            CarriedOverDays = carryOver
                        };
                        _context.LeaveBalances.Add(newBalance);
                    }
                }
            }
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Carry-over processed from {FromYear} to {ToYear}", fromYear, toYear);
    }

    public async Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int year)
    {
        return await _context.LeaveBalances
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
            .ToListAsync();
    }
}
