using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LaoHR.Shared.Models;

// =============================================================================
// Phase 4A — LAO BACK OFFICE foundation.
// -----------------------------------------------------------------------------
// Extends the HR platform into a modular back-office / ERP-lite suite:
// Procurement (Supplier → PR → PO → Goods Receipt), Inventory (Item → Warehouse
// → Stock Movement ledger), Fixed Assets, Budgeting, Contracts, and a generic
// Internal Request center. Reuses the existing ApprovalService, NotificationService,
// AuditLogInterceptor, EntityComment, and ConversionRate infrastructure.
//
// Design rules honored:
//   - decimal for all money (never float/double).
//   - Stock balance derives from the movement ledger (no mutable Item.Quantity).
//   - No client-supplied authority (approver/actor always resolved server-side).
//   - Additive migrations only (no rebase).
// =============================================================================

/// <summary>
/// Concurrency-safe document number sequence. One row per (prefix, year).
/// Callers increment within a transaction to avoid duplicate numbers.
/// </summary>
public class NumberSequence
{
    [Key]
    public int NumberSequenceId { get; set; }

    [Required, MaxLength(20)]
    public string Prefix { get; set; } = string.Empty;

    [Required]
    public int Year { get; set; }

    public int LastValue { get; set; } = 0;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Canonical supplier/vendor master. Bank + tax fields are sensitive and must be
/// restricted to Finance/Admin roles (never exposed to ordinary employees).
/// </summary>
public class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [Required, MaxLength(20)]
    public string SupplierCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? LegalName { get; set; }

    [MaxLength(50)]
    public string? TaxId { get; set; }

    [MaxLength(50)]
    public string? RegistrationNumber { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? Country { get; set; }

    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(50)]
    public string? BankAccount { get; set; }

    [MaxLength(100)]
    public string? PaymentTerms { get; set; }

    /// <summary>ACTIVE, INACTIVE, BLOCKED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ProvinceId")]
    [JsonIgnore]
    public virtual Province? Province { get; set; }

    [ForeignKey("DistrictId")]
    [JsonIgnore]
    public virtual District? District { get; set; }
}

/// <summary>
/// Financial tracking structure (distinct from Department, which is org structure).
/// </summary>
public class CostCenter
{
    [Key]
    public int CostCenterId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameLao { get; set; }

    public int? DepartmentId { get; set; }

    /// <summary>ACTIVE, INACTIVE.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }
}

/// <summary>
/// Lightweight budget envelope.
/// Available = Approved - Reserved - Committed - Actual.
/// Each consumed amount resides in exactly ONE state (no double counting):
///   Reserved  → PR approved (reservation)
///   Committed → PO created (reservation converted, or direct commitment)
///   Actual    → expense/AP posted
/// </summary>
public class Budget
{
    [Key]
    public int BudgetId { get; set; }

    [Required]
    public int FiscalYear { get; set; }

    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
    public int? CostCenterId { get; set; }

    [Required, MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    [Column(TypeName = "decimal(18,2)")]
    public decimal ApprovedAmount { get; set; }

    /// <summary>Amount reserved by approved purchase requests.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ReservedAmount { get; set; }

    /// <summary>Amount committed by purchase orders.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CommittedAmount { get; set; }

    /// <summary>Amount actually spent (posted expenses/AP).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualAmount { get; set; }

    /// <summary>DRAFT, APPROVED, CLOSED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("CostCenterId")]
    [JsonIgnore]
    public virtual CostCenter? CostCenter { get; set; }
}

/// <summary>
/// Purchase requisition / request. Flows through the approval engine.
/// </summary>
public class PurchaseRequest
{
    [Key]
    public int PurchaseRequestId { get; set; }

    [Required, MaxLength(20)]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    public int RequestedByEmployeeId { get; set; }

    public int? DepartmentId { get; set; }
    public int? ProjectId { get; set; }
    public int? CostCenterId { get; set; }

    /// <summary>Budget envelope this request reserves against (optional).</summary>
    public int? BudgetId { get; set; }

    public DateTime? RequiredDate { get; set; }

    [MaxLength(1000)]
    public string? Purpose { get; set; }

    /// <summary>DRAFT, PENDING_APPROVAL, APPROVED, REJECTED, CANCELLED, CONVERTED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalEstimatedAmount { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("RequestedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? RequestedBy { get; set; }

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("CostCenterId")]
    [JsonIgnore]
    public virtual CostCenter? CostCenter { get; set; }

    [ForeignKey("BudgetId")]
    [JsonIgnore]
    public virtual Budget? Budget { get; set; }

    [JsonIgnore]
    public virtual ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
}

/// <summary>
/// A line on a purchase request. Supports catalog item or free-text description.
/// </summary>
public class PurchaseRequestItem
{
    [Key]
    public int PurchaseRequestItemId { get; set; }

    [Required]
    public int PurchaseRequestId { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int? ItemId { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; } = 1;

    [MaxLength(20)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedUnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedAmount { get; set; }

    public int? PreferredSupplierId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey("PurchaseRequestId")]
    [JsonIgnore]
    public virtual PurchaseRequest? PurchaseRequest { get; set; }

    [ForeignKey("ItemId")]
    [JsonIgnore]
    public virtual InventoryItem? Item { get; set; }

    [ForeignKey("PreferredSupplierId")]
    [JsonIgnore]
    public virtual Supplier? PreferredSupplier { get; set; }
}

/// <summary>
/// Purchase order created from an approved request. Preserves a commercial snapshot.
/// </summary>
public class PurchaseOrder
{
    [Key]
    public int PurchaseOrderId { get; set; }

    [Required, MaxLength(20)]
    public string PONumber { get; set; } = string.Empty;

    [Required]
    public int SupplierId { get; set; }

    public int? RequestId { get; set; }

    /// <summary>Budget envelope this PO commits against (optional).</summary>
    public int? BudgetId { get; set; }

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "LAK";

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDate { get; set; }

    [MaxLength(100)]
    public string? PaymentTerms { get; set; }

    /// <summary>DRAFT, PENDING_APPROVAL, APPROVED, SENT, PARTIALLY_RECEIVED, RECEIVED, CLOSED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Tax { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("SupplierId")]
    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }

    [ForeignKey("RequestId")]
    [JsonIgnore]
    public virtual PurchaseRequest? Request { get; set; }

    [ForeignKey("BudgetId")]
    [JsonIgnore]
    public virtual Budget? Budget { get; set; }

    [JsonIgnore]
    public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}

/// <summary>
/// A purchase order line. Snapshot of description/price/tax/unit at order time.
/// </summary>
public class PurchaseOrderItem
{
    [Key]
    public int PurchaseOrderItemId { get; set; }

    [Required]
    public int PurchaseOrderId { get; set; }

    [Required, MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int? ItemId { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; } = 1;

    [MaxLength(20)]
    public string? Unit { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,4)")]
    public decimal TaxRate { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey("PurchaseOrderId")]
    [JsonIgnore]
    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    [ForeignKey("ItemId")]
    [JsonIgnore]
    public virtual InventoryItem? Item { get; set; }
}

/// <summary>
/// Goods receipt against a purchase order. Supports partial receipt.
/// </summary>
public class GoodsReceipt
{
    [Key]
    public int GoodsReceiptId { get; set; }

    [Required, MaxLength(20)]
    public string ReceiptNumber { get; set; } = string.Empty;

    [Required]
    public int PurchaseOrderId { get; set; }

    [Required]
    public int ReceivedByEmployeeId { get; set; }

    public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;

    public int? WarehouseId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>DRAFT, POSTED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("PurchaseOrderId")]
    [JsonIgnore]
    public virtual PurchaseOrder? PurchaseOrder { get; set; }

    [ForeignKey("ReceivedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? ReceivedBy { get; set; }

    [ForeignKey("WarehouseId")]
    [JsonIgnore]
    public virtual Warehouse? Warehouse { get; set; }

    [JsonIgnore]
    public virtual ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
}

/// <summary>
/// A goods receipt line (received/accepted/rejected quantities per PO line).
/// </summary>
public class GoodsReceiptItem
{
    [Key]
    public int GoodsReceiptItemId { get; set; }

    [Required]
    public int GoodsReceiptId { get; set; }

    [Required]
    public int PurchaseOrderItemId { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal QuantityReceived { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal AcceptedQuantity { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal RejectedQuantity { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey("GoodsReceiptId")]
    [JsonIgnore]
    public virtual GoodsReceipt? GoodsReceipt { get; set; }

    [ForeignKey("PurchaseOrderItemId")]
    [JsonIgnore]
    public virtual PurchaseOrderItem? PurchaseOrderItem { get; set; }
}

/// <summary>
/// Inventory item category (simple hierarchy, cycle-prevented at service level).
/// </summary>
public class InventoryCategory
{
    [Key]
    public int InventoryCategoryId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameLao { get; set; }

    public int? ParentCategoryId { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    [ForeignKey("ParentCategoryId")]
    [JsonIgnore]
    public virtual InventoryCategory? ParentCategory { get; set; }
}

/// <summary>
/// Canonical inventory item / product / material master.
/// </summary>
public class InventoryItem
{
    [Key]
    public int InventoryItemId { get; set; }

    [Required, MaxLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? NameLao { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int? CategoryId { get; set; }

    [MaxLength(20)]
    public string? UnitOfMeasure { get; set; }

    /// <summary>STOCK, CONSUMABLE, SERVICE, ASSET.</summary>
    [Required, MaxLength(20)]
    public string ItemType { get; set; } = "STOCK";

    public bool TrackInventory { get; set; } = true;

    [Column(TypeName = "decimal(18,3)")]
    public decimal? ReorderLevel { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public virtual InventoryCategory? Category { get; set; }
}

/// <summary>
/// A stock location. Multiple warehouses supported.
/// </summary>
public class Warehouse
{
    [Key]
    public int WarehouseId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameLao { get; set; }

    public int? WorkLocationId { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    public int? ManagerEmployeeId { get; set; }

    /// <summary>ACTIVE, INACTIVE.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("WorkLocationId")]
    [JsonIgnore]
    public virtual WorkLocation? WorkLocation { get; set; }

    [ForeignKey("ManagerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Manager { get; set; }
}

/// <summary>
/// Immutable stock movement ledger entry. Stock balance derives from these.
/// </summary>
public class StockMovement
{
    [Key]
    public int StockMovementId { get; set; }

    [Required]
    public int ItemId { get; set; }

    [Required]
    public int WarehouseId { get; set; }

    /// <summary>RECEIPT, ISSUE, TRANSFER_IN, TRANSFER_OUT, ADJUSTMENT_IN, ADJUSTMENT_OUT, RETURN.</summary>
    [Required, MaxLength(20)]
    public string MovementType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    [MaxLength(30)]
    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    [Required]
    public int PerformedByEmployeeId { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    [ForeignKey("ItemId")]
    [JsonIgnore]
    public virtual InventoryItem? Item { get; set; }

    [ForeignKey("WarehouseId")]
    [JsonIgnore]
    public virtual Warehouse? Warehouse { get; set; }

    [ForeignKey("PerformedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? PerformedBy { get; set; }
}

/// <summary>
/// Fixed asset register entry.
/// </summary>
public class Asset
{
    [Key]
    public int AssetId { get; set; }

    [Required, MaxLength(20)]
    public string AssetCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    public int? PurchaseOrderItemId { get; set; }

    /// <summary>Goods receipt line that generated this asset (traceability).</summary>
    public int? GoodsReceiptItemId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? AcquisitionCost { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public int? WorkLocationId { get; set; }

    public int? CustodianEmployeeId { get; set; }

    /// <summary>AVAILABLE, ASSIGNED, IN_MAINTENANCE, LOST, DAMAGED, RETIRED, DISPOSED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "AVAILABLE";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public virtual InventoryCategory? Category { get; set; }

    [ForeignKey("PurchaseOrderItemId")]
    [JsonIgnore]
    public virtual PurchaseOrderItem? PurchaseOrderItem { get; set; }

    [ForeignKey("GoodsReceiptItemId")]
    [JsonIgnore]
    public virtual GoodsReceiptItem? GoodsReceiptItem { get; set; }

    [ForeignKey("WorkLocationId")]
    [JsonIgnore]
    public virtual WorkLocation? WorkLocation { get; set; }

    [ForeignKey("CustodianEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Custodian { get; set; }
}

/// <summary>
/// Asset assignment history (append-only; never overwrite).
/// </summary>
public class AssetAssignment
{
    [Key]
    public int AssetAssignmentId { get; set; }

    [Required]
    public int AssetId { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReturnedAt { get; set; }

    [Required]
    public int AssignedByEmployeeId { get; set; }

    [MaxLength(500)]
    public string? ConditionAtAssignment { get; set; }

    [MaxLength(500)]
    public string? ConditionAtReturn { get; set; }

    [ForeignKey("AssetId")]
    [JsonIgnore]
    public virtual Asset? Asset { get; set; }

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("AssignedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? AssignedBy { get; set; }
}

/// <summary>
/// Lightweight contract register.
/// </summary>
public class Contract
{
    [Key]
    public int ContractId { get; set; }

    [Required, MaxLength(20)]
    public string ContractNumber { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public int? SupplierId { get; set; }

    [MaxLength(50)]
    public string? ContractType { get; set; }

    [Required]
    public int OwnerEmployeeId { get; set; }

    public int? DepartmentId { get; set; }

    public int? ProjectId { get; set; }

    public int? CostCenterId { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Amount { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>DRAFT, UNDER_REVIEW, PENDING_APPROVAL, ACTIVE, EXPIRING, EXPIRED, TERMINATED, CANCELLED, ARCHIVED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    /// <summary>AUTO, MANUAL, NONE.</summary>
    [MaxLength(20)]
    public string? RenewalType { get; set; }

    public bool AutoRenew { get; set; }

    public int? NoticePeriodDays { get; set; }

    public DateTime? NoticeDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("SupplierId")]
    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }

    [ForeignKey("OwnerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Owner { get; set; }

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [ForeignKey("CostCenterId")]
    [JsonIgnore]
    public virtual CostCenter? CostCenter { get; set; }
}

/// <summary>
/// Configurable service-request category (IT, FACILITIES, ADMIN, PROCUREMENT, HR, FINANCE).
/// </summary>
public class ServiceRequestCategory
{
    [Key]
    public int ServiceRequestCategoryId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameLao { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Generic internal/operational request (IT, facilities, admin, etc.).
/// </summary>
public class ServiceRequest
{
    [Key]
    public int ServiceRequestId { get; set; }

    [Required, MaxLength(20)]
    public string RequestNumber { get; set; } = string.Empty;

    [Required]
    public int RequesterEmployeeId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required, MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>LOW, MEDIUM, HIGH, URGENT.</summary>
    [Required, MaxLength(20)]
    public string Priority { get; set; } = "MEDIUM";

    /// <summary>OPEN, IN_PROGRESS, ON_HOLD, RESOLVED, CLOSED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "OPEN";

    public int? AssignedEmployeeId { get; set; }

    public int? DepartmentId { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    [ForeignKey("RequesterEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Requester { get; set; }

    [ForeignKey("CategoryId")]
    [JsonIgnore]
    public virtual ServiceRequestCategory? Category { get; set; }

    [ForeignKey("AssignedEmployeeId")]
    [JsonIgnore]
    public virtual Employee? AssignedTo { get; set; }

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }
}
