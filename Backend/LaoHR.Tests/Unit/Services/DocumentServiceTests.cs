using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class DocumentServiceTests
{
    private readonly LaoHRDbContext _context;
    private readonly DocumentService _service;

    public DocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new LaoHRDbContext(options);
        _service = new DocumentService(_context);
    }

    private async Task<int> SeedDocumentAsync()
    {
        var doc = new CorporateDocument
        {
            DocumentNumber = "DOC-1",
            Title = "Policy",
            DocumentType = "POLICY",
            OwnerEntityType = "GENERAL",
            OwnerEntityId = 1,
            Confidentiality = "INTERNAL",
            CurrentVersion = 1,
        };
        _context.CorporateDocuments.Add(doc);
        await _context.SaveChangesAsync();
        return doc.DocumentId;
    }

    [Fact]
    public async Task AddVersion_IncrementsMonotonically()
    {
        var docId = await SeedDocumentAsync();

        var v2 = await _service.AddVersionAsync(docId, "v2.pdf", "application/pdf", 100, "ref2", null, 1);
        var v3 = await _service.AddVersionAsync(docId, "v3.pdf", "application/pdf", 100, "ref3", null, 1);

        v2.VersionNumber.Should().Be(2);
        v3.VersionNumber.Should().Be(3);

        var doc = await _context.CorporateDocuments.FindAsync(docId);
        doc!.CurrentVersion.Should().Be(3);
    }

    [Fact]
    public async Task AddVersion_PreservesPreviousVersion()
    {
        var docId = await SeedDocumentAsync();

        await _service.AddVersionAsync(docId, "v2.pdf", "application/pdf", 100, "ref2", null, 1);

        var versions = await _context.DocumentVersions
            .Where(v => v.DocumentId == docId)
            .OrderBy(v => v.VersionNumber)
            .ToListAsync();

        versions.Should().HaveCount(1);
        versions[0].VersionNumber.Should().Be(2);
        versions[0].StorageReference.Should().Be("ref2");
    }

    [Fact]
    public async Task GetLatestVersion_ReturnsHighest()
    {
        var docId = await SeedDocumentAsync();
        await _service.AddVersionAsync(docId, "v2.pdf", "application/pdf", 100, "ref2", null, 1);
        await _service.AddVersionAsync(docId, "v3.pdf", "application/pdf", 100, "ref3", null, 1);

        var latest = await _service.GetLatestVersionAsync(docId);
        latest!.VersionNumber.Should().Be(3);
        latest.StorageReference.Should().Be("ref3");
    }
}
