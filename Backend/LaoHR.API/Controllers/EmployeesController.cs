using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LaoHR.API.Data;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    
    public EmployeesController(LaoHRDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Get all employees
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees(
        [FromQuery] bool? isActive = null,
        [FromQuery] int? departmentId = null,
        [FromQuery] string? search = null)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .AsQueryable();
        
        if (isActive.HasValue)
            query = query.Where(e => e.IsActive == isActive.Value);
        
        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);
        
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(e => 
                e.LaoName.Contains(search) || 
                (e.EnglishName != null && e.EnglishName.Contains(search)) ||
                e.EmployeeCode.Contains(search));
        
        return await query.OrderBy(e => e.EmployeeCode).ToListAsync();
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
    /// Create new employee
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
    {
        // Generate employee code if not provided
        if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
        {
            var count = await _context.Employees.CountAsync() + 1;
            employee.EmployeeCode = $"EMP{count:D4}";
        }
        
        employee.CreatedAt = DateTime.UtcNow;
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeId }, employee);
    }
    
    /// <summary>
    /// Update employee
    /// </summary>
    [Authorize(Roles = "Admin,HR")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, Employee employee)
    {
        if (id != employee.EmployeeId) return BadRequest();
        
        employee.UpdatedAt = DateTime.UtcNow;
        _context.Entry(employee).State = EntityState.Modified;
        
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

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest("Invalid file type. Only JPG and PNG allowed.");

        // Ensure directory exists
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
        Directory.CreateDirectory(uploadsFolder);

        // Generate unique filename
        var uniqueFileName = $"{employee.EmployeeCode}_{DateTime.Now.Ticks}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        try 
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update employee record
            // Delete old photo if exists/needed (optional)
            
            // Store relative path for frontend access
            employee.ProfilePath = $"/uploads/profiles/{uniqueFileName}";
            employee.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();

            return Ok(new { path = employee.ProfilePath });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    
    public DepartmentsController(LaoHRDbContext context)
    {
        _context = context;
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
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDepartment), new { id = department.DepartmentId }, department);
    }
}
