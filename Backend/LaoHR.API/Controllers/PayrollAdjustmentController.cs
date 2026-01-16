using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaoHR.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PayrollAdjustmentController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public PayrollAdjustmentController(LaoHRDbContext context)
    {
        _context = context;
    }

    // GET: api/PayrollAdjustment?periodId=1&employeeId=5
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PayrollAdjustment>>> GetAdjustments([FromQuery] int periodId, [FromQuery] int? employeeId)
    {
        var query = _context.PayrollAdjustments.AsQueryable();

        query = query.Where(a => a.PeriodId == periodId);

        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        return await query.OrderBy(a => a.Employee!.EmployeeCode).ThenBy(a => a.Type).ToListAsync();
    }

    // POST: api/PayrollAdjustment
    [HttpPost]
    public async Task<ActionResult<PayrollAdjustment>> CreateAdjustment(PayrollAdjustment adjustment)
    {
        // Verify period exists and is not locked
        var period = await _context.PayrollPeriods.FindAsync(adjustment.PeriodId);
        if (period == null) return NotFound("Period not found");
        if (period.Status == "APPROVED" || period.Status == "LOCKED" || period.Status == "COMPLETED")
            return BadRequest("Cannot add adjustments to a locked/approved period.");

        _context.PayrollAdjustments.Add(adjustment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAdjustments), new { id = adjustment.AdjustmentId }, adjustment);
    }

    // DELETE: api/PayrollAdjustment/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAdjustment(int id)
    {
        var adjustment = await _context.PayrollAdjustments.FindAsync(id);
        if (adjustment == null) return NotFound();

        // Verify period
        var period = await _context.PayrollPeriods.FindAsync(adjustment.PeriodId);
        if (period != null && (period.Status == "APPROVED" || period.Status == "LOCKED"))
            return BadRequest("Cannot delete adjustments from a locked/approved period.");

        _context.PayrollAdjustments.Remove(adjustment);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
