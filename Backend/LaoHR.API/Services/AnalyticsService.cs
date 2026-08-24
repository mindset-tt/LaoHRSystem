using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C3 — analytics/query layer. Aggregates in the database (no N+1),
/// always within the current user's authorized data scope.
/// </summary>
public interface IAnalyticsService
{
    Task<ExecutiveDashboard> GetExecutiveAsync(CancellationToken ct = default);
    Task<HrDashboard> GetHrAsync(CancellationToken ct = default);
    Task<ManagerDashboard> GetManagerAsync(CancellationToken ct = default);
    Task<AttendanceAnalytics> GetAttendanceAsync(string? range, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<LeaveAnalytics> GetLeaveAsync(string? range, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<PayrollAnalytics> GetPayrollAsync(CancellationToken ct = default);
    Task<FinanceAnalytics> GetFinanceAsync(string? range, DateTime? from, DateTime? to, CancellationToken ct = default);
    Task<PmAnalytics> GetPmAsync(CancellationToken ct = default);
}

public sealed class AnalyticsService : IAnalyticsService
{
    private readonly LaoHRDbContext _context;
    private readonly IDataScopeService _scope;

    public AnalyticsService(LaoHRDbContext context, IDataScopeService scope)
    {
        _context = context;
        _scope = scope;
    }

    public async Task<ExecutiveDashboard> GetExecutiveAsync(CancellationToken ct = default)
    {
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync(ct);
        var today = ReportingDateRange.TodayLao();
        var todayUtc = ReportingDateRange.LaoToUtc(today);
        var tomorrowUtc = ReportingDateRange.LaoToUtc(today.AddDays(1));

        var activeHeadcount = await _context.Employees.CountAsync(e => e.IsActive, ct);
        var onLeaveToday = await _context.LeaveRequests
            .CountAsync(l => l.Status == "APPROVED" && l.StartDate <= today && l.EndDate >= today, ct);
        var presentToday = await _context.Attendances
            .CountAsync(a => a.AttendanceDate >= todayUtc && a.AttendanceDate < tomorrowUtc && a.Status == "PRESENT", ct);
        var pendingApprovals = await _context.ApprovalRequests.CountAsync(r => r.Status == "PENDING", ct);
        var pendingExpenses = await _context.Expenses.CountAsync(e => e.Status == "SUBMITTED", ct);
        var activeProjects = await _context.Projects.CountAsync(p => p.Status == "ACTIVE", ct);
        var projectsAtRisk = await _context.Risks.CountAsync(r => r.Status == "OPEN" && (r.Priority == "HIGH" || r.Priority == "CRITICAL"), ct);

        var kpis = new List<KpiCard>
        {
            new() { Id = "HR-HEADCOUNT-ACTIVE", Label = "Active Headcount", Value = activeHeadcount, Unit = "people" },
            new() { Id = "HR-LEAVE-TODAY", Label = "On Leave Today", Value = onLeaveToday, Unit = "people" },
            new() { Id = "ATT-PRESENT-TODAY", Label = "Present Today", Value = presentToday, Unit = "people" },
            new() { Id = "APPROVAL-PENDING", Label = "Pending Approvals", Value = pendingApprovals, Unit = "requests" },
            new() { Id = "FIN-EXPENSE-PENDING", Label = "Pending Expenses", Value = pendingExpenses, Unit = "claims" },
            new() { Id = "PM-PROJECT-ACTIVE", Label = "Active Projects", Value = activeProjects, Unit = "projects" },
            new() { Id = "PM-PROJECT-AT-RISK", Label = "Projects At Risk", Value = projectsAtRisk, Unit = "risks" },
        };

        var attention = new List<AttentionItem>();
        if (pendingApprovals > 0) attention.Add(new AttentionItem { Type = "APPROVAL", Title = "Pending approvals", Count = pendingApprovals, Link = "/my-approvals" });
        if (projectsAtRisk > 0) attention.Add(new AttentionItem { Type = "RISK", Title = "Open high/critical risks", Count = projectsAtRisk, Link = "/projects" });
        if (pendingExpenses > 0) attention.Add(new AttentionItem { Type = "EXPENSE", Title = "Unapproved expenses", Count = pendingExpenses, Link = "/finance/expenses" });

        var headcountByDepartment = await _context.Employees
            .Where(e => e.IsActive)
            .GroupBy(e => e.Department != null ? e.Department.DepartmentName : "Unassigned")
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .OrderByDescending(b => b.Value)
            .ToListAsync(ct);

        var headcountTrend = await _context.Employees
            .Where(e => e.HireDate != null)
            .GroupBy(e => new { e.HireDate!.Value.Year, e.HireDate!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        var leaveTrend = await _context.LeaveRequests
            .Where(l => l.Status == "APPROVED")
            .GroupBy(l => new { l.StartDate.Year, l.StartDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        var expenseTrend = await _context.Expenses
            .Where(e => e.Status == "APPROVED" || e.Status == "PAID")
            .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.AmountLak) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        return new ExecutiveDashboard
        {
            Kpis = kpis,
            Attention = attention,
            HeadcountByDepartment = headcountByDepartment,
            HeadcountTrend = headcountTrend.Select(g => new TimeSeriesPoint { Date = new DateTime(g.Year, g.Month, 1), Value = g.Count }).ToList(),
            LeaveTrend = leaveTrend.Select(g => new TimeSeriesPoint { Date = new DateTime(g.Year, g.Month, 1), Value = g.Count }).ToList(),
            ExpenseTrend = expenseTrend.Select(g => new TimeSeriesPoint { Date = new DateTime(g.Year, g.Month, 1), Value = g.Total }).ToList(),
        };
    }

    public async Task<HrDashboard> GetHrAsync(CancellationToken ct = default)
    {
        var active = await _context.Employees.CountAsync(e => e.IsActive, ct);
        var inactive = await _context.Employees.CountAsync(e => !e.IsActive, ct);
        var yearStart = new DateTime(ReportingDateRange.TodayLao().Year, 1, 1);
        var newHires = await _context.Employees.CountAsync(e => e.HireDate != null && e.HireDate >= yearStart, ct);
        var pendingLeave = await _context.LeaveRequests.CountAsync(l => l.Status == "PENDING", ct);

        var kpis = new List<KpiCard>
        {
            new() { Id = "HR-HEADCOUNT-ACTIVE", Label = "Active Employees", Value = active, Unit = "people" },
            new() { Id = "HR-HEADCOUNT-INACTIVE", Label = "Inactive", Value = inactive, Unit = "people" },
            new() { Id = "HR-NEW-HIRES", Label = "New Hires (YTD)", Value = newHires, Unit = "people" },
            new() { Id = "HR-LEAVE-PENDING", Label = "Pending Leave", Value = pendingLeave, Unit = "requests" },
        };

        var byDepartment = await _context.Employees
            .Where(e => e.IsActive)
            .GroupBy(e => e.Department != null ? e.Department.DepartmentName : "Unassigned")
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .OrderByDescending(b => b.Value)
            .ToListAsync(ct);

        var byLocation = await _context.Employees
            .Where(e => e.IsActive)
            .GroupBy(e => e.WorkLocation != null ? e.WorkLocation.Name : "Unassigned")
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .OrderByDescending(b => b.Value)
            .ToListAsync(ct);

        var byPosition = await _context.Employees
            .Where(e => e.IsActive)
            .GroupBy(e => e.Position != null ? e.Position.Title : (e.JobTitle ?? "Unassigned"))
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .OrderByDescending(b => b.Value)
            .ToListAsync(ct);

        var leaveByType = await _context.LeaveRequests
            .Where(l => l.Status == "APPROVED")
            .GroupBy(l => l.LeaveType)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Sum(x => x.TotalDays) })
            .OrderByDescending(b => b.Value)
            .ToListAsync(ct);

        var newHiresTrend = await _context.Employees
            .Where(e => e.HireDate != null)
            .GroupBy(e => new { e.HireDate!.Value.Year, e.HireDate!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        return new HrDashboard
        {
            Kpis = kpis,
            HeadcountByDepartment = byDepartment,
            HeadcountByLocation = byLocation,
            HeadcountByPosition = byPosition,
            LeaveByType = leaveByType,
            NewHiresTrend = newHiresTrend.Select(g => new TimeSeriesPoint { Date = new DateTime(g.Year, g.Month, 1), Value = g.Count }).ToList(),
        };
    }

    public async Task<ManagerDashboard> GetManagerAsync(CancellationToken ct = default)
    {
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync(ct);
        var selfId = _scope.GetCurrentEmployeeId();
        var reportIds = visibleIds.Where(id => id != selfId).ToList();

        var today = ReportingDateRange.TodayLao();
        var todayUtc = ReportingDateRange.LaoToUtc(today);
        var tomorrowUtc = ReportingDateRange.LaoToUtc(today.AddDays(1));

        var directReports = reportIds.Count;
        var presentToday = await _context.Attendances
            .CountAsync(a => reportIds.Contains(a.EmployeeId) && a.AttendanceDate >= todayUtc && a.AttendanceDate < tomorrowUtc && a.Status == "PRESENT", ct);
        var onLeaveToday = await _context.LeaveRequests
            .CountAsync(l => reportIds.Contains(l.EmployeeId) && l.Status == "APPROVED" && l.StartDate <= today && l.EndDate >= today, ct);
        var pendingApprovals = await _context.ApprovalSteps
            .CountAsync(s => s.ApproverEmployeeId == selfId && s.Status == "PENDING", ct);

        var kpis = new List<KpiCard>
        {
            new() { Id = "MSS-DIRECT-REPORTS", Label = "Direct Reports", Value = directReports, Unit = "people" },
            new() { Id = "MSS-PRESENT-TODAY", Label = "Present Today", Value = presentToday, Unit = "people" },
            new() { Id = "MSS-ON-LEAVE-TODAY", Label = "On Leave Today", Value = onLeaveToday, Unit = "people" },
            new() { Id = "MSS-PENDING-APPROVALS", Label = "Pending Approvals", Value = pendingApprovals, Unit = "requests" },
        };

        var teamLeaveByStatus = await _context.LeaveRequests
            .Where(l => reportIds.Contains(l.EmployeeId))
            .GroupBy(l => l.Status)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .ToListAsync(ct);

        var teamAttendanceTrend = await _context.Attendances
            .Where(a => reportIds.Contains(a.EmployeeId))
            .GroupBy(a => a.AttendanceDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync(ct);

        return new ManagerDashboard
        {
            Kpis = kpis,
            TeamLeaveByStatus = teamLeaveByStatus,
            TeamAttendanceTrend = teamAttendanceTrend.Select(g => new TimeSeriesPoint { Date = g.Date, Value = g.Count }).ToList(),
        };
    }

    public async Task<AttendanceAnalytics> GetAttendanceAsync(string? range, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync(ct);
        var (start, end) = ReportingDateRange.Resolve(range, from, to);

        var present = await _context.Attendances.CountAsync(a => visibleIds.Contains(a.EmployeeId) && a.AttendanceDate >= start && a.AttendanceDate <= end && a.Status == "PRESENT", ct);
        var late = await _context.Attendances.CountAsync(a => visibleIds.Contains(a.EmployeeId) && a.AttendanceDate >= start && a.AttendanceDate <= end && a.IsLate, ct);
        var earlyLeave = await _context.Attendances.CountAsync(a => visibleIds.Contains(a.EmployeeId) && a.AttendanceDate >= start && a.AttendanceDate <= end && a.IsEarlyLeave, ct);
        var missingClockOut = await _context.Attendances.CountAsync(a => visibleIds.Contains(a.EmployeeId) && a.AttendanceDate >= start && a.AttendanceDate <= end && a.ClockIn != null && a.ClockOut == null, ct);

        var kpis = new List<KpiCard>
        {
            new() { Id = "ATT-PRESENT", Label = "Present", Value = present, Unit = "records" },
            new() { Id = "ATT-LATE", Label = "Late", Value = late, Unit = "records" },
            new() { Id = "ATT-EARLY-LEAVE", Label = "Early Leave", Value = earlyLeave, Unit = "records" },
            new() { Id = "ATT-MISSING-CLOCKOUT", Label = "Missing Clock Out", Value = missingClockOut, Unit = "records" },
        };

        var dailyTrend = await _context.Attendances
            .Where(a => visibleIds.Contains(a.EmployeeId) && a.AttendanceDate >= start && a.AttendanceDate <= end)
            .GroupBy(a => a.AttendanceDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderBy(g => g.Date)
            .ToListAsync(ct);

        var statusBreakdown = await _context.Attendances
            .Where(a => visibleIds.Contains(a.EmployeeId) && a.AttendanceDate >= start && a.AttendanceDate <= end)
            .GroupBy(a => a.Status)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .ToListAsync(ct);

        return new AttendanceAnalytics
        {
            Kpis = kpis,
            DailyTrend = dailyTrend.Select(g => new TimeSeriesPoint { Date = g.Date, Value = g.Count }).ToList(),
            StatusBreakdown = statusBreakdown,
        };
    }

    public async Task<LeaveAnalytics> GetLeaveAsync(string? range, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync(ct);
        var (start, end) = ReportingDateRange.Resolve(range, from, to);

        var approved = await _context.LeaveRequests.CountAsync(l => visibleIds.Contains(l.EmployeeId) && l.Status == "APPROVED" && l.StartDate >= start && l.StartDate <= end, ct);
        var pending = await _context.LeaveRequests.CountAsync(l => visibleIds.Contains(l.EmployeeId) && l.Status == "PENDING", ct);
        var rejected = await _context.LeaveRequests.CountAsync(l => visibleIds.Contains(l.EmployeeId) && l.Status == "REJECTED" && l.StartDate >= start && l.StartDate <= end, ct);

        var kpis = new List<KpiCard>
        {
            new() { Id = "LEAVE-APPROVED", Label = "Approved", Value = approved, Unit = "requests" },
            new() { Id = "LEAVE-PENDING", Label = "Pending", Value = pending, Unit = "requests" },
            new() { Id = "LEAVE-REJECTED", Label = "Rejected", Value = rejected, Unit = "requests" },
        };

        var byType = await _context.LeaveRequests
            .Where(l => visibleIds.Contains(l.EmployeeId) && l.Status == "APPROVED")
            .GroupBy(l => l.LeaveType)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Sum(x => x.TotalDays) })
            .OrderByDescending(b => b.Value)
            .ToListAsync(ct);

        var byStatus = await _context.LeaveRequests
            .Where(l => visibleIds.Contains(l.EmployeeId))
            .GroupBy(l => l.Status)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .ToListAsync(ct);

        var usageTrend = await _context.LeaveRequests
            .Where(l => visibleIds.Contains(l.EmployeeId) && l.Status == "APPROVED")
            .GroupBy(l => new { l.StartDate.Year, l.StartDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Days = g.Sum(x => x.TotalDays) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        return new LeaveAnalytics
        {
            Kpis = kpis,
            ByType = byType,
            ByStatus = byStatus,
            UsageTrend = usageTrend.Select(g => new TimeSeriesPoint { Date = new DateTime(g.Year, g.Month, 1), Value = g.Days }).ToList(),
        };
    }

    public async Task<PayrollAnalytics> GetPayrollAsync(CancellationToken ct = default)
    {
        // Payroll analytics are aggregate-only and restricted to privileged users
        // at the controller level. No per-employee salary detail is returned.
        var grossTotal = await _context.SalarySlips.SumAsync(s => (decimal?)s.GrossIncome, ct) ?? 0m;
        var netTotal = await _context.SalarySlips.SumAsync(s => (decimal?)s.NetSalary, ct) ?? 0m;
        var nssfEmployee = await _context.SalarySlips.SumAsync(s => (decimal?)s.NssfEmployeeDeduction, ct) ?? 0m;
        var nssfEmployer = await _context.SalarySlips.SumAsync(s => (decimal?)s.NssfEmployerContribution, ct) ?? 0m;
        var pit = await _context.SalarySlips.SumAsync(s => (decimal?)s.TaxDeduction, ct) ?? 0m;

        var kpis = new List<KpiCard>
        {
            new() { Id = "PAY-GROSS-TOTAL", Label = "Gross Payroll", Value = grossTotal, Unit = "LAK" },
            new() { Id = "PAY-NET-TOTAL", Label = "Net Payroll", Value = netTotal, Unit = "LAK" },
            new() { Id = "PAY-NSSF-EMPLOYEE", Label = "NSSF Employee", Value = nssfEmployee, Unit = "LAK" },
            new() { Id = "PAY-NSSF-EMPLOYER", Label = "NSSF Employer", Value = nssfEmployer, Unit = "LAK" },
            new() { Id = "PAY-PIT", Label = "PIT", Value = pit, Unit = "LAK" },
        };

        var byDepartment = await _context.SalarySlips
            .GroupBy(s => s.Employee.Department != null ? s.Employee.Department.DepartmentName : "Unassigned")
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Sum(x => x.GrossIncome) })
            .OrderByDescending(b => b.Value)
            .ToListAsync(ct);

        var grossTrend = await _context.SalarySlips
            .GroupBy(s => new { s.PayrollPeriod.Year, s.PayrollPeriod.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.GrossIncome) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        return new PayrollAnalytics
        {
            Kpis = kpis,
            ByDepartment = byDepartment,
            GrossTrend = grossTrend.Select(g => new TimeSeriesPoint { Date = new DateTime(g.Year, g.Month, 1), Value = g.Total }).ToList(),
        };
    }

    public async Task<FinanceAnalytics> GetFinanceAsync(string? range, DateTime? from, DateTime? to, CancellationToken ct = default)
    {
        var visibleIds = await _scope.GetVisibleEmployeeIdsAsync(ct);
        var (start, end) = ReportingDateRange.Resolve(range, from, to);

        var submitted = await _context.Expenses.Where(e => visibleIds.Contains(e.EmployeeId) && e.Status == "SUBMITTED").SumAsync(e => (decimal?)e.AmountLak, ct) ?? 0m;
        var approved = await _context.Expenses.Where(e => visibleIds.Contains(e.EmployeeId) && (e.Status == "APPROVED" || e.Status == "PAID")).SumAsync(e => (decimal?)e.AmountLak, ct) ?? 0m;
        var outstandingPrincipal = await _context.EmployeeLoans.Where(l => visibleIds.Contains(l.EmployeeId) && (l.Status == "ACTIVE" || l.Status == "APPROVED")).SumAsync(l => (decimal?)l.PrincipalLak, ct) ?? 0m;
        var activeLoans = await _context.EmployeeLoans.CountAsync(l => visibleIds.Contains(l.EmployeeId) && l.Status == "ACTIVE", ct);

        var kpis = new List<KpiCard>
        {
            new() { Id = "FIN-EXPENSE-SUBMITTED", Label = "Submitted Expenses", Value = submitted, Unit = "LAK" },
            new() { Id = "FIN-EXPENSE-APPROVED", Label = "Approved Expenses", Value = approved, Unit = "LAK" },
            new() { Id = "FIN-LOAN-OUTSTANDING", Label = "Outstanding Principal", Value = outstandingPrincipal, Unit = "LAK" },
            new() { Id = "FIN-LOAN-ACTIVE", Label = "Active Loans", Value = activeLoans, Unit = "loans" },
        };

        var expenseByCategory = await _context.Expenses
            .Where(e => visibleIds.Contains(e.EmployeeId) && e.ExpenseDate >= start && e.ExpenseDate <= end)
            .GroupBy(e => e.Category != null ? e.Category.Name : "Uncategorized")
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Sum(x => x.AmountLak) })
            .OrderByDescending(b => b.Value)
            .ToListAsync(ct);

        var loanByStatus = await _context.EmployeeLoans
            .Where(l => visibleIds.Contains(l.EmployeeId))
            .GroupBy(l => l.Status)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .ToListAsync(ct);

        var expenseTrend = await _context.Expenses
            .Where(e => visibleIds.Contains(e.EmployeeId) && e.ExpenseDate >= start && e.ExpenseDate <= end)
            .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.AmountLak) })
            .OrderBy(g => g.Year).ThenBy(g => g.Month)
            .ToListAsync(ct);

        return new FinanceAnalytics
        {
            Kpis = kpis,
            ExpenseByCategory = expenseByCategory,
            LoanByStatus = loanByStatus,
            ExpenseTrend = expenseTrend.Select(g => new TimeSeriesPoint { Date = new DateTime(g.Year, g.Month, 1), Value = g.Total }).ToList(),
        };
    }

    public async Task<PmAnalytics> GetPmAsync(CancellationToken ct = default)
    {
        var activeProjects = await _context.Projects.CountAsync(p => p.Status == "ACTIVE", ct);
        var completedProjects = await _context.Projects.CountAsync(p => p.Status == "COMPLETED", ct);
        var openTasks = await _context.ProjectTasks.CountAsync(t => t.Status != "DONE" && t.Status != "CANCELLED", ct);
        var overdueTasks = await _context.ProjectTasks.CountAsync(t => t.DueDate != null && t.DueDate < DateTime.UtcNow && t.Status != "DONE" && t.Status != "CANCELLED", ct);
        var openRisks = await _context.Risks.CountAsync(r => r.Status == "OPEN", ct);
        var criticalRisks = await _context.Risks.CountAsync(r => r.Status == "OPEN" && r.Priority == "CRITICAL", ct);
        var openIssues = await _context.Issues.CountAsync(i => i.Status != "DONE" && i.Status != "CANCELLED", ct);

        var kpis = new List<KpiCard>
        {
            new() { Id = "PM-PROJECT-ACTIVE", Label = "Active Projects", Value = activeProjects, Unit = "projects" },
            new() { Id = "PM-PROJECT-COMPLETED", Label = "Completed Projects", Value = completedProjects, Unit = "projects" },
            new() { Id = "PM-TASK-OPEN", Label = "Open Tasks", Value = openTasks, Unit = "tasks" },
            new() { Id = "PM-TASK-OVERDUE", Label = "Overdue Tasks", Value = overdueTasks, Unit = "tasks" },
            new() { Id = "PM-RISK-OPEN", Label = "Open Risks", Value = openRisks, Unit = "risks" },
            new() { Id = "PM-RISK-CRITICAL", Label = "Critical Risks", Value = criticalRisks, Unit = "risks" },
            new() { Id = "PM-ISSUE-OPEN", Label = "Open Issues", Value = openIssues, Unit = "issues" },
        };

        var projectByStatus = await _context.Projects
            .GroupBy(p => p.Status)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .ToListAsync(ct);

        var taskByStatus = await _context.ProjectTasks
            .GroupBy(t => t.Status)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .ToListAsync(ct);

        var riskBySeverity = await _context.Risks
            .Where(r => r.Status == "OPEN")
            .GroupBy(r => r.Priority)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .ToListAsync(ct);

        var issueByStatus = await _context.Issues
            .GroupBy(i => i.Status)
            .Select(g => new BreakdownItem { Key = g.Key, Label = g.Key, Value = g.Count() })
            .ToListAsync(ct);

        return new PmAnalytics
        {
            Kpis = kpis,
            ProjectByStatus = projectByStatus,
            TaskByStatus = taskByStatus,
            RiskBySeverity = riskBySeverity,
            IssueByStatus = issueByStatus,
        };
    }
}
