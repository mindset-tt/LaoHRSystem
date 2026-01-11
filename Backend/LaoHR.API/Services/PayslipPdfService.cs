using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// PDF Payslip Generator with Lao/English bilingual support
/// </summary>
public class PayslipPdfService
{
    public byte[] GeneratePayslip(SalarySlip slip, Employee employee, PayrollPeriod period)
    {
        // Configure QuestPDF license (Community license for open-source)
        QuestPDF.Settings.License = LicenseType.Community;
        
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Leelawadee UI"));
                
                page.Header().Element(c => ComposeHeader(c, period));
                page.Content().Element(c => ComposeContent(c, slip, employee, period));
                page.Footer().Element(ComposeFooter);
            });
        });
        
        return document.GeneratePdf();
    }
    
    private void ComposeHeader(IContainer container, PayrollPeriod period)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("LAO HR SYSTEM").Bold().FontSize(20).FontColor(Colors.Blue.Darken2);
                    col.Item().Text("ລະບົບບໍລິຫານບຸກຄະລາກອນ").FontSize(12).FontColor(Colors.Grey.Darken1);
                });
                
                row.ConstantItem(150).Column(col =>
                {
                    col.Item().AlignRight().Text("PAYSLIP").Bold().FontSize(16);
                    col.Item().AlignRight().Text("ໃບເງິນເດືອນ").FontSize(10);
                    col.Item().AlignRight().Text(period.PeriodName).FontSize(10).FontColor(Colors.Grey.Darken1);
                });
            });
            
            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }
    
    private void ComposeContent(IContainer container, SalarySlip slip, Employee employee, PayrollPeriod period)
    {
        container.Column(column =>
        {
            // Employee Info
            column.Item().PaddingBottom(15).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Employee Information / ຂໍ້ມູນພະນັກງານ").Bold().FontSize(11);
                    col.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(120);
                            c.RelativeColumn();
                        });
                        
                        table.Cell().Text("Name / ຊື່:").FontColor(Colors.Grey.Darken1);
                        table.Cell().Text(employee.LaoName + (employee.EnglishName != null ? $" ({employee.EnglishName})" : ""));
                        
                        table.Cell().Text("Employee Code / ລະຫັດ:").FontColor(Colors.Grey.Darken1);
                        table.Cell().Text(employee.EmployeeCode);
                        
                        table.Cell().Text("Department / ພະແນກ:").FontColor(Colors.Grey.Darken1);
                        table.Cell().Text(employee.Department?.DepartmentNameEn ?? "-");
                        
                        table.Cell().Text("Position / ຕຳແໜ່ງ:").FontColor(Colors.Grey.Darken1);
                        table.Cell().Text(employee.JobTitle ?? "-");
                        
                        table.Cell().Text("NSSF ID / ປະກັນສັງຄົມ:").FontColor(Colors.Grey.Darken1);
                        table.Cell().Text(employee.NssfId ?? "-");
                    });
                });
            });
            
            // Earnings
            column.Item().PaddingBottom(10).Element(c => ComposeEarningsTable(c, slip));
            
            // Deductions
            column.Item().PaddingBottom(10).Element(c => ComposeDeductionsTable(c, slip));
            
            // Net Pay
            column.Item().Background(Colors.Blue.Lighten5).Padding(15).Row(row =>
            {
                row.RelativeItem().Text("NET SALARY / ເງິນເດືອນສຸດທິ").Bold().FontSize(14);
                row.ConstantItem(150).AlignRight().Text($"₭ {slip.NetSalary:N0}").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
            });
            
            // Text Amount
            column.Item().PaddingTop(5).Text(text =>
            {
                text.Span("Amount in words: ").FontSize(9).FontColor(Colors.Grey.Darken1);
                text.Span(LaoNumberUtils.NumberToKipWords(slip.NetSalary)).FontSize(10).Bold();
            });
            
            // Bank Info
            if (!string.IsNullOrEmpty(employee.BankAccount))
            {
                column.Item().PaddingTop(15).Text(text =>
                {
                    text.Span("Bank Transfer: ").FontColor(Colors.Grey.Darken1);
                    text.Span($"{employee.BankName} - {employee.BankAccount}");
                });
            }
        });
    }
    
    private void ComposeEarningsTable(IContainer container, SalarySlip slip)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Green.Lighten5).Padding(8).Text("EARNINGS / ລາຍຮັບ").Bold().FontSize(11);
            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                });
                
                AddTableRow(table, "Base Salary / ເງິນເດືອນພື້ນຖານ", slip.BaseSalary);
                AddTableRow(table, "Overtime Pay / ຄ່າລ່ວງເວລາ", slip.OvertimePay);
                AddTableRow(table, "Allowances / ເງິນອຸດໜູນ", slip.Allowances);
                
                // Total
                table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
                {
                    row.RelativeItem().Text("GROSS INCOME / ລາຍຮັບລວມ").Bold();
                    row.ConstantItem(100).AlignRight().Text($"₭ {slip.GrossIncome:N0}").Bold();
                });
            });
        });
    }
    
    private void ComposeDeductionsTable(IContainer container, SalarySlip slip)
    {
        container.Column(column =>
        {
            column.Item().Background(Colors.Red.Lighten5).Padding(8).Text("DEDUCTIONS / ລາຍຈ່າຍ").Bold().FontSize(11);
            column.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.RelativeColumn(1);
                });
                
                AddTableRow(table, $"NSSF Employee (5.5%) / ປະກັນສັງຄົມ (Base: ₭{slip.NssfBase:N0})", slip.NssfEmployeeDeduction, true);
                AddTableRow(table, "Income Tax (PIT) / ອາກອນລາຍໄດ້", slip.TaxDeduction, true);
                AddTableRow(table, "Other Deductions / ຫັກອື່ນໆ", slip.OtherDeductions, true);
                
                // Total
                table.Cell().ColumnSpan(2).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
                {
                    row.RelativeItem().Text("TOTAL DEDUCTIONS / ລວມລາຍຈ່າຍ").Bold();
                    var totalDeductions = slip.NssfEmployeeDeduction + slip.TaxDeduction + slip.OtherDeductions;
                    row.ConstantItem(100).AlignRight().Text($"₭ {totalDeductions:N0}").Bold().FontColor(Colors.Red.Darken1);
                });
            });
        });
    }
    
    private void AddTableRow(TableDescriptor table, string label, decimal amount, bool isDeduction = false)
    {
        if (amount == 0) return;
        
        table.Cell().Padding(8).Text(label);
        var text = isDeduction ? $"- ₭ {amount:N0}" : $"₭ {amount:N0}";
        var color = isDeduction ? Colors.Red.Darken1 : Colors.Black;
        table.Cell().Padding(8).AlignRight().Text(text).FontColor(color);
    }
    
    private void ComposeFooter(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            column.Item().Row(row =>
            {
                row.RelativeItem().Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(8));
                    text.Span("Generated: ").FontColor(Colors.Grey.Darken1);
                    text.Span(DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
                });
                
                row.RelativeItem().AlignRight().Text("This is a computer-generated document").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }
}
