using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace LaoHR.API.Services;

public interface ICompanySettingsService
{
    Task<CompanySetting?> GetSettingsAsync();
    Task<CompanySetting> UpdateSettingsAsync(CompanySetting settings);
}

public class CompanySettingsService : ICompanySettingsService
{
    private readonly LaoHRDbContext _context;

    public CompanySettingsService(LaoHRDbContext context)
    {
        _context = context;
    }

    public async Task<CompanySetting?> GetSettingsAsync()
    {
        return await _context.CompanySettings
            .Include(c => c.Province)
            .Include(c => c.District)
            .Include(c => c.Village)
            .FirstOrDefaultAsync();
    }

    public async Task<CompanySetting> UpdateSettingsAsync(CompanySetting settings)
    {
        var existing = await _context.CompanySettings.FirstOrDefaultAsync();
        if (existing == null)
        {
            _context.CompanySettings.Add(settings);
        }
        else
        {
            existing.CompanyNameLao = settings.CompanyNameLao;
            existing.CompanyNameEn = settings.CompanyNameEn;
            existing.LSSOCode = settings.LSSOCode;
            existing.TaxRisId = settings.TaxRisId;
            existing.BankAccountNo = settings.BankAccountNo;
            existing.BankName = settings.BankName;
            existing.Tel = settings.Tel;
            existing.Phone = settings.Phone;
            existing.Email = settings.Email;
            existing.ProvinceId = settings.ProvinceId;
            existing.DistrictId = settings.DistrictId;
            existing.VillageId = settings.VillageId;
            existing.UpdatedAt = DateTime.UtcNow;
            
            _context.CompanySettings.Update(existing);
        }

        await _context.SaveChangesAsync();
        return existing ?? settings;
    }
}
