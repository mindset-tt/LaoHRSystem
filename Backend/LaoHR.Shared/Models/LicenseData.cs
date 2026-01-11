using System;

namespace LaoHR.Shared.Models;

public class LicenseData
{
    public Guid LicenseId { get; set; } = Guid.NewGuid();
    public string CustomerName { get; set; } = string.Empty;
    public string Type { get; set; } = "TRIAL"; // TRIAL, STANDARD, ENTERPRISE
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpirationDate { get; set; }
    public int MaxEmployees { get; set; } = 10;
    public string HardwareId { get; set; } = "*"; // * = Any, or specific UUID
    public string Features { get; set; } = "ALL"; // Comma separated
}
