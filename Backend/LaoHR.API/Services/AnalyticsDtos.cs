namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C3 — common analytics DTOs. Dashboard-specific, never EF entities.
/// </summary>

/// <summary>A single KPI card value with optional period-over-period delta.</summary>
public sealed class KpiCard
{
    public string Id { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? LabelLao { get; set; }
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal? Delta { get; set; }
    public string? ComparisonLabel { get; set; }
}

/// <summary>A single point in a time series.</summary>
public sealed class TimeSeriesPoint
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}

/// <summary>A category/status breakdown row.</summary>
public sealed class BreakdownItem
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public decimal? Percentage { get; set; }
}

/// <summary>An "attention" item surfaced on the executive dashboard.</summary>
public sealed class AttentionItem
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? Link { get; set; }
}

/// <summary>Executive dashboard aggregate payload.</summary>
public sealed class ExecutiveDashboard
{
    public List<KpiCard> Kpis { get; set; } = new();
    public List<AttentionItem> Attention { get; set; } = new();
    public List<BreakdownItem> HeadcountByDepartment { get; set; } = new();
    public List<TimeSeriesPoint> HeadcountTrend { get; set; } = new();
    public List<TimeSeriesPoint> LeaveTrend { get; set; } = new();
    public List<TimeSeriesPoint> ExpenseTrend { get; set; } = new();
}

/// <summary>HR dashboard aggregate payload.</summary>
public sealed class HrDashboard
{
    public List<KpiCard> Kpis { get; set; } = new();
    public List<BreakdownItem> HeadcountByDepartment { get; set; } = new();
    public List<BreakdownItem> HeadcountByLocation { get; set; } = new();
    public List<BreakdownItem> HeadcountByPosition { get; set; } = new();
    public List<BreakdownItem> LeaveByType { get; set; } = new();
    public List<TimeSeriesPoint> NewHiresTrend { get; set; } = new();
}

/// <summary>Manager (MSS) dashboard aggregate payload.</summary>
public sealed class ManagerDashboard
{
    public List<KpiCard> Kpis { get; set; } = new();
    public List<BreakdownItem> TeamLeaveByStatus { get; set; } = new();
    public List<TimeSeriesPoint> TeamAttendanceTrend { get; set; } = new();
}

/// <summary>Attendance analytics payload.</summary>
public sealed class AttendanceAnalytics
{
    public List<KpiCard> Kpis { get; set; } = new();
    public List<TimeSeriesPoint> DailyTrend { get; set; } = new();
    public List<BreakdownItem> StatusBreakdown { get; set; } = new();
}

/// <summary>Leave analytics payload.</summary>
public sealed class LeaveAnalytics
{
    public List<KpiCard> Kpis { get; set; } = new();
    public List<BreakdownItem> ByType { get; set; } = new();
    public List<BreakdownItem> ByStatus { get; set; } = new();
    public List<TimeSeriesPoint> UsageTrend { get; set; } = new();
}

/// <summary>Payroll analytics payload (aggregate only, no per-employee detail).</summary>
public sealed class PayrollAnalytics
{
    public List<KpiCard> Kpis { get; set; } = new();
    public List<BreakdownItem> ByDepartment { get; set; } = new();
    public List<TimeSeriesPoint> GrossTrend { get; set; } = new();
}

/// <summary>Finance (expense + loan) analytics payload.</summary>
public sealed class FinanceAnalytics
{
    public List<KpiCard> Kpis { get; set; } = new();
    public List<BreakdownItem> ExpenseByCategory { get; set; } = new();
    public List<BreakdownItem> LoanByStatus { get; set; } = new();
    public List<TimeSeriesPoint> ExpenseTrend { get; set; } = new();
}

/// <summary>PM / risk analytics payload.</summary>
public sealed class PmAnalytics
{
    public List<KpiCard> Kpis { get; set; } = new();
    public List<BreakdownItem> ProjectByStatus { get; set; } = new();
    public List<BreakdownItem> TaskByStatus { get; set; } = new();
    public List<BreakdownItem> RiskBySeverity { get; set; } = new();
    public List<BreakdownItem> IssueByStatus { get; set; } = new();
}
