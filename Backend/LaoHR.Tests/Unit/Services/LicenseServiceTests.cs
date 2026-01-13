using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using LaoHR.Shared.Models;
using LaoHR.Shared.Services;
using Newtonsoft.Json;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class LicenseServiceTests : IDisposable
{
    private const string KeyFile = "public.key";
    private readonly LicenseService _service;
    private readonly RSA _rsa;
    private readonly string _privateKeyPem;

    public LicenseServiceTests()
    {
        _service = new LicenseService();
        _rsa = RSA.Create(2048);
        _privateKeyPem = _rsa.ExportRSAPrivateKeyPem();
        
        // Write Public Key for Service to consume
        var publicPem = _rsa.ExportSubjectPublicKeyInfoPem();
        File.WriteAllText(KeyFile, publicPem);
    }
    
    public void Dispose()
    {
        _rsa.Dispose();
        if (File.Exists(KeyFile)) File.Delete(KeyFile);
    }

    private string GenerateValidKey(LicenseData data)
    {
        string json = JsonConvert.SerializeObject(data);
        byte[] payload = Encoding.UTF8.GetBytes(json);
        byte[] signature = _rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        
        return $"{Convert.ToBase64String(payload)}.{Convert.ToBase64String(signature)}";
    }

    [Fact]
    public void VerifyLicense_ValidKey_ReturnsData()
    {
        var data = new LicenseData
        {
            CustomerName = "Test Co",
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            MaxEmployees = 50,
            Type = "PRO"
        };
        string key = GenerateValidKey(data);
        
        var result = _service.VerifyLicense(key);
        
        result.Should().NotBeNull();
        result!.CustomerName.Should().Be("Test Co");
        result.MaxEmployees.Should().Be(50);
        
        _service.IsLicenseValid(key).Should().BeTrue();
    }
    
    [Fact]
    public void VerifyLicense_ExpiredKey_ReturnsNull()
    {
        var data = new LicenseData
        {
            CustomerName = "Expired Co",
            ExpirationDate = DateTime.UtcNow.AddDays(-1)
        };
        string key = GenerateValidKey(data);
        
        var result = _service.VerifyLicense(key);
        
        result.Should().BeNull();
        _service.IsLicenseValid(key).Should().BeFalse();
    }
    
    [Fact]
    public void VerifyLicense_InvalidSignature_ReturnsNull()
    {
        var data = new LicenseData { CustomerName = "Hacker" };
        string realKey = GenerateValidKey(data);
        
        // Tamper with payload (first part)
        var parts = realKey.Split('.');
        string tamperedJson = JsonConvert.SerializeObject(new LicenseData { CustomerName = "Hacker", MaxEmployees = 99999 });
        string tamperedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes(tamperedJson));
        
        string fakeKey = $"{tamperedPayload}.{parts[1]}";
        
        _service.VerifyLicense(fakeKey).Should().BeNull();
    }
    
    [Fact]
    public void VerifyLicense_GarbageFormat_ReturnsNull()
    {
        _service.VerifyLicense("not.a.key").Should().BeNull();
        _service.VerifyLicense("garbage").Should().BeNull();
        _service.VerifyLicense("").Should().BeNull();
        _service.VerifyLicense(null!).Should().BeNull();
    }
    
    [Fact]
    public void VerifyLicense_NoPublicKeyFile_ReturnsNull()
    {
        // Delete the key file specifically for this test
        if (File.Exists(KeyFile)) File.Delete(KeyFile);
        
        // Should handle missing file gracefully
        var data = new LicenseData { CustomerName = "Test" };
        string key = GenerateValidKey(data); // Generates fine (using in-memory private key)
        
        _service.VerifyLicense(key).Should().BeNull();
    }
}
