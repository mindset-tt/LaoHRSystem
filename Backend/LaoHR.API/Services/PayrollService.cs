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
    private decimal _exRateCny = 3100m;  // Chinese Yuan
    private List<TaxBracket>? _taxBrackets;
    private WorkSchedule? _workSchedule;
    
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
        
        // Load work schedule
        _workSchedule = await _context.WorkSchedules.FirstOrDefaultAsync();
    }
    
    /// <summary>
    /// Get conversion rate for a currency from database
    /// Falls back to settings if no rate found
    /// </summary>
    public async Task<decimal> GetConversionRateAsync(string fromCurrency, string toCurrency = "LAK", DateTime? asOfDate = null)
    {
        if (fromCurrency == toCurrency) return 1m;
        
        var date = asOfDate ?? DateTime.UtcNow;
        
        // Try to find rate from database
        var rate = await _context.ConversionRates
            .Where(r => r.FromCurrency == fromCurrency && 
                        r.ToCurrency == toCurrency && 
                        r.IsActive &&
                        r.EffectiveDate <= date &&
                        (r.ExpiryDate == null || r.ExpiryDate > date))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();
        
        if (rate != null) return rate.Rate;
        
        // Fallback to system settings (approximate rates to LAK)
        return fromCurrency switch
        {
            "USD" => _exRateUsd,   // ~22,000 LAK
            "THB" => _exRateThb,   // ~650 LAK
            "CNY" => _exRateCny,   // ~3,100 LAK
            "LAK" => 1m,
            _ => 1m
        };
    }
    
    /// <summary>
    /// Get standard monthly hours based on work schedule
    /// Mon-Fri = 160 hours
    /// Mon-Sat (half day) = 180 hours
    /// Mon-Sat (full day) = 200 hours
    /// </summary>
    public decimal GetStandardMonthlyHours()
    {
        if (_workSchedule == null) return 160m;
        
        return _workSchedule.SaturdayWorkType switch
        {
            "HALF" => 180m,
            "FULL" => 200m,
            _ => 160m
        };
    }
    
    /// <summary>
    /// Check if a specific date is a scheduled work day (including Saturday logic)
    /// </summary>
    public bool IsScheduledWorkDay(DateTime date)
    {
        if (_workSchedule == null) 
        {
            // Default: Mon-Fri
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }
        
        // Sunday is never a work day
        if (date.DayOfWeek == DayOfWeek.Sunday) return false;
        
        // Saturday special handling
        if (date.DayOfWeek == DayOfWeek.Saturday)
        {
            if (!_workSchedule.Saturday || _workSchedule.SaturdayWorkType == "NONE")
                return false;
            
            // Check which week of month this Saturday falls on
            int weekOfMonth = GetWeekOfMonth(date);
            return _workSchedule.IsSaturdayWorkDay(weekOfMonth);
        }
        
        // Mon-Fri: Check the day flags
        return _workSchedule.IsWorkDay(date.DayOfWeek);
    }
    
    /// <summary>
    /// Get work hours for a specific day
    /// </summary>
    public decimal GetWorkHoursForDay(DateTime date)
    {
        if (_workSchedule == null) return 8m;
        
        if (date.DayOfWeek == DayOfWeek.Saturday && _workSchedule.SaturdayWorkType == "HALF")
            return 4m;
        
        return _workSchedule.DailyWorkHours;
    }
    
    /// <summary>
    /// Calculate which week of the month a date falls on (1-5)
    /// </summary>
    private int GetWeekOfMonth(DateTime date)
    {
        int day = date.Day;
        return (day - 1) / 7 + 1;
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
    /// <summary>
    /// Calculate complete salary breakdown
    /// </summary>
    public SalaryCalculation CalculateSalary(decimal baseSalary, decimal overtimePay = 0, List<PayrollAdjustment>? adjustments = null, int dependentCount = 0)
    {
        adjustments ??= new List<PayrollAdjustment>();
        
        // Earnings
        decimal taxableAllowances = adjustments.Where(a => a.Type == "EARNING" && a.IsTaxable).Sum(a => a.Amount);
        decimal nonTaxableAllowances = adjustments.Where(a => a.Type == "EARNING" && !a.IsTaxable).Sum(a => a.Amount);
        decimal bonuses = adjustments.Where(a => a.Type == "BONUS").Sum(a => a.Amount); // If dynamic bonus uses Type="BONUS"

        // Deductions (Pre-Tax?) - Usually deductions are just Net deductions, unless specific tax-exempt deductions exist.
        // Assuming Adjustments type DEDUCTION are net deductions (loan repayment etc).
        decimal otherDeductions = adjustments.Where(a => a.Type == "DEDUCTION").Sum(a => a.Amount);

        decimal grossIncome = baseSalary + overtimePay + taxableAllowances + bonuses;
        
        // Calculate NSSF
        var (nssfBase, nssfEmployee, nssfEmployer) = CalculateNSSF(grossIncome);
        
        // Taxable income = Gross - NSSF employee deduction
        decimal taxableIncome = grossIncome - nssfEmployee;

        // Article 52 Deduction: Family Support
        // 5,000,000 LAK per person per year => ~416,667 per month
        // Max 3 persons
        decimal familyDeduction = Math.Min(dependentCount, 3) * 416667m;
        taxableIncome = Math.Max(0, taxableIncome - familyDeduction);
        
        // Calculate progressive tax
        decimal taxDeduction = CalculateProgressiveTax(taxableIncome);
        
        // Net salary
        // Net = (Gross - NSSF - Tax) + NonTaxableEarnings - Deductions
        decimal netSalary = (grossIncome - nssfEmployee - taxDeduction) + nonTaxableAllowances - otherDeductions;
        
        return new SalaryCalculation
        {
            BaseSalary = baseSalary,
            OvertimePay = overtimePay,
            Allowances = taxableAllowances + nonTaxableAllowances, // Total for display
            Bonus = bonuses,
            GrossIncome = grossIncome, // Note: Gross normally includes Taxable only for tax calc purposes
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
    /// Now considers work schedule for Saturday handling
    /// </summary>
    public async Task<decimal> CalculateOvertimePay(int employeeId, DateTime startDate, DateTime endDate, decimal baseSalary)
    {
        // Ensure settings are loaded
        if (_workSchedule == null)
        {
            await LoadSettingsAsync();
        }
        
        // Article 114: Hourly rate = Salary / work_days / daily_hours
        // Dynamic calculation based on work schedule:
        // Mon-Fri: 20 days × 8 hrs
        // Mon-Sat(half): 23 days × 8 hrs average
        // Mon-Sat(full): 26 days × 8 hrs
        decimal workDaysPerMonth = _workSchedule?.GetWorkDaysPerMonth() ?? 26m;
        decimal dailyHours = _workSchedule?.DailyWorkHours ?? 8m;
        decimal hourlyRate = baseSalary / workDaysPerMonth / dailyHours;
        decimal totalOvertimePay = 0;

        var attendances = await _context.Attendances
            .Where(a => a.EmployeeId == employeeId && a.AttendanceDate >= startDate && a.AttendanceDate <= endDate)
            .ToListAsync();

        var holidays = await _context.Holidays
            .Where(h => h.Date >= startDate && h.Date <= endDate && h.IsActive)
            .Select(h => h.Date.Date)
            .ToListAsync();
            
        // Get work schedule settings
        TimeSpan workStart = _workSchedule?.WorkStartTime ?? new TimeSpan(8, 30, 0);
        TimeSpan workEnd = _workSchedule?.WorkEndTime ?? new TimeSpan(17, 30, 0);

        foreach (var att in attendances)
        {
            if (att.ClockIn == null || att.ClockOut == null) continue;

            DateTime clockIn = att.ClockIn.Value;
            DateTime clockOut = att.ClockOut.Value;
            
            if (clockOut <= clockIn) continue;

            bool isHoliday = holidays.Contains(att.AttendanceDate.Date);
            
            // Check if this is a scheduled work day (considers Saturday config)
            bool isScheduledWorkDay = IsScheduledWorkDay(att.AttendanceDate.Date);
            bool isWeekendOrNotScheduled = !isScheduledWorkDay;
            
            // Define Rate Buckets for this day
            var buckets = new List<RateBucket>();
            
            if (isHoliday || isWeekendOrNotScheduled)
            {
                // Article 115: Weekly Rest, Holiday, or Non-Scheduled Day
                // ALL hours worked are OT
                // 06:00 - 16:00: 250%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(6), att.AttendanceDate.Date.AddHours(16), 2.5m));
                // 16:00 - 22:00: 300%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(16), att.AttendanceDate.Date.AddHours(22), 3.0m));
                // 22:00 - 06:00 (Next Day): 350%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(22), att.AttendanceDate.Date.AddDays(1).AddHours(6), 3.5m));
                // 00:00 - 06:00 (continuation of previous night): 350%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(0), att.AttendanceDate.Date.AddHours(6), 3.5m));
            }
            else
            {
                // Article 114: Normal Work Day (including scheduled Saturday)
                // Only hours OUTSIDE normal work hours are OT
                
                // Get the work end time for this specific day
                TimeSpan dayWorkEnd = workEnd;
                if (att.AttendanceDate.DayOfWeek == DayOfWeek.Saturday && _workSchedule?.SaturdayWorkType == "HALF")
                {
                    // Half day Saturday ends earlier (e.g., 12:00)
                    dayWorkEnd = _workSchedule?.SaturdayEndTime ?? new TimeSpan(12, 0, 0);
                }
                
                // After work ends until 22:00: 150%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.Add(dayWorkEnd), att.AttendanceDate.Date.AddHours(22), 1.5m));
                
                // 22:00 - 06:00 (Next Day): 200%
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(22), att.AttendanceDate.Date.AddDays(1).AddHours(6), 2.0m));
                
                // Early Morning 00:00 - 06:00: 200% (before work start)
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(0), att.AttendanceDate.Date.AddHours(6), 2.0m));
                
                // Before work start until work start: 150% (e.g., 06:00 - 08:30)
                buckets.Add(new RateBucket(att.AttendanceDate.Date.AddHours(6), att.AttendanceDate.Date.Add(workStart), 1.5m));
            }

            // Normal Working Hours (to exclude from OT) - only for scheduled work days
            DateTime workStartDateTime = att.AttendanceDate.Date.Add(workStart);
            DateTime workEndDateTime = att.AttendanceDate.Date.Add(workEnd);
            
            // Adjust for Saturday half day
            if (att.AttendanceDate.DayOfWeek == DayOfWeek.Saturday && _workSchedule?.SaturdayWorkType == "HALF")
            {
                workEndDateTime = att.AttendanceDate.Date.Add(_workSchedule?.SaturdayEndTime ?? new TimeSpan(12, 0, 0));
            }
            
            // Calculate Pay for this record
            foreach (var bucket in buckets)
            {
                // Find overlap between (ClockIn, ClockOut) and Bucket
                DateTime overlapStart = clockIn > bucket.Start ? clockIn : bucket.Start;
                DateTime overlapEnd = clockOut < bucket.End ? clockOut : bucket.End;
                
                if (overlapStart < overlapEnd)
                {
                    // If this is a scheduled work day, exclude normal working hours from OT
                    if (isScheduledWorkDay && !isHoliday)
                    {
                        // Subtract time that falls within Normal Work Hours
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
                        // Holiday or Non-Scheduled Day: All hours are OT
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
        
        // Prevent re-processing of locked periods
        if (period.Status == "APPROVED" || period.Status == "LOCKED")
            throw new InvalidOperationException($"Cannot re-run payroll for {period.Status} period. Create a new period or unlock first.");
        
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
            // Uses current conversion rate from database or fallback to settings
            decimal conversionRate = await GetConversionRateAsync(emp.SalaryCurrency, "LAK", period.EndDate);
            decimal effectiveBaseSalary = Math.Round(emp.BaseSalary * conversionRate, 2);

            // Fetch Adjustments for this employee in this period
            var adjustments = await _context.PayrollAdjustments
                .Where(a => a.EmployeeId == emp.EmployeeId && a.PeriodId == periodId)
                .ToListAsync();

            // Auto-calculate Overtime using LAK base salary
            decimal overtimePay = await CalculateOvertimePay(emp.EmployeeId, period.StartDate, period.EndDate, effectiveBaseSalary);
            
            // Calculate final salary components (using LAK values)
            var calc = CalculateSalary(effectiveBaseSalary, overtimePay: overtimePay, adjustments: adjustments, dependentCount: emp.DependentCount);
            
            // Calculate original currency amounts (for non-LAK contracts)
            decimal netSalaryOriginal = emp.SalaryCurrency == "LAK" 
                ? calc.NetSalary 
                : Math.Round(calc.NetSalary / conversionRate, 2);
            
            var slip = new SalarySlip
            {
                EmployeeId = emp.EmployeeId,
                PeriodId = periodId,
                // LAK amounts (for tax/NSSF calculation)
                BaseSalary = calc.BaseSalary,
                OvertimePay = calc.OvertimePay,
                Allowances = calc.Allowances,
                Bonus = calc.Bonus,
                GrossIncome = calc.GrossIncome,
                NssfBase = calc.NssfBase,
                NssfEmployeeDeduction = calc.NssfEmployeeDeduction,
                NssfEmployerContribution = calc.NssfEmployerContribution,
                TaxableIncome = calc.TaxableIncome,
                TaxDeduction = calc.TaxDeduction,
                OtherDeductions = calc.OtherDeductions,
                NetSalary = calc.NetSalary,
                // Original currency info
                ContractCurrency = emp.SalaryCurrency,
                ExchangeRateUsed = conversionRate,
                BaseSalaryOriginal = emp.BaseSalary,
                NetSalaryOriginal = netSalaryOriginal,
                PaymentCurrency = emp.SalaryCurrency, // Default to contract currency
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
    public decimal Bonus { get; set; }
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
