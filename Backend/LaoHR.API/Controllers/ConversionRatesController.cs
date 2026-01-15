using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Controllers;

[ApiController]
[Route("api/settings/conversion-rates")]
public class ConversionRatesController : ControllerBase
{
    private readonly LaoHRDbContext _context;

    public ConversionRatesController(LaoHRDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all conversion rates
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ConversionRate>>> GetAll()
    {
        return await _context.ConversionRates
            .OrderBy(r => r.FromCurrency)
            .ThenByDescending(r => r.EffectiveDate)
            .ToListAsync();
    }

    /// <summary>
    /// Get current active rates
    /// </summary>
    [HttpGet("current")]
    public async Task<ActionResult<List<ConversionRate>>> GetCurrentRates()
    {
        var now = DateTime.UtcNow;
        return await _context.ConversionRates
            .Where(r => r.IsActive && 
                        r.EffectiveDate <= now && 
                        (r.ExpiryDate == null || r.ExpiryDate > now))
            .OrderBy(r => r.FromCurrency)
            .ToListAsync();
    }

    /// <summary>
    /// Get rate for specific currency
    /// </summary>
    [HttpGet("{fromCurrency}/to/{toCurrency}")]
    public async Task<ActionResult<ConversionRate>> GetRate(string fromCurrency, string toCurrency)
    {
        var now = DateTime.UtcNow;
        var rate = await _context.ConversionRates
            .Where(r => r.FromCurrency == fromCurrency && 
                        r.ToCurrency == toCurrency && 
                        r.IsActive &&
                        r.EffectiveDate <= now &&
                        (r.ExpiryDate == null || r.ExpiryDate > now))
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();

        if (rate == null)
            return NotFound();

        return rate;
    }

    /// <summary>
    /// Create new conversion rate
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConversionRate>> Create([FromBody] CreateConversionRateDto dto)
    {
        // Expire any existing active rate for this currency pair
        var existingRates = await _context.ConversionRates
            .Where(r => r.FromCurrency == dto.FromCurrency && 
                        r.ToCurrency == dto.ToCurrency && 
                        r.IsActive &&
                        r.ExpiryDate == null)
            .ToListAsync();

        foreach (var existingRate in existingRates)
        {
            existingRate.ExpiryDate = dto.EffectiveDate;
            existingRate.UpdatedAt = DateTime.UtcNow;
        }

        var rate = new ConversionRate
        {
            FromCurrency = dto.FromCurrency,
            ToCurrency = dto.ToCurrency,
            Rate = dto.Rate,
            EffectiveDate = dto.EffectiveDate,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.ConversionRates.Add(rate);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetRate), new { fromCurrency = rate.FromCurrency, toCurrency = rate.ToCurrency }, rate);
    }

    /// <summary>
    /// Update conversion rate
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ConversionRate>> Update(int id, [FromBody] UpdateConversionRateDto dto)
    {
        var rate = await _context.ConversionRates.FindAsync(id);
        if (rate == null)
            return NotFound();

        rate.FromCurrency = dto.FromCurrency;
        rate.ToCurrency = dto.ToCurrency;
        rate.Rate = dto.Rate;
        rate.EffectiveDate = dto.EffectiveDate;
        rate.Notes = dto.Notes;
        rate.IsActive = dto.IsActive;
        rate.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return rate;
    }

    /// <summary>
    /// Delete conversion rate
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var rate = await _context.ConversionRates.FindAsync(id);
        if (rate == null)
            return NotFound();

        _context.ConversionRates.Remove(rate);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

public class CreateConversionRateDto
{
    public string FromCurrency { get; set; } = "USD";
    public string ToCurrency { get; set; } = "LAK";
    public decimal Rate { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public class UpdateConversionRateDto : CreateConversionRateDto
{
    public bool IsActive { get; set; } = true;
}
