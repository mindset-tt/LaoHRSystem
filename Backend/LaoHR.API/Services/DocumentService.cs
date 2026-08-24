using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4C — corporate document management (versioning + access).
///
/// Version invariant: version numbers increase monotonically per document; the
/// previous version is never overwritten.
/// </summary>
public interface IDocumentService
{
    /// <summary>Adds a new version to a document, incrementing CurrentVersion.</summary>
    Task<DocumentVersion> AddVersionAsync(
        int documentId, string fileName, string? mimeType, long fileSize,
        string storageReference, string? checksum, int? uploadedByEmployeeId,
        CancellationToken ct = default);

    /// <summary>Returns the latest version of a document.</summary>
    Task<DocumentVersion?> GetLatestVersionAsync(int documentId, CancellationToken ct = default);
}

public sealed class DocumentService : IDocumentService
{
    private readonly LaoHRDbContext _context;

    public DocumentService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<DocumentVersion> AddVersionAsync(
        int documentId, string fileName, string? mimeType, long fileSize,
        string storageReference, string? checksum, int? uploadedByEmployeeId,
        CancellationToken ct = default)
    {
        var document = await _context.CorporateDocuments
            .FirstOrDefaultAsync(d => d.DocumentId == documentId, ct)
            ?? throw new InvalidOperationException("Document not found.");

        var nextVersion = document.CurrentVersion + 1;
        var version = new DocumentVersion
        {
            DocumentId = documentId,
            VersionNumber = nextVersion,
            FileName = fileName,
            MimeType = mimeType,
            FileSize = fileSize,
            StorageReference = storageReference,
            Checksum = checksum,
            UploadedByEmployeeId = uploadedByEmployeeId,
        };
        _context.DocumentVersions.Add(version);

        document.CurrentVersion = nextVersion;
        document.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return version;
    }

    public async Task<DocumentVersion?> GetLatestVersionAsync(int documentId, CancellationToken ct = default)
    {
        return await _context.DocumentVersions
            .Where(v => v.DocumentId == documentId)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);
    }
}
