using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Models;
using System.Linq;

namespace LaoHR.Shared.Data;

public class LaoHRDbContext : DbContext
{
    public LaoHRDbContext(DbContextOptions<LaoHRDbContext> options) : base(options) 
    {
    }
    
    public DbSet<Department> Departments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<PayrollPeriod> PayrollPeriods { get; set; }
    public DbSet<SalarySlip> SalarySlips { get; set; }
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<LeavePolicy> LeavePolicies { get; set; }
    public DbSet<LeaveBalance> LeaveBalances { get; set; }
    public DbSet<TaxBracket> TaxBrackets { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }
    public DbSet<Holiday> Holidays { get; set; }
    public DbSet<EmployeeDocument> EmployeeDocuments { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<CompanySetting> CompanySettings { get; set; }
    public DbSet<Province> Provinces { get; set; }
    public DbSet<District> Districts { get; set; }
    public DbSet<Village> Villages { get; set; }
    public DbSet<WorkSchedule> WorkSchedules { get; set; }
    public DbSet<ConversionRate> ConversionRates { get; set; }
    public DbSet<PayrollAdjustment> PayrollAdjustments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Unique constraints
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeCode)
            .IsUnique();
        
        modelBuilder.Entity<Attendance>()
            .HasIndex(a => new { a.EmployeeId, a.AttendanceDate })
            .IsUnique();
        
        modelBuilder.Entity<PayrollPeriod>()
            .HasIndex(p => new { p.Year, p.Month })
            .IsUnique();
            
        modelBuilder.Entity<Holiday>()
            .HasIndex(h => h.Date)
            .IsUnique();
        
        // Leave policy unique constraint
        modelBuilder.Entity<LeavePolicy>()
            .HasIndex(lp => lp.LeaveType)
            .IsUnique();
        
        // Leave balance unique constraint (one balance per employee/type/year)
        modelBuilder.Entity<LeaveBalance>()
            .HasIndex(lb => new { lb.EmployeeId, lb.LeaveType, lb.Year })
            .IsUnique();
        
        // Seed default leave policies
        modelBuilder.Entity<LeavePolicy>().HasData(
            new LeavePolicy { LeavePolicyId = 1, LeaveType = "ANNUAL", LeaveTypeLao = "ພັກປະຈຳປີ", AnnualQuota = 15, MaxCarryOver = 5, AccrualPerMonth = 1.25m, AllowHalfDay = true },
            new LeavePolicy { LeavePolicyId = 2, LeaveType = "SICK", LeaveTypeLao = "ພັກປ່ວຍ", AnnualQuota = 30, MaxCarryOver = 0, RequiresAttachment = true, MinDaysForAttachment = 3, AllowHalfDay = true },
            new LeavePolicy { LeavePolicyId = 3, LeaveType = "PERSONAL", LeaveTypeLao = "ພັກສ່ວນຕົວ", AnnualQuota = 3, MaxCarryOver = 0, AllowHalfDay = true },
            new LeavePolicy { LeavePolicyId = 4, LeaveType = "MATERNITY", LeaveTypeLao = "ພັກເກີດລູກ", AnnualQuota = 90, MaxCarryOver = 0, AllowHalfDay = false },
            new LeavePolicy { LeavePolicyId = 5, LeaveType = "PATERNITY", LeaveTypeLao = "ພັກພໍ່ເກີດລູກ", AnnualQuota = 15, MaxCarryOver = 0, AllowHalfDay = false },
            new LeavePolicy { LeavePolicyId = 6, LeaveType = "UNPAID", LeaveTypeLao = "ພັກບໍ່ໄດ້ເງິນ", AnnualQuota = 365, MaxCarryOver = 0, AllowHalfDay = false }
        );
        
        // Seed default tax brackets (Lao PIT)
        modelBuilder.Entity<TaxBracket>().HasData(
            new TaxBracket { BracketId = 1, MinIncome = 0, MaxIncome = 1300000, TaxRate = 0.00m, SortOrder = 1 },
            new TaxBracket { BracketId = 2, MinIncome = 1300001, MaxIncome = 5000000, TaxRate = 0.05m, SortOrder = 2 },
            new TaxBracket { BracketId = 3, MinIncome = 5000001, MaxIncome = 15000000, TaxRate = 0.10m, SortOrder = 3 },
            new TaxBracket { BracketId = 4, MinIncome = 15000001, MaxIncome = 25000000, TaxRate = 0.15m, SortOrder = 4 },
            new TaxBracket { BracketId = 5, MinIncome = 25000001, MaxIncome = 65000000, TaxRate = 0.20m, SortOrder = 5 },
            new TaxBracket { BracketId = 6, MinIncome = 65000001, MaxIncome = 9999999999999999m, TaxRate = 0.25m, SortOrder = 6 }
        );
        
        // Seed NSSF settings
        modelBuilder.Entity<SystemSetting>().HasData(
            new SystemSetting { SettingKey = "NSSF_CEILING_BASE", SettingValue = "4500000", Description = "Maximum salary for NSSF calculation" },
            new SystemSetting { SettingKey = "NSSF_EMPLOYEE_RATE", SettingValue = "0.055", Description = "Employee NSSF contribution rate (5.5%)" },
            new SystemSetting { SettingKey = "NSSF_EMPLOYER_RATE", SettingValue = "0.060", Description = "Employer NSSF contribution rate (6.0%)" },
            new SystemSetting { SettingKey = "WORK_START_TIME", SettingValue = "08:30", Description = "Standard work start time" },
            new SystemSetting { SettingKey = "WORK_END_TIME", SettingValue = "17:30", Description = "Standard work end time" },
            new SystemSetting { SettingKey = "EX_RATE_USD", SettingValue = "22000", Description = "USD to LAK Exchange Rate" },
            new SystemSetting { SettingKey = "EX_RATE_THB", SettingValue = "650", Description = "THB to LAK Exchange Rate" },
            new SystemSetting { SettingKey = "ZKTECO_ENABLED", SettingValue = "false", Description = "Global Switch for ZKTeco Integration" }
        );
        
        // Seed sample departments
        modelBuilder.Entity<Department>().HasData(
            new Department { DepartmentId = 1, DepartmentName = "ບໍລິຫານ", DepartmentNameEn = "Administration", DepartmentCode = "ADMIN" },
            new Department { DepartmentId = 2, DepartmentName = "ການເງິນ", DepartmentNameEn = "Finance & Accounting", DepartmentCode = "FIN" },
            new Department { DepartmentId = 3, DepartmentName = "ເຕັກນິກ", DepartmentNameEn = "Information Technology", DepartmentCode = "IT" },
            new Department { DepartmentId = 4, DepartmentName = "ການຂາຍ", DepartmentNameEn = "Sales & Marketing", DepartmentCode = "SALES" }
        );

        // Seed 2026 Holidays
        modelBuilder.Entity<Holiday>().HasData(
            new Holiday { HolidayId = 1, Date = new DateTime(2026, 1, 1), Name = "International New Year", NameLao = "ປີໃໝ່ສາກົນ", Year = 2026, IsRecurring = true },
            new Holiday { HolidayId = 2, Date = new DateTime(2026, 3, 8), Name = "International Women's Day", NameLao = "ວັນແມ່ຍິງສາກົນ", Year = 2026, IsRecurring = true },
            new Holiday { HolidayId = 3, Date = new DateTime(2026, 4, 14), Name = "Lao New Year (Day 1)", NameLao = "ວັນປີໃໝ່ລາວ", Year = 2026, IsRecurring = false },
            new Holiday { HolidayId = 4, Date = new DateTime(2026, 4, 15), Name = "Lao New Year (Day 2)", NameLao = "ວັນປີໃໝ່ລາວ", Year = 2026, IsRecurring = false },
            new Holiday { HolidayId = 5, Date = new DateTime(2026, 4, 16), Name = "Lao New Year (Day 3)", NameLao = "ວັນປີໃໝ່ລາວ", Year = 2026, IsRecurring = false },
            new Holiday { HolidayId = 6, Date = new DateTime(2026, 5, 1), Name = "International Labour Day", NameLao = "ວັນກຳມະກອນສາກົນ", Year = 2026, IsRecurring = true },
            new Holiday { HolidayId = 7, Date = new DateTime(2026, 6, 1), Name = "International Children's Day", NameLao = "ວັນເດັກນ້ອຍສາກົນ", Year = 2026, IsRecurring = true },
            new Holiday { HolidayId = 8, Date = new DateTime(2026, 7, 20), Name = "Lao Women's Union Day", NameLao = "ວັນແມ່ຍິງລາວ", Year = 2026, IsRecurring = true },
            new Holiday { HolidayId = 9, Date = new DateTime(2026, 10, 7), Name = "National Teacher's Day", NameLao = "ວັນຄູແຫ່ງຊາດ", Year = 2026, IsRecurring = true },
            new Holiday { HolidayId = 10, Date = new DateTime(2026, 12, 2), Name = "National Day", NameLao = "ວັນຊາດ", Year = 2026, IsRecurring = true }
        );
        
        // Seed sample employees with Lao names
        modelBuilder.Entity<Employee>().HasData(
            new Employee { EmployeeId = 1, EmployeeCode = "EMP001", LaoName = "ສົມພອນ ຄຳສຸກ", EnglishName = "Somphon Khamsouk", JobTitle = "Software Engineer", DepartmentId = 3, HireDate = new DateTime(2024, 1, 15), BaseSalary = 8000000m, SalaryCurrency = "LAK", IsActive = true, Email = "somphon@laohr.la", Phone = "020 5555 1234", Gender = "Male" },
            new Employee { EmployeeId = 2, EmployeeCode = "EMP002", LaoName = "ດາວັນ ສີສະຫວັດ", EnglishName = "Davanh Sisavath", JobTitle = "HR Manager", DepartmentId = 1, HireDate = new DateTime(2023, 6, 1), BaseSalary = 12000000m, SalaryCurrency = "LAK", IsActive = true, Email = "davanh@laohr.la", Phone = "020 5555 5678", Gender = "Female" },
            new Employee { EmployeeId = 3, EmployeeCode = "EMP003", LaoName = "ມະນີວັນ ສຸກສະຫວັນ", EnglishName = "Manivanh Souksavan", JobTitle = "Accountant", DepartmentId = 2, HireDate = new DateTime(2023, 8, 15), BaseSalary = 9500000m, SalaryCurrency = "LAK", IsActive = true, Email = "manivanh@laohr.la", Phone = "020 5555 9012", Gender = "Female" },
            new Employee { EmployeeId = 4, EmployeeCode = "EMP004", LaoName = "ພູວົງ ໄຊຍະວົງ", EnglishName = "Phouvong Xaiyavong", JobTitle = "Sales Manager", DepartmentId = 4, HireDate = new DateTime(2022, 3, 1), BaseSalary = 11000000m, SalaryCurrency = "LAK", IsActive = true, Email = "phouvong@laohr.la", Phone = "020 5555 3456", Gender = "Male" },
            new Employee { EmployeeId = 5, EmployeeCode = "EMP005", LaoName = "ບຸນມີ ວົງພະຈັນ", EnglishName = "Bounmi Vongphachan", JobTitle = "Marketing Specialist", DepartmentId = 4, HireDate = new DateTime(2024, 2, 1), BaseSalary = 7500000m, SalaryCurrency = "LAK", IsActive = false, Email = "bounmi@laohr.la", Phone = "020 5555 7890", Gender = "Male" },
            new Employee { EmployeeId = 6, EmployeeCode = "EMP006", LaoName = "ນາງ ສຸພາພອນ", EnglishName = "Souphaphon Nang", JobTitle = "Office Administrator", DepartmentId = 1, HireDate = new DateTime(2023, 11, 15), BaseSalary = 6500000m, SalaryCurrency = "LAK", IsActive = true, Email = "souphaphon@laohr.la", Phone = "020 5555 2468", Gender = "Female" },
            new Employee { EmployeeId = 7, EmployeeCode = "EMP007", LaoName = "ວິໄລພອນ ແກ້ວມະນີ", EnglishName = "Vilayphon Keomanee", JobTitle = "Senior Developer", DepartmentId = 3, HireDate = new DateTime(2021, 9, 1), BaseSalary = 15000000m, SalaryCurrency = "LAK", IsActive = true, Email = "vilayphon@laohr.la", Phone = "020 5555 1357", Gender = "Male" },
            new Employee { EmployeeId = 8, EmployeeCode = "EMP008", LaoName = "ຈັນທະລາ ພົມມະວົງ", EnglishName = "Chanthala Phommavong", JobTitle = "CFO", DepartmentId = 2, HireDate = new DateTime(2020, 1, 1), BaseSalary = 25000000m, SalaryCurrency = "LAK", IsActive = true, Email = "chanthala@laohr.la", Phone = "020 5555 8642", Gender = "Female" }
        );
    }
}
