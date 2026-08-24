/**
 * API Endpoints Index
 * Central export for all API modules
 */

export { authApi } from './auth';
export { employeesApi, departmentsApi } from './employees';
export { organizationApi } from './organization';
export type { DepartmentNode, EmployeeSummary } from './organization';
export { approvalsApi } from './approvals';
export type { ApprovalInboxItem, ApprovalDetail, ApprovalStepDto, ApprovalActionDto } from './approvals';
export { notificationsApi } from './notifications';
export type { NotificationItem } from './notifications';
export { teamApi } from './team';
export type { TeamLeaveItem, TeamAttendanceItem } from './team';
export { pmApi } from './pm';
export type {
    KanbanBoard,
    KanbanColumn,
    KanbanCard,
    DependencyDto,
    GanttData,
    GanttTask,
    GanttMilestone,
    ResourceWorkload,
    ProjectHealth,
    PortfolioItem,
} from './pm';
export { recruitmentApi } from './recruitment';
export type {
    JobRequisition,
    JobOpening,
    Candidate,
    Application,
    ApplicationStageHistory,
    Interview,
    InterviewEvaluation,
    Offer,
    OnboardingProcess,
    OnboardingTask,
    HireResult,
} from './recruitment';
export { performanceApi } from './performance';
export type {
    Goal,
    GoalCheckIn,
    PerformanceCycle,
    PerformanceReview,
    Feedback,
    OneOnOne,
    Competency,
    PositionCompetency,
    CompetencyAssessment,
    DevelopmentPlan,
    LearningCourse,
    TrainingEnrollment,
    EmployeeCertification,
    CareerInterest,
    TalentReview,
} from './performance';
export { analyticsApi } from './analytics';
export type {
    KpiCard,
    TimeSeriesPoint,
    BreakdownItem,
    AttentionItem,
    ExecutiveDashboard,
    HrDashboard,
    ManagerDashboard,
    AttendanceAnalytics,
    LeaveAnalytics,
    PayrollAnalytics,
    FinanceAnalytics,
    PmAnalytics,
} from './analytics';
export { attendanceApi } from './attendance';
export { leaveApi } from './leave';
export { payrollApi } from './payroll';
export { reportsApi } from './reports';
export { workScheduleApi, holidaysApi } from './schedule';
export { conversionRatesApi } from './conversionRates';
export { adjustmentApi } from './adjustments';
export { projectsApi, projectTasksApi, myTasksApi, risksApi, issuesApi, resourcesApi } from './projects';
export { expensesApi, loansApi, EXPENSE_STATUSES, LOAN_STATUSES } from './finance';
export type {
    ExpenseCategoryItem,
    ExpenseListItem,
    ExpenseDetail,
    CreateExpenseInput,
    UpdateExpenseInput,
    LoanListItem,
    LoanDetail,
    LoanRepaymentItem,
    CreateLoanInput,
    UpdateLoanInput,
    RecordRepaymentInput,
} from './finance';

export { announcementsApi, knowledgeApi, commentsApi, ANNOUNCEMENT_SEVERITIES, ARTICLE_STATUSES, COMMENT_ENTITY_TYPES } from './knowledge';
export type {
    AnnouncementListItem,
    AnnouncementDetail,
    CreateAnnouncementInput,
    UpdateAnnouncementInput,
    KnowledgeCategoryItem,
    KnowledgeArticleListItem,
    KnowledgeArticleDetail,
    CreateArticleInput,
    UpdateArticleInput,
    CommentItem,
    CreateCommentInput,
    CommentSummary,
} from './knowledge';

// Re-export types
export type { AttendanceFilters, ClockInRequest } from './attendance';
export type { LeaveFilters, LeaveBalance } from './leave';
export type { CreatePeriodRequest, CalculateRequest } from './payroll';

// Phase 4C — Corporate Operations
export { corporateApi } from './corporate';
export type {
    CorporateDocumentDto,
    DocumentVersionDto,
    FacilityDto,
    RoomDto,
    RoomBookingDto,
    VehicleDto,
    VehicleBookingDto,
    TravelRequestDto,
    VisitorDto,
    VisitDto,
    WorkOrderDto,
} from './corporate';

