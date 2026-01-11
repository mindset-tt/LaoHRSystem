using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DocumentsController(LaoHRDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpGet("employee/{employeeId}")]
    public async Task<ActionResult<IEnumerable<EmployeeDocument>>> GetEmployeeDocuments(int employeeId)
    {
        return await _context.EmployeeDocuments
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDocument>> UploadDocument([FromForm] int employeeId, [FromForm] string documentType, [FromForm] IFormFile file)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound("Employee not found");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Validate file type (PDF, Images, Word)
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type. Only PDF, Images, and Word documents allowed.");

        // Ensure directory exists
        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "documents", employeeId.ToString());
        Directory.CreateDirectory(uploadsFolder);

        // Generate filename
        var fileName = $"{DateTime.Now.Ticks}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, fileName);
        var relativePath = $"/uploads/documents/{employeeId}/{fileName}";

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var document = new EmployeeDocument
        {
            EmployeeId = employeeId,
            DocumentType = documentType,
            FileName = file.FileName,
            FilePath = relativePath,
            UploadedAt = DateTime.UtcNow
        };

        _context.EmployeeDocuments.Add(document);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEmployeeDocuments), new { employeeId = employeeId }, document);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDocument(int id)
    {
        var document = await _context.EmployeeDocuments.FindAsync(id);
        if (document == null) return NotFound();

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
