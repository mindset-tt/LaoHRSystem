using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 3B — resolves the effective statutory compliance rule for a given
/// jurisdiction, category, and date. Rules are effective-dated and versioned;
/// BLOCKED rules are never returned for production calculation.
/// </summary>
public interface IComplianceRuleService
{
    /// <summary>
    /// Returns the effective rule for the given category as of the reference date,
    /// or null if none exists. BLOCKED rules are excluded.
    /// </summary>
    Task<ComplianceRule?> GetEffectiveRuleAsync(string category, DateTime referenceDate, CancellationToken ct = default);

    /// <summary>
    /// Returns all effective rules for a category as of the reference date.
    /// </summary>
    Task<List<ComplianceRule>> GetEffectiveRulesAsync(string category, DateTime referenceDate, CancellationToken ct = default);
}

public sealed class ComplianceRuleService : IComplianceRuleService
{
    private readonly LaoHRDbContext _context;

    public ComplianceRuleService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<ComplianceRule?> GetEffectiveRuleAsync(string category, DateTime referenceDate, CancellationToken ct = default)
    {
        return await _context.ComplianceRules
            .AsNoTracking()
            .Where(r => r.Category == category
                     && r.Status != "BLOCKED"
                     && r.EffectiveFrom <= referenceDate
                     && (r.EffectiveTo == null || r.EffectiveTo > referenceDate))
            .OrderByDescending(r => r.EffectiveFrom)
            .ThenByDescending(r => r.Version)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<ComplianceRule>> GetEffectiveRulesAsync(string category, DateTime referenceDate, CancellationToken ct = default)
    {
        return await _context.ComplianceRules
            .AsNoTracking()
            .Where(r => r.Category == category
                     && r.Status != "BLOCKED"
                     && r.EffectiveFrom <= referenceDate
                     && (r.EffectiveTo == null || r.EffectiveTo > referenceDate))
            .OrderBy(r => r.RuleId)
            .ToListAsync(ct);
    }
}