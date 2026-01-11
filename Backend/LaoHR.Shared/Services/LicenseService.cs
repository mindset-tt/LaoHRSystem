using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using LaoHR.Shared.Models;

namespace LaoHR.Shared.Services;

public class LicenseService
{
    private const string PublicKeyFileName = "public.key";
    
    public LicenseData? VerifyLicense(string licenseKey)
    {
        try
        {
            if (string.IsNullOrEmpty(licenseKey)) return null;
            if (!File.Exists(PublicKeyFileName)) return null;

            var parts = licenseKey.Split('.');
            if (parts.Length != 2) return null;

            byte[] payload = Convert.FromBase64String(parts[0]);
            byte[] signature = Convert.FromBase64String(parts[1]);

            using var rsa = RSA.Create();
            string pem = File.ReadAllText(PublicKeyFileName);
            rsa.ImportFromPem(pem);

            if (rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            {
                string json = Encoding.UTF8.GetString(payload);
                var data = JsonConvert.DeserializeObject<LicenseData>(json);
                
                // Check Expiration
                if (data == null) return null;
                if (data.ExpirationDate < DateTime.UtcNow) return null; // Expired
                
                return data;
            }
        }
        catch 
        {
            // Logging would be good here but we keep it simple
        }
        return null;
    }
    
    public bool IsLicenseValid(string licenseKey)
    {
        return VerifyLicense(licenseKey) != null;
    }
}
