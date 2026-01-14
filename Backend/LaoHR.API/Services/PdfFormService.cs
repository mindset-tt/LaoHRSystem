using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using LaoHR.Shared.Data;
using iText.IO.Font;
using iText.Kernel.Font;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

public class PdfFormService
{
    private readonly LaoHRDbContext _context;
    // Path to the template
    private readonly string _templatePath;
    private readonly string _fontPath;
    public PdfFormService(LaoHRDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _templatePath = Path.Combine(environment.ContentRootPath, "PDF", "LSSO_Payment_Form.pdf");
        _fontPath = Path.Combine(environment.ContentRootPath, "Font", "Phetsarath_OT.ttf");
    }

    public async Task<byte[]> FillNssfForm(int periodId)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId);
        if (period == null) throw new FileNotFoundException("Period not found");

        var slips = await _context.SalarySlips
             .Include(s => s.Employee)
             .Where(s => s.PeriodId == periodId)
             .ToListAsync();

        var totalBase = slips.Sum(s => s.NssfBase);
        var totalEmp = slips.Sum(s => s.NssfEmployeeDeduction);
        var totalEr = slips.Sum(s => s.NssfEmployerContribution);
        var totalAll = totalEmp + totalEr;
        
        // Fetch Company Settings
        var company = await _context.CompanySettings
            .Include(c => c.Village)
            .Include(c => c.District)
            .Include(c => c.Province)
            .FirstOrDefaultAsync();

        if (company == null) company = new CompanySetting(); // Fallback to empty if not set

        using var outputStream = new MemoryStream();
        
        using var reader = new PdfReader(_templatePath);
        using var writer = new PdfWriter(outputStream);
        using var pdfDoc = new PdfDocument(reader, writer);
        
        // Get the first page
        var page = pdfDoc.GetPage(1);
        var canvas = new PdfCanvas(page);
        
        FontProgram fontProgram = FontProgramFactory.CreateFont(_fontPath);

        // Create font with embedding (Strategy: Parse file, then create font)
        // var font = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H, PdfFontFactory.EmbeddingStrategy.FORCE_EMBEDDED);
        // For iText 7.x, the signature is typically (program, encoding, embedded)
        // If bool fails, let's use the explicit embedding strategy if available or just string/string.
        
        var font = PdfFontFactory.CreateFont(_fontPath, PdfEncodings.IDENTITY_H);
        
        canvas.SetFontAndSize(font, 10);
        canvas.SetFillColor(iText.Kernel.Colors.ColorConstants.BLACK);

        // Coordinate System: 0,0 is Bottom-Left
        // A4 is roughly 595 x 842 points
        
        // --- Header Section ---
        // Top Right: "Period: Month ... Year ..."
        DrawText(canvas, $"{period.Month:D2}", 250, 567); // Month (01, 02...)
        DrawText(canvas, $"{period.Year}", 400, 567);  // Year

        // --- Table Totals ---
        // Row 1: Employer Contribution (6%)
        // Y=475 (Lowered from 510)
        DrawText(canvas, totalEr.ToString("N0"), 412, 444);
        
        // Row 2: Employee Contribution (5.5%)
        // Y=450
        DrawText(canvas, totalEmp.ToString("N0"), 412, 426);

        // Row 3 & 4 (Additions/Deductions) - Empty for now

        // Row 5: Total (11.5%) - Sum of 1+2
        // Y=375
        DrawText(canvas, totalAll.ToString("N0"), 412, 373);
        
        // Row 6: Amount in Words
        // We don't have Lao Number-to-Text yet, so we'll leave it blank
        // or put the number in parentheses if preferred.
        DrawText(canvas, $"{LaoNumberUtils.NumberToKipWords(totalAll):N0}", 180, 354); 

        // --- Company Information (Added as requested) ---
        // TODO: User to adjust positions (X, Y)
        DrawText(canvas, company.CompanyNameLao ?? "", 120, 540);
        // Draw LSSO Code with spacing (adjust spacing '15' and size '13' as needed to fit boxes)
        DrawSpacedText(canvas, company.LSSOCode ?? "", 425, 537, font, 13, 18);
        DrawText(canvas, company.Tel ?? "", 100, 515);
        DrawText(canvas, company.Phone ?? "", 245, 515);
        DrawText(canvas, company.Email ?? "", 420, 515);
        
        // Address
        DrawText(canvas, company.Village?.VillName ?? "", 120, 490);
        DrawText(canvas, company.District?.DiName ?? "", 270, 490);
        DrawText(canvas, company.Province?.PrName ?? "", 420, 490); 

        pdfDoc.Close();
        
        return outputStream.ToArray();
    }

    private void DrawText(PdfCanvas canvas, string text, float x, float y)
    {
        canvas.BeginText();
        canvas.MoveText(x, y);
        canvas.ShowText(text);
        canvas.EndText();
    }

    private void DrawSpacedText(PdfCanvas canvas, string text, float x, float y, PdfFont font, float fontSize, float charSpacing)
    {
        canvas.SaveState();
        canvas.SetFontAndSize(font, fontSize);
        
        float currentX = x;
        if (!string.IsNullOrEmpty(text))
        {
            foreach (char c in text)
            {
                canvas.BeginText();
                canvas.MoveText(currentX, y);
                canvas.ShowText(c.ToString());
                canvas.EndText();
                currentX += charSpacing;
            }
        }
        canvas.RestoreState();
    }
}
