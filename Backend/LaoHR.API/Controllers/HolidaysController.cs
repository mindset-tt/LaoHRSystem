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

    /// <summary>
    /// Get all holidays, optionally filtered by year
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Holiday>>> GetHolidays([FromQuery] int? year = null)
    {
        var query = _context.Holidays.Where(h => h.IsActive);
        
        if (year.HasValue)
        {
            query = query.Where(h => 
                h.Date.Year == year.Value || h.IsRecurring);
        }
        
        return await query.OrderBy(h => h.Date.Month).ThenBy(h => h.Date.Day).ToListAsync();
    }

    /// <summary>
    /// Get a specific holiday by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Holiday>> GetHoliday(int id)
    {
        var holiday = await _context.Holidays.FindAsync(id);
        if (holiday == null) return NotFound();
        return holiday;
    }

    /// <summary>
    /// Create a new holiday
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Holiday>> CreateHoliday([FromBody] CreateHolidayDto dto)
    {
        var holiday = new Holiday
        {
            Date = dto.Date,
            Name = dto.Name,
            NameLao = dto.NameLao,
            Description = dto.Description,
            Year = dto.Date.Year,
            IsRecurring = dto.IsRecurring,
            IsActive = true
        };
        
        _context.Holidays.Add(holiday);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetHoliday), new { id = holiday.HolidayId }, holiday);
    }

    /// <summary>
    /// Update a holiday
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHoliday(int id, [FromBody] UpdateHolidayDto dto)
    {
        var holiday = await _context.Holidays.FindAsync(id);
        if (holiday == null) return NotFound();
        
        holiday.Date = dto.Date;
        holiday.Name = dto.Name;
        holiday.NameLao = dto.NameLao;
        holiday.Description = dto.Description;
        holiday.Year = dto.Date.Year;
        holiday.IsRecurring = dto.IsRecurring;
        holiday.IsActive = dto.IsActive;
        holiday.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return Ok(holiday);
    }

    /// <summary>
    /// Delete a holiday (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHoliday(int id)
    {
        var holiday = await _context.Holidays.FindAsync(id);
        if (holiday == null) return NotFound();
        
        holiday.IsActive = false;
        holiday.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    /// <summary>
    /// Seed default Laos holidays
    /// </summary>
    [HttpPost("seed-defaults")]
    public async Task<ActionResult> SeedDefaults()
    {
        var defaultHolidays = new List<Holiday>
        {
            new() { Date = new DateTime(2026, 1, 1), Name = "New Year's Day", NameLao = "ວັນປີໃໝ່ສາກົນ", Year = 2026, IsRecurring = true },
            new() { Date = new DateTime(2026, 3, 8), Name = "International Women's Day", NameLao = "ວັນແມ່ຍິງສາກົນ", Year = 2026, IsRecurring = true },
            new() { Date = new DateTime(2026, 4, 14), Name = "Lao New Year (Day 1)", NameLao = "ບຸນປີໃໝ່ລາວ", Year = 2026, IsRecurring = true },
            new() { Date = new DateTime(2026, 4, 15), Name = "Lao New Year (Day 2)", NameLao = "ບຸນປີໃໝ່ລາວ", Year = 2026, IsRecurring = true },
            new() { Date = new DateTime(2026, 4, 16), Name = "Lao New Year (Day 3)", NameLao = "ບຸນປີໃໝ່ລາວ", Year = 2026, IsRecurring = true },
            new() { Date = new DateTime(2026, 5, 1), Name = "Labor Day", NameLao = "ວັນກຳມະກອນສາກົນ", Year = 2026, IsRecurring = true },
            new() { Date = new DateTime(2026, 12, 2), Name = "National Day", NameLao = "ວັນຊາດ", Year = 2026, IsRecurring = true },
        };

        foreach (var holiday in defaultHolidays)
        {
            var exists = await _context.Holidays.AnyAsync(h => 
                h.Date.Month == holiday.Date.Month && 
                h.Date.Day == holiday.Date.Day &&
                h.Name == holiday.Name);
            
            if (!exists)
            {
                _context.Holidays.Add(holiday);
            }
        }
        
        await _context.SaveChangesAsync();
        
        return Ok(new { message = "Default holidays seeded successfully" });
    }
}

public class CreateHolidayDto
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public string? Description { get; set; }
    public bool IsRecurring { get; set; } = true;
}

public class UpdateHolidayDto
{
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameLao { get; set; }
    public string? Description { get; set; }
    public bool IsRecurring { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
