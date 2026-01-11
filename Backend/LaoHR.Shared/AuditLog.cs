using System.ComponentModel.DataAnnotations;

namespace LaoHR.Shared.Models;

/// <summary>
/// Audit Log used to track changes to critical data
/// </summary>
public class AuditLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    [MaxLength(100)]
    public string? UserId { get; set; } // Username or User ID
    
    [Required, MaxLength(100)]
    public string EntityName { get; set; } = string.Empty;
    
    [Required, MaxLength(20)]
    public string Action { get; set; } = string.Empty; // UPDATE, INSERT, DELETE
    
    [Required, MaxLength(100)]
    public string KeyValues { get; set; } = string.Empty;
    
    public string? OldValues { get; set; } // JSON
    
    public string? NewValues { get; set; } // JSON
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
