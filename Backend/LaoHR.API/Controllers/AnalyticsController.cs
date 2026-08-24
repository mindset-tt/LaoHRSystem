using LaoHR.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 3C3 — analytics/dashboard endpoints. Every endpoint resolves the
/// current user's authorized data scope server-side before aggregating.
/// </summary>
[Authorize]
[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;
    private readonly IDataScopeService _scope;

    public AnalyticsController(IAnalyticsService analytics, IDataScopeService scope)
    {
        _analytics = analytics;
        _scope = scope;
    }

    /// <summary>Executive dashboard (Admin/HR only).</summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("executive")]
    public async Task<ActionResult<ExecutiveDashboard>> GetExecutive()
    {
        return Ok(await _analytics.GetExecutiveAsync());
    }

    /// <summary>HR dashboard (Admin/HR only).</summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("hr")]
    public async Task<ActionResult<HrDashboard>> GetHr()
    {
        return Ok(await _analytics.GetHrAsync());
    }

    /// <summary>Manager dashboard (any user; scope is self + direct reports).</summary>
    [HttpGet("my-team")]
    public async Task<ActionResult<ManagerDashboard>> GetManager()
    {
        return Ok(await _analytics.GetManagerAsync());
    }

    /// <summary>Attendance analytics (scoped).</summary>
    [HttpGet("attendance")]
    public async Task<ActionResult<AttendanceAnalytics>> GetAttendance(
        [FromQuery] string? range = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        return Ok(await _analytics.GetAttendanceAsync(range, from, to));
    }

    /// <summary>Leave analytics (scoped).</summary>
    [HttpGet("leave")]
    public async Task<ActionResult<LeaveAnalytics>> GetLeave(
        [FromQuery] string? range = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        return Ok(await _analytics.GetLeaveAsync(range, from, to));
    }

    /// <summary>Payroll analytics (Admin/HR only; aggregate only).</summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpGet("payroll")]
    public async Task<ActionResult<PayrollAnalytics>> GetPayroll()
    {
        return Ok(await _analytics.GetPayrollAsync());
    }

    /// <summary>Finance analytics (scoped).</summary>
    [HttpGet("finance")]
    public async Task<ActionResult<FinanceAnalytics>> GetFinance(
        [FromQuery] string? range = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        return Ok(await _analytics.GetFinanceAsync(range, from, to));
    }

    /// <summary>PM / risk analytics.</summary>
    [HttpGet("pm")]
    public async Task<ActionResult<PmAnalytics>> GetPm()
    {
        return Ok(await _analytics.GetPmAsync());
    }
}
