using System.Text;
using FluentAssertions;
using LaoHR.API.Services;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 4B.2 — finance export service tests.
///
/// Proves CSV formula-injection protection, Lao UTF-8 (BOM) output, and Excel
/// generation.
/// </summary>
public class FinanceExportServiceTests
{
    private readonly FinanceExportService _service = new();

    [Fact]
    public void BuildCsv_PrefixesFormulaInjectionCharacters()
    {
        var headers = new[] { "Name" };
        var rows = new List<IEnumerable<object?>>
        {
            new object?[] { "=SUM(A1:A9)" },
            new object?[] { "+cmd|' /C calc'!A0" },
            new object?[] { "-2+3" },
            new object?[] { "@SUM(A1)" },
            new object?[] { "normal text" },
        };

        var bytes = _service.BuildCsv(headers, rows);
        var text = Encoding.UTF8.GetString(bytes);

        text.Should().Contain("'=SUM(A1:A9)");
        text.Should().Contain("'+cmd");
        text.Should().Contain("'-2+3");
        text.Should().Contain("'@SUM(A1)");
        text.Should().Contain("normal text");
    }

    [Fact]
    public void BuildCsv_EmitsUtf8Bom_ForLaoText()
    {
        var headers = new[] { "Name" };
        var rows = new List<IEnumerable<object?>>
        {
            new object?[] { "ບໍລິສັດ ລາວ" }, // Lao supplier name
        };

        var bytes = _service.BuildCsv(headers, rows);

        // UTF-8 BOM present (EF BB BF).
        bytes[0].Should().Be(0xEF);
        bytes[1].Should().Be(0xBB);
        bytes[2].Should().Be(0xBF);

        var text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        text.Should().Contain("ບໍລິສັດ ລາວ");
    }

    [Fact]
    public void BuildCsv_EscapesCommasAndQuotes()
    {
        var headers = new[] { "Name" };
        var rows = new List<IEnumerable<object?>>
        {
            new object?[] { "Smith, John" },
            new object?[] { "He said \"hi\"" },
        };

        var bytes = _service.BuildCsv(headers, rows);
        var text = Encoding.UTF8.GetString(bytes);

        text.Should().Contain("\"Smith, John\"");
        text.Should().Contain("\"He said \"\"hi\"\"\"");
    }

    [Fact]
    public void BuildExcel_ProducesValidXlsx()
    {
        var headers = new[] { "Account", "Debit", "Credit" };
        var rows = new List<IEnumerable<object?>>
        {
            new object?[] { "Cash", 100.00m, 0m },
            new object?[] { "AP", 0m, 100.00m },
        };

        var bytes = _service.BuildExcel("Trial Balance", headers, rows);

        bytes.Should().NotBeNullOrEmpty();
        // XLSX is a ZIP archive; signature is PK.
        bytes[0].Should().Be((byte)'P');
        bytes[1].Should().Be((byte)'K');
    }
}
