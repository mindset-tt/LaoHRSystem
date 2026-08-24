using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 3C1 — organization hierarchy, manager reporting lines, and My Team.
/// </summary>
[Authorize]
[ApiController]
[Route("api/organization")]
public class OrganizationController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IOrganizationHierarchyService _hierarchy;

    public OrganizationController(LaoHRDbContext context, IOrganizationHierarchyService hierarchy)
    {
        _context = context;
        _hierarchy = hierarchy;
    }

    /// <summary>Full department tree (root departments with nested children).</summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<DepartmentNode>>> GetTree()
    {
        return Ok(await _hierarchy.GetDepartmentTreeAsync());
    }

    /// <summary>Manager chain for an employee (employee → manager → ... → top).</summary>
    [HttpGet("employees/{employeeId}/manager-chain")]
    public async Task<ActionResult<List<EmployeeSummary>>> GetManagerChain(int employeeId)
    {
        var chain = await _hierarchy.GetManagerChainAsync(employeeId);
        return Ok(chain.Select(ToSummary).ToList());
    }

    /// <summary>Direct reports of an employee.</summary>
    [HttpGet("employees/{employeeId}/reports")]
    public async Task<ActionResult<List<EmployeeSummary>>> GetDirectReports(int employeeId)
    {
        var reports = await _hierarchy.GetDirectReportsAsync(employeeId);
        return Ok(reports.Select(ToSummary).ToList());
    }

    /// <summary>My Team — direct reports of the current user's linked employee.</summary>
    [HttpGet("my-team")]
    public async Task<ActionResult<List<EmployeeSummary>>> GetMyTeam()
    {
        var employeeId = GetCurrentEmployeeId();
        if (employeeId == null)
            return Ok(new List<EmployeeSummary>());

        var reports = await _hierarchy.GetDirectReportsAsync(employeeId.Value);
        return Ok(reports.Select(ToSummary).ToList());
    }

    private int? GetCurrentEmployeeId()
    {
        var empIdStr = User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(empIdStr, out var empId)) return empId;
        return null;
    }

    private static EmployeeSummary ToSummary(Employee e) => new()
    {
        EmployeeId = e.EmployeeId,
        EmployeeCode = e.EmployeeCode,
        LaoName = e.LaoName,
        EnglishName = e.EnglishName,
        JobTitle = e.JobTitle,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.DepartmentName,
        PositionId = e.PositionId,
        PositionTitle = e.Position?.Title,
        WorkLocationId = e.WorkLocationId,
        WorkLocationName = e.WorkLocation?.Name,
        Email = e.Email,
        Phone = e.Phone,
        IsActive = e.IsActive
    };
}

public sealed class EmployeeSummary
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string LaoName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? JobTitle { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? PositionId { get; set; }
    public string? PositionTitle { get; set; }
    public int? WorkLocationId { get; set; }
    public string? WorkLocationName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
}