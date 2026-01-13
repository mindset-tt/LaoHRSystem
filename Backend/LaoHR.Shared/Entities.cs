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
    
    public int TotalDays { get; set; }
    
    [MaxLength(500)]
    public string? Reason { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "PENDING";
    
    public int? ApprovedById { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    [MaxLength(500)]
    public string? ApproverNotes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    //[ForeignKey("EmployeeId")] -- Standard Convention prefers placement or attribute
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
