using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public AuditLogsController(LaoHRDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get recent audit logs (paged).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<AuditLog>>> GetLogs(
        [FromQuery] string? entity = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.AuditLogs
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(entity))
        {
            query = query.Where(l => l.EntityName == entity);
        }

        var total = await query.LongCountAsync();

        var items = await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<AuditLog>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }
}
