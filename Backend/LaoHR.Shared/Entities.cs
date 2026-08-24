using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LaoHR.Shared.Models;

/// <summary>
/// Department entity
/// </summary>
public class Department
{
    [Key]
    public int DepartmentId { get; set; }
    
    [Required, MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? DepartmentNameEn { get; set; }
    
    [MaxLength(20)]
    public string? DepartmentCode { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Phase 3C1 — department hierarchy + accountable head.
    /// <summary>Parent department (null = top-level unit).</summary>
    public int? ParentDepartmentId { get; set; }

    /// <summary>Accountable head of the department (distinct from an employee's direct manager).</summary>
    public int? ManagerEmployeeId { get; set; }

    /// <summary>Display order within siblings.</summary>
    public int SortOrder { get; set; } = 0;
    
    // Navigation
    [ForeignKey("ParentDepartmentId")]
    [JsonIgnore]
    public virtual Department? ParentDepartment { get; set; }

    [JsonIgnore]
    public virtual ICollection<Department> ChildDepartments { get; set; } = new List<Department>();

    [ForeignKey("ManagerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Manager { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

/// <summary>
/// Phase 3C1 — organizational position/slot. A position is a named role within
/// a department (e.g. "HR Officer"), distinct from an employee's free-text
/// JobTitle. Lightweight: no compensation bands yet.
/// </summary>
public class Position
{
    [Key]
    public int PositionId { get; set; }

    [Required, MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? TitleLao { get; set; }

    [MaxLength(20)]
    public string? JobCode { get; set; }

    public int? DepartmentId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }

    [JsonIgnore]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

/// <summary>
/// Phase 3C1 — work location / branch. Organizations may operate across
/// locations. Uses the Lao province/district/village address structure.
/// </summary>
public class WorkLocation
{
    [Key]
    public int WorkLocationId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameLao { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public int? VillageId { get; set; }

    /// <summary>IANA timezone, e.g. "Asia/Vientiane".</summary>
    [MaxLength(50)]
    public string? Timezone { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProvinceId")]
    [JsonIgnore]
    public virtual Province? Province { get; set; }

    [ForeignKey("DistrictId")]
    [JsonIgnore]
    public virtual District? District { get; set; }

    [ForeignKey("VillageId")]
    [JsonIgnore]
    public virtual Village? Village { get; set; }

    [JsonIgnore]
    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

/// <summary>
/// Employee entity - Core HR data
/// </summary>
public class Employee
{
    [Key]
    public int EmployeeId { get; set; }
    
    [Required, MaxLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;
    
    [Required, MaxLength(100)]
    public string LaoName { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? EnglishName { get; set; }
    
    [MaxLength(20)]
    public string? NssfId { get; set; }
    
    [MaxLength(20)]
    public string? TaxId { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    
    [MaxLength(10)]
    public string? Gender { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? Email { get; set; }

    public int DependentCount { get; set; } = 0;
    public string SalaryCurrency { get; set; } = "LAK"; // LAK, USD, THB

    [MaxLength(500)]
    public string? ProfilePath { get; set; }
    
    public int? DepartmentId { get; set; }
    
    [MaxLength(100)]
    public string? JobTitle { get; set; }

    // Phase 3C1 — organizational assignment.
    /// <summary>Direct reporting manager (null = top-level executive).</summary>
    public int? ManagerId { get; set; }

    /// <summary>Organizational position/slot (optional).</summary>
    public int? PositionId { get; set; }

    /// <summary>Work location/branch (optional).</summary>
    public int? WorkLocationId { get; set; }
    
    public DateTime? HireDate { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }
    
    [MaxLength(50)]
    public string? BankName { get; set; }
    
    [MaxLength(50)]
    public string? BankAccount { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation
    [ForeignKey("DepartmentId")]
    public virtual Department? Department { get; set; }

    [ForeignKey("ManagerId")]
    [JsonIgnore]
    public virtual Employee? Manager { get; set; }

    [JsonIgnore]
    public virtual ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    [ForeignKey("PositionId")]
    [JsonIgnore]
    public virtual Position? Position { get; set; }

    [ForeignKey("WorkLocationId")]
    [JsonIgnore]
    public virtual WorkLocation? WorkLocation { get; set; }
    
    public virtual ICollection<Attendance> AttendanceRecords { get; set; } = new List<Attendance>();
    public virtual ICollection<SalarySlip> SalarySlips { get; set; } = new List<SalarySlip>();
    public virtual ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}

/// <summary>
/// Attendance record with geolocation
/// </summary>
public class Attendance
{
    [Key]
    public int AttendanceId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public DateTime AttendanceDate { get; set; }
    
    public DateTime? ClockIn { get; set; }
    
    [Column(TypeName = "decimal(9,6)")]
    public decimal? ClockInLatitude { get; set; }
    
    [Column(TypeName = "decimal(10,6)")]
    public decimal? ClockInLongitude { get; set; }
    
    [MaxLength(20)]
    public string? ClockInMethod { get; set; }
    
    public DateTime? ClockOut { get; set; }
    
    [Column(TypeName = "decimal(9,6)")]
    public decimal? ClockOutLatitude { get; set; }
    
    [Column(TypeName = "decimal(10,6)")]
    public decimal? ClockOutLongitude { get; set; }
    
    [MaxLength(20)]
    public string? ClockOutMethod { get; set; }
    
    [Column(TypeName = "decimal(5,2)")]
    public decimal? WorkHours { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "PRESENT";
    
    public bool IsLate { get; set; }
    
    public bool IsEarlyLeave { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    // Navigation
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;
}

/// <summary>
/// Payroll Period
/// </summary>
public class PayrollPeriod
{
    [Key]
    public int PeriodId { get; set; }
    
    [Required]
    public int Year { get; set; }
    
    [Required]
    public int Month { get; set; }
    
    [Required, MaxLength(50)]
    public string PeriodName { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "DRAFT";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    public virtual ICollection<SalarySlip> SalarySlips { get; set; } = new List<SalarySlip>();
}

/// <summary>
/// Salary Slip with NSSF and Tax calculations
/// </summary>
public class SalarySlip
{
    [Key]
    public int SlipId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public int PeriodId { get; set; }
    
    // All amounts below are stored in LAK (converted if employee has foreign currency)
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OvertimePay { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Allowances { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Bonus { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal GrossIncome { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NssfBase { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NssfEmployeeDeduction { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NssfEmployerContribution { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxableIncome { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxDeduction { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OtherDeductions { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalary { get; set; }
    
    // ========== Original Currency (Contract) ==========
    /// <summary>
    /// Employee's contract currency (LAK, USD, THB, CNY)
    /// </summary>
    [MaxLength(3)]
    public string ContractCurrency { get; set; } = "LAK";
    
    /// <summary>
    /// Exchange rate used (1 ContractCurrency = X LAK)
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal ExchangeRateUsed { get; set; } = 1;
    
    /// <summary>
    /// Base salary in original contract currency
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalaryOriginal { get; set; }
    
    /// <summary>
    /// Net salary in original contract currency
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal NetSalaryOriginal { get; set; }
    
    /// <summary>
    /// Payment currency: ORIGINAL = pay in contract currency, LAK = pay in Kip
    /// </summary>
    [MaxLength(10)]
    public string PaymentCurrency { get; set; } = "LAK";
    
    [MaxLength(20)]
    public string Status { get; set; } = "CALCULATED";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // ========== Service Record (Not Mapped) ==========
    [NotMapped] public decimal AnnualLeaveRemaining { get; set; }
    [NotMapped] public decimal SickLeaveUsed { get; set; }
    [NotMapped] public decimal WorkDays { get; set; }
    [NotMapped] public decimal AbsentDays { get; set; }

    // Navigation
    [ForeignKey("EmployeeId")]
    public virtual Employee Employee { get; set; } = null!;
    
    [ForeignKey("PeriodId")]
    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;
}

/// <summary>
/// Leave Request
/// </summary>
public class LeaveRequest
{
    [Key]
    public int LeaveId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required, MaxLength(30)]
    public string LeaveType { get; set; } = "ANNUAL";
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// Total days requested (supports 0.5 for half-day)
    /// </summary>
    [Column(TypeName = "decimal(5,1)")]
    public decimal TotalDays { get; set; }
    
    /// <summary>
    /// Whether this is a half-day leave request
    /// </summary>
    public bool IsHalfDay { get; set; } = false;
    
    /// <summary>
    /// For half-day: "MORNING" or "AFTERNOON"
    /// </summary>
    [MaxLength(20)]
    public string? HalfDayType { get; set; }
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    /// <summary>
    /// Path to attachment file (e.g., medical certificate)
    /// </summary>
    [MaxLength(500)]
    public string? AttachmentPath { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "PENDING";
    
    public int? ApprovedById { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    [MaxLength(500)]
    public string? ApproverNotes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// Leave Policy - Configurable leave quotas per type
/// </summary>
public class LeavePolicy
{
    [Key]
    public int LeavePolicyId { get; set; }
    
    [Required, MaxLength(30)]
    public string LeaveType { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? LeaveTypeLao { get; set; }
    
    /// <summary>
    /// Annual quota (days per year)
    /// </summary>
    public int AnnualQuota { get; set; }
    
    /// <summary>
    /// Maximum days that can be carried over to next year
    /// </summary>
    public int MaxCarryOver { get; set; } = 0;
    
    /// <summary>
    /// Days accrued per month (for accrual-based policies)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal AccrualPerMonth { get; set; } = 0;
    
    /// <summary>
    /// Whether attachment is required
    /// </summary>
    public bool RequiresAttachment { get; set; } = false;
    
    /// <summary>
    /// Minimum consecutive days before attachment is required
    /// </summary>
    public int MinDaysForAttachment { get; set; } = 0;
    
    /// <summary>
    /// Allow half-day requests for this leave type
    /// </summary>
    public bool AllowHalfDay { get; set; } = true;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Leave Balance - Per-employee leave balance tracking
/// </summary>
public class LeaveBalance
{
    [Key]
    public int LeaveBalanceId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required, MaxLength(30)]
    public string LeaveType { get; set; } = string.Empty;
    
    [Required]
    public int Year { get; set; }
    
    /// <summary>
    /// Total entitled days for the year
    /// </summary>
    [Column(TypeName = "decimal(5,1)")]
    public decimal TotalDays { get; set; }
    
    /// <summary>
    /// Days used (approved leaves)
    /// </summary>
    [Column(TypeName = "decimal(5,1)")]
    public decimal UsedDays { get; set; } = 0;
    
    /// <summary>
    /// Days carried over from previous year
    /// </summary>
    [Column(TypeName = "decimal(5,1)")]
    public decimal CarriedOverDays { get; set; } = 0;
    
    /// <summary>
    /// Remaining = TotalDays + CarriedOverDays - UsedDays
    /// </summary>
    [NotMapped]
    public decimal RemainingDays => TotalDays + CarriedOverDays - UsedDays;
    
    // Navigation
    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// Tax Bracket for progressive tax calculation
/// </summary>
public class TaxBracket
{
    [Key]
    public int BracketId { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinIncome { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxIncome { get; set; }
    
    [Column(TypeName = "decimal(5,4)")]
    public decimal TaxRate { get; set; }
    
    public int SortOrder { get; set; }
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// System Settings (NSSF rates, etc.)
/// </summary>
public class SystemSetting
{
    [Key, MaxLength(50)]
    public string SettingKey { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string SettingValue { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Holiday - Days when the company is closed
/// </summary>
public class Holiday
{
    [Key]
    public int HolidayId { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string? NameLao { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int Year { get; set; }
    
    /// <summary>
    /// If true, this holiday repeats every year on the same month/day
    /// </summary>
    public bool IsRecurring { get; set; } = true;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Employee Document (Contract, ID Card, etc.)
/// </summary>
public class EmployeeDocument
{
    [Key]
    public int DocumentId { get; set; }

    public int EmployeeId { get; set; }
    
    [Required, MaxLength(50)]
    public string DocumentType { get; set; } = "Other"; // Contract, ID_Card, Resume, Other

    [Required, MaxLength(100)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    [JsonIgnore]
    public Employee? Employee { get; set; }
}

/// <summary>
/// Province entity
/// </summary>
public class Province
{
    [Key]
    public int PrId { get; set; }
    
    [Required, MaxLength(200)]
    public string PrName { get; set; } = string.Empty;
    
    [Required, MaxLength(200)]
    public string PrNameEn { get; set; } = string.Empty;
    
    // Navigation
    public virtual ICollection<District> Districts { get; set; } = new List<District>();
}

/// <summary>
/// District entity
/// </summary>
public class District
{
    [Key]
    public int DiId { get; set; }
    
    [Required, MaxLength(200)]
    public string DiName { get; set; } = string.Empty;
    
    [Required, MaxLength(200)]
    public string DiNameEn { get; set; } = string.Empty;
    
    [Required]
    public int PrId { get; set; }
    
    // Navigation
    [ForeignKey("PrId")]
    [JsonIgnore]
    public virtual Province? Province { get; set; }
    
    public virtual ICollection<Village> Villages { get; set; } = new List<Village>();
}

/// <summary>
/// Village entity
/// </summary>
public class Village
{
    [Key]
    public int VillId { get; set; }
    
    [Required, MaxLength(200)]
    public string VillName { get; set; } = string.Empty;
    
    [Required, MaxLength(200)]
    public string VillNameEn { get; set; } = string.Empty;
    
    [Required]
    public int DiId { get; set; }
    
    // Navigation
    [ForeignKey("DiId")]
    [JsonIgnore]
    public virtual District? District { get; set; }
}

/// <summary>
/// Company Settings entity
/// </summary>
public class CompanySetting
{
    [Key]
    public int Id { get; set; }
    
    [Required, MaxLength(200)]
    public string CompanyNameLao { get; set; } = string.Empty;
    
    [Required, MaxLength(200)]
    public string CompanyNameEn { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string? LSSOCode { get; set; }
    
    [MaxLength(50)]
    public string? TaxRisId { get; set; }
    
    [MaxLength(50)]
    public string? BankAccountNo { get; set; }
    
    [MaxLength(100)]
    public string? BankName { get; set; }
    
    [MaxLength(20)]
    public string? Tel { get; set; }
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? Email { get; set; }
    
    // Address
    public int? VillageId { get; set; }
    public int? DistrictId { get; set; }
    public int? ProvinceId { get; set; }
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    [ForeignKey("VillageId")]
    public virtual Village? Village { get; set; }
    
    [ForeignKey("DistrictId")]
    public virtual District? District { get; set; }
    
    [ForeignKey("ProvinceId")]
    public virtual Province? Province { get; set; }
}

/// <summary>
/// Work Schedule - Defines which days are working days
/// Single row per company (singleton pattern)
/// </summary>
public class WorkSchedule
{
    [Key]
    public int WorkScheduleId { get; set; }
    
    // Which days are work days (Mon-Fri standard)
    public bool Monday { get; set; } = true;
    public bool Tuesday { get; set; } = true;
    public bool Wednesday { get; set; } = true;
    public bool Thursday { get; set; } = true;
    public bool Friday { get; set; } = true;
    public bool Saturday { get; set; } = false;
    public bool Sunday { get; set; } = false;
    
    // Saturday Configuration
    /// <summary>
    /// NONE = No Saturday work
    /// FULL = Full day (8 hours)
    /// HALF = Half day (4 hours)
    /// </summary>
    [MaxLength(10)]
    public string SaturdayWorkType { get; set; } = "NONE";
    
    /// <summary>
    /// Hours for Saturday (4 for half day, 8 for full day)
    /// </summary>
    public decimal SaturdayHours { get; set; } = 0;
    
    /// <summary>
    /// Which Saturdays of the month are work days (comma separated: 1,2,3,4 or ALL)
    /// e.g., "1,3" = 1st and 3rd Saturday of month
    /// "ALL" = All Saturdays
    /// </summary>
    [MaxLength(20)]
    public string SaturdayWeeks { get; set; } = "";
    
    // Work hours (Mon-Fri standard)
    public TimeSpan WorkStartTime { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan WorkEndTime { get; set; } = new TimeSpan(17, 0, 0);
    public TimeSpan BreakStartTime { get; set; } = new TimeSpan(12, 0, 0);
    public TimeSpan BreakEndTime { get; set; } = new TimeSpan(13, 0, 0);
    
    // Saturday work hours (if different from regular)
    public TimeSpan? SaturdayStartTime { get; set; }
    public TimeSpan? SaturdayEndTime { get; set; }
    
    // Late threshold in minutes
    public int LateThresholdMinutes { get; set; } = 15;
    
    // Laos Law: Standard monthly hours (160 hours = 20 days × 8 hours)
    public decimal StandardMonthlyHours { get; set; } = 160;
    
    // Daily work hours (excluding break)
    public decimal DailyWorkHours { get; set; } = 8;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Check if a given day of week is a work day
    /// </summary>
    public bool IsWorkDay(DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => Monday,
            DayOfWeek.Tuesday => Tuesday,
            DayOfWeek.Wednesday => Wednesday,
            DayOfWeek.Thursday => Thursday,
            DayOfWeek.Friday => Friday,
            DayOfWeek.Saturday => Saturday,
            DayOfWeek.Sunday => Sunday,
            _ => false
        };
    }
    
    /// <summary>
    /// Check if a specific Saturday of the month is a work day
    /// </summary>
    /// <param name="weekOfMonth">Which week (1-5)</param>
    public bool IsSaturdayWorkDay(int weekOfMonth)
    {
        if (!Saturday || SaturdayWorkType == "NONE") return false;
        if (SaturdayWeeks == "ALL") return true;
        
        var weeks = SaturdayWeeks?.Split(',') ?? Array.Empty<string>();
        return weeks.Contains(weekOfMonth.ToString());
    }
    
    /// <summary>
    /// Get work hours for Saturday
    /// </summary>
    public decimal GetSaturdayHours()
    {
        return SaturdayWorkType switch
        {
            "FULL" => 8,
            "HALF" => 4,
            _ => SaturdayHours
        };
    }
    
    /// <summary>
    /// Get work days per month based on schedule
    /// Mon-Fri = 20 days, Mon-Sat(half) = 23 days, Mon-Sat(full) = 26 days
    /// </summary>
    public decimal GetWorkDaysPerMonth()
    {
        return SaturdayWorkType switch
        {
            "HALF" => 23m, // 20 + (4 saturdays × 0.5)
            "FULL" => 26m,   // ~26 work days 
            _ => 20m         // Mon-Fri standard
        };
    }
}

/// <summary>
/// Currency conversion rate - stores historical rates
/// </summary>
public class ConversionRate
{
    [Key]
    public int ConversionRateId { get; set; }
    
    [Required, MaxLength(3)]
    public string FromCurrency { get; set; } = "USD";
    
    [Required, MaxLength(3)]
    public string ToCurrency { get; set; } = "LAK";
    
    /// <summary>
    /// Exchange rate: 1 FromCurrency = Rate ToCurrency
    /// e.g., USD to LAK = 22000 means 1 USD = 22,000 LAK
    /// </summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal Rate { get; set; }
    
    /// <summary>
    /// Date this rate becomes effective
    /// </summary>
    public DateTime EffectiveDate { get; set; }
    
    /// <summary>
    /// Date this rate expires (null = current rate)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(200)]
    public string? Notes { get; set; }
}

/// <summary>
/// App User - System User for Authentication
/// </summary>
public class AppUser
{
    [Key]
    public int UserId { get; set; }
    
    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    [Required, MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Password hash algorithm version: 1 = legacy SHA-256 (unsalted), 2 = PBKDF2-HMAC-SHA256 (salted, 600k iterations).
    /// Phase 3A — enables rehash-on-login migration from SHA-256 to PBKDF2.
    /// </summary>
    public int PasswordHashVersion { get; set; } = 1;
    
    [Required, MaxLength(20)]
    public string Role { get; set; } = "Employee"; // Admin, HR, Employee
    
    [MaxLength(100)]
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Link to Employee record (optional, Admins might not be employees)
    /// </summary>
    public int? EmployeeId { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    // Navigation
    [ForeignKey("EmployeeId")]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// Phase 6c — Server-side refresh token record.
/// We store the SHA-256 hash, not the raw token, so a database leak
/// doesn't yield usable tokens. Tokens are rotated on every use
/// (one-time use) and revoked on logout.
/// </summary>
public class RefreshToken
{
    [Key]
    public int RefreshTokenId { get; set; }

    public int UserId { get; set; }

    /// <summary>SHA-256 hash of the raw token (lowercase hex).</summary>
    [Required, MaxLength(128)]
    public string TokenHash { get; set; } = string.Empty;

    /// <summary>
    /// Optional: the parent token's hash. Populated on rotation so we can
    /// detect replay of a stolen (already-used) refresh token.
    /// </summary>
    [MaxLength(128)]
    public string? ReplacedByHash { get; set; }

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when the token is rotated, replaced, or explicitly revoked.</summary>
    public DateTime? RevokedAt { get; set; }

    [MaxLength(64)]
    public string? RevokedReason { get; set; }

    [MaxLength(64)]
    public string? CreatedByIp { get; set; }

    [ForeignKey("UserId")]
    public virtual AppUser? User { get; set; }
}

// =============================================================================
// Phase 2 — Project Workspace Foundation
// -----------------------------------------------------------------------------
// Lightweight PM slice: Project / Milestone / Task / Assignee / Comment / Activity.
// Status, priority, due-date, percent-complete. No dependency DAG, no Gantt.
// =============================================================================

/// <summary>
/// A workspace project. Has members, milestones, and tasks.
/// </summary>
public class Project
{
    [Key]
    public int ProjectId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>PLANNING, ACTIVE, ON_HOLD, COMPLETED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "PLANNING";

    /// <summary>LOW, MEDIUM, HIGH, CRITICAL.</summary>
    [Required, MaxLength(10)]
    public string Priority { get; set; } = "MEDIUM";

    [MaxLength(20)]
    public string? Color { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    public int OwnerId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("OwnerId")]
    [JsonIgnore]
    public virtual Employee? Owner { get; set; }

    public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public virtual ICollection<ActivityLog> Activities { get; set; } = new List<ActivityLog>();
}

/// <summary>
/// Many-to-many between Project and Employee. Captures the role a member plays.
/// </summary>
public class ProjectMember
{
    [Key]
    public int ProjectMemberId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    /// <summary>OWNER, LEAD, MEMBER, VIEWER.</summary>
    [Required, MaxLength(20)]
    public string Role { get; set; } = "MEMBER";

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// A milestone within a project — a grouping of tasks around a target date.
/// </summary>
public class Milestone
{
    [Key]
    public int MilestoneId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>OPEN, COMPLETED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "OPEN";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
}

/// <summary>
/// A unit of work. Assignable to multiple employees.
/// </summary>
public class ProjectTask
{
    [Key]
    public int TaskId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public int? MilestoneId { get; set; }

    /// <summary>
    /// Phase 3C4 — optional parent task for a simple two-level hierarchy
    /// (summary → subtask). Self-referencing; cycles are prevented server-side.
    /// </summary>
    public int? ParentTaskId { get; set; }

    /// <summary>e.g. PRJ-1-3 (project, sequence).</summary>
    [MaxLength(20)]
    public string? TaskNumber { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Description { get; set; }

    /// <summary>TODO, IN_PROGRESS, BLOCKED, REVIEW, DONE, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "TODO";

    /// <summary>LOW, MEDIUM, HIGH, CRITICAL.</summary>
    [Required, MaxLength(10)]
    public string Priority { get; set; } = "MEDIUM";

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }

    /// <summary>0-100. Manual percentage; for lightweight rollup only.</summary>
    [Range(0, 100)]
    public int ProgressPercent { get; set; } = 0;

    [Column(TypeName = "decimal(6,2)")]
    public decimal? EstimatedHours { get; set; }

    [Column(TypeName = "decimal(6,2)")]
    public decimal? ActualHours { get; set; }

    public int ReporterId { get; set; }
    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("MilestoneId")]
    [JsonIgnore]
    public virtual Milestone? Milestone { get; set; }

    [ForeignKey("ReporterId")]
    [JsonIgnore]
    public virtual Employee? Reporter { get; set; }

    [ForeignKey("ParentTaskId")]
    [JsonIgnore]
    public virtual ProjectTask? ParentTask { get; set; }

    [JsonIgnore]
    public virtual ICollection<ProjectTask> ChildTasks { get; set; } = new List<ProjectTask>();

    public virtual ICollection<TaskAssignee> Assignees { get; set; } = new List<TaskAssignee>();
    public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}

/// <summary>
/// Phase 3C4 — a finish-to-start dependency between two tasks in the same project.
/// </summary>
public class TaskDependency
{
    [Key]
    public int TaskDependencyId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    /// <summary>The task that must finish first.</summary>
    [Required]
    public int PredecessorTaskId { get; set; }

    /// <summary>The task that starts after the predecessor finishes.</summary>
    [Required]
    public int SuccessorTaskId { get; set; }

    /// <summary>FS (finish-to-start) only for now.</summary>
    [Required, MaxLength(10)]
    public string Type { get; set; } = "FS";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("PredecessorTaskId")]
    [JsonIgnore]
    public virtual ProjectTask? Predecessor { get; set; }

    [ForeignKey("SuccessorTaskId")]
    [JsonIgnore]
    public virtual ProjectTask? Successor { get; set; }
}

/// <summary>
/// Many-to-many between Task and Employee. Captures assignment role.
/// </summary>
public class TaskAssignee
{
    [Key]
    public int TaskAssigneeId { get; set; }

    [Required]
    public int TaskId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    /// <summary>ASSIGNEE, REVIEWER, WATCHER.</summary>
    [Required, MaxLength(20)]
    public string Role { get; set; } = "ASSIGNEE";

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("TaskId")]
    [JsonIgnore]
    public virtual ProjectTask? Task { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// A comment on a task. Light thread via optional ParentCommentId.
/// </summary>
public class TaskComment
{
    [Key]
    public int CommentId { get; set; }

    [Required]
    public int TaskId { get; set; }

    [Required]
    public int AuthorId { get; set; }

    [Required, MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("TaskId")]
    [JsonIgnore]
    public virtual ProjectTask? Task { get; set; }

    [ForeignKey("AuthorId")]
    [JsonIgnore]
    public virtual Employee? Author { get; set; }

    [ForeignKey("ParentCommentId")]
    [JsonIgnore]
    public virtual TaskComment? Parent { get; set; }
}

/// <summary>
/// Append-only activity log for projects and tasks. Powers the audit/timeline view.
/// </summary>
public class ActivityLog
{
    [Key]
    public long ActivityId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public int? TaskId { get; set; }

    [Required]
    public int ActorId { get; set; }

    /// <summary>CREATED, UPDATED_STATUS, ASSIGNED, COMMENTED, COMPLETED, MEMBER_ADDED, ...</summary>
    [Required, MaxLength(40)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? PayloadJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("TaskId")]
    [JsonIgnore]
    public virtual ProjectTask? Task { get; set; }

    [ForeignKey("ActorId")]
    [JsonIgnore]
    public virtual Employee? Actor { get; set; }
}

// =============================================================================
// Phase 3 — Risk, Issue, Resource
// -----------------------------------------------------------------------------
// Lightweight operational entities complementing the project workspace.
// - Risk: identified project-level risk with likelihood / impact / mitigation.
// - Issue: ad-hoc blocker or problem (with optional linked task); has comments.
// - Resource: lightweight allocation of an employee to a project with a role
//   and date range. No capacity / utilization calculations — just ownership
//   and a window of commitment.
// =============================================================================

/// <summary>
/// A risk identified for a project. Status reflects how it has been handled.
/// </summary>
public class Risk
{
    [Key]
    public int RiskId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>LOW, MEDIUM, HIGH, CRITICAL.</summary>
    [Required, MaxLength(10)]
    public string Priority { get; set; } = "MEDIUM";

    /// <summary>1-5. Higher = more likely.</summary>
    [Range(1, 5)]
    public int Likelihood { get; set; } = 3;

    /// <summary>1-5. Higher = more impactful.</summary>
    [Range(1, 5)]
    public int Impact { get; set; } = 3;

    /// <summary>Computed: Likelihood * Impact. Not stored; cheap to derive.</summary>
    [NotMapped]
    public int Score => Likelihood * Impact;

    /// <summary>OPEN, MITIGATING, CLOSED, ACCEPTED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "OPEN";

    [MaxLength(2000)]
    public string? Mitigation { get; set; }

    public int? OwnerId { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("OwnerId")]
    [JsonIgnore]
    public virtual Employee? Owner { get; set; }
}

/// <summary>
/// An ad-hoc issue / blocker. Can be linked back to a project (and optionally
/// a task) and carries its own comment thread.
/// </summary>
public class Issue
{
    [Key]
    public int IssueId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    public int? TaskId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(5000)]
    public string? Description { get; set; }

    /// <summary>TODO, IN_PROGRESS, BLOCKED, REVIEW, DONE, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "TODO";

    /// <summary>LOW, MEDIUM, HIGH, CRITICAL.</summary>
    [Required, MaxLength(10)]
    public string Priority { get; set; } = "MEDIUM";

    public int ReporterId { get; set; }
    public int? AssigneeId { get; set; }

    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("TaskId")]
    [JsonIgnore]
    public virtual ProjectTask? Task { get; set; }

    [ForeignKey("ReporterId")]
    [JsonIgnore]
    public virtual Employee? Reporter { get; set; }

    [ForeignKey("AssigneeId")]
    [JsonIgnore]
    public virtual Employee? Assignee { get; set; }

    public virtual ICollection<IssueComment> Comments { get; set; } = new List<IssueComment>();
}

/// <summary>
/// A comment on an issue. Same threading model as TaskComment.
/// </summary>
public class IssueComment
{
    [Key]
    public int IssueCommentId { get; set; }

    [Required]
    public int IssueId { get; set; }

    [Required]
    public int AuthorId { get; set; }

    [Required, MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public int? ParentCommentId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("IssueId")]
    [JsonIgnore]
    public virtual Issue? Issue { get; set; }

    [ForeignKey("AuthorId")]
    [JsonIgnore]
    public virtual Employee? Author { get; set; }

    [ForeignKey("ParentCommentId")]
    [JsonIgnore]
    public virtual IssueComment? Parent { get; set; }
}

/// <summary>
/// A lightweight allocation of an employee to a project. Captures the role and
/// the date window during which the allocation is valid. Intentionally not a
/// capacity engine — just enough to answer "who is working on what, when".
/// </summary>
public class Resource
{
    [Key]
    public int ResourceId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    /// <summary>LEAD, CONTRIBUTOR, REVIEWER, ADVISOR.</summary>
    [Required, MaxLength(20)]
    public string Role { get; set; } = "CONTRIBUTOR";

    /// <summary>0-100. Allocation percentage; informational only.</summary>
    [Range(0, 100)]
    public int AllocationPercent { get; set; } = 100;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// Phase 3C4 — RAID: a project assumption to be validated.
/// </summary>
public class ProjectAssumption
{
    [Key]
    public int AssumptionId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int? OwnerId { get; set; }

    /// <summary>OPEN, VALIDATED, INVALIDATED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "OPEN";

    public DateTime? ValidationDate { get; set; }

    [MaxLength(1000)]
    public string? Outcome { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("OwnerId")]
    [JsonIgnore]
    public virtual Employee? Owner { get; set; }
}

/// <summary>
/// Phase 3C4 — RAID: a project decision log entry.
/// </summary>
public class ProjectDecision
{
    [Key]
    public int DecisionId { get; set; }

    [Required]
    public int ProjectId { get; set; }

    [Required, MaxLength(500)]
    public string Decision { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Context { get; set; }

    public int? OwnerId { get; set; }

    public DateTime? DecisionDate { get; set; }

    [MaxLength(1000)]
    public string? Outcome { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("OwnerId")]
    [JsonIgnore]
    public virtual Employee? Owner { get; set; }
}

// =============================================================================
// Phase 4 — Finance Extension
// -----------------------------------------------------------------------------
// Lightweight finance: expense claims + employee loans/advances. Both share a
// small status workflow. Amounts stored in original currency; converted to LAK
// at submission time using the current conversion rate.
// =============================================================================

/// <summary>
/// Lookup for expense categories. Seeded with common types.
/// </summary>
public class ExpenseCategory
{
    [Key]
    public int ExpenseCategoryId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameLao { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool RequiresReceipt { get; set; } = true;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? DefaultLimit { get; set; }

    /// <summary>Phase 4B.1 — optional GL expense account for auto-posting.</summary>
    public int? AccountId { get; set; }

    public bool IsActive { get; set; } = true;

    [ForeignKey("AccountId")]
    [JsonIgnore]
    public virtual Account? Account { get; set; }
}

/// <summary>
/// An employee-submitted expense claim. Goes through a simple approval workflow.
/// </summary>
public class Expense
{
    [Key]
    public int ExpenseId { get; set; }

    [Required, MaxLength(20)]
    public string ExpenseNumber { get; set; } = string.Empty;

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public int? PayrollPeriodId { get; set; }

    /// <summary>Phase 4C — optional link to a business travel request.</summary>
    public int? TravelRequestId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime ExpenseDate { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal ExchangeRateUsed { get; set; } = 1;

    /// <summary>LAK-converted amount for reporting / payroll netting.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountLak { get; set; }

    [MaxLength(500)]
    public string? ReceiptPath { get; set; }

    /// <summary>DRAFT, SUBMITTED, APPROVED, REJECTED, PAID.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    [MaxLength(500)]
    public string? ApproverNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public virtual ExpenseCategory? Category { get; set; }

    [ForeignKey("PayrollPeriodId")]
    [JsonIgnore]
    public virtual PayrollPeriod? PayrollPeriod { get; set; }

    [ForeignKey("ApproverId")]
    [JsonIgnore]
    public virtual Employee? Approver { get; set; }

    [ForeignKey("TravelRequestId")]
    [JsonIgnore]
    public virtual TravelRequest? TravelRequest { get; set; }
}

/// <summary>
/// Employee loan or advance. Has a principal amount + simple installment plan
/// that can be deducted across future payroll periods.
/// </summary>
public class EmployeeLoan
{
    [Key]
    public int LoanId { get; set; }

    [Required, MaxLength(20)]
    public string LoanNumber { get; set; } = string.Empty;

    [Required]
    public int EmployeeId { get; set; }

    /// <summary>LOAN, ADVANCE.</summary>
    [Required, MaxLength(20)]
    public string LoanType { get; set; } = "LOAN";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Principal { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal InterestRate { get; set; } = 0;

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    [Column(TypeName = "decimal(18,4)")]
    public decimal ExchangeRateUsed { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrincipalLak { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    /// <summary>Number of installments the loan is divided over.</summary>
    public int Installments { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal InstallmentAmount { get; set; }

    /// <summary>Total repaid so far (LAK).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal RepaidAmount { get; set; } = 0;

    [MaxLength(500)]
    public string? Purpose { get; set; }

    /// <summary>DRAFT, APPROVED, ACTIVE, SETTLED, REJECTED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public int? ApproverId { get; set; }
    public DateTime? ApprovedAt { get; set; }

    [MaxLength(500)]
    public string? ApproverNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("ApproverId")]
    [JsonIgnore]
    public virtual Employee? Approver { get; set; }

    public virtual ICollection<LoanRepayment> Repayments { get; set; } = new List<LoanRepayment>();
}

/// <summary>
/// One repayment row per payroll period that deducted from this loan.
/// </summary>
public class LoanRepayment
{
    [Key]
    public int RepaymentId { get; set; }

    [Required]
    public int LoanId { get; set; }

    public int? PayrollPeriodId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AmountLak { get; set; }

    public DateTime RepaidAt { get; set; } = DateTime.UtcNow;

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    [ForeignKey("LoanId")]
    [JsonIgnore]
    public virtual EmployeeLoan? Loan { get; set; }

    [ForeignKey("PayrollPeriodId")]
    [JsonIgnore]
    public virtual PayrollPeriod? PayrollPeriod { get; set; }
}

// ============================================================================
// Phase 5 — Knowledge & Collaboration
// Lightweight slices: announcements, knowledge articles, polymorphic comments.
// Designed to be cheap to operate and small to render — no rich-text editor,
// no notifications fan-out, no threaded nesting beyond one reply level.
// ============================================================================

/// <summary>
/// Company-wide announcement (news, policy update, holiday reminder).
/// Pinned items always show first; PublishFrom/PublishUntil bound the window.
/// </summary>
public class Announcement
{
    [Key]
    public int AnnouncementId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? TitleLao { get; set; }

    [Required]
    public string Body { get; set; } = string.Empty;

    public string? BodyLao { get; set; }

    /// <summary>INFO, WARNING, URGENT — drives the visual chip.</summary>
    [Required, MaxLength(20)]
    public string Severity { get; set; } = "INFO";

    /// <summary>ALL, ROLE, DEPARTMENT — scope of audience.</summary>
    [Required, MaxLength(20)]
    public string Audience { get; set; } = "ALL";

    public int? AudienceRoleId { get; set; }
    public int? AudienceDepartmentId { get; set; }

    public bool IsPinned { get; set; } = false;

    public DateTime? PublishFrom { get; set; }
    public DateTime? PublishUntil { get; set; }

    public int AuthorId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("AuthorId")]
    [JsonIgnore]
    public virtual Employee? Author { get; set; }
}

/// <summary>Tracks per-user read state of an announcement (idempotent).</summary>
public class AnnouncementRead
{
    [Key]
    public int AnnouncementReadId { get; set; }

    [Required]
    public int AnnouncementId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("AnnouncementId")]
    [JsonIgnore]
    public virtual Announcement? Announcement { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>Knowledge-base category (e.g., HR Policies, IT, Benefits).</summary>
public class KnowledgeCategory
{
    [Key]
    public int KnowledgeCategoryId { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? NameLao { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// A self-contained knowledge-base article. Body is plain markdown
/// (rendered server-side would be heavier — render client-side instead).
/// </summary>
public class KnowledgeArticle
{
    [Key]
    public int KnowledgeArticleId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? TitleLao { get; set; }

    [Required, MaxLength(500)]
    public string Summary { get; set; } = string.Empty;

    [Required]
    public string Body { get; set; } = string.Empty;

    public string? BodyLao { get; set; }

    [Required]
    public int CategoryId { get; set; }

    /// <summary>DRAFT, PUBLISHED, ARCHIVED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public int ViewCount { get; set; } = 0;

    public int AuthorId { get; set; }

    public DateTime? PublishedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public virtual KnowledgeCategory? Category { get; set; }

    [ForeignKey("AuthorId")]
    [JsonIgnore]
    public virtual Employee? Author { get; set; }
}

/// <summary>
/// Polymorphic comment thread. Attaches to any entity via (EntityType, EntityId)
/// to avoid duplicating comment tables per feature. One-level threading via
/// optional ParentCommentId.
/// </summary>
public class EntityComment
{
    [Key]
    public int EntityCommentId { get; set; }

    /// <summary>e.g. PROJECT, TASK, ISSUE, EXPENSE, LOAN.</summary>
    [Required, MaxLength(30)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    public int EntityId { get; set; }

    public int? ParentCommentId { get; set; }

    [Required]
    public int AuthorId { get; set; }

    [Required, MaxLength(4000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    [ForeignKey("ParentCommentId")]
    [JsonIgnore]
    public virtual EntityComment? Parent { get; set; }

    [ForeignKey("AuthorId")]
    [JsonIgnore]
    public virtual Employee? Author { get; set; }
}

// =============================================================================
// Phase 3B — Versioned Lao statutory compliance rules.
// -----------------------------------------------------------------------------
// Lightweight domain model for statutory parameters that change over time
// (PIT brackets, NSSF rates/ceiling, minimum wage, overtime multipliers, leave
// floors). NOT a generic rules engine. Each rule is effective-dated and carries
// source/verification metadata for auditability and historical reproducibility.
// =============================================================================

/// <summary>
/// A versioned statutory compliance rule for Lao PDR.
/// </summary>
public class ComplianceRule
{
    [Key]
    public int ComplianceRuleId { get; set; }

    /// <summary>Stable rule identifier, e.g. "LAO-PIT-2026-BRACKET-01".</summary>
    [Required, MaxLength(100)]
    public string RuleId { get; set; } = string.Empty;

    /// <summary>Jurisdiction code, e.g. "LA" (Lao PDR).</summary>
    [Required, MaxLength(10)]
    public string Jurisdiction { get; set; } = "LA";

    /// <summary>Category: PIT, NSSF, MINIMUM_WAGE, OVERTIME, LEAVE, HOLIDAY.</summary>
    [Required, MaxLength(30)]
    public string Category { get; set; } = string.Empty;

    /// <summary>Human-readable name.</summary>
    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Effective from (inclusive).</summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>Effective to (exclusive). Null = currently effective.</summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>Monotonic version number for this RuleId.</summary>
    public int Version { get; set; } = 1;

    /// <summary>VERIFIED, PROVISIONAL, BLOCKED, SUPERSEDED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "PROVISIONAL";

    /// <summary>Calculation parameters as JSON (brackets, rates, ceilings, etc.).</summary>
    public string? ParametersJson { get; set; }

    // ---- Source / legal traceability metadata ----
    [MaxLength(200)]
    public string? SourceTitle { get; set; }

    [MaxLength(200)]
    public string? Authority { get; set; }

    [MaxLength(100)]
    public string? LawNumber { get; set; }

    [MaxLength(100)]
    public string? Article { get; set; }

    [MaxLength(500)]
    public string? SourceUrl { get; set; }

    public DateTime? VerifiedDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Immutable snapshot of the compliance rules used for a specific payroll run.
/// Preserves historical reproducibility: a later rule change must not alter an
/// already-finalized payroll result.
/// </summary>
public class PayrollRuleSnapshot
{
    [Key]
    public int PayrollRuleSnapshotId { get; set; }

    /// <summary>The payroll period this snapshot belongs to.</summary>
    [Required]
    public int PeriodId { get; set; }

    /// <summary>JSON array of the ComplianceRule values in effect at calculation time.</summary>
    [Required]
    public string RulesJson { get; set; } = string.Empty;

    /// <summary>Exchange rates in effect (JSON).</summary>
    public string? ExchangeRatesJson { get; set; }

    public DateTime CapturedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PeriodId")]
    [JsonIgnore]
    public virtual PayrollPeriod? PayrollPeriod { get; set; }
}

// =============================================================================
// Phase 3C1 — Approval engine foundation.
// -----------------------------------------------------------------------------
// Lightweight, database-backed approval requests with sequential steps. NOT a
// generic BPM engine. Supports Leave/Expense/Loan/Attendance/Overtime via a
// polymorphic RequestType + EntityId. Approver identity is resolved server-side
// (never trusted from the client).
// =============================================================================

/// <summary>
/// A reusable approval request for a business entity (leave, expense, loan, etc.).
/// </summary>
public class ApprovalRequest
{
    [Key]
    public int ApprovalRequestId { get; set; }

    /// <summary>LEAVE, EXPENSE, LOAN, ATTENDANCE_CORRECTION, OVERTIME.</summary>
    [Required, MaxLength(30)]
    public string RequestType { get; set; } = string.Empty;

    /// <summary>The business entity id (e.g. LeaveId, ExpenseId).</summary>
    [Required]
    public int EntityId { get; set; }

    /// <summary>The employee who submitted the request.</summary>
    [Required]
    public int RequesterEmployeeId { get; set; }

    /// <summary>DRAFT, PENDING, APPROVED, REJECTED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "PENDING";

    /// <summary>Index of the current step (0-based).</summary>
    public int CurrentStepIndex { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    [ForeignKey("RequesterEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Requester { get; set; }

    [JsonIgnore]
    public virtual ICollection<ApprovalStep> Steps { get; set; } = new List<ApprovalStep>();
}

/// <summary>
/// A single sequential step in an approval request.
/// </summary>
public class ApprovalStep
{
    [Key]
    public int ApprovalStepId { get; set; }

    [Required]
    public int ApprovalRequestId { get; set; }

    /// <summary>Order of this step (0-based).</summary>
    public int StepOrder { get; set; }

    /// <summary>Resolver type: DIRECT_MANAGER, DEPARTMENT_MANAGER, ROLE, EMPLOYEE.</summary>
    [Required, MaxLength(30)]
    public string ResolverType { get; set; } = "DIRECT_MANAGER";

    /// <summary>Resolved approver employee id (set at request creation).</summary>
    public int? ApproverEmployeeId { get; set; }

    /// <summary>For ROLE resolver: the role name (e.g. "HR").</summary>
    [MaxLength(20)]
    public string? RoleName { get; set; }

    /// <summary>PENDING, APPROVED, REJECTED, SKIPPED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "PENDING";

    public DateTime? ActedAt { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    [ForeignKey("ApprovalRequestId")]
    [JsonIgnore]
    public virtual ApprovalRequest? ApprovalRequest { get; set; }

    [ForeignKey("ApproverEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Approver { get; set; }
}

/// <summary>
/// Immutable history of every approval action.
/// </summary>
public class ApprovalAction
{
    [Key]
    public int ApprovalActionId { get; set; }

    [Required]
    public int ApprovalRequestId { get; set; }

    [Required]
    public int ActorEmployeeId { get; set; }

    /// <summary>APPROVED, REJECTED, CANCELLED, SUBMITTED.</summary>
    [Required, MaxLength(20)]
    public string Action { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime ActedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ApprovalRequestId")]
    [JsonIgnore]
    public virtual ApprovalRequest? ApprovalRequest { get; set; }
}

// =============================================================================
// Phase 3C2 — In-app notification domain.
// -----------------------------------------------------------------------------
// Lightweight internal notification system. In-app only (no email/SMS yet).
// Recipient is an AppUser (UserId); notifications are created server-side and
// link to a domain entity via EntityType + EntityId.
// =============================================================================

/// <summary>
/// An in-app notification for a user.
/// </summary>
public class Notification
{
    [Key]
    public int NotificationId { get; set; }

    /// <summary>Recipient AppUser id.</summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>APPROVAL_REQUESTED, APPROVAL_APPROVED, APPROVAL_REJECTED, LEAVE_UPDATED, EXPENSE_UPDATED, LOAN_UPDATED, ATTENDANCE_CORRECTION_UPDATED, MANAGER_CHANGED.</summary>
    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Message { get; set; }

    /// <summary>Domain entity type for deep-linking (e.g. LEAVE, EXPENSE, LOAN, APPROVAL).</summary>
    [MaxLength(30)]
    public string? EntityType { get; set; }

    /// <summary>Domain entity id for deep-linking.</summary>
    public int? EntityId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }

    [ForeignKey("UserId")]
    [JsonIgnore]
    public virtual AppUser? User { get; set; }
}

/// <summary>
/// Phase 3C2 — attendance correction request. An employee requests a correction
/// to a clock-in/out record; it flows through the approval engine.
/// </summary>
public class AttendanceCorrection
{
    [Key]
    public int CorrectionId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int AttendanceId { get; set; }

    [Required]
    public DateTime AttendanceDate { get; set; }

    /// <summary>Proposed corrected clock-in (nullable = no change).</summary>
    public DateTime? CorrectedClockIn { get; set; }

    /// <summary>Proposed corrected clock-out (nullable = no change).</summary>
    public DateTime? CorrectedClockOut { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    /// <summary>PENDING, APPROVED, REJECTED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "PENDING";

    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }

    [MaxLength(500)]
    public string? ApproverNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("AttendanceId")]
    [JsonIgnore]
    public virtual Attendance? Attendance { get; set; }
}

// =============================================================================
// Phase 3C5 — Recruitment + ATS + Onboarding
// -----------------------------------------------------------------------------
// Connects workforce need → requisition → opening → candidate → application →
// pipeline → interview → offer → hire → employee → onboarding.
// Candidate is the person-level identity BEFORE hire; Employee is the canonical
// worker identity AFTER hire. No duplicate master data.
// =============================================================================

/// <summary>
/// A job requisition — approval to recruit for a Position.
/// </summary>
public class JobRequisition
{
    [Key]
    public int RequisitionId { get; set; }

    [Required, MaxLength(20)]
    public string RequisitionNumber { get; set; } = string.Empty;

    [Required]
    public int PositionId { get; set; }

    public int? DepartmentId { get; set; }

    public int? WorkLocationId { get; set; }

    /// <summary>Employee who requested the requisition.</summary>
    public int RequestedByEmployeeId { get; set; }

    /// <summary>Hiring manager for this requisition.</summary>
    public int? HiringManagerEmployeeId { get; set; }

    /// <summary>Number of hires authorized.</summary>
    public int Headcount { get; set; } = 1;

    /// <summary>Replacement, Growth, Temporary, Other.</summary>
    [MaxLength(20)]
    public string Reason { get; set; } = "Growth";

    [MaxLength(1000)]
    public string? Justification { get; set; }

    public DateTime? TargetStartDate { get; set; }

    /// <summary>LOW, MEDIUM, HIGH, CRITICAL.</summary>
    [MaxLength(10)]
    public string Priority { get; set; } = "MEDIUM";

    /// <summary>DRAFT, PENDING_APPROVAL, APPROVED, OPEN, ON_HOLD, FILLED, CANCELLED, CLOSED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PositionId")]
    [JsonIgnore]
    public virtual Position? Position { get; set; }

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }

    [ForeignKey("WorkLocationId")]
    [JsonIgnore]
    public virtual WorkLocation? WorkLocation { get; set; }

    [ForeignKey("RequestedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? RequestedBy { get; set; }

    [ForeignKey("HiringManagerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? HiringManager { get; set; }
}

/// <summary>
/// A job opening (vacancy) derived from an approved requisition.
/// </summary>
public class JobOpening
{
    [Key]
    public int OpeningId { get; set; }

    [Required]
    public int RequisitionId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? TitleLao { get; set; }

    /// <summary>Public-safe summary (no internal notes).</summary>
    [MaxLength(4000)]
    public string? Summary { get; set; }

    [MaxLength(4000)]
    public string? Responsibilities { get; set; }

    [MaxLength(4000)]
    public string? Requirements { get; set; }

    /// <summary>DRAFT, OPEN, PAUSED, CLOSED, FILLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("RequisitionId")]
    [JsonIgnore]
    public virtual JobRequisition? Requisition { get; set; }
}

/// <summary>
/// A candidate — person-level recruitment identity (before hire).
/// </summary>
public class Candidate
{
    [Key]
    public int CandidateId { get; set; }

    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? FirstNameLao { get; set; }

    [MaxLength(100)]
    public string? LastNameLao { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? CurrentLocation { get; set; }

    [MaxLength(200)]
    public string? CurrentCompany { get; set; }

    [MaxLength(200)]
    public string? CurrentTitle { get; set; }

    [MaxLength(4000)]
    public string? Summary { get; set; }

    /// <summary>CareerPage, Referral, Agency, Direct, Other.</summary>
    [MaxLength(20)]
    public string? Source { get; set; }

    /// <summary>ACTIVE, HIRED, REJECTED, ARCHIVED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A candidate's application to a specific job opening.
/// </summary>
public class Application
{
    [Key]
    public int ApplicationId { get; set; }

    [Required]
    public int CandidateId { get; set; }

    [Required]
    public int OpeningId { get; set; }

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string? Source { get; set; }

    /// <summary>Current pipeline stage (see ATS pipeline).</summary>
    [Required, MaxLength(30)]
    public string CurrentStage { get; set; } = "APPLIED";

    /// <summary>ACTIVE, HIRED, REJECTED, WITHDRAWN.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    [MaxLength(50)]
    public string? RejectionReason { get; set; }

    [MaxLength(1000)]
    public string? RejectionComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CandidateId")]
    [JsonIgnore]
    public virtual Candidate? Candidate { get; set; }

    [ForeignKey("OpeningId")]
    [JsonIgnore]
    public virtual JobOpening? Opening { get; set; }
}

/// <summary>
/// Immutable history of an application's pipeline stage movement.
/// </summary>
public class ApplicationStageHistory
{
    [Key]
    public int StageHistoryId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required, MaxLength(30)]
    public string FromStage { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string ToStage { get; set; } = string.Empty;

    public int? ActorEmployeeId { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ApplicationId")]
    [JsonIgnore]
    public virtual Application? Application { get; set; }
}

/// <summary>
/// A candidate document (CV, cover letter, certificate, etc.).
/// </summary>
public class CandidateDocument
{
    [Key]
    public int CandidateDocumentId { get; set; }

    [Required]
    public int CandidateId { get; set; }

    /// <summary>Resume, CoverLetter, Certificate, Portfolio, Other.</summary>
    [Required, MaxLength(30)]
    public string DocumentType { get; set; } = "Resume";

    [Required, MaxLength(200)]
    public string FileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CandidateId")]
    [JsonIgnore]
    public virtual Candidate? Candidate { get; set; }
}

/// <summary>
/// An interview scheduled for an application.
/// </summary>
public class Interview
{
    [Key]
    public int InterviewId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    /// <summary>Phone, HR, HiringManager, Technical, Panel, Final.</summary>
    [Required, MaxLength(30)]
    public string InterviewType { get; set; } = "HR";

    public DateTime? ScheduledStart { get; set; }
    public DateTime? ScheduledEnd { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    /// <summary>SCHEDULED, COMPLETED, CANCELLED, NO_SHOW.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "SCHEDULED";

    public int? OrganizerEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ApplicationId")]
    [JsonIgnore]
    public virtual Application? Application { get; set; }

    [ForeignKey("OrganizerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Organizer { get; set; }

    [JsonIgnore]
    public virtual ICollection<InterviewParticipant> Participants { get; set; } = new List<InterviewParticipant>();
}

/// <summary>
/// An interviewer on an interview panel.
/// </summary>
public class InterviewParticipant
{
    [Key]
    public int ParticipantId { get; set; }

    [Required]
    public int InterviewId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    /// <summary>INTERVIEWER, OBSERVER.</summary>
    [Required, MaxLength(20)]
    public string Role { get; set; } = "INTERVIEWER";

    [ForeignKey("InterviewId")]
    [JsonIgnore]
    public virtual Interview? Interview { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// An interviewer's structured evaluation (scorecard) for an interview.
/// </summary>
public class InterviewEvaluation
{
    [Key]
    public int EvaluationId { get; set; }

    [Required]
    public int InterviewId { get; set; }

    [Required]
    public int EvaluatorEmployeeId { get; set; }

    /// <summary>1-5 score for communication.</summary>
    [Range(1, 5)]
    public int CommunicationScore { get; set; } = 3;

    /// <summary>1-5 score for relevant experience.</summary>
    [Range(1, 5)]
    public int ExperienceScore { get; set; } = 3;

    /// <summary>1-5 score for role fit.</summary>
    [Range(1, 5)]
    public int RoleFitScore { get; set; } = 3;

    /// <summary>Recommend, Neutral, DoNotRecommend.</summary>
    [Required, MaxLength(20)]
    public string Recommendation { get; set; } = "Neutral";

    [MaxLength(4000)]
    public string? Comments { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("InterviewId")]
    [JsonIgnore]
    public virtual Interview? Interview { get; set; }

    [ForeignKey("EvaluatorEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Evaluator { get; set; }
}

/// <summary>
/// An offer made to a candidate for an application.
/// </summary>
public class Offer
{
    [Key]
    public int OfferId { get; set; }

    [Required]
    public int ApplicationId { get; set; }

    [Required]
    public int PositionId { get; set; }

    public DateTime? ProposedStartDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Salary { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>FullTime, PartTime, Contract, Temporary.</summary>
    [MaxLength(20)]
    public string? EmploymentType { get; set; }

    /// <summary>DRAFT, PENDING_APPROVAL, APPROVED, SENT, ACCEPTED, DECLINED, EXPIRED, WITHDRAWN.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public int? CreatedByEmployeeId { get; set; }

    public DateTime? ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DeclinedAt { get; set; }

    [MaxLength(1000)]
    public string? DeclineReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ApplicationId")]
    [JsonIgnore]
    public virtual Application? Application { get; set; }

    [ForeignKey("PositionId")]
    [JsonIgnore]
    public virtual Position? Position { get; set; }

    [ForeignKey("CreatedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? CreatedBy { get; set; }
}

/// <summary>
/// An onboarding process linked to a hired Employee.
/// </summary>
public class OnboardingProcess
{
    [Key]
    public int OnboardingProcessId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    /// <summary>Source traceability: the application that produced this hire.</summary>
    public int? ApplicationId { get; set; }

    public int? CandidateId { get; set; }

    public DateTime? StartDate { get; set; }

    public int? OwnerEmployeeId { get; set; }

    /// <summary>NOT_STARTED, IN_PROGRESS, COMPLETED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "NOT_STARTED";

    public DateTime? CompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("OwnerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Owner { get; set; }

    [JsonIgnore]
    public virtual ICollection<OnboardingTask> Tasks { get; set; } = new List<OnboardingTask>();
}

/// <summary>
/// A single onboarding checklist task.
/// </summary>
public class OnboardingTask
{
    [Key]
    public int OnboardingTaskId { get; set; }

    [Required]
    public int OnboardingProcessId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int? OwnerEmployeeId { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CompletedAt { get; set; }

    /// <summary>HR, IT, Manager, Facilities, Employee.</summary>
    [MaxLength(20)]
    public string Category { get; set; } = "HR";

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("OnboardingProcessId")]
    [JsonIgnore]
    public virtual OnboardingProcess? OnboardingProcess { get; set; }

    [ForeignKey("OwnerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Owner { get; set; }
}

// =============================================================================
// Phase 3C6 — Performance Management + Talent + Learning
// -----------------------------------------------------------------------------
// Post-hire employee growth lifecycle: goals → performance → competencies →
// development → learning → career. Reuses Employee/Position/ManagerId.
// No AI scoring/ranking. Human-governed.
// =============================================================================

/// <summary>
/// An individual/team goal with an accountable owner.
/// </summary>
public class Goal
{
    [Key]
    public int GoalId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public int? ManagerEmployeeId { get; set; }

    /// <summary>Optional parent goal for alignment (org → dept → employee).</summary>
    public int? ParentGoalId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>Individual, Team, Organization.</summary>
    [Required, MaxLength(20)]
    public string GoalType { get; set; } = "Individual";

    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }

    /// <summary>0-100 manual progress.</summary>
    [Range(0, 100)]
    public int ProgressPercent { get; set; } = 0;

    /// <summary>DRAFT, ACTIVE, COMPLETED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public int? CreatedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("ManagerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Manager { get; set; }

    [ForeignKey("ParentGoalId")]
    [JsonIgnore]
    public virtual Goal? ParentGoal { get; set; }
}

/// <summary>
/// A lightweight progress check-in on a goal (preserves history).
/// </summary>
public class GoalCheckIn
{
    [Key]
    public int CheckInId { get; set; }

    [Required]
    public int GoalId { get; set; }

    [Range(0, 100)]
    public int ProgressPercent { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public int? CreatedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("GoalId")]
    [JsonIgnore]
    public virtual Goal? Goal { get; set; }
}

/// <summary>
/// A formal performance review cycle.
/// </summary>
public class PerformanceCycle
{
    [Key]
    public int CycleId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? ReviewDueDate { get; set; }

    /// <summary>DRAFT, ACTIVE, REVIEW_OPEN, CLOSED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A performance review for an employee within a cycle. Manager is snapshotted
/// at creation (does not change if Employee.ManagerId changes later).
/// </summary>
public class PerformanceReview
{
    [Key]
    public int ReviewId { get; set; }

    [Required]
    public int CycleId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    /// <summary>Snapshotted reviewer (manager at review creation).</summary>
    public int? ManagerEmployeeId { get; set; }

    /// <summary>Snapshotted position/department for review context.</summary>
    public int? PositionId { get; set; }
    public int? DepartmentId { get; set; }

    /// <summary>NOT_STARTED, SELF_REVIEW, MANAGER_REVIEW, FINALIZED, ACKNOWLEDGED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "NOT_STARTED";

    /// <summary>1-5 manager rating (documented scale).</summary>
    [Range(1, 5)]
    public int? OverallRating { get; set; }

    [MaxLength(4000)]
    public string? SelfAchievements { get; set; }

    [MaxLength(4000)]
    public string? SelfChallenges { get; set; }

    [MaxLength(4000)]
    public string? ManagerComments { get; set; }

    [MaxLength(4000)]
    public string? DevelopmentNeeds { get; set; }

    public DateTime? EmployeeSubmittedAt { get; set; }
    public DateTime? ManagerSubmittedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }

    [MaxLength(2000)]
    public string? AcknowledgementComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CycleId")]
    [JsonIgnore]
    public virtual PerformanceCycle? Cycle { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("ManagerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Manager { get; set; }
}

/// <summary>
/// Continuous feedback / recognition between employees.
/// </summary>
public class Feedback
{
    [Key]
    public int FeedbackId { get; set; }

    [Required]
    public int FromEmployeeId { get; set; }

    [Required]
    public int ToEmployeeId { get; set; }

    /// <summary>Feedback, Recognition.</summary>
    [Required, MaxLength(20)]
    public string Type { get; set; } = "Feedback";

    [Required, MaxLength(4000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>Private, RecipientAndManager.</summary>
    [Required, MaxLength(30)]
    public string Visibility { get; set; } = "Private";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("FromEmployeeId")]
    [JsonIgnore]
    public virtual Employee? FromEmployee { get; set; }

    [ForeignKey("ToEmployeeId")]
    [JsonIgnore]
    public virtual Employee? ToEmployee { get; set; }
}

/// <summary>
/// A 1:1 meeting between a manager and a direct report.
/// </summary>
public class OneOnOne
{
    [Key]
    public int OneOnOneId { get; set; }

    [Required]
    public int ManagerEmployeeId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public DateTime? ScheduledAt { get; set; }

    /// <summary>SCHEDULED, COMPLETED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "SCHEDULED";

    [MaxLength(4000)]
    public string? SharedNotes { get; set; }

    [MaxLength(4000)]
    public string? ManagerPrivateNotes { get; set; }

    [MaxLength(4000)]
    public string? EmployeeNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ManagerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Manager { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// A competency in the framework.
/// </summary>
public class Competency
{
    [Key]
    public int CompetencyId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameLao { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>Core, Leadership, Functional, Technical.</summary>
    [MaxLength(20)]
    public string Category { get; set; } = "Core";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A position's required competency level (role expectation).
/// </summary>
public class PositionCompetency
{
    [Key]
    public int PositionCompetencyId { get; set; }

    [Required]
    public int PositionId { get; set; }

    [Required]
    public int CompetencyId { get; set; }

    /// <summary>1-5 required proficiency level.</summary>
    [Range(1, 5)]
    public int RequiredLevel { get; set; } = 3;

    public bool IsRequired { get; set; } = true;

    [ForeignKey("PositionId")]
    [JsonIgnore]
    public virtual Position? Position { get; set; }

    [ForeignKey("CompetencyId")]
    [JsonIgnore]
    public virtual Competency? Competency { get; set; }
}

/// <summary>
/// An employee's competency assessment (with provenance).
/// </summary>
public class CompetencyAssessment
{
    [Key]
    public int AssessmentId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int CompetencyId { get; set; }

    [Required]
    public int AssessorEmployeeId { get; set; }

    /// <summary>Self, Manager.</summary>
    [Required, MaxLength(20)]
    public string AssessmentType { get; set; } = "Manager";

    /// <summary>1-5 assessed proficiency level.</summary>
    [Range(1, 5)]
    public int Level { get; set; } = 3;

    public int? CycleId { get; set; }

    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("CompetencyId")]
    [JsonIgnore]
    public virtual Competency? Competency { get; set; }

    [ForeignKey("AssessorEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Assessor { get; set; }
}

/// <summary>
/// An individual development plan.
/// </summary>
public class DevelopmentPlan
{
    [Key]
    public int DevelopmentPlanId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public int? ManagerEmployeeId { get; set; }

    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }

    /// <summary>DRAFT, ACTIVE, COMPLETED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("ManagerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Manager { get; set; }
}

/// <summary>
/// A development goal within a plan (optionally linked to a competency gap).
/// </summary>
public class DevelopmentGoal
{
    [Key]
    public int DevelopmentGoalId { get; set; }

    [Required]
    public int DevelopmentPlanId { get; set; }

    public int? CompetencyId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? DesiredOutcome { get; set; }

    public DateTime? TargetDate { get; set; }

    /// <summary>PLANNED, IN_PROGRESS, COMPLETED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "PLANNED";

    [ForeignKey("DevelopmentPlanId")]
    [JsonIgnore]
    public virtual DevelopmentPlan? DevelopmentPlan { get; set; }

    [ForeignKey("CompetencyId")]
    [JsonIgnore]
    public virtual Competency? Competency { get; set; }
}

/// <summary>
/// A learning course in the catalog.
/// </summary>
public class LearningCourse
{
    [Key]
    public int CourseId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? TitleLao { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }

    /// <summary>Classroom, Online, Workshop, External, SelfStudy.</summary>
    [MaxLength(20)]
    public string DeliveryType { get; set; } = "Classroom";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A scheduled training session for a course.
/// </summary>
public class TrainingSession
{
    [Key]
    public int SessionId { get; set; }

    [Required]
    public int CourseId { get; set; }

    public DateTime? Start { get; set; }
    public DateTime? End { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Instructor { get; set; }

    public int? Capacity { get; set; }

    /// <summary>SCHEDULED, COMPLETED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "SCHEDULED";

    [ForeignKey("CourseId")]
    [JsonIgnore]
    public virtual LearningCourse? Course { get; set; }
}

/// <summary>
/// An employee's enrollment in a training session.
/// </summary>
public class TrainingEnrollment
{
    [Key]
    public int EnrollmentId { get; set; }

    [Required]
    public int SessionId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    /// <summary>ASSIGNED, ENROLLED, IN_PROGRESS, COMPLETED, CANCELLED, NO_SHOW.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ASSIGNED";

    public int? AssignedByEmployeeId { get; set; }

    public DateTime? EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(50)]
    public string? Result { get; set; }

    [ForeignKey("SessionId")]
    [JsonIgnore]
    public virtual TrainingSession? Session { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// An employee certification.
/// </summary>
public class EmployeeCertification
{
    [Key]
    public int CertificationId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Issuer { get; set; }

    public DateTime? IssuedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    /// <summary>ACTIVE, EXPIRED, REVOKED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// An employee's career interest/aspiration (not a promotion commitment).
/// </summary>
public class CareerInterest
{
    [Key]
    public int CareerInterestId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [MaxLength(2000)]
    public string? Interests { get; set; }

    [MaxLength(2000)]
    public string? FutureRoles { get; set; }

    [MaxLength(2000)]
    public string? DevelopmentInterests { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

/// <summary>
/// A talent review entry (human-entered performance × potential).
/// </summary>
public class TalentReview
{
    [Key]
    public int TalentReviewId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public int? CycleId { get; set; }

    /// <summary>LOW, MEDIUM, HIGH (human-entered potential).</summary>
    [MaxLength(10)]
    public string? Potential { get; set; }

    /// <summary>LOW, MEDIUM, HIGH (human-entered performance).</summary>
    [MaxLength(10)]
    public string? Performance { get; set; }

    /// <summary>READY_NOW, READY_1_2_YEARS, DEVELOPING, NOT_ASSESSED.</summary>
    [MaxLength(20)]
    public string? Readiness { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int? ReviewedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
}

