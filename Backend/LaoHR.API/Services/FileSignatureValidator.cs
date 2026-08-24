using LaoHR.API.Metrics;

namespace LaoHR.API.Services;

/// <summary>
/// Phase 4D.1 — content-signature ("magic byte") validation for uploads.
///
/// The extension allow-list alone does not prove content type: an .exe renamed
/// to .pdf passes an extension check. This validator reads the leading bytes of
/// the uploaded stream and compares them against the expected signature for the
/// declared extension before anything is written to storage.
///
/// Signatures checked:
///   .pdf        "%PDF-" within the first 1024 bytes (spec-tolerant of leading junk)
///   .jpg/.jpeg  FF D8 FF
///   .png        89 50 4E 47 0D 0A 1A 0A
///   .docx       PK\x03\x04 (ZIP container; OOXML)
///   .doc        D0 CF 11 E0 A1 B1 1A E1 (OLE2 compound document)
///
/// Dangerous formats (.exe/.dll/.bat/.cmd/.ps1/.sh/.js/.html/.hta/.svg and
/// macro-enabled Office files) are denied by the extension allow-list in the
/// controllers; this class additionally guarantees their content cannot masquerade
/// as an allowed type.
/// </summary>
public static class FileSignatureValidator
{
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] Ole2 = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];
    private static readonly byte[] Zip = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] Pdf = [0x25, 0x50, 0x44, 0x46, 0x2D]; // %PDF-

    /// <summary>
    /// Returns true when the stream's leading bytes match the declared extension.
    /// The stream position is reset before returning so callers can continue
    /// reading (e.g. CopyToAsync).
    /// </summary>
    public static bool IsContentValid(string extension, Stream stream)
    {
        var originalPosition = stream.CanSeek ? stream.Position : -1;
        try
        {
            return extension.ToLowerInvariant() switch
            {
                ".pdf" => ContainsSequence(ReadPrefix(stream, 1024), Pdf),
                ".jpg" or ".jpeg" => Matches(ReadPrefix(stream, 3), [0xFF, 0xD8, 0xFF]),
                ".png" => Matches(ReadPrefix(stream, Png.Length), Png),
                ".docx" => Matches(ReadPrefix(stream, Zip.Length), Zip),
                ".doc" => Matches(ReadPrefix(stream, Ole2.Length), Ole2),
                // Unknown extension: no signature contract exists — reject.
                _ => false,
            };
        }
        finally
        {
            if (originalPosition >= 0) stream.Position = originalPosition;
        }
    }

    /// <summary>
    /// Validates content and records a rejected-upload metric when invalid.
    /// Shared by all upload endpoints so rejection telemetry stays consistent.
    /// </summary>
    public static bool ValidateOrRecord(string extension, Stream stream, string reason = "signature_mismatch")
    {
        if (IsContentValid(extension, stream)) return true;
        AppMetrics.UploadsRejected.Add(1, new KeyValuePair<string, object?>("reason", reason));
        return false;
    }

    private static byte[] ReadPrefix(Stream s, int count)
    {
        var buffer = new byte[count];
        var read = 0;
        while (read < count)
        {
            var n = s.Read(buffer, read, count - read);
            if (n <= 0) break;
            read += n;
        }
        return read == buffer.Length ? buffer : buffer[..read];
    }

    private static bool Matches(byte[] data, byte[] signature)
        => data.Length >= signature.Length && data.AsSpan(0, signature.Length).SequenceEqual(signature);

    private static bool ContainsSequence(byte[] data, byte[] pattern)
    {
        if (data.Length < pattern.Length) return false;
        for (var i = 0; i <= data.Length - pattern.Length; i++)
        {
            if (data[i] != pattern[0]) continue;
            if (data.AsSpan(i, pattern.Length).SequenceEqual(pattern)) return true;
        }
        return false;
    }
}
