using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using LaoHR.Shared.Models;
using LaoHR.Shared.Data;

namespace LaoHR.API.Services;

/// <summary>
/// NSSF Social Security Report Generator
/// Redesigned: Landscape, Government Standard
/// </summary>
public class NssfReportService
{
    private readonly LaoHRDbContext _context;

    public NssfReportService(LaoHRDbContext context)
    {
        _context = context;
        try
        {
            var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Font", "Phetsarath_OT.ttf");
            if (File.Exists(fontPath))
            {
                using var stream = File.OpenRead(fontPath);
                FontManager.RegisterFont(stream);
            }
        }
        catch
        {
             // Ignore
        }
    }

    public async Task<byte[]> GenerateNssfReport(int periodId)
    {
        var period = await _context.PayrollPeriods.FindAsync(periodId);
        if (period == null) throw new FileNotFoundException("Period not found");

        var company = await _context.CompanySettings.FirstOrDefaultAsync();

        var slips = await _context.SalarySlips
            .Include(s => s.Employee)
            .Where(s => s.PeriodId == periodId)
            .OrderBy(s => s.Employee.EmployeeCode)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(DualTextStyle());

                page.Header().Element(c => ComposeHeader(c, period, company));
                page.Content().Element(c => ComposeContent(c, slips));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }
    
    private TextStyle DualTextStyle()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return TextStyle.Default.FontFamily("Times New Roman").Fallback(x => x.FontFamily("Phetsarath OT"));
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private void ComposeHeader(IContainer container, PayrollPeriod period, CompanySetting? company)
    {
        container.Column(col =>
        {
            // Government Header (Centered)
            col.Item().AlignCenter().Text("Lao People's Democratic Republic").Bold().FontSize(12).FontFamily("Times New Roman");
            col.Item().AlignCenter().Text("Peace Independence Democracy Unity Prosperity").Bold().FontSize(12).FontFamily("Times New Roman");
            
            // Title
            col.Item().PaddingTop(20).AlignCenter().Text("REPORT ON SOCIAL SECURITY CONTRIBUTIONS").Bold().FontSize(16);
            col.Item().AlignCenter().Text("ບົດລາຍງານການສົ່ງເງິນສົມທົບປະກັນສັງຄົມ").FontSize(14);

            // Metadata Row
            col.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    var companyName = company?.CompanyNameEn ?? "Company Name Not Set";
                    var lssoCode = company?.LSSOCode ?? "N/A";

                    c.Item().Text(t => { t.Span("Company Name: ").Bold(); t.Span(companyName); });
                    c.Item().Text(t => { t.Span("Enterprise NSSF ID: ").Bold(); t.Span(lssoCode); });
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text(t => { t.Span("Period: ").Bold(); t.Span(period.PeriodName); });
                    c.Item().Text(t => { t.Span("Report Date: ").Bold(); t.Span(DateTime.Now.ToString("dd/MM/yyyy")); });
                });
            });
            
            col.Item().PaddingBottom(10);
        });
    }

    private void ComposeContent(IContainer container, List<SalarySlip> slips)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(40); // No
                columns.ConstantColumn(100); // NSSF ID
                columns.RelativeColumn(3); // Name
                columns.RelativeColumn(2); // Salary Base
                columns.RelativeColumn(1.5f); // Emp (5.5%)
                columns.RelativeColumn(1.5f); // Er (6.0%)
                columns.RelativeColumn(2); // Total
            });

            // Header
            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("No").Bold();
                header.Cell().Element(CellStyle).Text("NSSF ID").Bold();
                header.Cell().Element(CellStyle).Text("Employee Name").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("Income Base").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("5.5% (Emp)").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("6.0% (Employer)").Bold();
                header.Cell().Element(CellStyle).AlignRight().Text("Total Contrib.").Bold();
            });

            // Data
            for (int i = 0; i < slips.Count; i++)
            {
                var slip = slips[i];
                var bgColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten5;

                table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).Text((i + 1).ToString());
                table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(slip.Employee.NssfId ?? "-");
                table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(slip.Employee.LaoName);
                
                table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{slip.NssfBase:N0}");
                table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{slip.NssfEmployeeDeduction:N0}");
                table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{slip.NssfEmployerContribution:N0}");
                
                var total = slip.NssfEmployeeDeduction + slip.NssfEmployerContribution;
                table.Cell().Background(bgColor).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"{total:N0}").Bold();
            }

            // Totals
            var totalBase = slips.Sum(s => s.NssfBase);
            var totalEmp = slips.Sum(s => s.NssfEmployeeDeduction);
            var totalEr = slips.Sum(s => s.NssfEmployerContribution);
            var grandTotal = totalEmp + totalEr;

            table.Cell().ColumnSpan(3).BorderTop(1).Padding(5).AlignRight().Text("GRAND TOTAL:").Bold().FontSize(11);
            table.Cell().BorderTop(1).Padding(5).AlignRight().Text($"{totalBase:N0}").Bold().FontSize(11);
            table.Cell().BorderTop(1).Padding(5).AlignRight().Text($"{totalEmp:N0}").Bold().FontSize(11);
            table.Cell().BorderTop(1).Padding(5).AlignRight().Text($"{totalEr:N0}").Bold().FontSize(11);
            table.Cell().BorderTop(1).Padding(5).AlignRight().Text($"{grandTotal:N0}").Bold().FontSize(12).FontColor(Colors.DeepPurple.Darken2);
        });
    }
    
    private static IContainer CellStyle(IContainer container)
    {
        return container.Background(Colors.Grey.Lighten3).BorderBottom(1).Padding(5);
    }

    private void ComposeFooter(IContainer container)
    {
        container.PaddingTop(40).Row(row =>
        {
            row.RelativeItem().Column(c => SignatureBlock(c, "Prepared By", "Accountant"));
            row.RelativeItem().Column(c => SignatureBlock(c, "Verified By", "HR Manager"));
            row.RelativeItem().Column(c => SignatureBlock(c, "Approved By", "Director"));
        });
    }

    private void SignatureBlock(ColumnDescriptor col, string action, string title)
    {
        col.Item().PaddingBottom(40).AlignCenter().Text(action).FontSize(10);
        col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);
        col.Item().AlignCenter().Text(title).Bold().FontSize(10);
    }
}
