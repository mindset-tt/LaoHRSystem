using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C4 — project access authorization. Determines what a user can do
/// within a project, based on project membership + global role. Distinct from
/// HR/manager scope (being a people-manager does NOT grant project authority).
/// </summary>
public interface IProjectAccessService
{
    /// <summary>True if the user is Admin/HR (full project access).</summary>
    bool IsPrivileged();

    /// <summary>Returns the current user's EmployeeId, or null if unlinked.</summary>
    int? GetCurrentEmployeeId();

    /// <summary>True if the user can view the project (privileged, owner, or member).</summary>
    Task<bool> CanViewProjectAsync(int projectId, CancellationToken ct = default);

    /// <summary>True if the user can edit the project (privileged, owner, or LEAD member).</summary>
    Task<bool> CanEditProjectAsync(int projectId, CancellationToken ct = default);

    /// <summary>True if the user can manage tasks (privileged, owner, LEAD, or MEMBER).</summary>
    Task<bool> CanManageTasksAsync(int projectId, CancellationToken ct = default);

    /// <summary>True if the user can manage resources/team (privileged, owner, or LEAD).</summary>
    Task<bool> CanManageResourcesAsync(int projectId, CancellationToken ct = default);

    /// <summary>True if the user can manage risks/issues (privileged, owner, LEAD, or MEMBER).</summary>
    Task<bool> CanManageRisksAsync(int projectId, CancellationToken ct = default);
}

public sealed class ProjectAccessService : IProjectAccessService
{
    private readonly LaoHRDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProjectAccessService(LaoHRDbContext context, IHttpContextAccessor httpContextAccessor)
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

    private async Task<string?> GetMemberRoleAsync(int projectId, int employeeId, CancellationToken ct)
    {
        return await _context.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId && pm.EmployeeId == employeeId)
            .Select(pm => (string?)pm.Role)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<bool> IsOwnerAsync(int projectId, int employeeId, CancellationToken ct)
    {
        return await _context.Projects
            .AsNoTracking()
            .AnyAsync(p => p.ProjectId == projectId && p.OwnerId == employeeId, ct);
    }

    public async Task<bool> CanViewProjectAsync(int projectId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        if (await IsOwnerAsync(projectId, empId.Value, ct)) return true;
        return await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == projectId && pm.EmployeeId == empId.Value, ct);
    }

    public async Task<bool> CanEditProjectAsync(int projectId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        if (await IsOwnerAsync(projectId, empId.Value, ct)) return true;
        var role = await GetMemberRoleAsync(projectId, empId.Value, ct);
        return role == "LEAD";
    }

    public async Task<bool> CanManageTasksAsync(int projectId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        if (await IsOwnerAsync(projectId, empId.Value, ct)) return true;
        var role = await GetMemberRoleAsync(projectId, empId.Value, ct);
        return role == "LEAD" || role == "MEMBER";
    }

    public async Task<bool> CanManageResourcesAsync(int projectId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        if (await IsOwnerAsync(projectId, empId.Value, ct)) return true;
        var role = await GetMemberRoleAsync(projectId, empId.Value, ct);
        return role == "LEAD";
    }

    public async Task<bool> CanManageRisksAsync(int projectId, CancellationToken ct = default)
    {
        if (IsPrivileged()) return true;
        var empId = GetCurrentEmployeeId();
        if (empId == null) return false;
        if (await IsOwnerAsync(projectId, empId.Value, ct)) return true;
        var role = await GetMemberRoleAsync(projectId, empId.Value, ct);
        return role == "LEAD" || role == "MEMBER";
    }
}
