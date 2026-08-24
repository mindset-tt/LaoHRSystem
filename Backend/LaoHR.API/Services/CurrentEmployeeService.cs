using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3C2 — resolves the current authenticated user's linked Employee.
/// Centralizes the AppUser → Employee claim parsing that would otherwise be
/// duplicated across controllers.
/// </summary>
public interface ICurrentEmployeeService
{
    /// <summary>Returns the Employee linked to the current user, or null if unlinked.</summary>
    Task<Employee?> GetCurrentEmployeeAsync(CancellationToken ct = default);

    /// <summary>Returns the current user's EmployeeId, or null if unlinked.</summary>
    int? GetCurrentEmployeeId();
}

public sealed class CurrentEmployeeService : ICurrentEmployeeService
{
    private readonly LaoHRDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentEmployeeService(LaoHRDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public int? GetCurrentEmployeeId()
    {
        var empIdStr = _httpContextAccessor.HttpContext?.User.FindFirst("EmployeeId")?.Value;
        if (int.TryParse(empIdStr, out var empId)) return empId;
        return null;
    }

    public async Task<Employee?> GetCurrentEmployeeAsync(CancellationToken ct = default)
    {
        var empId = GetCurrentEmployeeId();
        if (empId == null) return null;

        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.WorkLocation)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.EmployeeId == empId.Value, ct);
    }
}