using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IDataScopeService _scope;
    private readonly IConfiguration _configuration;

    public DocumentsController(LaoHRDbContext context, IWebHostEnvironment environment, IDataScopeService scope, IConfiguration configuration)
    {
        _context = context;
        _environment = environment;
        _scope = scope;
        _configuration = configuration;
    }

    /// <summary>
    /// Phase 4D.1 — document storage lives OUTSIDE the web root so uploaded
    /// files are never served by static-file middleware (which bypasses
    /// authorization). Downloads go through the authorized GetDocumentFile
    /// endpoint. Override via Storage:DocumentsRoot for volume mounts.
    /// </summary>
    private string StorageRoot => _configuration["Storage:DocumentsRoot"]
        ?? Path.Combine(_environment.ContentRootPath, "App_Data", "uploads");

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<EmployeeDocument>>> GetEmployeeDocuments(int employeeId)
    {
        // Phase 3C3 — read-path IDOR: only visible employees' documents.
        if (!await _scope.CanViewEmployeeAsync(employeeId))
            return Forbid();

        return await _context.EmployeeDocuments
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Phase 4D.1 — authorized document download. Streams from the protected
    /// storage root after an IDOR check. Attachment disposition prevents the
    /// browser from executing active content inline.
    /// </summary>
    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetDocumentFile(int id)
    {
        var document = await _context.EmployeeDocuments.FindAsync(id);
        if (document == null) return NotFound();

        // Same IDOR policy as listing/deleting.
        if (!await _scope.CanViewEmployeeAsync(document.EmployeeId))
            return Forbid();

        var fullPath = ResolveStoragePath(document.FilePath);
        if (fullPath == null || !System.IO.File.Exists(fullPath)) return NotFound();

        var contentType = ContentTypeFor(Path.GetExtension(document.FilePath));
        return File(
            new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read),
            contentType,
            fileDownloadName: SanitizeFileName(document.FileName));
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDocument>> UploadDocument([FromForm] int employeeId, [FromForm] string documentType, [FromForm] IFormFile file)
    {
        // Phase 4D — IDOR: only authorized users may upload for an employee.
        if (!await _scope.CanViewEmployeeAsync(employeeId))
            return Forbid();

        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        if (file == null || file.Length == 0)
            return RejectUpload("No file uploaded");

        // Phase 4D — server-side size limit (10 MB).
        const long maxBytes = 10 * 1024 * 1024;
        if (file.Length > maxBytes)
            return RejectUpload("File exceeds the 10 MB size limit.", "too_large");

        // Validate declared file type (PDF, Images, Word)
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return RejectUpload("Invalid file type. Only PDF, Images, and Word documents allowed.", "extension_denied");

        // Phase 4D.1 — content-signature validation: the bytes must match the
        // declared extension before anything is written to storage.
        await using var content = file.OpenReadStream();
        if (!FileSignatureValidator.ValidateOrRecord(extension, content))
            return RejectUpload("File content does not match its declared type.");

        // Phase 4D — sanitize the display filename (strip path separators and
        // control characters). The storage name is generated independently.
        var safeDisplayName = SanitizeFileName(file.FileName);

        // Ensure directory exists (outside the web root).
        var uploadsFolder = Path.Combine(StorageRoot, "documents", employeeId.ToString());
        Directory.CreateDirectory(uploadsFolder);

        // Generate a safe storage filename (never derived from user input).
        var storageName = $"{DateTime.Now.Ticks}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, storageName);
        var storageKey = $"documents/{employeeId}/{storageName}";

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentType = documentType,
            FileName = safeDisplayName,
            FilePath = storageKey,
            UploadedAt = DateTime.UtcNow
        };

        _context.EmployeeDocuments.Add(document);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployeeDocuments), new { employeeId = employeeId }, document);
    }

    /// <summary>
    /// Phase 4D — sanitize a user-supplied filename for display/storage safety.
    /// Strips directory separators and control characters; falls back to a
    /// generic name if the result is empty.
    /// </summary>
    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return "document";
        var name = Path.GetFileName(fileName); // strips any directory components
        name = new string(name.Where(c => !char.IsControl(c)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(name) ? "document" : name;
    }

    private BadRequestObjectResult RejectUpload(string message, string reason = "signature_mismatch")
    {
        Metrics.AppMetrics.UploadsRejected.Add(1, new KeyValuePair<string, object?>("reason", reason));
        return BadRequest(message);
    }

    /// <summary>
    /// Resolves a stored relative key against the storage root, refusing any
    /// value that escapes it (defense-in-depth against traversal).
    /// </summary>
    private string? ResolveStoragePath(string? storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey)) return null;

        // Legacy keys ("/uploads/...") were served from the web root; they no
        // longer exist under the protected root.
        var normalized = storageKey.Replace('\\', '/').TrimStart('/');
        if (normalized.Contains("..")) return null;

        var root = Path.GetFullPath(StorageRoot);
        var candidate = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        return candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            || candidate.Equals(root, StringComparison.OrdinalIgnoreCase)
            ? candidate
            : null;
    }

    private static string ContentTypeFor(string? extension) => extension?.ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var document = await _context.EmployeeDocuments.FindAsync(id);
        if (document == null) return NotFound();

        // Phase 4D — IDOR: only authorized users may delete an employee's document.
        if (!await _scope.CanViewEmployeeAsync(document.EmployeeId))
            return Forbid();

        // Delete physical file (protected storage root).
        var fullPath = ResolveStoragePath(document.FilePath);
        if (fullPath != null && System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        _context.EmployeeDocuments.Remove(document);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
