using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LaoHR.Shared.Models;

// =============================================================================
// Phase 4B — Finance + Accounting foundation.
// -----------------------------------------------------------------------------
// Connects the operational Back Office chain (Budget → PR → PO → Receipt →
// Inventory/Asset) to a financial/accounting chain (Supplier Invoice → 3-way
// match → AP → Payment → Journal → General Ledger → Reporting).
//
// Design rules honored:
//   - decimal for all money (never float/double).
//   - Operational event ≠ accounting entry unless posting semantics are defined.
//   - Double-entry invariant: SUM(DEBIT) == SUM(CREDIT) for every POSTED journal.
//   - Posted journals are immutable (correction via reversal + new journal).
//   - No guessed Lao VAT/withholding/tax values (configurable, LEGAL_CONFIRMATION_REQUIRED).
//   - No client-supplied financial authority.
// =============================================================================

/// <summary>
/// Chart of Accounts entry. Configurable; no statutory numbering is generated.
/// </summary>
public class Account
{
    [Key]
    public int AccountId { get; set; }

    [Required, MaxLength(50)]
    public string AccountCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameLao { get; set; }

    /// <summary>ASSET, LIABILITY, EQUITY, REVENUE, EXPENSE.</summary>
    [Required, MaxLength(20)]
    public string AccountType { get; set; } = "ASSET";

    public int? ParentAccountId { get; set; }

    /// <summary>False = header/summary account; journal lines must target posting accounts.</summary>
    public bool IsPostingAccount { get; set; } = true;

    public bool IsActive { get; set; } = true;

    [MaxLength(3)]
    public string? Currency { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ParentAccountId")]
    [JsonIgnore]
    public virtual Account? ParentAccount { get; set; }
}

/// <summary>
/// Fiscal year. NOT assumed to be a calendar year.
/// </summary>
public class FiscalYear
{
    [Key]
    public int FiscalYearId { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>OPEN, CLOSED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "OPEN";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Fiscal period within a fiscal year.
/// </summary>
public class FiscalPeriod
{
    [Key]
    public int FiscalPeriodId { get; set; }

    [Required]
    public int FiscalYearId { get; set; }

    [Required]
    public int PeriodNumber { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>OPEN, CLOSED, LOCKED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "OPEN";

    [ForeignKey("FiscalYearId")]
    [JsonIgnore]
    public virtual FiscalYear? FiscalYear { get; set; }
}

/// <summary>
/// Journal entry (header). Lines carry the debit/credit amounts.
/// </summary>
public class JournalEntry
{
    [Key]
    public int JournalEntryId { get; set; }

    [Required, MaxLength(20)]
    public string JournalNumber { get; set; } = string.Empty;

    [Required]
    public DateTime PostingDate { get; set; }

    [Required]
    public int FiscalPeriodId { get; set; }

    /// <summary>SUPPLIER_INVOICE, PAYMENT, EXPENSE, ASSET, MANUAL_JOURNAL.</summary>
    [Required, MaxLength(30)]
    public string SourceType { get; set; } = "MANUAL_JOURNAL";

    public int? SourceId { get; set; }

    /// <summary>
    /// Posting purpose discriminator for source uniqueness (e.g. "INVOICE",
    /// "PAYMENT", "EXPENSE"). Combined with SourceType + SourceId to prevent
    /// a source from auto-posting twice.
    /// </summary>
    [MaxLength(30)]
    public string? PostingPurpose { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>DRAFT, POSTED, REVERSED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    public int? CreatedByEmployeeId { get; set; }
    public int? PostedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PostedAt { get; set; }

    /// <summary>For reversals: the original journal this entry reverses.</summary>
    public int? ReversesJournalEntryId { get; set; }

    [ForeignKey("FiscalPeriodId")]
    [JsonIgnore]
    public virtual FiscalPeriod? FiscalPeriod { get; set; }

    [ForeignKey("ReversesJournalEntryId")]
    [JsonIgnore]
    public virtual JournalEntry? ReversesJournalEntry { get; set; }

    [JsonIgnore]
    public virtual ICollection<JournalLine> Lines { get; set; } = new List<JournalLine>();
}

/// <summary>
/// A single journal line. Debit and Credit must not both be > 0, and not both zero.
/// </summary>
public class JournalLine
{
    [Key]
    public int JournalLineId { get; set; }

    [Required]
    public int JournalEntryId { get; set; }

    [Required]
    public int AccountId { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Debit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Credit { get; set; }

    public int? CostCenterId { get; set; }
    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal? ExchangeRate { get; set; }

    [MaxLength(200)]
    public string? Reference { get; set; }

    [ForeignKey("JournalEntryId")]
    [JsonIgnore]
    public virtual JournalEntry? JournalEntry { get; set; }

    [ForeignKey("AccountId")]
    [JsonIgnore]
    public virtual Account? Account { get; set; }

    [ForeignKey("CostCenterId")]
    [JsonIgnore]
    public virtual CostCenter? CostCenter { get; set; }

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }
}

/// <summary>
/// Supplier invoice (payable obligation). PO-backed or non-PO.
/// </summary>
public class SupplierInvoice
{
    [Key]
    public int SupplierInvoiceId { get; set; }

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public int SupplierId { get; set; }

    public int? PurchaseOrderId { get; set; }

    [Required]
    public DateTime InvoiceDate { get; set; }

    public DateTime? ReceivedDate { get; set; }
    public DateTime? DueDate { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount { get; set; }

    /// <summary>DRAFT, PENDING_MATCH, MATCH_EXCEPTION, PENDING_APPROVAL, APPROVED, PARTIALLY_PAID, PAID, VOID.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    /// <summary>MATCHED, QUANTITY_VARIANCE, PRICE_VARIANCE, QUANTITY_AND_PRICE_VARIANCE, NOT_APPLICABLE.</summary>
    [MaxLength(30)]
    public string? MatchStatus { get; set; }

    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }
    public int? DepartmentId { get; set; }

    public int? CreatedByEmployeeId { get; set; }
    public int? ApprovedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("SupplierId")]
    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }

    [ForeignKey("PurchaseOrderId")]
    [JsonIgnore]
    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    [ForeignKey("CostCenterId")]
    [JsonIgnore]
    public virtual CostCenter? CostCenter { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }

    [JsonIgnore]
    public virtual ICollection<SupplierInvoiceLine> Lines { get; set; } = new List<SupplierInvoiceLine>();
}

/// <summary>
/// A supplier invoice line.
/// </summary>
public class SupplierInvoiceLine
{
    [Key]
    public int SupplierInvoiceLineId { get; set; }

    [Required]
    public int SupplierInvoiceId { get; set; }

    public int? PurchaseOrderItemId { get; set; }
    public int? InventoryItemId { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    public int? AccountId { get; set; }
    public int? CostCenterId { get; set; }
    public int? ProjectId { get; set; }

    [ForeignKey("SupplierInvoiceId")]
    [JsonIgnore]
    public virtual SupplierInvoice? SupplierInvoice { get; set; }

    [ForeignKey("PurchaseOrderItemId")]
    [JsonIgnore]
    public virtual PurchaseOrderItem? PurchaseOrderItem { get; set; }

    [ForeignKey("InventoryItemId")]
    [JsonIgnore]
    public virtual InventoryItem? InventoryItem { get; set; }

    [ForeignKey("AccountId")]
    [JsonIgnore]
    public virtual Account? Account { get; set; }
}

/// <summary>
/// A payment (settlement of one or more supplier invoices).
/// </summary>
public class Payment
{
    [Key]
    public int PaymentId { get; set; }

    [Required, MaxLength(20)]
    public string PaymentNumber { get; set; } = string.Empty;

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required, MaxLength(30)]
    public string PaymentMethod { get; set; } = "BANK_TRANSFER";

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public int? BankAccountId { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    /// <summary>DRAFT, PENDING_APPROVAL, APPROVED, POSTED, VOID.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public int? CreatedByEmployeeId { get; set; }
    public int? ApprovedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("BankAccountId")]
    [JsonIgnore]
    public virtual BankAccount? BankAccount { get; set; }

    [JsonIgnore]
    public virtual ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}

/// <summary>
/// Allocation of a payment to a supplier invoice (supports partial + multi-invoice).
/// </summary>
public class PaymentAllocation
{
    [Key]
    public int PaymentAllocationId { get; set; }

    [Required]
    public int PaymentId { get; set; }

    [Required]
    public int SupplierInvoiceId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [ForeignKey("PaymentId")]
    [JsonIgnore]
    public virtual Payment? Payment { get; set; }

    [ForeignKey("SupplierInvoiceId")]
    [JsonIgnore]
    public virtual SupplierInvoice? SupplierInvoice { get; set; }
}

/// <summary>
/// Corporate bank account. Full account number is Finance/Admin only.
/// </summary>
public class BankAccount
{
    [Key]
    public int BankAccountId { get; set; }

    [Required, MaxLength(100)]
    public string BankName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string AccountName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string AccountNumber { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    [MaxLength(100)]
    public string? Branch { get; set; }

    [MaxLength(20)]
    public string? Swift { get; set; }

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OpeningBalance { get; set; }

    public int? GLAccountId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("GLAccountId")]
    [JsonIgnore]
    public virtual Account? GLAccount { get; set; }
}

// =============================================================================
// Phase 4B — Accounts Receivable foundation (minimal; full billing deferred).
// =============================================================================

/// <summary>
/// Customer master (AR foundation). Not a full CRM.
/// </summary>
public class Customer
{
    [Key]
    public int CustomerId { get; set; }

    [Required, MaxLength(20)]
    public string CustomerCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LegalName { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>ACTIVE, INACTIVE, BLOCKED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Customer invoice (receivable). Minimal foundation.
/// </summary>
public class CustomerInvoice
{
    [Key]
    public int CustomerInvoiceId { get; set; }

    [Required, MaxLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public DateTime InvoiceDate { get; set; }

    public DateTime? DueDate { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount { get; set; }

    /// <summary>DRAFT, APPROVED, PARTIALLY_PAID, PAID, VOID.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CustomerId")]
    [JsonIgnore]
    public virtual Customer? Customer { get; set; }

    [JsonIgnore]
    public virtual ICollection<CustomerInvoiceLine> Lines { get; set; } = new List<CustomerInvoiceLine>();
}

/// <summary>
/// A customer invoice line.
/// </summary>
public class CustomerInvoiceLine
{
    [Key]
    public int CustomerInvoiceLineId { get; set; }

    [Required]
    public int CustomerInvoiceId { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    public int? AccountId { get; set; }

    [ForeignKey("CustomerInvoiceId")]
    [JsonIgnore]
    public virtual CustomerInvoice? CustomerInvoice { get; set; }

    [ForeignKey("AccountId")]
    [JsonIgnore]
    public virtual Account? Account { get; set; }
}

/// <summary>
/// AR receipt (customer payment received).
/// </summary>
public class Receipt
{
    [Key]
    public int ReceiptId { get; set; }

    [Required, MaxLength(20)]
    public string ReceiptNumber { get; set; } = string.Empty;

    [Required]
    public DateTime ReceiptDate { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public int? BankAccountId { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    /// <summary>DRAFT, POSTED, VOID.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("BankAccountId")]
    [JsonIgnore]
    public virtual BankAccount? BankAccount { get; set; }

    [JsonIgnore]
    public virtual ICollection<ReceiptAllocation> Allocations { get; set; } = new List<ReceiptAllocation>();
}

/// <summary>
/// Allocation of an AR receipt to a customer invoice.
/// </summary>
public class ReceiptAllocation
{
    [Key]
    public int ReceiptAllocationId { get; set; }

    [Required]
    public int ReceiptId { get; set; }

    [Required]
    public int CustomerInvoiceId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [ForeignKey("ReceiptId")]
    [JsonIgnore]
    public virtual Receipt? Receipt { get; set; }

    [ForeignKey("CustomerInvoiceId")]
    [JsonIgnore]
    public virtual CustomerInvoice? CustomerInvoice { get; set; }
}
