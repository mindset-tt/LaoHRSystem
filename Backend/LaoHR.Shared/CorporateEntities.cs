using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace LaoHR.Shared.Models;

// =============================================================================
// Phase 4C — Corporate Operations expansion.
// -----------------------------------------------------------------------------
// Extends the Back Office into a broader business-operations platform:
//   - Corporate Document Management (polymorphic, versioned)
//   - Contract lifecycle (approval, renewal, history)
//   - Service Desk (assignment, resolution, history)
//   - Facilities + Rooms + Room booking (concurrency-safe)
//   - Maintenance work orders (polymorphic source)
//   - Fleet / Vehicles + booking + trips + fuel
//   - Travel management (request, approval, expense integration)
//   - Visitor / front-desk foundation
//
// Design rules honored:
//   - decimal for all money (never float/double).
//   - No client-supplied authority (actor always resolved server-side).
//   - Additive migrations only (no rebase).
//   - Reuse existing masters (Supplier, Employee, Department, Project, CostCenter,
//     Asset, Expense, WorkLocation) — no duplicate master data.
//   - Booking overlap is enforced server-side and proven on PostgreSQL 16.
// =============================================================================

// =============================================================================
// Corporate Document Management (DMS)
// =============================================================================

/// <summary>
/// A corporate document linked to any owner entity (polymorphic). Versioned via
/// <see cref="DocumentVersion"/>; the current version is never overwritten.
/// </summary>
public class CorporateDocument
{
    [Key]
    public int DocumentId { get; set; }

    [Required, MaxLength(30)]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>POLICY, CONTRACT, INVOICE, QUOTATION, RECEIPT, CERTIFICATE, LICENSE, IDENTIFICATION, ASSET_DOCUMENT, VEHICLE_DOCUMENT, TRAVEL_DOCUMENT, PROJECT_DOCUMENT, GENERAL.</summary>
    [Required, MaxLength(30)]
    public string DocumentType { get; set; } = "GENERAL";

    /// <summary>Polymorphic owner: EMPLOYEE, SUPPLIER, PURCHASE_ORDER, CONTRACT, ASSET, VEHICLE, TRAVEL_REQUEST, SERVICE_REQUEST, PROJECT, etc.</summary>
    [Required, MaxLength(30)]
    public string OwnerEntityType { get; set; } = "GENERAL";

    [Required]
    public int OwnerEntityId { get; set; }

    /// <summary>INTERNAL, CONFIDENTIAL, RESTRICTED.</summary>
    [Required, MaxLength(20)]
    public string Confidentiality { get; set; } = "INTERNAL";

    /// <summary>ACTIVE, ARCHIVED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    public int CurrentVersion { get; set; } = 1;

    public DateTime? DocumentDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? CreatedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("CreatedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? CreatedBy { get; set; }

    [JsonIgnore]
    public virtual ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}

/// <summary>
/// A single immutable version of a document's file. New versions append; the
/// previous version is never overwritten.
/// </summary>
public class DocumentVersion
{
    [Key]
    public int DocumentVersionId { get; set; }

    [Required]
    public int DocumentId { get; set; }

    [Required]
    public int VersionNumber { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MimeType { get; set; }

    public long FileSize { get; set; }

    [Required, MaxLength(500)]
    public string StorageReference { get; set; } = string.Empty;

    [MaxLength(64)]
    public string? Checksum { get; set; }

    public int? UploadedByEmployeeId { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("DocumentId")]
    [JsonIgnore]
    public virtual CorporateDocument? Document { get; set; }

    [ForeignKey("UploadedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? UploadedBy { get; set; }
}

// =============================================================================
// Contract lifecycle
// =============================================================================

/// <summary>
/// Append-only contract history (renewal / amendment / termination). The
/// canonical <see cref="Contract"/> row is updated, but prior terms are preserved
/// here so history is never lost.
/// </summary>
public class ContractHistory
{
    [Key]
    public int ContractHistoryId { get; set; }

    [Required]
    public int ContractId { get; set; }

    /// <summary>RENEWAL, AMENDMENT, TERMINATION, STATUS_CHANGE.</summary>
    [Required, MaxLength(20)]
    public string ChangeType { get; set; } = "AMENDMENT";

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime? PreviousStartDate { get; set; }
    public DateTime? PreviousEndDate { get; set; }
    public decimal? PreviousAmount { get; set; }
    public string? PreviousStatus { get; set; }

    public DateTime? NewStartDate { get; set; }
    public DateTime? NewEndDate { get; set; }
    public decimal? NewAmount { get; set; }
    public string? NewStatus { get; set; }

    public int? ChangedByEmployeeId { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ContractId")]
    [JsonIgnore]
    public virtual Contract? Contract { get; set; }

    [ForeignKey("ChangedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? ChangedBy { get; set; }
}

// =============================================================================
// Service Desk
// =============================================================================

/// <summary>
/// Append-only service-request history (assignment / status / priority changes).
/// </summary>
public class ServiceRequestHistory
{
    [Key]
    public int ServiceRequestHistoryId { get; set; }

    [Required]
    public int ServiceRequestId { get; set; }

    /// <summary>ASSIGNED, STATUS_CHANGE, PRIORITY_CHANGE, RESOLVED, CLOSED.</summary>
    [Required, MaxLength(20)]
    public string ChangeType { get; set; } = "STATUS_CHANGE";

    public string? FromValue { get; set; }
    public string? ToValue { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int? ChangedByEmployeeId { get; set; }

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("ServiceRequestId")]
    [JsonIgnore]
    public virtual ServiceRequest? ServiceRequest { get; set; }

    [ForeignKey("ChangedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? ChangedBy { get; set; }
}

// =============================================================================
// Facilities + Rooms + Room booking
// =============================================================================

/// <summary>
/// A physical operational site/building/office. Distinct from WorkLocation
/// (employment location) and Warehouse (stock location).
/// </summary>
public class Facility
{
    [Key]
    public int FacilityId { get; set; }

    [Required, MaxLength(20)]
    public string FacilityCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? NameLao { get; set; }

    public int? WorkLocationId { get; set; }

    /// <summary>OFFICE, BUILDING, BRANCH, WAREHOUSE_SITE, OTHER.</summary>
    [Required, MaxLength(20)]
    public string FacilityType { get; set; } = "OFFICE";

    public int? ManagerEmployeeId { get; set; }

    [MaxLength(500)]
    public string? Address { get; set; }

    /// <summary>ACTIVE, INACTIVE.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ACTIVE";

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("WorkLocationId")]
    [JsonIgnore]
    public virtual WorkLocation? WorkLocation { get; set; }

    [ForeignKey("ManagerEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Manager { get; set; }

    [JsonIgnore]
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}

/// <summary>
/// A bookable/maintainable space inside a facility.
/// </summary>
public class Room
{
    [Key]
    public int RoomId { get; set; }

    [Required]
    public int FacilityId { get; set; }

    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>MEETING_ROOM, TRAINING_ROOM, WORKSPACE, CONFERENCE_ROOM, OTHER.</summary>
    [Required, MaxLength(20)]
    public string RoomType { get; set; } = "MEETING_ROOM";

    public int? Capacity { get; set; }

    [MaxLength(20)]
    public string? Floor { get; set; }

    public bool Bookable { get; set; } = true;

    public bool IsActive { get; set; } = true;

    [ForeignKey("FacilityId")]
    [JsonIgnore]
    public virtual Facility? Facility { get; set; }

    [JsonIgnore]
    public virtual ICollection<RoomBooking> Bookings { get; set; } = new List<RoomBooking>();
}

/// <summary>
/// A room reservation. Overlapping active bookings for the same room are rejected.
/// </summary>
public class RoomBooking
{
    [Key]
    public int RoomBookingId { get; set; }

    [Required, MaxLength(30)]
    public string BookingNumber { get; set; } = string.Empty;

    [Required]
    public int RoomId { get; set; }

    [Required]
    public int BookedByEmployeeId { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Purpose { get; set; }

    public int? ParticipantCount { get; set; }

    /// <summary>CONFIRMED, CANCELLED, COMPLETED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "CONFIRMED";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("RoomId")]
    [JsonIgnore]
    public virtual Room? Room { get; set; }

    [ForeignKey("BookedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? BookedBy { get; set; }
}

// =============================================================================
// Maintenance work orders
// =============================================================================

/// <summary>
/// A maintenance work order for a facility, room, asset, vehicle, or service request.
/// </summary>
public class WorkOrder
{
    [Key]
    public int WorkOrderId { get; set; }

    [Required, MaxLength(30)]
    public string WorkOrderNumber { get; set; } = string.Empty;

    /// <summary>FACILITY, ROOM, ASSET, VEHICLE, SERVICE_REQUEST.</summary>
    [Required, MaxLength(30)]
    public string SourceType { get; set; } = "FACILITY";

    [Required]
    public int SourceId { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    /// <summary>LOW, MEDIUM, HIGH, URGENT.</summary>
    [Required, MaxLength(20)]
    public string Priority { get; set; } = "MEDIUM";

    /// <summary>OPEN, ASSIGNED, IN_PROGRESS, COMPLETED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "OPEN";

    public int? AssignedEmployeeId { get; set; }

    public int? SupplierId { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("AssignedEmployeeId")]
    [JsonIgnore]
    public virtual Employee? AssignedTo { get; set; }

    [ForeignKey("SupplierId")]
    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }
}

// =============================================================================
// Fleet / Vehicles
// =============================================================================

/// <summary>
/// A company vehicle. May reference a canonical <see cref="Asset"/> (fixed asset).
/// </summary>
public class Vehicle
{
    [Key]
    public int VehicleId { get; set; }

    [Required, MaxLength(20)]
    public string VehicleCode { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string RegistrationNumber { get; set; } = string.Empty;

    public int? AssetId { get; set; }

    [MaxLength(100)]
    public string? Make { get; set; }

    [MaxLength(100)]
    public string? Model { get; set; }

    public int? Year { get; set; }

    [MaxLength(50)]
    public string? VIN { get; set; }

    /// <summary>CAR, TRUCK, VAN, MOTORBIKE, OTHER.</summary>
    [MaxLength(20)]
    public string? VehicleType { get; set; }

    [MaxLength(20)]
    public string? FuelType { get; set; }

    /// <summary>AVAILABLE, RESERVED, IN_USE, IN_MAINTENANCE, OUT_OF_SERVICE, DISPOSED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "AVAILABLE";

    public int? WorkLocationId { get; set; }

    public int CurrentOdometer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("AssetId")]
    [JsonIgnore]
    public virtual Asset? Asset { get; set; }

    [ForeignKey("WorkLocationId")]
    [JsonIgnore]
    public virtual WorkLocation? WorkLocation { get; set; }

    [JsonIgnore]
    public virtual ICollection<VehicleBooking> Bookings { get; set; } = new List<VehicleBooking>();
}

/// <summary>
/// A vehicle reservation. Overlapping active bookings for the same vehicle are rejected.
/// </summary>
public class VehicleBooking
{
    [Key]
    public int VehicleBookingId { get; set; }

    [Required, MaxLength(30)]
    public string BookingNumber { get; set; } = string.Empty;

    [Required]
    public int VehicleId { get; set; }

    [Required]
    public int RequesterEmployeeId { get; set; }

    public int? DriverEmployeeId { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Required, MaxLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Destination { get; set; }

    public int? ProjectId { get; set; }

    /// <summary>CONFIRMED, CANCELLED, COMPLETED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "CONFIRMED";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("VehicleId")]
    [JsonIgnore]
    public virtual Vehicle? Vehicle { get; set; }

    [ForeignKey("RequesterEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Requester { get; set; }

    [ForeignKey("DriverEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Driver { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }
}

/// <summary>
/// A completed (or in-progress) vehicle trip with odometer readings.
/// </summary>
public class VehicleTrip
{
    [Key]
    public int VehicleTripId { get; set; }

    public int? VehicleBookingId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    public int DriverEmployeeId { get; set; }

    [Required]
    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    [Required]
    public int StartOdometer { get; set; }

    public int? EndOdometer { get; set; }

    [MaxLength(500)]
    public string? Destination { get; set; }

    [MaxLength(1000)]
    public string? Purpose { get; set; }

    [ForeignKey("VehicleBookingId")]
    [JsonIgnore]
    public virtual VehicleBooking? VehicleBooking { get; set; }

    [ForeignKey("VehicleId")]
    [JsonIgnore]
    public virtual Vehicle? Vehicle { get; set; }

    [ForeignKey("DriverEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Driver { get; set; }
}

/// <summary>
/// Optional fuel log. Cost ultimately links to Expense/SupplierInvoice for posting.
/// </summary>
public class FuelLog
{
    [Key]
    public int FuelLogId { get; set; }

    [Required]
    public int VehicleId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public int Odometer { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? TotalAmount { get; set; }

    public int? SupplierId { get; set; }

    public int? ExpenseId { get; set; }

    public int? ReceiptDocumentId { get; set; }

    [ForeignKey("VehicleId")]
    [JsonIgnore]
    public virtual Vehicle? Vehicle { get; set; }

    [ForeignKey("SupplierId")]
    [JsonIgnore]
    public virtual Supplier? Supplier { get; set; }

    [ForeignKey("ExpenseId")]
    [JsonIgnore]
    public virtual Expense? Expense { get; set; }
}

// =============================================================================
// Travel management
// =============================================================================

/// <summary>
/// An employee business travel request.
/// </summary>
public class TravelRequest
{
    [Key]
    public int TravelRequestId { get; set; }

    [Required, MaxLength(30)]
    public string TravelNumber { get; set; } = string.Empty;

    [Required]
    public int EmployeeId { get; set; }

    public int? DepartmentId { get; set; }

    public int? ProjectId { get; set; }

    [Required, MaxLength(1000)]
    public string Purpose { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string Destination { get; set; } = string.Empty;

    [Required]
    public DateTime DepartureDate { get; set; }

    [Required]
    public DateTime ReturnDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedCost { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>DRAFT, PENDING_APPROVAL, APPROVED, BOOKED, IN_PROGRESS, COMPLETED, CANCELLED, SETTLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "DRAFT";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("EmployeeId")]
    [JsonIgnore]
    public virtual Employee? Employee { get; set; }

    [ForeignKey("DepartmentId")]
    [JsonIgnore]
    public virtual Department? Department { get; set; }

    [ForeignKey("ProjectId")]
    [JsonIgnore]
    public virtual Project? Project { get; set; }

    [JsonIgnore]
    public virtual ICollection<TravelSegment> Segments { get; set; } = new List<TravelSegment>();

    [JsonIgnore]
    public virtual ICollection<TravelAccommodation> Accommodations { get; set; } = new List<TravelAccommodation>();
}

/// <summary>
/// A travel segment (flight, train, bus, car, other).
/// </summary>
public class TravelSegment
{
    [Key]
    public int TravelSegmentId { get; set; }

    [Required]
    public int TravelRequestId { get; set; }

    /// <summary>FLIGHT, TRAIN, BUS, CAR, OTHER.</summary>
    [Required, MaxLength(20)]
    public string SegmentType { get; set; } = "FLIGHT";

    [MaxLength(200)]
    public string? Origin { get; set; }

    [MaxLength(200)]
    public string? Destination { get; set; }

    public DateTime? DepartureAt { get; set; }

    public DateTime? ArrivalAt { get; set; }

    [MaxLength(200)]
    public string? Reference { get; set; }

    [ForeignKey("TravelRequestId")]
    [JsonIgnore]
    public virtual TravelRequest? TravelRequest { get; set; }
}

/// <summary>
/// A travel accommodation record (hotel). Not a booking engine.
/// </summary>
public class TravelAccommodation
{
    [Key]
    public int TravelAccommodationId { get; set; }

    [Required]
    public int TravelRequestId { get; set; }

    [MaxLength(200)]
    public string? HotelName { get; set; }

    [MaxLength(200)]
    public string? City { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    [MaxLength(200)]
    public string? BookingReference { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    [ForeignKey("TravelRequestId")]
    [JsonIgnore]
    public virtual TravelRequest? TravelRequest { get; set; }
}

/// <summary>
/// A travel advance (distinct from EmployeeLoan). Settled against expenses.
/// </summary>
public class TravelAdvance
{
    [Key]
    public int TravelAdvanceId { get; set; }

    [Required]
    public int TravelRequestId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; }

    /// <summary>ISSUED, SETTLED, CANCELLED.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "ISSUED";

    public DateTime? IssuedAt { get; set; }

    public DateTime? SettledAt { get; set; }

    [ForeignKey("TravelRequestId")]
    [JsonIgnore]
    public virtual TravelRequest? TravelRequest { get; set; }
}

// =============================================================================
// Visitor / front-desk foundation
// =============================================================================

/// <summary>
/// A visitor (minimal personal data; no ID/passport storage by default).
/// </summary>
public class Visitor
{
    [Key]
    public int VisitorId { get; set; }

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Company { get; set; }

    [MaxLength(30)]
    public string? Phone { get; set; }

    [MaxLength(100)]
    public string? Email { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}

/// <summary>
/// A single visit (pre-registration → check-in → check-out).
/// </summary>
public class Visit
{
    [Key]
    public int VisitId { get; set; }

    [Required]
    public int VisitorId { get; set; }

    [Required]
    public int HostEmployeeId { get; set; }

    public int? FacilityId { get; set; }

    [MaxLength(1000)]
    public string? Purpose { get; set; }

    public DateTime? ExpectedAt { get; set; }

    public DateTime? CheckedInAt { get; set; }

    public DateTime? CheckedOutAt { get; set; }

    /// <summary>EXPECTED, CHECKED_IN, CHECKED_OUT, CANCELLED, NO_SHOW.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = "EXPECTED";

    public int? CreatedByEmployeeId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("VisitorId")]
    [JsonIgnore]
    public virtual Visitor? Visitor { get; set; }

    [ForeignKey("HostEmployeeId")]
    [JsonIgnore]
    public virtual Employee? Host { get; set; }

    [ForeignKey("FacilityId")]
    [JsonIgnore]
    public virtual Facility? Facility { get; set; }

    [ForeignKey("CreatedByEmployeeId")]
    [JsonIgnore]
    public virtual Employee? CreatedBy { get; set; }
}
