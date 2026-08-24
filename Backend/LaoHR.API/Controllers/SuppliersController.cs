using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.Shared.Pagination;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

public class SupplierListItem
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Country { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SupplierDetail
{
    public int SupplierId { get; set; }
    public string SupplierCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? PaymentTerms { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSupplierRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? PaymentTerms { get; set; }
}

public class UpdateSupplierRequest
{
    public string? Name { get; set; }
    public string? LegalName { get; set; }
    public string? TaxId { get; set; }
    public string? RegistrationNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public int? ProvinceId { get; set; }
    public int? DistrictId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccount { get; set; }
    public string? PaymentTerms { get; set; }
    public string? Status { get; set; }
}

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly IBackOfficeAccessService _access;
    private readonly INumberSequenceService _numbers;

    public SuppliersController(
        LaoHRDbContext context,
        IBackOfficeAccessService access,
        INumberSequenceService numbers)
    {
        _context = context;
        _access = access;
        _numbers = numbers;
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<SupplierListItem>>> GetSuppliers(
        [FromQuery] string? status = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        if (!_access.CanViewProcurement())
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, PaginatedQuery.MaxPageSize);

        var query = _context.Suppliers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(s => s.Status == status);
        if (!string.IsNullOrEmpty(search))
        {
            var s = search.ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(s) ||
                x.SupplierCode.ToLower().Contains(s) ||
                (x.LegalName != null && x.LegalName.ToLower().Contains(s)));
        }

        var total = await query.LongCountAsync();
        var items = await query
            .OrderBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SupplierListItem
            {
                SupplierId = s.SupplierId,
                SupplierCode = s.SupplierCode,
                Name = s.Name,
                LegalName = s.LegalName,
                Phone = s.Phone,
                Email = s.Email,
                Country = s.Country,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
            })
            .ToListAsync();

        return new PaginatedResponse<SupplierListItem>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierDetail>> GetSupplier(int id)
    {
        if (!_access.CanViewProcurement())
            return Forbid();

        var s = await _context.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.SupplierId == id);
        if (s == null) return NotFound();

        // Bank + tax fields are sensitive: only Admin (Finance) may see them.
        // HR does NOT automatically gain supplier banking access.
        var canSeeSensitive = _access.CanViewSupplierSensitive();

        return new SupplierDetail
        {
            SupplierId = s.SupplierId,
            SupplierCode = s.SupplierCode,
            Name = s.Name,
            LegalName = s.LegalName,
            TaxId = canSeeSensitive ? s.TaxId : null,
            RegistrationNumber = canSeeSensitive ? s.RegistrationNumber : null,
            Phone = s.Phone,
            Email = s.Email,
            Address = s.Address,
            Country = s.Country,
            ProvinceId = s.ProvinceId,
            DistrictId = s.DistrictId,
            BankName = canSeeSensitive ? s.BankName : null,
            BankAccount = canSeeSensitive ? s.BankAccount : null,
            PaymentTerms = s.PaymentTerms,
            Status = s.Status,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt,
        };
    }

    [HttpPost]
    public async Task<ActionResult<Supplier>> CreateSupplier([FromBody] CreateSupplierRequest request)
    {
        if (!_access.CanManageProcurement())
            return Forbid();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var code = await _numbers.NextAsync("SUP");

        var supplier = new Supplier
        {
            SupplierCode = code,
            Name = request.Name,
            LegalName = request.LegalName,
            TaxId = request.TaxId,
            RegistrationNumber = request.RegistrationNumber,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            Country = request.Country,
            ProvinceId = request.ProvinceId,
            DistrictId = request.DistrictId,
            BankName = request.BankName,
            BankAccount = request.BankAccount,
            PaymentTerms = request.PaymentTerms,
            Status = "ACTIVE",
        };
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetSupplier), new { id = supplier.SupplierId }, supplier);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
    {
        if (!_access.CanManageProcurement())
            return Forbid();

        var s = await _context.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == id);
        if (s == null) return NotFound();

        if (request.Name != null) s.Name = request.Name;
        if (request.LegalName != null) s.LegalName = request.LegalName;
        if (request.TaxId != null) s.TaxId = request.TaxId;
        if (request.RegistrationNumber != null) s.RegistrationNumber = request.RegistrationNumber;
        if (request.Phone != null) s.Phone = request.Phone;
        if (request.Email != null) s.Email = request.Email;
        if (request.Address != null) s.Address = request.Address;
        if (request.Country != null) s.Country = request.Country;
        if (request.ProvinceId.HasValue) s.ProvinceId = request.ProvinceId;
        if (request.DistrictId.HasValue) s.DistrictId = request.DistrictId;
        if (request.BankName != null) s.BankName = request.BankName;
        if (request.BankAccount != null) s.BankAccount = request.BankAccount;
        if (request.PaymentTerms != null) s.PaymentTerms = request.PaymentTerms;
        if (request.Status != null)
        {
            if (request.Status is not ("ACTIVE" or "INACTIVE" or "BLOCKED"))
                return BadRequest("Invalid status.");
            s.Status = request.Status;
        }

        s.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // Soft-delete via status (never physically delete a supplier with history).
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateSupplier(int id)
    {
        if (!_access.CanManageProcurement())
            return Forbid();

        var s = await _context.Suppliers.FirstOrDefaultAsync(x => x.SupplierId == id);
        if (s == null) return NotFound();

        var hasOrders = await _context.PurchaseOrders.AnyAsync(p => p.SupplierId == id);
        if (hasOrders)
            return BadRequest("Supplier has purchase orders; set status to INACTIVE/BLOCKED instead of deleting.");

        s.Status = "INACTIVE";
        s.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
