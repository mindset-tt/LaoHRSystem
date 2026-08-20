using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

/// <summary>
/// Lightweight risk list response — embeds the cheap Score (Likelihood*Impact).
/// </summary>
public class RiskListItem
{
    public int RiskId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int Likelihood { get; set; }
    public int Impact { get; set; }
    public int Score { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RiskDetail
{
    public int RiskId { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Priority { get; set; } = string.Empty;
    public int Likelihood { get; set; }
    public int Impact { get; set; }
    public int Score => Likelihood * Impact;
    public string Status { get; set; } = string.Empty;
    public string? Mitigation { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRiskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public int Likelihood { get; set; } = 3;
    public int Impact { get; set; } = 3;
    public string? Status { get; set; }
    public string? Mitigation { get; set; }
    public int? OwnerId { get; set; }
    public DateTime? DueDate { get; set; }
}

public class UpdateRiskRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public int? Likelihood { get; set; }
    public int? Impact { get; set; }
    public string? Status { get; set; }
    public string? Mitigation { get; set; }
    public int? OwnerId { get; set; }
    public DateTime? DueDate { get; set; }
}

[Authorize]
[ApiController]
[Route("api/projects/{projectId:int}/[controller]")]
public class RisksController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public RisksController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<RiskListItem>>> GetRisks(
        int projectId,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Risks.AsNoTracking().Where(r => r.ProjectId == projectId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(r => r.Status == status);
        if (!string.IsNullOrEmpty(priority))
            query = query.Where(r => r.Priority == priority);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(s));
        }

        var total = await query.LongCountAsync();

        var items = await query
            .OrderByDescending(r => r.Likelihood * r.Impact)
            .ThenByDescending(r => r.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RiskListItem
            {
                RiskId = r.RiskId,
                ProjectId = r.ProjectId,
                Title = r.Title,
                Priority = r.Priority,
                Likelihood = r.Likelihood,
                Impact = r.Impact,
                Score = r.Likelihood * r.Impact,
                Status = r.Status,
                OwnerId = r.OwnerId,
                OwnerName = r.Owner != null ? (r.Owner.EnglishName ?? r.Owner.LaoName) : null,
                DueDate = r.DueDate,
                UpdatedAt = r.UpdatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<RiskListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RiskDetail>> GetRisk(int projectId, int id)
    {
        var risk = await _context.Risks
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RiskId == id && r.ProjectId == projectId);
        if (risk == null) return NotFound();

        var ownerName = risk.OwnerId.HasValue
            ? await _context.Employees
                .Where(e => e.EmployeeId == risk.OwnerId.Value)
                .Select(e => e.EnglishName ?? e.LaoName)
                .FirstOrDefaultAsync()
            : null;

        return new RiskDetail
        {
            RiskId = risk.RiskId,
            ProjectId = risk.ProjectId,
            Title = risk.Title,
            Description = risk.Description,
            Priority = risk.Priority,
            Likelihood = risk.Likelihood,
            Impact = risk.Impact,
            Status = risk.Status,
            Mitigation = risk.Mitigation,
            OwnerId = risk.OwnerId,
            OwnerName = ownerName,
            DueDate = risk.DueDate,
            CreatedAt = risk.CreatedAt,
            UpdatedAt = risk.UpdatedAt,
        };
    }

    [HttpPost]
    public async Task<ActionResult<Risk>> CreateRisk(int projectId, [FromBody] CreateRiskRequest request)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound("Project not found");

        var risk = new Risk
        {
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            Priority = request.Priority ?? "MEDIUM",
            Likelihood = Math.Clamp(request.Likelihood, 1, 5),
            Impact = Math.Clamp(request.Impact, 1, 5),
            Status = request.Status ?? "OPEN",
            Mitigation = request.Mitigation,
            OwnerId = request.OwnerId,
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        _context.Risks.Add(risk);
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"riskId\":{risk.RiskId},\"title\":\"{Escape(risk.Title)}\"}}");
        return CreatedAtAction(nameof(GetRisk), new { projectId, id = risk.RiskId }, risk);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateRisk(int projectId, int id, [FromBody] UpdateRiskRequest request)
    {
        var risk = await _context.Risks.FirstOrDefaultAsync(r => r.RiskId == id && r.ProjectId == projectId);
        if (risk == null) return NotFound();

        if (request.Title != null) risk.Title = request.Title;
        if (request.Description != null) risk.Description = request.Description;
        if (request.Priority != null) risk.Priority = request.Priority;
        if (request.Likelihood.HasValue) risk.Likelihood = Math.Clamp(request.Likelihood.Value, 1, 5);
        if (request.Impact.HasValue) risk.Impact = Math.Clamp(request.Impact.Value, 1, 5);
        if (request.Status != null) risk.Status = request.Status;
        if (request.Mitigation != null) risk.Mitigation = request.Mitigation;
        if (request.OwnerId.HasValue) risk.OwnerId = request.OwnerId;
        if (request.DueDate.HasValue) risk.DueDate = request.DueDate;

        risk.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"riskId\":{risk.RiskId},\"action\":\"updated\",\"status\":\"{risk.Status}\"}}");
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteRisk(int projectId, int id)
    {
        var risk = await _context.Risks.FirstOrDefaultAsync(r => r.RiskId == id && r.ProjectId == projectId);
        if (risk == null) return NotFound();

        _context.Risks.Remove(risk);
        await _context.SaveChangesAsync();

        await LogActivity(projectId, $"{{\"riskId\":{id},\"action\":\"deleted\"}}");
        return NoContent();
    }

    private int? GetCurrentEmployeeId()
    {
        var username = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name;
        if (string.IsNullOrEmpty(username)) return null;
        return _context.Users
            .Where(u => u.Username == username)
            .Select(u => u.EmployeeId)
            .FirstOrDefault();
    }

    private async Task LogActivity(int projectId, string payloadJson)
    {
        _context.ActivityLogs.Add(new ActivityLog
        {
            ProjectId = projectId,
            ActorId = GetCurrentEmployeeId() ?? 1,
            Action = "RISK_UPDATED",
            PayloadJson = payloadJson,
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}