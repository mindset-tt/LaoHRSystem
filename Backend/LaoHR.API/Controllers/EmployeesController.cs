using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LaoHR.API.Data;
using LaoHR.API.Services;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;

namespace LaoHR.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IOrganizationHierarchyService _hierarchy;
    private readonly IDataScopeService _scope;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public EmployeesController(LaoHRDbContext context, IOrganizationHierarchyService hierarchy, IDataScopeService scope, IWebHostEnvironment environment, IConfiguration configuration)
    {
        _context = context;
        _hierarchy = hierarchy;
        _scope = scope;
        _environment = environment;
        _configuration = configuration;
    }

    /// <summary>
    /// Get employees (paged + projected). Use the filters to narrow the result
    /// set server-side rather than paging through everything client-side.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<EmployeeListItem>>> GetEmployees(
        [FromQuery] bool? isActive = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(e => e.IsActive == isActive.Value);

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                e.LaoName.Contains(term) ||
                (e.EnglishName != null && e.EnglishName.Contains(term)) ||
                e.EmployeeCode.Contains(term));
        }

        var total = await query.LongCountAsync();

        var items = await query
            .OrderBy(e => e.EmployeeCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeeListItem
            {
                EmployeeId = e.EmployeeId,
                EmployeeCode = e.EmployeeCode,
                LaoName = e.LaoName,
                EnglishName = e.EnglishName,
                Email = e.Email,
                Phone = e.Phone,
                JobTitle = e.JobTitle,
                IsActive = e.IsActive,
                DepartmentId = e.DepartmentId,
                DepartmentName = e.Department != null ? e.Department.DepartmentName : null
            })
            .ToListAsync();

        return new PaginatedResponse<EmployeeListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }
    
    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeId == id);
        
        if (employee == null) return NotFound();
        return employee;
    }

    /// <summary>
    /// Phase 3C2 ESS — get the current user's own employee profile.
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<Employee>> GetMyProfile()
    {
        var empIdStr = User.FindFirst("EmployeeId")?.Value;
        if (!int.TryParse(empIdStr, out var empId))
            return Unauthorized("No linked employee profile.");

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.WorkLocation)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.EmployeeId == empId);

        if (employee == null) return NotFound();
        return employee;
    }
    
    /// <summary>
    /// Create new employee
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee([FromBody] CreateEmployeeDto request)
    {
        // Generate employee code if not provided
        var code = request.EmployeeCode;
        if (string.IsNullOrWhiteSpace(code))
        {
            var count = await _context.Employees.CountAsync() + 1;
            code = $"EMP{count:D4}";
        }
        
        var employee = new Employee
        {
            EmployeeCode = code,
            LaoName = request.LaoName,
            EnglishName = request.EnglishName,
            NssfId = request.NssfId,
            TaxId = request.TaxId,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Phone = request.Phone,
            Email = request.Email,
            DependentCount = request.DependentCount,
            SalaryCurrency = request.SalaryCurrency ?? "LAK",
            DepartmentId = request.DepartmentId,
            JobTitle = request.JobTitle,
            HireDate = request.HireDate,
            BaseSalary = request.BaseSalary,
            BankName = request.BankName,
            BankAccount = request.BankAccount,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
    }
    
    // ... UpdateEmployee ...
    
    // ... existing actions ...



// DTOs
public class CreateEmployeeDto
{
    public string? EmployeeCode { get; set; }
    
    [System.ComponentModel.DataAnnotations.Required]
    public string LaoName { get; set; } = string.Empty;
    
    public string? EnglishName { get; set; }
    public string? NssfId { get; set; }
    public string? TaxId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public int DependentCount { get; set; }
    public string? SalaryCurrency { get; set; }
    public int? DepartmentId { get; set; }
    public string? JobTitle { get; set; }
    public int? ManagerId { get; set; }
    public int? PositionId { get; set; }
    public int? WorkLocationId { get; set; }
    public DateTime? HireDate { get; set; }
    public decimal BaseSalary { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
}
    
    /// <summary>
    /// Update employee
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] CreateEmployeeDto request)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();
        
        // Update fields
        employee.LaoName = request.LaoName;
        employee.EnglishName = request.EnglishName;
        employee.NssfId = request.NssfId;
        employee.TaxId = request.TaxId;
        employee.DateOfBirth = request.DateOfBirth;
        employee.Gender = request.Gender;
        employee.Phone = request.Phone;
        employee.Email = request.Email;
        employee.DependentCount = request.DependentCount;
        employee.SalaryCurrency = request.SalaryCurrency ?? "LAK";
        employee.DepartmentId = request.DepartmentId;
        employee.JobTitle = request.JobTitle;
        employee.HireDate = request.HireDate;
        employee.BaseSalary = request.BaseSalary;
        employee.BankName = request.BankName;
        employee.BankAccount = request.BankAccount;

        // Phase 3C1 — organizational assignment with cycle prevention.
        if (request.ManagerId.HasValue)
        {
            if (request.ManagerId.Value == id)
                return BadRequest("An employee cannot be their own manager.");
            var wouldCycle = await _hierarchy.WouldCreateReportingCycleAsync(id, request.ManagerId);
            if (wouldCycle)
                return BadRequest("Assigning this manager would create a reporting cycle.");
        }
        employee.ManagerId = request.ManagerId;
        employee.PositionId = request.PositionId;
        employee.WorkLocationId = request.WorkLocationId;
        
        employee.UpdatedAt = DateTime.UtcNow;
        // EmployeeCode is generally not updated, but could be if needed. keeping it as is for now.
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Employees.AnyAsync(e => e.EmployeeId == id))
                return NotFound();
            throw;
        }
        
        return NoContent();
    }
    
    /// <summary>
    /// Delete (deactivate) employee
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();
        
        // Soft delete
        employee.IsActive = false;
        employee.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Upload employee profile photo
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("{id}/photo")]
    public async Task<IActionResult> UploadProfilePhoto(int id, IFormFile file)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Phase 4D — server-side size limit (5 MB for photos).
        const long maxBytes = 5 * 1024 * 1024;
        if (file.Length > maxBytes)
            return RejectUpload("File exceeds the 5 MB size limit.", "too_large");

        // Validate declared file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return RejectUpload("Invalid file type. Only JPG and PNG allowed.", "extension_denied");

        // Phase 4D.1 — content-signature validation before writing to storage.
        await using var content = file.OpenReadStream();
        if (!FileSignatureValidator.ValidateOrRecord(extension, content))
            return RejectUpload("File content does not match its declared type.");

        // Phase 4D.1 — store OUTSIDE the web root; photos are served through
        // the authorized GET {id}/photo endpoint (static files bypass authz).
        var uploadsFolder = Path.Combine(
            _configuration["Storage:DocumentsRoot"] ?? Path.Combine(_environment.ContentRootPath, "App_Data", "uploads"),
            "profiles");
        Directory.CreateDirectory(uploadsFolder);

        // Generate unique filename (never derived from user input beyond the
        // validated extension).
        var uniqueFileName = $"{employee.EmployeeCode}_{DateTime.Now.Ticks}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        try
        {
            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update employee record with the storage key (opaque reference).
            employee.ProfilePath = $"profiles/{uniqueFileName}";
            employee.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { path = $"/api/employees/{id}/photo" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    /// <summary>
    /// Phase 4D.1 — authorized photo download from the protected storage root.
    /// </summary>
    [HttpGet("{id}/photo")]
    public async Task<IActionResult> GetProfilePhoto(int id)
    {
        var employee = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeId == id);
        if (employee == null) return NotFound();

        // IDOR: same visibility policy as the employee read path.
        if (!await _scope.CanViewEmployeeAsync(id))
            return Forbid();

        if (string.IsNullOrWhiteSpace(employee.ProfilePath)) return NotFound();

        var normalized = employee.ProfilePath.Replace('\\', '/').TrimStart('/');
        if (normalized.Contains("..") || normalized.StartsWith("/uploads/")) return NotFound();

        var root = Path.GetFullPath(
            _configuration["Storage:DocumentsRoot"] ?? Path.Combine(_environment.ContentRootPath, "App_Data", "uploads"));
        var candidate = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
        if (!candidate.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return NotFound();
        if (!System.IO.File.Exists(candidate)) return NotFound();

        var contentType = Path.GetExtension(candidate).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };
        return PhysicalFile(candidate, contentType);
    }

    private BadRequestObjectResult RejectUpload(string message, string reason = "signature_mismatch")
    {
        Metrics.AppMetrics.UploadsRejected.Add(1, new KeyValuePair<string, object?>("reason", reason));
        return BadRequest(message);
    }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IOrganizationHierarchyService _hierarchy;
    
    public DepartmentsController(LaoHRDbContext context, IOrganizationHierarchyService hierarchy)
    {
        _context = context;
        _hierarchy = hierarchy;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Department>>> GetDepartments()
    {
        return await _context.Departments
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentCode)
            .ToListAsync();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Department>> GetDepartment(int id)
    {
        var dept = await _context.Departments.FindAsync(id);
        if (dept == null) return NotFound();
        return dept;
    }
    
    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<ActionResult<Department>> CreateDepartment(Department department)
    {
        // Phase 3C1 — validate parent does not create a cycle.
        if (department.ParentDepartmentId.HasValue)
        {
            var parentExists = await _context.Departments.AnyAsync(d => d.DepartmentId == department.ParentDepartmentId.Value);
            if (!parentExists)
                return BadRequest("Parent department does not exist.");
        }

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, department);
    }

    [Authorize(Roles = "Admin,HR")]
    [HttpPut("{id}")]
    public async Task<ActionResult<Department>> UpdateDepartment(int id, Department department)
    {
        var existing = await _context.Departments.FindAsync(id);
        if (existing == null) return NotFound();

        // Phase 3C1 — cycle prevention on reparenting.
        if (department.ParentDepartmentId.HasValue)
        {
            if (department.ParentDepartmentId.Value == id)
                return BadRequest("A department cannot be its own parent.");

            var wouldCycle = await _hierarchy.WouldCreateDepartmentCycleAsync(id, department.ParentDepartmentId);
            if (wouldCycle)
                return BadRequest("Reparenting would create a department hierarchy cycle.");
        }

        existing.DepartmentName = department.DepartmentName;
        existing.DepartmentNameEn = department.DepartmentNameEn;
        existing.DepartmentCode = department.DepartmentCode;
        existing.ParentDepartmentId = department.ParentDepartmentId;
        existing.ManagerEmployeeId = department.ManagerEmployeeId;
        existing.SortOrder = department.SortOrder;
        existing.IsActive = department.IsActive;

        await _context.SaveChangesAsync();
        return existing;
    }
}

public sealed class EmployeeListItem
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string LaoName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
}
