using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class NumberSequenceServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly NumberSequenceService _service;

    public NumberSequenceServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new NumberSequenceService(_context);
    }

    [Fact]
    public async Task NextAsync_ProducesSequentialNumbers()
    {
        var first = await _service.NextAsync("PR");
        var second = await _service.NextAsync("PR");
        var third = await _service.NextAsync("PR");

        first.Should().Be($"PR-{DateTime.UtcNow.Year}-000001");
        second.Should().Be($"PR-{DateTime.UtcNow.Year}-000002");
        third.Should().Be($"PR-{DateTime.UtcNow.Year}-000003");
    }

    [Fact]
    public async Task NextAsync_DifferentPrefixes_AreIndependent()
    {
        var pr = await _service.NextAsync("PR");
        var po = await _service.NextAsync("PO");

        pr.Should().EndWith("000001");
        po.Should().EndWith("000001");
    }

    [Fact]
    public async Task NextAsync_ConcurrentRequests_ProduceUniqueNumbers()
    {
        // 100 concurrent requests must yield 100 unique numbers (no duplicates).
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => _service.NextAsync("GRN"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyHaveUniqueItems();
        results.Should().HaveCount(100);
    }
}
