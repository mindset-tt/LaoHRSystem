using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using LaoHR.Shared.Models;

namespace LaoHR.LicenseGen;

// Phase 4D.2 — P1-SEC-001 closure.
//
// The private signing key NO LONGER lives in this repository and this tool
// no longer generates keys into its working directory. The operator supplies
// the signing key from secure off-repo storage:
//
//   1) path to the private key file:
//        arg[0], or env LICENSEGEN_PRIVATE_KEY
//   2) passphrase (only when the key is an ENCRYPTED PKCS#8 PEM):
//        env LICENSEGEN_KEY_PASSPHRASE
//
// The matching PUBLIC verification key ships with the application
// (LaoHR.API/public.key). See docs/production-phase-4d.2/01+02.

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== LaoHR License Generator ===");

        var keyPath = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("LICENSEGEN_PRIVATE_KEY");
        if (string.IsNullOrWhiteSpace(keyPath) || !File.Exists(keyPath))
        {
            Console.WriteLine("❌ Signing key not provided or not found.");
            Console.WriteLine("   Pass the OPERATOR-held key path as arg[0] or set LICENSEGEN_PRIVATE_KEY.");
            Console.WriteLine("   This tool never creates or stores private keys itself.");
            Environment.Exit(1);
            return;
        }

        try
        {
            GenerateLicense(keyPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ {ex.Message}");
            Environment.Exit(1);
        }
    }

    static RSA LoadOperatorKey(string keyPath)
    {
        string pem = File.ReadAllText(keyPath);
        var rsa = RSA.Create();
        if (pem.Contains("ENCRYPTED PRIVATE KEY"))
        {
            var pass = Environment.GetEnvironmentVariable("LICENSEGEN_KEY_PASSPHRASE");
            if (string.IsNullOrEmpty(pass))
                throw new InvalidOperationException(
                    "Key is an ENCRYPTED PRIVATE KEY; set LICENSEGEN_KEY_PASSPHRASE.");
            rsa.ImportFromEncryptedPem(pem, pass);
        }
        else
        {
            rsa.ImportFromPem(pem);
        }
        return rsa;
    }

    static void GenerateLicense(string keyPath)
    {
        using var rsa = LoadOperatorKey(keyPath);

        var data = new LicenseData
        {
            CustomerName = "Lao HR Demo",
            ExpirationDate = DateTime.UtcNow.AddDays(365),
            MaxEmployees = 100,
            HardwareId = "*",
            Type = "ENTERPRISE"
        };

        string json = JsonConvert.SerializeObject(data);
        byte[] dataBytes = Encoding.UTF8.GetBytes(json);
        byte[] signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        string licenseKey = $"{Convert.ToBase64String(dataBytes)}.{Convert.ToBase64String(signature)}";

        Console.WriteLine("\n=== LICENSE KEY ===");
        Console.WriteLine(licenseKey);
        Console.WriteLine("===================");
        File.WriteAllText("license.key", licenseKey);
        Console.WriteLine("Saved to license.key (untracked operator output)");
    }
}
