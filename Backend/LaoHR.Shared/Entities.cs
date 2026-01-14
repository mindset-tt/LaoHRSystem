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
    
    // Navigation
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
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BaseSalary { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal OvertimePay { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Allowances { get; set; }
    
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
    
    [MaxLength(20)]
    public string Status { get; set; } = "CALCULATED";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
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
/// Public Holiday
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
    public string? NameEn { get; set; }
    
    public int Year { get; set; }
    
    public bool IsRecurring { get; set; }
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
