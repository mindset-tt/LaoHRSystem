using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using LaoHR.Shared.Models;
using System.Collections.Generic;

namespace LaoHR.API.Services;

/// <summary>
/// PDF Payslip Generator with Lao/English bilingual support
/// Redesigned: Landscape "Ryobi Style" Grid
/// </summary>
public class PayslipPdfService
{
    public PayslipPdfService()
    {
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
            // Ignore if font loading fails or already registered
            Console.WriteLine("Font registration failed or already registered.");
        }
    }

    public byte[] GeneratePayslip(SalarySlip slip, Employee employee, PayrollPeriod period, List<PayrollAdjustment>? adjustments = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Mock Service Record Data (Temporary until API is updated)
        slip.WorkDays = 22; // Default Standard
        slip.AnnualLeaveRemaining = 12; 
        slip.SickLeaveUsed = 0;
        slip.AbsentDays = 0;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(DualTextStyle());
                
                page.Header().Element(c => ComposeHeader(c, slip, employee, period));
                page.Content().Element(c => ComposeContent(c, slip, adjustments));
                page.Footer().Element(ComposeFooter);
            });
        });
        
        return document.GeneratePdf();
    }

    private TextStyle DualTextStyle()
    {
        // Arial for cleaner, modern look. Fallback to Phetsarath OT for Lao.
#pragma warning disable CS0618 // Type or member is obsolete
        return TextStyle.Default.FontFamily("Arial").Fallback(x => x.FontFamily("Phetsarath OT"));
#pragma warning restore CS0618 // Type or member is obsolete
    }
    
    private void ComposeHeader(IContainer container, SalarySlip slip, Employee employee, PayrollPeriod period)
    {
        var purpleTheme = Colors.DeepPurple.Darken2;
        var lightPurple = Colors.DeepPurple.Lighten5;

        container.Column(col =>
        {
            // Top Row: Logo & Title
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Lao HR System").Bold().FontSize(18).FontColor(purpleTheme);
                    c.Item().Text("Company Ltd.").FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text("PAYSLIP").Bold().FontSize(28).FontColor(purpleTheme);
                    c.Item().Text("ໃບເງິນເດືອນ").FontSize(16).FontColor(Colors.Grey.Darken2);
                    c.Item().Text(period.PeriodName).FontSize(14).FontColor(Colors.Grey.Medium);
                });
            });

            // Employee Info Bar (Full Width)
            col.Item().PaddingTop(10).Background(lightPurple).Border(1).BorderColor(Colors.DeepPurple.Lighten3).Padding(8).Row(row =>
            {
                // Compact but detailed info using Grid
                row.RelativeItem().Column(c => {
                    c.Item().Text("EMPLOYEE").FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"{employee.EnglishName}").Bold().FontSize(11).FontColor(purpleTheme);
                    c.Item().Text($"{employee.LaoName}").FontSize(10);
                });
                
                row.ConstantItem(150).Column(c => {
                    c.Item().Text("ID & NSSF").FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"ID: {employee.EmployeeCode}").Bold().FontSize(10);
                    c.Item().Text($"NSSF: {employee.NssfId ?? "-"}").FontSize(10);
                });

                row.RelativeItem().Column(c => {
                    c.Item().Text("DEPARTMENT & POSITION").FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"{employee.Department?.DepartmentNameEn ?? "-"}").Bold().FontSize(10);
                    c.Item().Text(employee.JobTitle ?? "-").FontSize(10);
                });

                row.ConstantItem(150).AlignRight().Column(c => {
                     c.Item().Text("PAY PERIOD").FontSize(8).FontColor(Colors.Grey.Darken2);
                     c.Item().Text(period.PeriodName).Bold().FontSize(11);
                     c.Item().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(9);
                });
            });
            
            col.Item().PaddingBottom(10);
        });
    }

    private void ComposeContent(IContainer container, SalarySlip slip, List<PayrollAdjustment>? adjustments = null)
    {
        var finalAdjustments = adjustments ?? new List<PayrollAdjustment>();

        // 1. Prepare Payment Data
        var paymentRows = new List<(string, string, bool)>();
        paymentRows.Add(("Base Salary", $"{slip.BaseSalary:N0}", false));
        
        // Dynamic Taxable Earnings (e.g., Commission, Fuel)
        var taxableEarnings = finalAdjustments.Where(a => a.Type == "EARNING" && a.IsTaxable).ToList();
        foreach (var adj in taxableEarnings)
        {
            paymentRows.Add((adj.Name, $"{adj.Amount:N0}", false));
        }
        
        // If no dynamic earnings, can show "Allowances" if value exists (legacy fallback)
        if (!taxableEarnings.Any() && slip.Allowances > 0)
        {
             paymentRows.Add(("Allowances", $"{slip.Allowances:N0}", false));
        }

        paymentRows.Add(("Overtime Pay (Total)", $"{slip.OvertimePay:N0}", false));
        paymentRows.Add(("  - Normal OT (x1.5)", "0", false));
        paymentRows.Add(("  - Weekend OT (x2.0)", "0", false));
        paymentRows.Add(("  - Holiday OT (x2.5)", "0", false));
        paymentRows.Add(("  - Night Shift (x3.0)", "0", false));
        paymentRows.Add(("Bonus", $"{slip.Bonus:N0}", true));

        // 2. Prepare Deduction Data
        var deductionRows = new List<(string, string, bool)>();
        deductionRows.Add(("Social Security (5.5%)", $"{slip.NssfEmployeeDeduction:N0}", false));
        
        // Tax Breakdown
        deductionRows.AddRange(GetTaxBreakdown(slip.TaxableIncome));
        
        // Dynamic Deductions
        var dynamicDeductions = finalAdjustments.Where(a => a.Type == "DEDUCTION").ToList();
        foreach (var adj in dynamicDeductions)
        {
            deductionRows.Add((adj.Name, $"{adj.Amount:N0}", false));
        }
        
        if (!dynamicDeductions.Any() && slip.OtherDeductions > 0)
             deductionRows.Add(("Other", $"{slip.OtherDeductions:N0}", false));


        container.Row(row =>
        {
            // Column 1: Payment
            row.RelativeItem().Element(c => DrawRyobiColumn(c, "Payment", 
                paymentRows,
                "Payment Total", $"{slip.GrossIncome:N0}"
            ));

            // Column 2: Deduction (Expanded)
            row.RelativeItem().Element(c => DrawRyobiColumn(c, "Deduction", 
                deductionRows,
                "Deduction Total", $"{(slip.TaxDeduction + slip.NssfEmployeeDeduction + slip.OtherDeductions):N0}"
            ));

            // Column 3: Service Record
            row.RelativeItem().Element(c => DrawRyobiColumn(c, "Service Record", 
                new List<(string, string, bool)> {
                    ("Working Days", $"{slip.WorkDays} Days", false),
                    ("Days Off", "0 Days", false),
                    ("Sick Leave", $"{slip.SickLeaveUsed} Days", false),
                    ("Absent", $"{slip.AbsentDays} Days", false)
                },
                null, null
            ));

            // Column 4: Others (Net Pay + Non-Taxable Income)
            
            var otherRows = new List<(string, string, bool)>();
            
            // Dynamic Non-Taxable Earnings (e.g., Per Diem, Reimbursement)
            var nonTaxableEarnings = finalAdjustments.Where(a => a.Type == "EARNING" && !a.IsTaxable).ToList();
            if (nonTaxableEarnings.Any())
            {
                 otherRows.Add(("Non-Taxable Income", "", true)); 
                 foreach (var adj in nonTaxableEarnings)
                 {
                     otherRows.Add(($"  {adj.Name}", $"{adj.Amount:N0}", false));
                 }
            }
            
            otherRows.Add(("Annual Paid Leave", "", false));
            otherRows.Add(("   Total", "15 Days", false));
            otherRows.Add(("   Used", "0 Days", false));
            otherRows.Add(("   Remaining", $"{slip.AnnualLeaveRemaining} Days", true));
            otherRows.Add(("", "", false)); 
            otherRows.Add(("Exchange Rate", "21,605", false));

            row.RelativeItem().Element(c => DrawRyobiColumn(c, "Others", 
                 otherRows,
                 "Net Payment Total", $"{slip.NetSalary:N0}"
            ));
        });
    }

    // Helper to calculate Tax Breakdown for Display
    private List<(string, string, bool)> GetTaxBreakdown(decimal taxableIncome)
    {
        var result = new List<(string, string, bool)>();
        decimal remaining = taxableIncome;
        
        // Standard Lao Tax Brackets (approximate for display)
        // 0-1.3M: 0%
        decimal level1 = Math.Min(remaining, 1300000m);
        // result.Add(("Tax Lv1 (0-1.3M) 0%", "0", false)); // Usually hidden if 0
        remaining -= level1;
        
        // 1.3M-5M: 5%
        if (remaining > 0)
        {
            decimal range = 5000000 - 1300000;
            decimal taxable = Math.Min(remaining, range);
            decimal tax = taxable * 0.05m;
            result.Add(("Tax Lv2 (5%)", $"{Math.Round(tax):N0}", false));
            remaining -= taxable;
        }
        
        // 5M-15M: 10%
        if (remaining > 0)
        {
            decimal range = 15000000 - 5000000;
            decimal taxable = Math.Min(remaining, range);
            decimal tax = taxable * 0.10m;
            result.Add(("Tax Lv3 (10%)", $"{Math.Round(tax):N0}", false));
            remaining -= taxable;
        }
        
        // 15M-25M: 15%
        if (remaining > 0)
        {
            decimal range = 25000000 - 15000000;
            decimal taxable = Math.Min(remaining, range);
            decimal tax = taxable * 0.15m;
            result.Add(("Tax Lv4 (15%)", $"{Math.Round(tax):N0}", false));
            remaining -= taxable;
        }

        // 25M-65M: 20%
        if (remaining > 0)
        {
            decimal range = 65000000 - 25000000;
            decimal taxable = Math.Min(remaining, range);
            decimal tax = taxable * 0.20m;
            result.Add(("Tax Lv5 (20%)", $"{Math.Round(tax):N0}", false));
            remaining -= taxable;
        }
        
        // 65M+: 25%
        if (remaining > 0)
        {
            decimal tax = remaining * 0.25m;
            result.Add(("Tax Lv6 (25%)", $"{Math.Round(tax):N0}", false));
        }

        if (result.Count == 0 && taxableIncome > 0)
            result.Add(("Total Tax", "0", false)); // Should theoretically overlap with Lv1 but if totally exempt

        return result;
    }

    private void DrawRyobiColumn(IContainer container, string title, List<(string Label, string Value, bool IsRed)> data, string? footerLabel, string? footerValue)
    {
        var headerColor = Colors.DeepPurple.Darken2; 
        var stripeColor = Colors.DeepPurple.Lighten5;
        var footerColor = Colors.DeepPurple.Lighten4; 
        var borderColor = Colors.DeepPurple.Darken2;

        container.Column(col =>
        {
            // Header
            col.Item().Background(headerColor).Padding(2).Text(title).Bold().FontColor(Colors.White).FontSize(10);

            // Grid Box
            col.Item().Border(1).BorderColor(borderColor).Table(table =>
            {
                table.ColumnsDefinition(cd => 
                {
                    cd.RelativeColumn();
                    cd.ConstantColumn(70);
                });

                // Fixed rows to create the spreadsheet look
                for (int i = 0; i < 20; i++)
                {
                    var isFooter = (i == 19); // Last row is footer
                    
                    string label = "";
                    string value = "";
                    bool isRed = false;
                    
                    if (isFooter && footerLabel != null)
                    {
                        label = footerLabel;
                        value = footerValue!;
                    }
                    else if (!isFooter && i < data.Count)
                    {
                        var item = data[i];
                        label = item.Label;
                        value = item.Value;
                        isRed = item.IsRed;
                    }

                    var bg = Colors.White;
                    if (isFooter) bg = footerColor;
                    else if (i % 2 != 0) bg = stripeColor; 

                    var textStyle = TextStyle.Default.FontSize(9).FontColor(Colors.Black);
                    if (isFooter) textStyle = textStyle.Bold();

                    table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).Text(label).Style(textStyle);
                    
                    var valCell = table.Cell().Background(bg).BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(2).AlignRight().Text(value).Style(textStyle);
                    
                    if (isRed) valCell.FontColor(Colors.Red.Medium);
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(c => SignatureBox(c, "Prepared By"));
                row.ConstantItem(30);
                row.RelativeItem().Column(c => SignatureBox(c, "Verified By"));
                row.ConstantItem(30);
                row.RelativeItem().Column(c => SignatureBox(c, "Received By"));
            });
            
            col.Item().PaddingTop(10).AlignCenter().Text("Generated by Lao HR System - Confidential Document").FontSize(8).FontColor(Colors.Grey.Lighten1);
        });
    }
    
    private void SignatureBox(ColumnDescriptor col, string title)
    {
        col.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Black);
        col.Item().AlignCenter().Text(title).FontSize(9);
    }
}
