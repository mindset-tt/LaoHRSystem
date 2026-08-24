using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class ComplianceRuleServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly ComplianceRuleService _service;

    public ComplianceRuleServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new ComplianceRuleService(_context);
    }

    [Fact]
    public async Task GetEffectiveRule_ReturnsRuleActiveOnDate()
    {
        // Arrange — a rule effective from 2026-07-01, no end date
        _context.ComplianceRules.Add(new ComplianceRule
        {
            RuleId = "LAO-PIT-2026-BRACKET-01",
            Category = "PIT",
            Name = "PIT bracket 1",
            EffectiveFrom = new DateTime(2026, 7, 1),
            Version = 1,
            Status = "VERIFIED",
            ParametersJson = "{\"min\":0,\"max\":2500000,\"rate\":0.00}"
        });
        await _context.SaveChangesAsync();

        // Act — query a date after effective
        var rule = await _service.GetEffectiveRuleAsync("PIT", new DateTime(2026, 8, 31));

        // Assert
        rule.Should().NotBeNull();
        rule!.RuleId.Should().Be("LAO-PIT-2026-BRACKET-01");
    }

    [Fact]
    public async Task GetEffectiveRule_ReturnsNullBeforeEffectiveDate()
    {
        // Arrange
        _context.ComplianceRules.Add(new ComplianceRule
        {
            RuleId = "LAO-PIT-2026-BRACKET-01",
            Category = "PIT",
            Name = "PIT bracket 1",
            EffectiveFrom = new DateTime(2026, 7, 1),
            Version = 1,
            Status = "VERIFIED"
        });
        await _context.SaveChangesAsync();

        // Act — query a date BEFORE effective
        var rule = await _service.GetEffectiveRuleAsync("PIT", new DateTime(2026, 6, 30));

        // Assert
        rule.Should().BeNull();
    }

    [Fact]
    public async Task GetEffectiveRule_ExcludesBlockedRules()
    {
        // Arrange — a BLOCKED rule must never be returned for production calculation
        _context.ComplianceRules.Add(new ComplianceRule
        {
            RuleId = "LAO-OT-DIVISOR",
            Category = "OVERTIME",
            Name = "OT hourly divisor (UNRESOLVED)",
            EffectiveFrom = new DateTime(2006, 12, 27),
            Version = 1,
            Status = "BLOCKED"
        });
        await _context.SaveChangesAsync();

        // Act
        var rule = await _service.GetEffectiveRuleAsync("OVERTIME", new DateTime(2026, 8, 31));

        // Assert — BLOCKED rule is excluded
        rule.Should().BeNull();
    }

    [Fact]
    public async Task GetEffectiveRule_ReturnsSupersededRuleCorrectly()
    {
        // Arrange — old rule (superseded) + new rule (active)
        _context.ComplianceRules.AddRange(
            new ComplianceRule
            {
                RuleId = "LAO-PIT-THRESHOLD",
                Category = "PIT",
                Name = "PIT threshold (old)",
                EffectiveFrom = new DateTime(2020, 1, 1),
                EffectiveTo = new DateTime(2026, 7, 1),
                Version = 1,
                Status = "SUPERSEDED",
                ParametersJson = "{\"threshold\":1300000}"
            },
            new ComplianceRule
            {
                RuleId = "LAO-PIT-THRESHOLD",
                Category = "PIT",
                Name = "PIT threshold (new)",
                EffectiveFrom = new DateTime(2026, 7, 1),
                Version = 2,
                Status = "VERIFIED",
                ParametersJson = "{\"threshold\":2500000}"
            });
        await _context.SaveChangesAsync();

        // Act — query a date in the old period
        var oldRule = await _service.GetEffectiveRuleAsync("PIT", new DateTime(2025, 3, 1));
        // Act — query a date in the new period
        var newRule = await _service.GetEffectiveRuleAsync("PIT", new DateTime(2026, 8, 1));

        // Assert
        oldRule.Should().NotBeNull();
        oldRule!.ParametersJson.Should().Contain("1300000");
        newRule.Should().NotBeNull();
        newRule!.ParametersJson.Should().Contain("2500000");
    }
}