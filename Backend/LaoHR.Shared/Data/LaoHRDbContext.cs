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
    public DbSet<Position> Positions { get; set; }
    public DbSet<WorkLocation> WorkLocations { get; set; }
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
    public DbSet<AppUser> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    // Phase 2 — Project Workspace
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<ProjectTask> ProjectTasks { get; set; }
    public DbSet<TaskAssignee> TaskAssignees { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    // Phase 3 — Risk, Issue, Resource
    public DbSet<Risk> Risks { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<IssueComment> IssueComments { get; set; }
    public DbSet<Resource> Resources { get; set; }

    // Phase 3C4 — PM planning: dependencies + RAID assumptions/decisions
    public DbSet<TaskDependency> TaskDependencies { get; set; }
    public DbSet<ProjectAssumption> ProjectAssumptions { get; set; }
    public DbSet<ProjectDecision> ProjectDecisions { get; set; }

    // Phase 3C5 — Recruitment + ATS + Onboarding
    public DbSet<JobRequisition> JobRequisitions { get; set; }
    public DbSet<JobOpening> JobOpenings { get; set; }
    public DbSet<Candidate> Candidates { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<ApplicationStageHistory> ApplicationStageHistories { get; set; }
    public DbSet<CandidateDocument> CandidateDocuments { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<InterviewParticipant> InterviewParticipants { get; set; }
    public DbSet<InterviewEvaluation> InterviewEvaluations { get; set; }
    public DbSet<Offer> Offers { get; set; }
    public DbSet<OnboardingProcess> OnboardingProcesses { get; set; }
    public DbSet<OnboardingTask> OnboardingTasks { get; set; }

    // Phase 3C6 — Performance + Talent + Learning
    public DbSet<Goal> Goals { get; set; }
    public DbSet<GoalCheckIn> GoalCheckIns { get; set; }
    public DbSet<PerformanceCycle> PerformanceCycles { get; set; }
    public DbSet<PerformanceReview> PerformanceReviews { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<OneOnOne> OneOnOnes { get; set; }
    public DbSet<Competency> Competencies { get; set; }
    public DbSet<PositionCompetency> PositionCompetencies { get; set; }
    public DbSet<CompetencyAssessment> CompetencyAssessments { get; set; }
    public DbSet<DevelopmentPlan> DevelopmentPlans { get; set; }
    public DbSet<DevelopmentGoal> DevelopmentGoals { get; set; }
    public DbSet<LearningCourse> LearningCourses { get; set; }
    public DbSet<TrainingSession> TrainingSessions { get; set; }
    public DbSet<TrainingEnrollment> TrainingEnrollments { get; set; }
    public DbSet<EmployeeCertification> EmployeeCertifications { get; set; }
    public DbSet<CareerInterest> CareerInterests { get; set; }
    public DbSet<TalentReview> TalentReviews { get; set; }

    // Phase 4 — Finance Extension
    public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<EmployeeLoan> EmployeeLoans { get; set; }
    public DbSet<LoanRepayment> LoanRepayments { get; set; }

    // Phase 5 — Knowledge & Collaboration
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<AnnouncementRead> AnnouncementReads { get; set; }
    public DbSet<KnowledgeCategory> KnowledgeCategories { get; set; }
    public DbSet<KnowledgeArticle> KnowledgeArticles { get; set; }
    public DbSet<EntityComment> EntityComments { get; set; }

    // Phase 3B — Versioned compliance rules + payroll rule snapshots
    public DbSet<ComplianceRule> ComplianceRules { get; set; }
    public DbSet<PayrollRuleSnapshot> PayrollRuleSnapshots { get; set; }

    // Phase 3C1 — Approval engine
    public DbSet<ApprovalRequest> ApprovalRequests { get; set; }
    public DbSet<ApprovalStep> ApprovalSteps { get; set; }
    public DbSet<ApprovalAction> ApprovalActions { get; set; }

    // Phase 3C2 — Notifications
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AttendanceCorrection> AttendanceCorrections { get; set; }

    // Phase 4A — Back Office foundation
    public DbSet<NumberSequence> NumberSequences { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<CostCenter> CostCenters { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    public DbSet<PurchaseRequestItem> PurchaseRequestItems { get; set; }
    public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
    public DbSet<PurchaseOrderItem> PurchaseOrderItems { get; set; }
    public DbSet<GoodsReceipt> GoodsReceipts { get; set; }
    public DbSet<GoodsReceiptItem> GoodsReceiptItems { get; set; }
    public DbSet<InventoryCategory> InventoryCategories { get; set; }
    public DbSet<InventoryItem> InventoryItems { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<StockMovement> StockMovements { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<AssetAssignment> AssetAssignments { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ServiceRequestCategory> ServiceRequestCategories { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }

    // Phase 4B — Finance + Accounting foundation
    public DbSet<Account> Accounts { get; set; }
    public DbSet<FiscalYear> FiscalYears { get; set; }
    public DbSet<FiscalPeriod> FiscalPeriods { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalLine> JournalLines { get; set; }
    public DbSet<SupplierInvoice> SupplierInvoices { get; set; }
    public DbSet<SupplierInvoiceLine> SupplierInvoiceLines { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<PaymentAllocation> PaymentAllocations { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerInvoice> CustomerInvoices { get; set; }
    public DbSet<CustomerInvoiceLine> CustomerInvoiceLines { get; set; }
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<ReceiptAllocation> ReceiptAllocations { get; set; }

    // Phase 4C — Corporate Operations
    public DbSet<CorporateDocument> CorporateDocuments { get; set; }
    public DbSet<DocumentVersion> DocumentVersions { get; set; }
    public DbSet<ContractHistory> ContractHistories { get; set; }
    public DbSet<ServiceRequestHistory> ServiceRequestHistories { get; set; }
    public DbSet<Facility> Facilities { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomBooking> RoomBookings { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<VehicleBooking> VehicleBookings { get; set; }
    public DbSet<VehicleTrip> VehicleTrips { get; set; }
    public DbSet<FuelLog> FuelLogs { get; set; }
    public DbSet<TravelRequest> TravelRequests { get; set; }
    public DbSet<TravelSegment> TravelSegments { get; set; }
    public DbSet<TravelAccommodation> TravelAccommodations { get; set; }
    public DbSet<TravelAdvance> TravelAdvances { get; set; }
    public DbSet<Visitor> Visitors { get; set; }
    public DbSet<Visit> Visits { get; set; }

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

        // Phase 3C1 — organization hierarchy indexes + self-referencing FKs.
        modelBuilder.Entity<Department>()
            .HasIndex(d => d.ParentDepartmentId);

        modelBuilder.Entity<Department>()
            .HasIndex(d => d.ManagerEmployeeId);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.ParentDepartment)
            .WithMany(d => d.ChildDepartments)
            .HasForeignKey(d => d.ParentDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Department>()
            .HasOne(d => d.Manager)
            .WithMany()
            .HasForeignKey(d => d.ManagerEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.ManagerId);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.PositionId);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.WorkLocationId);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany(m => m.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Position>()
            .HasIndex(p => p.DepartmentId);

        modelBuilder.Entity<WorkLocation>()
            .HasIndex(w => w.Code)
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

        // Phase 2 — Project Workspace indexes
        modelBuilder.Entity<Project>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<ProjectMember>()
            .HasIndex(pm => new { pm.ProjectId, pm.EmployeeId })
            .IsUnique();

        modelBuilder.Entity<TaskAssignee>()
            .HasIndex(ta => new { ta.TaskId, ta.EmployeeId })
            .IsUnique();

        modelBuilder.Entity<ProjectTask>()
            .HasIndex(t => new { t.ProjectId, t.Status });

        modelBuilder.Entity<ActivityLog>()
            .HasIndex(a => new { a.ProjectId, a.CreatedAt });

        // Phase 3 — Risk, Issue, Resource indexes
        modelBuilder.Entity<Risk>()
            .HasIndex(r => new { r.ProjectId, r.Status });

        modelBuilder.Entity<Risk>()
            .HasIndex(r => new { r.ProjectId, r.Priority });

        modelBuilder.Entity<Issue>()
            .HasIndex(i => new { i.ProjectId, i.Status });

        modelBuilder.Entity<Issue>()
            .HasIndex(i => new { i.ProjectId, i.AssigneeId });

        modelBuilder.Entity<IssueComment>()
            .HasIndex(c => new { c.IssueId, c.CreatedAt });

        modelBuilder.Entity<Resource>()
            .HasIndex(r => new { r.ProjectId, r.EmployeeId })
            .IsUnique();

        modelBuilder.Entity<Resource>()
            .HasIndex(r => new { r.EmployeeId, r.StartDate, r.EndDate });

        // Phase 3C4 — PM planning indexes
        modelBuilder.Entity<ProjectTask>()
            .HasIndex(t => t.ParentTaskId);

        modelBuilder.Entity<ProjectTask>()
            .HasIndex(t => new { t.ProjectId, t.DueDate });

        modelBuilder.Entity<TaskDependency>()
            .HasIndex(d => new { d.ProjectId, d.PredecessorTaskId });

        modelBuilder.Entity<TaskDependency>()
            .HasIndex(d => new { d.ProjectId, d.SuccessorTaskId });

        modelBuilder.Entity<ProjectAssumption>()
            .HasIndex(a => new { a.ProjectId, a.Status });

        modelBuilder.Entity<ProjectDecision>()
            .HasIndex(d => d.ProjectId);

        // Phase 3C5 — Recruitment indexes
        modelBuilder.Entity<JobRequisition>()
            .HasIndex(r => r.Status);

        modelBuilder.Entity<JobRequisition>()
            .HasIndex(r => r.PositionId);

        modelBuilder.Entity<JobRequisition>()
            .HasIndex(r => r.HiringManagerEmployeeId);

        modelBuilder.Entity<JobOpening>()
            .HasIndex(o => new { o.RequisitionId, o.Status });

        modelBuilder.Entity<Candidate>()
            .HasIndex(c => c.Email);

        modelBuilder.Entity<Application>()
            .HasIndex(a => a.CandidateId);

        modelBuilder.Entity<Application>()
            .HasIndex(a => a.OpeningId);

        modelBuilder.Entity<Application>()
            .HasIndex(a => new { a.CurrentStage, a.Status });

        modelBuilder.Entity<ApplicationStageHistory>()
            .HasIndex(h => new { h.ApplicationId, h.CreatedAt });

        modelBuilder.Entity<CandidateDocument>()
            .HasIndex(d => d.CandidateId);

        modelBuilder.Entity<Interview>()
            .HasIndex(i => i.ApplicationId);

        modelBuilder.Entity<Interview>()
            .HasIndex(i => i.ScheduledStart);

        modelBuilder.Entity<InterviewParticipant>()
            .HasIndex(p => new { p.InterviewId, p.EmployeeId })
            .IsUnique();

        modelBuilder.Entity<InterviewEvaluation>()
            .HasIndex(e => new { e.InterviewId, e.EvaluatorEmployeeId })
            .IsUnique();

        modelBuilder.Entity<Offer>()
            .HasIndex(o => o.ApplicationId);

        modelBuilder.Entity<Offer>()
            .HasIndex(o => o.Status);

        modelBuilder.Entity<OnboardingProcess>()
            .HasIndex(p => p.EmployeeId);

        modelBuilder.Entity<OnboardingTask>()
            .HasIndex(t => t.OnboardingProcessId);

        modelBuilder.Entity<OnboardingTask>()
            .HasIndex(t => new { t.OwnerEmployeeId, t.DueDate });

        // Phase 3C6 — Performance/Talent/Learning indexes
        modelBuilder.Entity<Goal>()
            .HasIndex(g => g.EmployeeId);

        modelBuilder.Entity<Goal>()
            .HasIndex(g => new { g.Status, g.DueDate });

        modelBuilder.Entity<GoalCheckIn>()
            .HasIndex(c => new { c.GoalId, c.CreatedAt });

        modelBuilder.Entity<PerformanceReview>()
            .HasIndex(r => r.EmployeeId);

        modelBuilder.Entity<PerformanceReview>()
            .HasIndex(r => new { r.CycleId, r.Status });

        modelBuilder.Entity<Feedback>()
            .HasIndex(f => f.ToEmployeeId);

        modelBuilder.Entity<OneOnOne>()
            .HasIndex(o => new { o.ManagerEmployeeId, o.EmployeeId });

        modelBuilder.Entity<PositionCompetency>()
            .HasIndex(pc => pc.PositionId);

        modelBuilder.Entity<CompetencyAssessment>()
            .HasIndex(a => new { a.EmployeeId, a.CompetencyId });

        modelBuilder.Entity<DevelopmentPlan>()
            .HasIndex(p => p.EmployeeId);

        modelBuilder.Entity<DevelopmentGoal>()
            .HasIndex(g => g.DevelopmentPlanId);

        modelBuilder.Entity<TrainingEnrollment>()
            .HasIndex(e => e.EmployeeId);

        modelBuilder.Entity<TrainingEnrollment>()
            .HasIndex(e => new { e.SessionId, e.Status });

        modelBuilder.Entity<EmployeeCertification>()
            .HasIndex(c => c.EmployeeId);

        modelBuilder.Entity<EmployeeCertification>()
            .HasIndex(c => c.ExpiryDate);

        modelBuilder.Entity<CareerInterest>()
            .HasIndex(c => c.EmployeeId)
            .IsUnique();

        modelBuilder.Entity<TalentReview>()
            .HasIndex(t => t.EmployeeId);

        // Phase 4 — Finance indexes
        modelBuilder.Entity<Expense>()
            .HasIndex(e => e.ExpenseNumber)
            .IsUnique();

        modelBuilder.Entity<Expense>()
            .HasIndex(e => new { e.EmployeeId, e.Status });

        modelBuilder.Entity<Expense>()
            .HasIndex(e => new { e.Status, e.ExpenseDate });

        modelBuilder.Entity<EmployeeLoan>()
            .HasIndex(l => l.LoanNumber)
            .IsUnique();

        modelBuilder.Entity<EmployeeLoan>()
            .HasIndex(l => new { l.EmployeeId, l.Status });

        modelBuilder.Entity<LoanRepayment>()
            .HasIndex(r => new { r.LoanId, r.RepaidAt });

        // Phase 5 — Knowledge & Collaboration indexes
        modelBuilder.Entity<Announcement>()
            .HasIndex(a => new { a.IsPinned, a.CreatedAt });

        modelBuilder.Entity<Announcement>()
            .HasIndex(a => new { a.Audience, a.PublishFrom });

        modelBuilder.Entity<AnnouncementRead>()
            .HasIndex(ar => new { ar.AnnouncementId, ar.EmployeeId })
            .IsUnique();

        modelBuilder.Entity<KnowledgeArticle>()
            .HasIndex(a => new { a.CategoryId, a.Status });

        modelBuilder.Entity<KnowledgeArticle>()
            .HasIndex(a => a.Status);

        modelBuilder.Entity<EntityComment>()
            .HasIndex(c => new { c.EntityType, c.EntityId, c.CreatedAt });

        modelBuilder.Entity<EntityComment>()
            .HasIndex(c => c.ParentCommentId);

        // Phase 6c — Refresh token indexes.
        // TokenHash lookup is the hot path on /api/auth/refresh; UserId lookup
        // powers bulk revocation on logout-everywhere.
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.TokenHash)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => new { rt.UserId, rt.RevokedAt });

        // Phase 3C1 — approval engine indexes.
        modelBuilder.Entity<ApprovalRequest>()
            .HasIndex(ar => new { ar.RequestType, ar.EntityId });

        modelBuilder.Entity<ApprovalRequest>()
            .HasIndex(ar => new { ar.RequesterEmployeeId, ar.Status });

        modelBuilder.Entity<ApprovalStep>()
            .HasIndex(s => new { s.ApprovalRequestId, s.StepOrder });

        modelBuilder.Entity<ApprovalStep>()
            .HasIndex(s => new { s.ApproverEmployeeId, s.Status });

        modelBuilder.Entity<ApprovalAction>()
            .HasIndex(a => new { a.ApprovalRequestId, a.ActedAt });

        // Phase 3C2 — notification indexes.
        modelBuilder.Entity<Notification>()
            .HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });

        // Phase 3C2 — attendance correction indexes.
        modelBuilder.Entity<AttendanceCorrection>()
            .HasIndex(c => new { c.EmployeeId, c.Status });

        modelBuilder.Entity<AttendanceCorrection>()
            .HasIndex(c => c.AttendanceId);

        // Phase 4A — Back Office indexes
        modelBuilder.Entity<NumberSequence>()
            .HasIndex(n => new { n.Prefix, n.Year })
            .IsUnique();

        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.SupplierCode)
            .IsUnique();

        modelBuilder.Entity<Supplier>()
            .HasIndex(s => s.Status);

        modelBuilder.Entity<CostCenter>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<Budget>()
            .HasIndex(b => new { b.FiscalYear, b.DepartmentId, b.Category });

        modelBuilder.Entity<Budget>()
            .HasIndex(b => new { b.FiscalYear, b.CostCenterId });

        modelBuilder.Entity<PurchaseRequest>()
            .HasIndex(p => p.RequestNumber)
            .IsUnique();

        modelBuilder.Entity<PurchaseRequest>()
            .HasIndex(p => new { p.Status, p.CreatedAt });

        modelBuilder.Entity<PurchaseRequest>()
            .HasIndex(p => p.BudgetId);

        modelBuilder.Entity<PurchaseRequestItem>()
            .HasIndex(i => i.PurchaseRequestId);

        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(p => p.PONumber)
            .IsUnique();

        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(p => new { p.SupplierId, p.Status });

        modelBuilder.Entity<PurchaseOrder>()
            .HasIndex(p => p.BudgetId);

        modelBuilder.Entity<PurchaseOrderItem>()
            .HasIndex(i => i.PurchaseOrderId);

        modelBuilder.Entity<GoodsReceipt>()
            .HasIndex(g => g.ReceiptNumber)
            .IsUnique();

        modelBuilder.Entity<GoodsReceipt>()
            .HasIndex(g => g.PurchaseOrderId);

        modelBuilder.Entity<GoodsReceiptItem>()
            .HasIndex(i => i.GoodsReceiptId);

        modelBuilder.Entity<InventoryCategory>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<InventoryItem>()
            .HasIndex(i => i.SKU)
            .IsUnique();

        modelBuilder.Entity<InventoryItem>()
            .HasIndex(i => new { i.ItemType, i.IsActive });

        modelBuilder.Entity<Warehouse>()
            .HasIndex(w => w.Code)
            .IsUnique();

        modelBuilder.Entity<StockMovement>()
            .HasIndex(m => new { m.ItemId, m.WarehouseId, m.OccurredAt });

        modelBuilder.Entity<StockMovement>()
            .HasIndex(m => new { m.ReferenceType, m.ReferenceId });

        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.AssetCode)
            .IsUnique();

        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.Status);

        modelBuilder.Entity<Asset>()
            .HasIndex(a => a.GoodsReceiptItemId);

        modelBuilder.Entity<AssetAssignment>()
            .HasIndex(a => new { a.AssetId, a.AssignedAt });

        modelBuilder.Entity<AssetAssignment>()
            .HasIndex(a => new { a.EmployeeId, a.ReturnedAt });

        modelBuilder.Entity<Contract>()
            .HasIndex(c => c.ContractNumber)
            .IsUnique();

        modelBuilder.Entity<Contract>()
            .HasIndex(c => new { c.Status, c.EndDate });

        modelBuilder.Entity<ServiceRequestCategory>()
            .HasIndex(c => c.Code)
            .IsUnique();

        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(s => s.RequestNumber)
            .IsUnique();

        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(s => new { s.Status, s.CreatedAt });

        modelBuilder.Entity<ServiceRequest>()
            .HasIndex(s => new { s.AssignedEmployeeId, s.Status });

        // Phase 4B — Finance + Accounting indexes
        modelBuilder.Entity<Account>()
            .HasIndex(a => a.AccountCode)
            .IsUnique();

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.ParentAccountId);

        modelBuilder.Entity<FiscalPeriod>()
            .HasIndex(p => new { p.FiscalYearId, p.PeriodNumber })
            .IsUnique();

        modelBuilder.Entity<JournalEntry>()
            .HasIndex(j => j.JournalNumber)
            .IsUnique();

        modelBuilder.Entity<JournalEntry>()
            .HasIndex(j => new { j.Status, j.PostingDate });

        modelBuilder.Entity<JournalEntry>()
            .HasIndex(j => j.FiscalPeriodId);

        modelBuilder.Entity<JournalEntry>()
            .HasIndex(j => new { j.SourceType, j.SourceId });

        // Phase 4B.2 — source uniqueness at the database level. A filtered unique
        // index enforces that an auto-posted source (non-null SourceId + PostingPurpose)
        // can only produce one journal, even under concurrent requests. Manual
        // journals (null SourceId/PostingPurpose) are unaffected.
        modelBuilder.Entity<JournalEntry>()
            .HasIndex(j => new { j.SourceType, j.SourceId, j.PostingPurpose })
            .IsUnique()
            .HasFilter("\"SourceId\" IS NOT NULL AND \"PostingPurpose\" IS NOT NULL");

        modelBuilder.Entity<JournalLine>()
            .HasIndex(l => l.JournalEntryId);

        modelBuilder.Entity<JournalLine>()
            .HasIndex(l => l.AccountId);

        modelBuilder.Entity<JournalLine>()
            .HasIndex(l => l.CostCenterId);

        modelBuilder.Entity<JournalLine>()
            .HasIndex(l => l.ProjectId);

        modelBuilder.Entity<SupplierInvoice>()
            .HasIndex(i => new { i.SupplierId, i.InvoiceNumber })
            .IsUnique();

        modelBuilder.Entity<SupplierInvoice>()
            .HasIndex(i => i.Status);

        modelBuilder.Entity<SupplierInvoice>()
            .HasIndex(i => i.DueDate);

        modelBuilder.Entity<SupplierInvoice>()
            .HasIndex(i => i.PurchaseOrderId);

        modelBuilder.Entity<SupplierInvoiceLine>()
            .HasIndex(l => l.SupplierInvoiceId);

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.PaymentNumber)
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasIndex(p => new { p.Status, p.PaymentDate });

        modelBuilder.Entity<PaymentAllocation>()
            .HasIndex(a => a.PaymentId);

        modelBuilder.Entity<PaymentAllocation>()
            .HasIndex(a => a.SupplierInvoiceId);

        modelBuilder.Entity<BankAccount>()
            .HasIndex(b => b.AccountNumber);

        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.CustomerCode)
            .IsUnique();

        modelBuilder.Entity<CustomerInvoice>()
            .HasIndex(i => new { i.CustomerId, i.InvoiceNumber })
            .IsUnique();

        modelBuilder.Entity<CustomerInvoice>()
            .HasIndex(i => new { i.Status, i.DueDate });

        modelBuilder.Entity<CustomerInvoiceLine>()
            .HasIndex(l => l.CustomerInvoiceId);

        modelBuilder.Entity<Receipt>()
            .HasIndex(r => r.ReceiptNumber)
            .IsUnique();

        modelBuilder.Entity<ReceiptAllocation>()
            .HasIndex(a => a.ReceiptId);

        modelBuilder.Entity<ReceiptAllocation>()
            .HasIndex(a => a.CustomerInvoiceId);

        // ---- Phase 4C — Corporate Operations indexes ----

        modelBuilder.Entity<CorporateDocument>()
            .HasIndex(d => new { d.OwnerEntityType, d.OwnerEntityId });

        modelBuilder.Entity<CorporateDocument>()
            .HasIndex(d => d.ExpiryDate);

        modelBuilder.Entity<DocumentVersion>()
            .HasIndex(v => v.DocumentId);

        modelBuilder.Entity<ContractHistory>()
            .HasIndex(h => h.ContractId);

        modelBuilder.Entity<ServiceRequestHistory>()
            .HasIndex(h => h.ServiceRequestId);

        modelBuilder.Entity<Facility>()
            .HasIndex(f => f.FacilityCode)
            .IsUnique();

        modelBuilder.Entity<Room>()
            .HasIndex(r => r.FacilityId);

        modelBuilder.Entity<RoomBooking>()
            .HasIndex(b => new { b.RoomId, b.StartAt, b.EndAt });

        modelBuilder.Entity<WorkOrder>()
            .HasIndex(w => new { w.SourceType, w.SourceId });

        modelBuilder.Entity<WorkOrder>()
            .HasIndex(w => w.Status);

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.RegistrationNumber)
            .IsUnique();

        modelBuilder.Entity<VehicleBooking>()
            .HasIndex(b => new { b.VehicleId, b.StartAt, b.EndAt });

        modelBuilder.Entity<VehicleTrip>()
            .HasIndex(t => t.VehicleId);

        modelBuilder.Entity<FuelLog>()
            .HasIndex(f => f.VehicleId);

        modelBuilder.Entity<TravelRequest>()
            .HasIndex(t => t.EmployeeId);

        modelBuilder.Entity<TravelRequest>()
            .HasIndex(t => t.Status);

        modelBuilder.Entity<TravelSegment>()
            .HasIndex(s => s.TravelRequestId);

        modelBuilder.Entity<TravelAccommodation>()
            .HasIndex(a => a.TravelRequestId);

        modelBuilder.Entity<TravelAdvance>()
            .HasIndex(a => a.TravelRequestId);

        modelBuilder.Entity<Visit>()
            .HasIndex(v => v.HostEmployeeId);

        modelBuilder.Entity<Visit>()
            .HasIndex(v => v.ExpectedAt);

        // Seed service request categories
        modelBuilder.Entity<ServiceRequestCategory>().HasData(
            new ServiceRequestCategory { ServiceRequestCategoryId = 1, Code = "IT", Name = "IT Support", NameLao = "ສະໜັບສະໜູນ IT" },
            new ServiceRequestCategory { ServiceRequestCategoryId = 2, Code = "FACILITIES", Name = "Facilities", NameLao = "ສິ່ງອຳນວຍຄວາມສະດວກ" },
            new ServiceRequestCategory { ServiceRequestCategoryId = 3, Code = "ADMIN", Name = "Administration", NameLao = "ບໍລິຫານ" },
            new ServiceRequestCategory { ServiceRequestCategoryId = 4, Code = "PROCUREMENT", Name = "Procurement", NameLao = "ການຈັດຊື້" },
            new ServiceRequestCategory { ServiceRequestCategoryId = 5, Code = "HR", Name = "Human Resources", NameLao = "ຊັບພະຍາກອນມະນຸດ" },
            new ServiceRequestCategory { ServiceRequestCategoryId = 6, Code = "FINANCE", Name = "Finance", NameLao = "ການເງິນ" }
        );

        // Seed expense categories
        modelBuilder.Entity<ExpenseCategory>().HasData(
            new ExpenseCategory { ExpenseCategoryId = 1, Code = "TRAVEL", Name = "Travel", NameLao = "ການເດີນທາງ", Description = "Flights, taxis, mileage", DefaultLimit = 2000000m },
            new ExpenseCategory { ExpenseCategoryId = 2, Code = "MEALS", Name = "Meals & Entertainment", NameLao = "ອາຫານ ແລະ ການບັນເທີງ", DefaultLimit = 300000m },
            new ExpenseCategory { ExpenseCategoryId = 3, Code = "OFFICE", Name = "Office Supplies", NameLao = "ອຸປະກອນຫ້ອງການ", DefaultLimit = 500000m },
            new ExpenseCategory { ExpenseCategoryId = 4, Code = "TRAINING", Name = "Training & Development", NameLao = "ການຝຶກອົບຮົມ", DefaultLimit = 3000000m },
            new ExpenseCategory { ExpenseCategoryId = 5, Code = "COMMUNICATION", Name = "Communication", NameLao = "ການສື່ສານ", DefaultLimit = 200000m },
            new ExpenseCategory { ExpenseCategoryId = 6, Code = "OTHER", Name = "Other", NameLao = "ອື່ນໆ", DefaultLimit = null, RequiresReceipt = false }
        );

        // Seed knowledge categories
        modelBuilder.Entity<KnowledgeCategory>().HasData(
            new KnowledgeCategory { KnowledgeCategoryId = 1, Code = "HR", Name = "HR Policies", NameLao = "ນະໂຍບາຍ HR", Description = "Leave, attendance, benefits", SortOrder = 1 },
            new KnowledgeCategory { KnowledgeCategoryId = 2, Code = "IT", Name = "IT & Systems", NameLao = "IT ແລະ ລະບົບ", Description = "How-tos for software and access", SortOrder = 2 },
            new KnowledgeCategory { KnowledgeCategoryId = 3, Code = "PAYROLL", Name = "Payroll & Benefits", NameLao = "ເງິນເດືອນ ແລະ ສະຫວັດດີການ", Description = "Salary, tax, advances", SortOrder = 3 },
            new KnowledgeCategory { KnowledgeCategoryId = 4, Code = "GENERAL", Name = "General", NameLao = "ທົ່ວໄປ", Description = "Company-wide guides", SortOrder = 4 }
        );
        
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
        // Phase 3A — Updated PIT brackets per Amended Income Tax Law No. 88/NA (25 June 2025, effective July 2026).
        // VERIFIED by PwC Worldwide Tax Summaries (quality 5/5). Rule IDs: LAO-PIT-2026-BRACKET-01 through 06.
        // Previous brackets used 1,300,000 tax-free threshold (pre-2025 law) — now superseded.
        modelBuilder.Entity<TaxBracket>().HasData(
            new TaxBracket { BracketId = 1, MinIncome = 0, MaxIncome = 2500000, TaxRate = 0.00m, SortOrder = 1 },
            new TaxBracket { BracketId = 2, MinIncome = 2500001, MaxIncome = 5000000, TaxRate = 0.05m, SortOrder = 2 },
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
