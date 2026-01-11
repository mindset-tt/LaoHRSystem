using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Payroll calculation service with Lao NSSF and progressive tax logic
/// </summary>
public class PayrollService
{
    private readonly LaoHRDbContext _context;
    
    // Cached settings
    private decimal _nssfCeilingBase = 4500000m;
    private decimal _nssfEmployeeRate = 0.055m;
    private decimal _nssfEmployerRate = 0.060m;
    private decimal _exRateUsd = 22000m;
    private decimal _exRateThb = 650m;
    private List<TaxBracket>? _taxBrackets;
    
    public PayrollService(LaoHRDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Load current settings from database
    /// </summary>
    public async Task LoadSettingsAsync()
    {
        var settings = await _context.SystemSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
        
        if (settings.TryGetValue("NSSF_CEILING_BASE", out var ceiling))
            _nssfCeilingBase = decimal.Parse(ceiling);
        if (settings.TryGetValue("NSSF_EMPLOYEE_RATE", out var empRate))
            _nssfEmployeeRate = decimal.Parse(empRate);
        if (settings.TryGetValue("NSSF_EMPLOYER_RATE", out var erRate))
            _nssfEmployerRate = decimal.Parse(erRate);
        if (settings.TryGetValue("EX_RATE_USD", out var usd))
            _exRateUsd = decimal.Parse(usd);
        if (settings.TryGetValue("EX_RATE_THB", out var thb))
            _exRateThb = decimal.Parse(thb);
        
        _taxBrackets = await _context.TaxBrackets
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();
    }
    
    /// <summary>
    /// Calculate NSSF deductions
    /// </summary>
    public (decimal nssfBase, decimal employeeDeduction, decimal employerContribution) CalculateNSSF(decimal grossSalary)
    {
        // NSSF is calculated on base salary up to ceiling
        decimal nssfBase = Math.Min(grossSalary, _nssfCeilingBase);
        decimal employeeDeduction = Math.Round(nssfBase * _nssfEmployeeRate, 2);
        decimal employerContribution = Math.Round(nssfBase * _nssfEmployerRate, 2);
        
        return (nssfBase, employeeDeduction, employerContribution);
    }
    
    /// <summary>
    /// Calculate progressive income tax (Lao PIT)
    /// </summary>
    public decimal CalculateProgressiveTax(decimal taxableIncome)
    {
        if (_taxBrackets == null || !_taxBrackets.Any())
            return 0;
        
        decimal totalTax = 0;
        decimal remainingIncome = taxableIncome;
        
        foreach (var bracket in _taxBrackets)
        {
            if (remainingIncome <= 0) break;
            
            // Calculate taxable amount in this bracket
            decimal bracketRange = bracket.MaxIncome - bracket.MinIncome + 1;
            decimal taxableInBracket = Math.Min(remainingIncome, bracketRange);
            
            // Apply tax rate
            totalTax += Math.Round(taxableInBracket * bracket.TaxRate, 2);
            remainingIncome -= taxableInBracket;
        }
        
        return totalTax;
    }
    
    /// <summary>
    /// Calculate complete salary breakdown
    /// </summary>
    public SalaryCalculation CalculateSalary(decimal baseSalary, decimal overtimePay = 0, decimal allowances = 0, decimal otherDeductions = 0, int dependentCount = 0)
    {
        decimal grossIncome = baseSalary + overtimePay + allowances;
        
        // Calculate NSSF
        var (nssfBase, nssfEmployee, nssfEmployer) = CalculateNSSF(grossIncome);
        
        // Taxable income = Gross - NSSF employee deduction
        decimal taxableIncome = grossIncome - nssfEmployee;

        // Article 52 Deduction: Family Support
        // 5,000,000 LAK per person per year => ~416,667 per month
        // Max 3 persons
        // Note: For now we assume standard deduction. In a real scenario we'd fetch this from Employee properties.
        // We need to pass employee object or dependent count to CalculateSalary.
        
        // This method signature update requires careful refactoring.
        // For now, I will add an optional parameter for dependentCount to avoid breaking existing calls immediately
        // but typically this service should access the employee data.
        // Wait, the caller ProcessPayrollAsync has 'emp' object. I should update CalculateSalary signature.
        
        // Let's check the method signature in the file content again. It's public.
        // I will add 'int dependentCount = 0' to arguments.
        
        decimal familyDeduction = Math.Min(dependentCount, 3) * 416667m;
        taxableIncome = Math.Max(0, taxableIncome - familyDeduction);
        
        // Calculate progressive tax
        decimal taxDeduction = CalculateProgressiveTax(taxableIncome);
        
        // Net salary
        decimal netSalary = grossIncome - nssfEmployee - taxDeduction - otherDeductions;
        
        return new SalaryCalculation
        {
            BaseSalary = baseSalary,
            OvertimePay = overtimePay,
            Allowances = allowances,
            GrossIncome = grossIncome,
            NssfBase = nssfBase,
            NssfEmployeeDeduction = nssfEmployee,
            NssfEmployerContribution = nssfEmployer,
            TaxableIncome = taxableIncome,
            TaxDeduction = taxDeduction,
            OtherDeductions = otherDeductions,
            NetSalary = netSalary,
            FamilyDeduction = familyDeduction
        };
    }
    
    /// <summary>
    /// Calculate overtime pay based on attendance records (Articles 114-116)
    /// </summary>
    public async Task<decimal> CalculateOvertimePay(int employeeId, DateTime startDate, DateTime endDate, decimal baseSalary)
    {
        // Article 114: Hourly rate = Salary / 26 days / 8 hours
        decimal hourlyRate = baseSalary / 26 / 8;
        decimal totalOvertimePay = 0;

        var attendances = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId && a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
            .ToListAsync();

        var holidays = await _context.Holidays
            .Where(h => h.Date >= startDate && h.Date <= endDate)
            .Select(h => h.Date)
            .ToListAsync();
            
        // Get work schedule settings
        var settings = await _context.SystemSettings.ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);
        // Default 08:30 - 17:30
        TimeSpan workStart = TimeSpan.Parse(settings.GetValueOrDefault("WORK_START_TIME", "08:30"));
        TimeSpan workEnd = TimeSpan.Parse(settings.GetValueOrDefault("WORK_END_TIME", "17:30"));

        foreach (var att in attendances)
        {
            if (att.ClockIn == null || att.ClockOut == null) continue;

            DateTime clockIn = att.ClockIn.Value;
            DateTime clockOut = att.ClockOut.Value;
            
            // If clockOut is before clockIn (e.g. overnight shift error), skip or fix. 
            // Assuming valid data for now. If clockOut < clockIn, it might mean next day, but data usually stores absolute DateTime.
            if (clockOut <= clockIn) continue;

            bool isHoliday = holidays.Contains(att.AttendanceDate.Date);
            bool isWeekend = att.AttendanceDate.DayOfWeek == DayOfWeek.Saturday || att.AttendanceDate.DayOfWeek == DayOfWeek.Sunday;
            
            // Define Rate Buckets for this day
            var buckets = new List<RateBucket>();
            
            if (isHoliday || isWeekend)
            {
                // Article 115: Weekly Rest or Holiday
                // 06:00 - 16:00: 250%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(6), att.AttendanceDate.Date.AddHours(16), 2.5m));
                // 16:00 - 22:00: 300%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(16), att.AttendanceDate.Date.AddHours(22), 3.0m));
                // 22:00 - 06:00 (Next Day): 350%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(22), att.AttendanceDate.Date.AddDays(1).AddHours(6), 3.5m));
                
                // Also cover early morning before 06:00? The law implies 22:00-06:00 is night.
                // So 00:00 - 06:00 on the day itself should also be 350% (continuation of previous night).
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(0), att.AttendanceDate.Date.AddHours(6), 3.5m));
            }
            else
            {
                // Article 114: Normal Work Day
                // Normal hours are NOT overtime. We must exclude 08:30-17:30 (or configured work hours).
                // Actually, OT is usually calculated ONLY outside work hours.
                // But if someone works through lunch or comes early, that's OT.
                
                // 17:00 - 22:00: 150%
                // Note: There's an overlap if work ends at 17:30.
                // Strictly speaking, OT starts after work ends.
                // If work ends at 17:30, 17:00-17:30 is NOT OT.
                // We will handle "Is Working Hour" exclusion logic below.
                
                // 06:00 - 22:00: 150% (General Day OT) - Specific window 17-22 is 150%, assuming general day OT is also 150%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(6), att.AttendanceDate.Date.AddHours(22), 1.5m));
                
                // 22:00 - 06:00 (Next Day): 200%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(22), att.AttendanceDate.Date.AddDays(1).AddHours(6), 2.0m));
                
                // Early Morning 00:00 - 06:00: 200%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(0), att.AttendanceDate.Date.AddHours(6), 2.0m));
            }

            // Normal Working Hours (to exclude from OT)
            DateTime workStartDateTime = att.AttendanceDate.Date.Add(workStart);
            DateTime workEndDateTime = att.AttendanceDate.Date.Add(workEnd);
            
            // Calculate Pay for this record
            foreach (var bucket in buckets)
            {
                // Find overlap between (ClockIn, ClockOut) and Bucket
                DateTime overlapStart = clockIn > bucket.Start ? clockIn : bucket.Start;
                DateTime overlapEnd = clockOut < bucket.End ? clockOut : bucket.End;
                
                if (overlapStart < overlapEnd)
                {
                    // If this is a work day, exclude normal working hours from OT
                    if (!isHoliday && !isWeekend)
                    {
                        // We need to subtract any time that falls within Normal Work Hours
                        // Simple way: Calculate intersection of (OverlapStart, OverlapEnd) and (WorkStart, WorkEnd)
                        DateTime workOverlapStart = overlapStart > workStartDateTime ? overlapStart : workStartDateTime;
                        DateTime workOverlapEnd = overlapEnd < workEndDateTime ? overlapEnd : workEndDateTime;
                        
                        decimal totalHours = (decimal)(overlapEnd - overlapStart).TotalHours;
                        decimal deductedHours = 0;
                        
                        if (workOverlapStart < workOverlapEnd)
                        {
                            deductedHours = (decimal)(workOverlapEnd - workOverlapStart).TotalHours;
                        }
                        
                        decimal payableHours = totalHours - deductedHours;
                        if (payableHours > 0)
                        {
                            totalOvertimePay += payableHours * hourlyRate * bucket.Rate;
                        }
                    }
                    else
                    {
                        // Holiday/Weekend: All hours are OT (simplification: assuming no "normal hours" on these days)
                        // Or does "weekly rest day" mean NO work is normal? Yes.
                        decimal hours = (decimal)(overlapEnd - overlapStart).TotalHours;
                        totalOvertimePay += hours * hourlyRate * bucket.Rate;
                    }
                }
            }
        }

        return Math.Round(totalOvertimePay, 2);
    }

    private class RateBucket
    {
        public DateTime Start { get; }
        public DateTime End { get; }
        public decimal Rate { get; }

        public RateBucket(DateTime start, DateTime end, decimal rate)
        {
            Start = start;
            End = end;
            Rate = rate;
        }
    }
    
    /// <summary>
    /// Process payroll for all active employees in a period
    /// </summary>
    public async Task<List<SalarySlip>> ProcessPayrollAsync(int periodId)
    {
        await LoadSettingsAsync();
        
        var period = await _context.PayrollPeriods.FindAsync(periodId);
        if (period == null) throw new InvalidOperationException("Period not found");
        
        var activeEmployees = await _context.Employees
            .Where(e => e.IsActive)
            .ToListAsync();
        
        // Remove existing slips for this period
        var existingSlips = await _context.SalarySlips
            .Where(s => s.PeriodId == periodId)
            .ToListAsync();
        _context.SalarySlips.RemoveRange(existingSlips);
        
        var newSlips = new List<SalarySlip>();
        
        foreach (var emp in activeEmployees)
        {
            // Convert Base Salary to LAK if dealing with foreign currency
            decimal effectiveBaseSalary = emp.BaseSalary;
            if (emp.SalaryCurrency == "USD")
            {
                effectiveBaseSalary = Math.Round(emp.BaseSalary * _exRateUsd, 2);
            }
            else if (emp.SalaryCurrency == "THB")
            {
                effectiveBaseSalary = Math.Round(emp.BaseSalary * _exRateThb, 2);
            }

            // Auto-calculate Overtime using LAK base salary
            decimal overtimePay = await CalculateOvertimePay(emp.EmployeeId, period.StartDate, period.EndDate, effectiveBaseSalary);
            
            // Calculate final salary components (using LAK values)
            var calc = CalculateSalary(effectiveBaseSalary, overtimePay: overtimePay, dependentCount: emp.DependentCount);
            
            var slip = new SalarySlip
            {
                EmployeeId = emp.EmployeeId,
                PeriodId = periodId,
                BaseSalary = calc.BaseSalary,
                OvertimePay = calc.OvertimePay,
                Allowances = calc.Allowances,
                GrossIncome = calc.GrossIncome,
                NssfBase = calc.NssfBase,
                NssfEmployeeDeduction = calc.NssfEmployeeDeduction,
                NssfEmployerContribution = calc.NssfEmployerContribution,
                TaxableIncome = calc.TaxableIncome,
                TaxDeduction = calc.TaxDeduction,
                OtherDeductions = calc.OtherDeductions,
                NetSalary = calc.NetSalary,
                Status = "CALCULATED"
            };
            
            newSlips.Add(slip);
        }
        
        _context.SalarySlips.AddRange(newSlips);
        period.Status = "PROCESSING";
        
        await _context.SaveChangesAsync();
        
        return newSlips;
    }
}

/// <summary>
/// DTO for salary calculation result
/// </summary>
public class SalaryCalculation
{
    public decimal BaseSalary { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal Allowances { get; set; }
    public decimal GrossIncome { get; set; }
    public decimal NssfBase { get; set; }
    public decimal NssfEmployeeDeduction { get; set; }
    public decimal NssfEmployerContribution { get; set; }
    public decimal TaxableIncome { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal OtherDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public decimal FamilyDeduction { get; set; }
}
