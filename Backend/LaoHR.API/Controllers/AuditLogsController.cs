using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

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
    /// Get recent audit logs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuditLog>>> GetLogs([FromQuery] int limit = 100, [FromQuery] string? entity = null)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(entity))
        {
            query = query.Where(l => l.EntityName == entity);
        }

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .ToListAsync();
    }
}
