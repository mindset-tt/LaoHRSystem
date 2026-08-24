using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using LaoHR.API.Services;

namespace LaoHR.API.Controllers;

/// <summary>
/// Phase 4C.1 — Corporate Operations reports + CSV/Excel export.
///
/// Reuses the shared <see cref="IFinanceExportService"/> (formula-injection safe,
/// UTF-8 BOM). Sensitive bulk exports (contracts, visitors, travel) are audited.
/// </summary>
[Authorize]
[ApiController]
[Route("api/corporate/reports")]
public class CorporateReportsController : ControllerBase
{
    private readonly LaoHRDbContext _context;
    private readonly ICorporateOperationsAccessService _access;
    private readonly IFinanceExportService _export;

    public CorporateReportsController(
        LaoHRDbContext context,
        ICorporateOperationsAccessService access,
        IFinanceExportService export)
    {
        _context = context;
        _access = access;
        _export = export;
    }

    // ---- Contract Register ----

    [HttpGet("contracts")]
    public async Task<ActionResult<List<ContractRegisterRow>>> ContractRegister([FromQuery] string? status = null)
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var query = _context.Contracts.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(c => c.Status == status);

        return await query
            .OrderByDescending(c => c.StartDate)
            .Select(c => new ContractRegisterRow
            {
                ContractNumber = c.ContractNumber,
                Title = c.Title,
                SupplierName = c.Supplier != null ? c.Supplier.Name : null,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Amount = c.Amount,
                Status = c.Status,
            })
            .ToListAsync();
    }

    [HttpGet("contracts/export")]
    public async Task<IActionResult> ExportContracts([FromQuery] string? status = null, [FromQuery] string format = "csv")
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var rows = await ContractRegister(status);
        var list = (rows.Result as OkObjectResult)?.Value as List<ContractRegisterRow> ?? new();
        var headers = new[] { "ContractNumber", "Title", "Supplier", "StartDate", "EndDate", "Amount", "Status" };
        var data = list.Select(r => new object?[] { r.ContractNumber, r.Title, r.SupplierName, r.StartDate, r.EndDate, r.Amount, r.Status });

        await RecordExportAuditAsync("CONTRACT_REGISTER");
        return Export(headers, data, "contract-register", format);
    }

    // ---- Document Expiry ----

    [HttpGet("document-expiry")]
    public async Task<ActionResult<List<DocumentExpiryRow>>> DocumentExpiry([FromQuery] DateTime? before = null)
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var query = _context.CorporateDocuments.AsNoTracking()
            .Where(d => d.ExpiryDate != null);
        if (before.HasValue) query = query.Where(d => d.ExpiryDate <= before.Value);

        return await query
            .OrderBy(d => d.ExpiryDate)
            .Select(d => new DocumentExpiryRow
            {
                DocumentNumber = d.DocumentNumber,
                Title = d.Title,
                DocumentType = d.DocumentType,
                ExpiryDate = d.ExpiryDate,
            })
            .ToListAsync();
    }

    [HttpGet("document-expiry/export")]
    public async Task<IActionResult> ExportDocumentExpiry([FromQuery] DateTime? before = null)
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var rows = await DocumentExpiry(before);
        var list = (rows.Result as OkObjectResult)?.Value as List<DocumentExpiryRow> ?? new();
        var headers = new[] { "DocumentNumber", "Title", "Type", "ExpiryDate" };
        var data = list.Select(r => new object?[] { r.DocumentNumber, r.Title, r.DocumentType, r.ExpiryDate });

        await RecordExportAuditAsync("DOCUMENT_EXPIRY");
        return Export(headers, data, "document-expiry", "csv");
    }

    // ---- Service Request Backlog ----

    [HttpGet("service-backlog")]
    public async Task<ActionResult<List<ServiceBacklogRow>>> ServiceBacklog()
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        return await _context.ServiceRequests.AsNoTracking()
            .Where(s => s.Status == "OPEN" || s.Status == "IN_PROGRESS")
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new ServiceBacklogRow
            {
                RequestNumber = s.RequestNumber,
                Subject = s.Subject,
                CategoryName = s.Category != null ? s.Category.Name : null,
                Priority = s.Priority,
                Status = s.Status,
                CreatedAt = s.CreatedAt,
            })
            .ToListAsync();
    }

    [HttpGet("service-backlog/export")]
    public async Task<IActionResult> ExportServiceBacklog([FromQuery] string format = "csv")
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var rows = await ServiceBacklog();
        var list = (rows.Result as OkObjectResult)?.Value as List<ServiceBacklogRow> ?? new();
        var headers = new[] { "RequestNumber", "Subject", "Category", "Priority", "Status", "CreatedAt" };
        var data = list.Select(r => new object?[] { r.RequestNumber, r.Subject, r.CategoryName, r.Priority, r.Status, r.CreatedAt });

        await RecordExportAuditAsync("SERVICE_BACKLOG");
        return Export(headers, data, "service-backlog", format);
    }

    // ---- Fleet Register ----

    [HttpGet("fleet")]
    public async Task<ActionResult<List<FleetRegisterRow>>> FleetRegister()
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        return await _context.Vehicles.AsNoTracking()
            .OrderBy(v => v.VehicleCode)
            .Select(v => new FleetRegisterRow
            {
                VehicleCode = v.VehicleCode,
                RegistrationNumber = v.RegistrationNumber,
                Make = v.Make,
                Model = v.Model,
                Status = v.Status,
                CurrentOdometer = v.CurrentOdometer,
            })
            .ToListAsync();
    }

    [HttpGet("fleet/export")]
    public async Task<IActionResult> ExportFleet([FromQuery] string format = "csv")
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var rows = await FleetRegister();
        var list = (rows.Result as OkObjectResult)?.Value as List<FleetRegisterRow> ?? new();
        var headers = new[] { "VehicleCode", "RegistrationNumber", "Make", "Model", "Status", "Odometer" };
        var data = list.Select(r => new object?[] { r.VehicleCode, r.RegistrationNumber, r.Make, r.Model, r.Status, r.CurrentOdometer });

        await RecordExportAuditAsync("FLEET_REGISTER");
        return Export(headers, data, "fleet-register", format);
    }

    // ---- Travel Register ----

    [HttpGet("travel")]
    public async Task<ActionResult<List<TravelRegisterRow>>> TravelRegister([FromQuery] string? status = null)
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var query = _context.TravelRequests.AsNoTracking().AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TravelRegisterRow
            {
                TravelNumber = t.TravelNumber,
                EmployeeName = t.Employee != null ? (t.Employee.EnglishName ?? t.Employee.LaoName) : null,
                Destination = t.Destination,
                DepartureDate = t.DepartureDate,
                ReturnDate = t.ReturnDate,
                Status = t.Status,
            })
            .ToListAsync();
    }

    [HttpGet("travel/export")]
    public async Task<IActionResult> ExportTravel([FromQuery] string? status = null, [FromQuery] string format = "csv")
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var rows = await TravelRegister(status);
        var list = (rows.Result as OkObjectResult)?.Value as List<TravelRegisterRow> ?? new();
        var headers = new[] { "TravelNumber", "Employee", "Destination", "Departure", "Return", "Status" };
        var data = list.Select(r => new object?[] { r.TravelNumber, r.EmployeeName, r.Destination, r.DepartureDate, r.ReturnDate, r.Status });

        await RecordExportAuditAsync("TRAVEL_REGISTER");
        return Export(headers, data, "travel-register", format);
    }

    // ---- Visitor Log ----

    [HttpGet("visitor-log")]
    public async Task<ActionResult<List<VisitorLogRow>>> VisitorLog()
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        return await _context.Visits.AsNoTracking()
            .OrderByDescending(v => v.ExpectedAt)
            .Select(v => new VisitorLogRow
            {
                VisitorName = v.Visitor != null ? v.Visitor.FullName : null,
                Company = v.Visitor != null ? v.Visitor.Company : null,
                HostName = v.Host != null ? (v.Host.EnglishName ?? v.Host.LaoName) : null,
                ExpectedAt = v.ExpectedAt,
                Status = v.Status,
            })
            .ToListAsync();
    }

    [HttpGet("visitor-log/export")]
    public async Task<IActionResult> ExportVisitorLog()
    {
        if (!_access.CanViewCorporateReports())
            return Forbid();

        var rows = await VisitorLog();
        var list = (rows.Result as OkObjectResult)?.Value as List<VisitorLogRow> ?? new();
        var headers = new[] { "Visitor", "Company", "Host", "ExpectedAt", "Status" };
        var data = list.Select(r => new object?[] { r.VisitorName, r.Company, r.HostName, r.ExpectedAt, r.Status });

        await RecordExportAuditAsync("VISITOR_LOG");
        return Export(headers, data, "visitor-log", "csv");
    }

    // ---- Helpers ----

    private IActionResult Export(string[] headers, IEnumerable<object?[]> data, string name, string format)
    {
        if (string.Equals(format, "xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return File(_export.BuildExcel(name, headers, data),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{name}.xlsx");
        }
        return File(_export.BuildCsv(headers, data), "text/csv", $"{name}.csv");
    }

    private async Task RecordExportAuditAsync(string reportName)
    {
        var username = User.Identity?.Name ?? "SYSTEM";
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = username,
            EntityName = "CORPORATE_EXPORT",
            Action = "EXPORT",
            KeyValues = System.Text.Json.JsonSerializer.Serialize(new { Report = reportName }),
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { Report = reportName, At = DateTime.UtcNow }),
            Timestamp = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();
    }
}

public class ContractRegisterRow
{
    public string ContractNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DocumentExpiryRow
{
    public string DocumentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
}

public class ServiceBacklogRow
{
    public string RequestNumber { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class FleetRegisterRow
{
    public string VehicleCode { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string Status { get; set; } = string.Empty;
    public int CurrentOdometer { get; set; }
}

public class TravelRegisterRow
{
    public string TravelNumber { get; set; } = string.Empty;
    public string? EmployeeName { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime ReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class VisitorLogRow
{
    public string? VisitorName { get; set; }
    public string? Company { get; set; }
    public string? HostName { get; set; }
    public DateTime? ExpectedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
