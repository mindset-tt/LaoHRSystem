using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C3 — centralized read-path authorization scope.
///
/// Resolves what employee records the current authenticated user is allowed to
/// see, based on role + manager hierarchy. This is the single source of truth
/// for read-path authorization so that list/detail/export endpoints and
/// analytics queries all apply the SAME rules (no scattered role checks).
///
/// Scope rules:
///   - Admin / HR: full organization scope.
///   - Manager: self + direct reports (MSS policy — direct reports only).
///   - Employee: self only.
///   - Unlinked user (no EmployeeId): no employee scope.
/// </summary>
public interface IDataScopeService
{
    /// <summary>True if the current user is Admin or HR (full org scope).</summary>
    bool IsPrivileged();

    /// <summary>True if the current user is a manager (has direct reports).</summary>
    Task<bool> IsManagerAsync(CancellationToken ct = default);

    /// <summary>Returns the current user's EmployeeId, or null if unlinked.</summary>
    int? GetCurrentEmployeeId();

    /// <summary>
    /// Returns the set of employee ids the current user may view.
    /// Admin/HR → all active employees; manager → self + direct reports;
    /// employee → self only.
    /// </summary>
    Task<HashSet<int>> GetVisibleEmployeeIdsAsync(CancellationToken ct = default);

    /// <summary>True if the current user may view the given employee's records.</summary>
    Task<bool> CanViewEmployeeAsync(int employeeId, CancellationToken ct = default);

    /// <summary>True if the given employee is the current user (self).</summary>
    bool IsSelf(int employeeId);

    /// <summary>True if the given employee is a direct report of the current user.</summary>
    Task<bool> IsManagerOfAsync(int employeeId, CancellationToken ct = default);
}

public sealed class DataScopeService : IDataScopeService
{
    private readonly LaoHRDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DataScopeService(LaoHRDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsPrivileged()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user != null && (user.IsInRole("Admin") || user.IsInRole("HR"));
    }

    public int? GetCurrentEmployeeId()
    {
        var empIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(empIdStr, out var empId)) return empId;
        return null;
    }

    public bool IsSelf(int employeeId)
    {
        return GetCurrentEmployeeId() == employeeId;
    }

    public async Task<bool> IsManagerAsync(CancellationToken ct = default)
    {
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        return await _context.Employees
            .AnyAsync(e => e.ManagerId == empId.Value && e.IsActive, ct);
    }

    public async Task<bool> IsManagerOfAsync(int employeeId, CancellationToken ct = default)
    {
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        return await _context.Employees
            .AnyAsync(e => e.EmployeeId == employeeId && e.ManagerId == empId.Value, ct);
    }

    public async Task<HashSet<int>> GetVisibleEmployeeIdsAsync(CancellationToken ct = default)
    {
        var result = new HashSet<int>();

        if (IsPrivileged())
        {
            var all = await _context.Employees
                .AsNoTracking()
                .Where(e => e.IsActive)
                .Select(e => e.EmployeeId)
                .ToListAsync(ct);
            result.UnionWith(all);
            return result;
        }

        var selfId = GetCurrentEmployeeId();
        if (selfId == null) return result;

        result.Add(selfId.Value);

        // Manager: add direct reports.
        var reports = await _context.Employees
            .AsNoTracking()
            .Where(e => e.ManagerId == selfId.Value && e.IsActive)
            .Select(e => e.EmployeeId)
            .ToListAsync(ct);
        result.UnionWith(reports);

        return result;
    }

    public async Task<bool> CanViewEmployeeAsync(int employeeId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        if (IsSelf(employeeId)) return true;
        return await IsManagerOfAsync(employeeId, ct);
    }
}
