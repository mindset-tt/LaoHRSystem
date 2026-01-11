using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HolidaysController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public HolidaysController(LaoHRDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Holiday>>> GetHolidays([FromQuery] int? year)
    {
        var query = _context.Holidays.AsQueryable();

        if (year.HasValue)
        {
            query = query.Where(h => h.Year == year.Value);
        }

        return await query.OrderBy(h => h.Date).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Holiday>> GetHoliday(int id)
    {
        var holiday = await _context.Holidays.FindAsync(id);

        if (holiday == null)
        {
            return NotFound();
        }

        return holiday;
    }

    [HttpPost]
    public async Task<ActionResult<Holiday>> CreateHoliday(Holiday holiday)
    {
        _context.Holidays.Add(holiday);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHoliday), new { id = holiday.HolidayId }, holiday);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHoliday(int id, Holiday holiday)
    {
        if (id != holiday.HolidayId)
        {
            return BadRequest();
        }

        _context.Entry(holiday).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!HolidayExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHoliday(int id)
    {
        var holiday = await _context.Holidays.FindAsync(id);
        if (holiday == null)
        {
            return NotFound();
        }

        _context.Holidays.Remove(holiday);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool HolidayExists(int id)
    {
        return _context.Holidays.Any(e => e.HolidayId == id);
    }
}
