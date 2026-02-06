using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LaoHR.Shared.Models;

/// <summary>
/// Dynamic Payroll Adjustment (Earning or Deduction)
/// Used for one-off or variable monthly items like Commission, Fuel Allowance, etc.
/// </summary>
public class PayrollAdjustment
{
    [Key]
    public int AdjustmentId { get; set; }
    
    [Required]
    public int EmployeeId { get; set; }
    
    [Required]
    public int PeriodId { get; set; }
    
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty; // e.g., "Commission", "Fuel Allowance"
    
    [Required, MaxLength(20)]
    public string Type { get; set; } = "EARNING"; // EARNING, DEDUCTION
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// If true, this earning is added to TaxableIncome before tax calculation.
    /// If false, it is added to NetSalary after tax (Non-Taxable).
    /// </summary>
    public bool IsTaxable { get; set; } = true;
    
    /// <summary>
    /// If true, this amount is considered for NSSF calculation (rare for allowances).
    /// </summary>
    public bool IsNssfAssessable { get; set; } = false;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation
    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }
    
    [ForeignKey("PeriodId")]
    [JsonIgnore]
    public virtual PayrollPeriod? PayrollPeriod { get; set; }
}
