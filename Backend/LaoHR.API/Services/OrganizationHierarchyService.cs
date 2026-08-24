using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C1 — organization hierarchy operations: department tree, manager
/// reporting lines, cycle prevention, and manager-chain resolution.
/// </summary>
public interface IOrganizationHierarchyService
{
    /// <summary>Returns the full department tree (root departments with nested children).</summary>
    Task<List<DepartmentNode>> GetDepartmentTreeAsync(CancellationToken ct = default);

    /// <summary>Returns the direct reports of an employee.</summary>
    Task<List<Employee>> GetDirectReportsAsync(int employeeId, CancellationToken ct = default);

    /// <summary>Returns the manager chain (employee → manager → manager's manager → ...).</summary>
    Task<List<Employee>> GetManagerChainAsync(int employeeId, CancellationToken ct = default);

    /// <summary>Validates that assigning parentDepartmentId to departmentId does not create a cycle.</summary>
    Task<bool> WouldCreateDepartmentCycleAsync(int departmentId, int? parentDepartmentId, CancellationToken ct = default);

    /// <summary>Validates that assigning managerId to employeeId does not create a reporting cycle.</summary>
    Task<bool> WouldCreateReportingCycleAsync(int employeeId, int? managerId, CancellationToken ct = default);
}

public sealed class DepartmentNode
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? DepartmentNameEn { get; set; }
    public string? DepartmentCode { get; set; }
    public int? ManagerEmployeeId { get; set; }
    public string? ManagerName { get; set; }
    public int DirectHeadcount { get; set; }
    public List<DepartmentNode> Children { get; set; } = new();
}

public sealed class OrganizationHierarchyService : IOrganizationHierarchyService
{
    private readonly LaoHRDbContext _context;

    public OrganizationHierarchyService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<List<DepartmentNode>> GetDepartmentTreeAsync(CancellationToken ct = default)
    {
        var departments = await _context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .Select(d => new
            {
                d.DepartmentId,
                d.DepartmentName,
                d.DepartmentNameEn,
                d.DepartmentCode,
                d.ParentDepartmentId,
                d.ManagerEmployeeId,
                ManagerName = d.Manager != null ? (d.Manager.EnglishName ?? d.Manager.LaoName) : null,
                DirectHeadcount = d.Employees.Count(e => e.IsActive)
            })
            .ToListAsync(ct);

        var nodes = departments.Select(d => new DepartmentNode
        {
            DepartmentId = d.DepartmentId,
            DepartmentName = d.DepartmentName,
            DepartmentNameEn = d.DepartmentNameEn,
            DepartmentCode = d.DepartmentCode,
            ManagerEmployeeId = d.ManagerEmployeeId,
            ManagerName = d.ManagerName,
            DirectHeadcount = d.DirectHeadcount
        }).ToDictionary(n => n.DepartmentId);

        var roots = new List<DepartmentNode>();
        foreach (var node in nodes.Values)
        {
            var parentId = departments.First(d => d.DepartmentId == node.DepartmentId).ParentDepartmentId;
            if (parentId.HasValue && nodes.TryGetValue(parentId.Value, out var parent))
            {
                parent.Children.Add(node);
            }
            else
            {
                roots.Add(node);
            }
        }

        return roots;
    }

    public async Task<List<Employee>> GetDirectReportsAsync(int employeeId, CancellationToken ct = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.WorkLocation)
            .Where(e => e.ManagerId == employeeId && e.IsActive)
            .OrderBy(e => e.EmployeeCode)
            .ToListAsync(ct);
    }

    public async Task<List<Employee>> GetManagerChainAsync(int employeeId, CancellationToken ct = default)
    {
        var chain = new List<Employee>();
        var visited = new HashSet<int>();
        int? currentId = employeeId;

        while (currentId.HasValue && visited.Add(currentId.Value))
        {
            var employee = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Include(e => e.Position)
                .Include(e => e.WorkLocation)
                .FirstOrDefaultAsync(e => e.EmployeeId == currentId.Value, ct);
            if (employee == null) break;

            if (employee.ManagerId.HasValue)
            {
                var manager = await _context.Employees
                    .AsNoTracking()
                    .Include(e => e.Department)
                    .Include(e => e.Position)
                    .Include(e => e.WorkLocation)
                    .FirstOrDefaultAsync(e => e.EmployeeId == employee.ManagerId.Value, ct);
                if (manager != null) chain.Add(manager);
            }
            currentId = employee.ManagerId;
        }

        return chain;
    }

    public async Task<bool> WouldCreateDepartmentCycleAsync(int departmentId, int? parentDepartmentId, CancellationToken ct = default)
    {
        if (!parentDepartmentId.HasValue) return false;
        if (parentDepartmentId.Value == departmentId) return true; // self-parent

        // Walk up from the proposed parent; if we reach departmentId, it's a cycle.
        int? cursor = parentDepartmentId;
        var visited = new HashSet<int>();
        while (cursor.HasValue && visited.Add(cursor.Value))
        {
            if (cursor.Value == departmentId) return true;
            var parent = await _context.Departments
                .AsNoTracking()
                .Where(d => d.DepartmentId == cursor.Value)
                .Select(d => d.ParentDepartmentId)
                .FirstOrDefaultAsync(ct);
            cursor = parent;
        }
        return false;
    }

    public async Task<bool> WouldCreateReportingCycleAsync(int employeeId, int? managerId, CancellationToken ct = default)
    {
        if (!managerId.HasValue) return false;
        if (managerId.Value == employeeId) return true; // self-manager

        // Walk up the manager chain from the proposed manager; if we reach employeeId, it's a cycle.
        int? cursor = managerId;
        var visited = new HashSet<int>();
        while (cursor.HasValue && visited.Add(cursor.Value))
        {
            if (cursor.Value == employeeId) return true;
            var manager = await _context.Employees
                .AsNoTracking()
                .Where(e => e.EmployeeId == cursor.Value)
                .Select(e => e.ManagerId)
                .FirstOrDefaultAsync(ct);
            cursor = manager;
        }
        return false;
    }
}