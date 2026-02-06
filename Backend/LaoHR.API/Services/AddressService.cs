using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Services;

public interface IAddressService
{
    Task<List<Province>> GetProvincesAsync();
    Task<List<District>> GetDistrictsAsync(int provinceId);
    Task<List<Village>> GetVillagesAsync(int districtId);
}

public class AddressService : IAddressService
{
    private readonly LaoHRDbContext _context;

    public AddressService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<List<Province>> GetProvincesAsync()
    {
        return await _context.Provinces
            .OrderBy(p => p.PrId) // Or NameEn?
            .ToListAsync();
    }

    public async Task<List<District>> GetDistrictsAsync(int provinceId)
    {
        return await _context.Districts
            .Where(d => d.PrId == provinceId)
            .OrderBy(d => d.DiNameEn)
            .ToListAsync();
    }

    public async Task<List<Village>> GetVillagesAsync(int districtId)
    {
        return await _context.Villages
            .Where(v => v.DiId == districtId)
            .OrderBy(v => v.VillNameEn)
            .ToListAsync();
    }
}
