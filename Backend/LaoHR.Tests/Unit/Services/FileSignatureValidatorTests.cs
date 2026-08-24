using FluentAssertions;
using LaoHR.API.Services;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 4D.1 — content-signature (magic byte) validation unit tests.
/// Extension allow-list alone is not content validation; these tests pin the
/// behavior for both genuine and masquerading files.
/// </summary>
public class FileSignatureValidatorTests
{
    private static MemoryStream Stream(params byte[] bytes) => new(bytes);

    [Theory]
    [InlineData(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 }, ".pdf", true)]   // %PDF-1.4
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ".jpg", true)]                            // JPEG
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, ".jpeg", true)]                           // JPEG EXIF
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ".png", true)]    // PNG
    [InlineData(new byte[] { 0x50, 0x4B, 0x03, 0x04, 0x14, 0x00 }, ".docx", true)]               // ZIP/OOXML
    [InlineData(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 }, ".doc", true)]    // OLE2
    public void Genuine_Content_Matches_Declared_Extension(byte[] content, string ext, bool expected)
    {
        using var s = Stream(content);
        FileSignatureValidator.IsContentValid(ext, s).Should().Be(expected);
        // Stream position must be reset so callers can continue reading.
        s.Position.Should().Be(0);
    }

    [Fact]
    public void Exe_Renamed_Pdf_Is_Rejected()
    {
        // MZ header — a Windows executable masquerading as .pdf.
        var exe = new byte[512];
        exe[0] = (byte)'M'; exe[1] = (byte)'Z';
        using var s = Stream(exe);
        FileSignatureValidator.IsContentValid(".pdf", s).Should().BeFalse();
    }

    [Fact]
    public void Html_Renamed_Pdf_Is_Rejected()
    {
        var html = "<!DOCTYPE html><html><body>x</body></html>"u8.ToArray();
        using var s = Stream(html);
        FileSignatureValidator.IsContentValid(".pdf", s).Should().BeFalse();
    }

    [Fact]
    public void Script_Renamed_Png_Is_Rejected()
    {
        var js = "alert(1)"u8.ToArray();
        using var s = Stream(js);
        FileSignatureValidator.IsContentValid(".png", s).Should().BeFalse();
    }

    [Fact]
    public void Empty_Stream_Is_Rejected()
    {
        using var s = Stream();
        FileSignatureValidator.IsContentValid(".pdf", s).Should().BeFalse();
    }

    [Fact]
    public void Unknown_Extension_Is_Rejected()
    {
        using var s = Stream(0x25, 0x50, 0x44, 0x46, 0x2D);
        FileSignatureValidator.IsContentValid(".exe", s).Should().BeFalse();
    }

    [Fact]
    public void Pdf_With_Leading_Junk_Within_Window_Is_Accepted()
    {
        // Some generators prepend bytes before %PDF-; the validator scans the
        // first 1024 bytes rather than requiring offset 0.
        var junk = new byte[64];
        var pdf = "%PDF-1.7"u8.ToArray();
        var combined = new byte[junk.Length + pdf.Length];
        junk.CopyTo(combined, 0);
        pdf.CopyTo(combined, junk.Length);
        using var s = Stream(combined);
        FileSignatureValidator.IsContentValid(".pdf", s).Should().BeTrue();
    }

    [Fact]
    public void Truncated_Prefix_Is_Rejected()
    {
        using var s = Stream(0xFF, 0xD8); // JPEG needs 3 bytes minimum
        FileSignatureValidator.IsContentValid(".jpg", s).Should().BeFalse();
    }
}
