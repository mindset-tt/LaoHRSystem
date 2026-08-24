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

    public DocumentsController(LaoHRDbContext context, IWebHostEnvironment environment, IDataScopeService scope)
    {
        _context = context;
        _environment = environment;
        _scope = scope;
    }

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

    [HttpPost]
    public async Task<ActionResult<EmployeeDocument>> UploadDocument([FromForm] int employeeId, [FromForm] string documentType, [FromForm] IFormFile file)
    {
        // Phase 4D — IDOR: only authorized users may upload for an employee.
        if (!await _scope.CanViewEmployeeAsync(employeeId))
            return Forbid();

        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Phase 4D — server-side size limit (10 MB).
        const long maxBytes = 10 * 1024 * 1024;
        if (file.Length > maxBytes)
            return BadRequest("File exceeds the 10 MB size limit.");

        // Validate file type (PDF, Images, Word)
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type. Only PDF, Images, and Word documents allowed.");

        // Phase 4D — sanitize the display filename (strip path separators and
        // control characters). The storage name is generated independently.
        var safeDisplayName = SanitizeFileName(file.FileName);

        // Ensure directory exists
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents", employeeId.ToString());
        Directory.CreateDirectory(uploadsFolder);

        // Generate a safe storage filename (never derived from user input).
        var storageName = $"{DateTime.Now.Ticks}_{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsFolder, storageName);
        var relativePath = $"/uploads/documents/{employeeId}/{storageName}";

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentType = documentType,
            FileName = safeDisplayName,
            FilePath = relativePath,
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

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var document = await _context.EmployeeDocuments.FindAsync(id);
        if (document == null) return NotFound();

        // Phase 4D — IDOR: only authorized users may delete an employee's document.
        if (!await _scope.CanViewEmployeeAsync(document.EmployeeId))
            return Forbid();

        // Delete physical file
        var fullPath = Path.Combine(_environment.WebRootPath, document.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        _context.EmployeeDocuments.Remove(document);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
