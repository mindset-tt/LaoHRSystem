using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class CorporateDocumentDto
{
    public int DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string OwnerEntityType { get; set; } = string.Empty;
    public int OwnerEntityId { get; set; }
    public string Confidentiality { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CurrentVersion { get; set; }
    public DateTime? DocumentDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class DocumentVersionDto
{
    public int DocumentVersionId { get; set; }
    public int DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long FileSize { get; set; }
    public string StorageReference { get; set; } = string.Empty;
    public string? Checksum { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class CreateCorporateDocumentRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string DocumentType { get; set; } = "GENERAL";
    public string OwnerEntityType { get; set; } = "GENERAL";
    public int OwnerEntityId { get; set; }
    public string Confidentiality { get; set; } = "INTERNAL";
    public DateTime? DocumentDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class AddDocumentVersionRequest
{
    public string FileName { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long FileSize { get; set; }
    public string StorageReference { get; set; } = string.Empty;
    public string? Checksum { get; set; }
}

[Authorize]
[ApiController]
[Route("api/corporate-documents")]
public class CorporateDocumentsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICorporateOperationsAccessService _access;
    private readonly ICurrentEmployeeService _currentEmployee;
    private readonly INumberSequenceService _numbers;
    private readonly IDocumentService _documents;

    public CorporateDocumentsController(
        LaoHRDbContext context,
        ICorporateOperationsAccessService access,
        ICurrentEmployeeService currentEmployee,
        INumberSequenceService numbers,
        IDocumentService documents)
    {
        _context = context;
        _access = access;
        _currentEmployee = currentEmployee;
        _numbers = numbers;
        _documents = documents;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<CorporateDocumentDto>>> GetDocuments(
        [FromQuery] string? ownerEntityType = null,
        [FromQuery] int? ownerEntityId = null,
        [FromQuery] string? documentType = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewDocuments())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.CorporateDocuments.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(ownerEntityType))
            query = query.Where(d => d.OwnerEntityType == ownerEntityType);
        if (ownerEntityId.HasValue)
            query = query.Where(d => d.OwnerEntityId == ownerEntityId.Value);
        if (!string.IsNullOrEmpty(documentType))
            query = query.Where(d => d.DocumentType == documentType);

        var total = await query.LongCountAsync();
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new CorporateDocumentDto
            {
                DocumentId = d.DocumentId,
                DocumentNumber = d.DocumentNumber,
                Title = d.Title,
                Description = d.Description,
                Category = d.Category,
                DocumentType = d.DocumentType,
                OwnerEntityType = d.OwnerEntityType,
                OwnerEntityId = d.OwnerEntityId,
                Confidentiality = d.Confidentiality,
                Status = d.Status,
                CurrentVersion = d.CurrentVersion,
                DocumentDate = d.DocumentDate,
                ExpiryDate = d.ExpiryDate,
                CreatedAt = d.CreatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<CorporateDocumentDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CorporateDocumentDto>> GetDocument(int id)
    {
        if (!_access.CanViewDocuments())
            return Forbid();

        var d = await _context.CorporateDocuments.AsNoTracking()
            .FirstOrDefaultAsync(x => x.DocumentId == id);
        if (d == null) return NotFound();

        return new CorporateDocumentDto
        {
            DocumentId = d.DocumentId,
            DocumentNumber = d.DocumentNumber,
            Title = d.Title,
            Description = d.Description,
            Category = d.Category,
            DocumentType = d.DocumentType,
            OwnerEntityType = d.OwnerEntityType,
            OwnerEntityId = d.OwnerEntityId,
            Confidentiality = d.Confidentiality,
            Status = d.Status,
            CurrentVersion = d.CurrentVersion,
            DocumentDate = d.DocumentDate,
            ExpiryDate = d.ExpiryDate,
            CreatedAt = d.CreatedAt,
        };
    }

    [HttpGet("{id:int}/versions")]
    public async Task<ActionResult<List<DocumentVersionDto>>> GetVersions(int id)
    {
        if (!_access.CanViewDocuments())
            return Forbid();

        return await _context.DocumentVersions.AsNoTracking()
            .Where(v => v.DocumentId == id)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new DocumentVersionDto
            {
                DocumentVersionId = v.DocumentVersionId,
                DocumentId = v.DocumentId,
                VersionNumber = v.VersionNumber,
                FileName = v.FileName,
                MimeType = v.MimeType,
                FileSize = v.FileSize,
                StorageReference = v.StorageReference,
                Checksum = v.Checksum,
                UploadedAt = v.UploadedAt,
            })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<CorporateDocument>> CreateDocument([FromBody] CreateCorporateDocumentRequest request)
    {
        if (!_access.CanManageDocuments())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required.");

        var document = new CorporateDocument
        {
            DocumentNumber = await _numbers.NextAsync("DOC"),
            Title = request.Title,
            Description = request.Description,
            Category = request.Category,
            DocumentType = request.DocumentType,
            OwnerEntityType = request.OwnerEntityType,
            OwnerEntityId = request.OwnerEntityId,
            Confidentiality = request.Confidentiality,
            DocumentDate = request.DocumentDate,
            ExpiryDate = request.ExpiryDate,
            CreatedByEmployeeId = _currentEmployee.GetCurrentEmployeeId(),
        };
        _context.CorporateDocuments.Add(document);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDocument), new { id = document.DocumentId }, document);
    }

    [HttpPost("{id:int}/versions")]
    public async Task<ActionResult<DocumentVersion>> AddVersion(int id, [FromBody] AddDocumentVersionRequest request)
    {
        if (!_access.CanManageDocuments())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.FileName) || string.IsNullOrWhiteSpace(request.StorageReference))
            return BadRequest("FileName and StorageReference are required.");

        var version = await _documents.AddVersionAsync(
            id, request.FileName, request.MimeType, request.FileSize,
            request.StorageReference, request.Checksum, _currentEmployee.GetCurrentEmployeeId());

        return CreatedAtAction(nameof(GetVersions), new { id = id }, version);
    }
}
