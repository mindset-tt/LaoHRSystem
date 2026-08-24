using System.Text;
using ClosedXML.Excel;
using LaoHR.Shared.Models;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4B.2 — finance report export (CSV + Excel).
///
/// Security & correctness rules:
///   - CSV formula-injection protection: fields beginning with = + - @ (and
///     tab/CR) are prefixed with a single quote so they cannot execute as
///     spreadsheet formulas.
///   - UTF-8 with BOM so Lao text opens correctly in Excel.
///   - Accounting values are computed server-side; exports carry authoritative
///     values and never rely on spreadsheet formulas for balances.
/// </summary>
public interface IFinanceExportService
{
    /// <summary>Builds a CSV byte payload (UTF-8 with BOM) from rows.</summary>
    byte[] BuildCsv(IEnumerable<string> headers, IEnumerable<IEnumerable<object?>> rows);

    /// <summary>Builds an XLSX byte payload from a single sheet of rows.</summary>
    byte[] BuildExcel(string sheetName, IEnumerable<string> headers, IEnumerable<IEnumerable<object?>> rows);
}

public sealed class FinanceExportService : IFinanceExportService
{
    public byte[] BuildCsv(IEnumerable<string> headers, IEnumerable<IEnumerable<object?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsv)));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(cell => EscapeCsv(SanitizeCell(cell)))));
        }

        // UTF-8 BOM so Lao/Unicode text opens correctly in Excel.
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var bom = Encoding.UTF8.GetPreamble();
        var result = new byte[bom.Length + body.Length];
        Buffer.BlockCopy(bom, 0, result, 0, bom.Length);
        Buffer.BlockCopy(body, 0, result, bom.Length, body.Length);
        return result;
    }

    public byte[] BuildExcel(string sheetName, IEnumerable<string> headers, IEnumerable<IEnumerable<object?>> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);

        var headerList = headers.ToList();
        for (var c = 0; c < headerList.Count; c++)
        {
            var cell = sheet.Cell(1, c + 1);
            cell.Value = headerList[c];
            cell.Style.Font.Bold = true;
        }

        var r = 2;
        foreach (var row in rows)
        {
            var cells = row.ToList();
            for (var c = 0; c < cells.Count; c++)
            {
                var value = cells[c];
                var cell = sheet.Cell(r, c + 1);
                if (value is decimal d)
                {
                    cell.Value = d;
                    cell.Style.NumberFormat.Format = "#,##0.00";
                }
                else if (value is DateTime dt)
                {
                    cell.Value = dt;
                    cell.Style.DateFormat.Format = "yyyy-mm-dd";
                }
                else
                {
                    cell.Value = SanitizeCell(value);
                }
            }
            r++;
        }

        sheet.Columns().AdjustToContents();
        sheet.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Spreadsheet formula-injection guard. A cell whose text begins with
    /// = + - @ (or tab/CR) is prefixed with a single quote so it is treated as
    /// literal text, not a formula.
    /// </summary>
    private static string SanitizeCell(object? value)
    {
        if (value == null) return string.Empty;
        var s = value.ToString() ?? string.Empty;
        if (s.Length == 0) return s;
        var first = s[0];
        if (first == '=' || first == '+' || first == '-' || first == '@' || first == '\t' || first == '\r')
            return "'" + s;
        return s;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
