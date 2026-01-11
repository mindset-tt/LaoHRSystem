using System.Text;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

public interface IBankTransferService
{
    Task<byte[]> GenerateBcelTransferFile(int periodId);
    Task<byte[]> GenerateLdbTransferFile(int periodId);
}

public class BankTransferService : IBankTransferService
{
    private readonly LaoHRDbContext _context;

    public BankTransferService(LaoHRDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Generates BCEL transfer text file (Simulated Format)
    /// H,YYYYMMDD,COMPANY_ACCOUNT,TOTAL_RECORDS,TOTAL_AMOUNT
    /// D,ACCOUNT_NO,AMOUNT,CURRENCY,REF
    /// T,TOTAL_RECORDS,TOTAL_AMOUNT
    /// </summary>
    public async Task<byte[]> GenerateBcelTransferFile(int periodId)
    {
        var slips = await GetSalarySlips(periodId);
        var sb = new StringBuilder();
        var dateStr = DateTime.Now.ToString("yyyyMMdd");
        var companyAcc = "1011111111111"; // Mock company account

        decimal totalAmount = slips.Sum(s => s.NetSalary);
        int totalRecords = slips.Count;

        // Header
        sb.AppendLine($"H,{dateStr},{companyAcc},{totalRecords},{totalAmount:F2}");

        // Details
        foreach (var slip in slips)
        {
            var accountNo = slip.Employee?.BankAccount ?? "0000000000";
            var amount = slip.NetSalary;
            var currency = "LAK";
            var refNo = $"SAL{slip.PayrollPeriod.Month:D2}{slip.PayrollPeriod.Year}";

            sb.AppendLine($"D,{accountNo},{amount:F2},{currency},{refNo}");
        }

        // Trailer
        sb.AppendLine($"T,{totalRecords},{totalAmount:F2}");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    /// <summary>
    /// Generates LDB transfer CSV file (Simulated Format)
    /// Account No,Account Name,Amount,Currency,Description
    /// </summary>
    public async Task<byte[]> GenerateLdbTransferFile(int periodId)
    {
        var slips = await GetSalarySlips(periodId);
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("Account No,Account Name,Amount,Currency,Description");

        foreach (var slip in slips)
        {
            var accountNo = slip.Employee?.BankAccount ?? "0000000000";
            var name = slip.Employee?.EnglishName?.ToUpper() ?? "UNKNOWN";
            var amount = slip.NetSalary;
            var desc = $"Salary {slip.PayrollPeriod.Month}/{slip.PayrollPeriod.Year}";

            sb.AppendLine($"{accountNo},{name},{amount:F2},LAK,{desc}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private async Task<List<SalarySlip>> GetSalarySlips(int periodId)
    {
        return await _context.SalarySlips
            .Include(s => s.Employee)
            .Include(s => s.PayrollPeriod)
            .Where(s => s.PeriodId == periodId)
            .OrderBy(s => s.Employee.EmployeeCode)
            .ToListAsync();
    }
}
