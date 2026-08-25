using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using LaoHR.Shared.Models;
using LaoHR.Shared.Services;
using Newtonsoft.Json;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

/// <summary>
/// Phase 4D.2 — P1-SEC-001 rotation regression.
///
/// Simulates the signing-root rotation: a license produced under the RETIRED
/// key must fail verification once the application trusts the NEW key, and a
/// license produced under the NEW key must pass. Ephemeral in-memory keys
/// stand in for the real operator-held material (never committed).
/// </summary>
[Collection("LicenseKeyFileSequential")]
public class LicenseRotationTests : IDisposable
{
    private const string KeyFile = "public.key";
    private readonly LicenseService _service;
    private readonly RSA _oldRsa;
    private readonly RSA _newRsa;

    public LicenseRotationTests()
    {
        _service = new LicenseService();
        _oldRsa = RSA.Create(2048);
        _newRsa = RSA.Create(2048);

        // The application's verification anchor after rotation: NEW public key.
        File.WriteAllText(KeyFile, _newRsa.ExportSubjectPublicKeyInfoPem());
    }

    public void Dispose()
    {
        _oldRsa.Dispose();
        _newRsa.Dispose();
        if (File.Exists(KeyFile)) File.Delete(KeyFile);
    }

    private static string Sign(RSA rsa, LicenseData data)
    {
        byte[] payload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data));
        byte[] signature = rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return $"{Convert.ToBase64String(payload)}.{Convert.ToBase64String(signature)}";
    }

    [Fact]
    public void Rotation_LicenseSignedWithRetiredKey_IsRejectedUnderNewAnchor()
    {
        string retiredLicense = Sign(_oldRsa, new LicenseData
        {
            CustomerName = "Pre-rotation holder",
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            MaxEmployees = 100,
            Type = "ENTERPRISE"
        });

        _service.VerifyLicense(retiredLicense).Should().BeNull();
        _service.IsLicenseValid(retiredLicense).Should().BeFalse();
    }

    [Fact]
    public void Rotation_LicenseSignedWithNewKey_IsAccepted()
    {
        string freshLicense = Sign(_newRsa, new LicenseData
        {
            CustomerName = "Lao HR Demo",
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            MaxEmployees = 100,
            Type = "ENTERPRISE"
        });

        var result = _service.VerifyLicense(freshLicense);

        result.Should().NotBeNull();
        result!.CustomerName.Should().Be("Lao HR Demo");
    }

    [Fact]
    public void Rotation_ExpiredNewKeyLicense_IsRejected()
    {
        string expired = Sign(_newRsa, new LicenseData
        {
            CustomerName = "Lapsed",
            ExpirationDate = DateTime.UtcNow.AddDays(-1)
        });

        _service.VerifyLicense(expired).Should().BeNull();
    }

    [Fact]
    public void Rotation_TamperedNewKeyPayload_IsRejected()
    {
        var data = new LicenseData
        {
            CustomerName = "Lao HR Demo",
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            MaxEmployees = 100
        };
        string legit = Sign(_newRsa, data);
        var parts = legit.Split('.');

        var forged = new LicenseData
        {
            CustomerName = "Lao HR Demo",
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            MaxEmployees = 999999 // privilege escalation attempt on payload
        };
        string forgedPayload = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(forged)));

        _service.VerifyLicense($"{forgedPayload}.{parts[1]}").Should().BeNull();
    }
}
