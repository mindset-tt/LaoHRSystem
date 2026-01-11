using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using LaoHR.Shared.Models;

namespace LaoHR.LicenseGen;

class Program
{
    static string PrivateKeyPath = "private.key";
    static string PublicKeyPath = "public.key";

    static void Main(string[] args)
    {
        Console.WriteLine("=== LaoHR License Generator ===");
        
        if (!File.Exists(PrivateKeyPath))
        {
            Console.WriteLine("⚠️ No keys found. Generating new RSA Key Pair...");
            GenerateKeys();
        }
        else
        {
            Console.WriteLine("✅ Keys found.");
        }

        while (true)
        {
            Console.WriteLine("\nMenu:");
            Console.WriteLine("1. Generate New License");
            Console.WriteLine("2. Verify License (Test)");
            Console.WriteLine("3. Exit");
            Console.Write("Select: ");
            var choice = Console.ReadLine();

            GenerateLicense();
            return;
        }
    }

    static void GenerateKeys()
    {
        using var rsa = RSA.Create(2048);
        var privateKey = rsa.ExportRSAPrivateKeyPem();
        var publicKey = rsa.ExportSubjectPublicKeyInfoPem();

        File.WriteAllText(PrivateKeyPath, privateKey);
        File.WriteAllText(PublicKeyPath, publicKey);
        
        Console.WriteLine($"✅ Private Key saved to {Path.GetFullPath(PrivateKeyPath)}");
        Console.WriteLine($"✅ Public Key saved to {Path.GetFullPath(PublicKeyPath)}");
        Console.WriteLine("⚠️ IMPORTANT: Copy public.key to your API root folder!");
    }

    static void GenerateLicense()
    {
        string name = "Lao HR Demo";
        int days = 365;
        int maxEmp = 100;
        string hwId = "*";

        var data = new LicenseData
        {
            CustomerName = name,
            ExpirationDate = DateTime.UtcNow.AddDays(days),
            MaxEmployees = maxEmp,
            HardwareId = hwId,
            Type = "ENTERPRISE"
        };

        string json = JsonConvert.SerializeObject(data);
        string licenseKey = SignData(json);

        Console.WriteLine("\n=== LICENSE KEY ===");
        Console.WriteLine(licenseKey);
        Console.WriteLine("===================");
        File.WriteAllText("license.key", licenseKey);
        Console.WriteLine("Saved to license.key");
    }

    static string SignData(string data)
    {
        using var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(PrivateKeyPath));
        
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        string base64Payload = Convert.ToBase64String(dataBytes);
        string base64Sig = Convert.ToBase64String(signature);

        return $"{base64Payload}.{base64Sig}";
    }

    static void VerifyLicense()
    {
        Console.WriteLine("Paste License Key:");
        string key = Console.ReadLine() ?? "";
        
        try
        {
            var parts = key.Split('.');
            if (parts.Length != 2) throw new Exception("Invalid format");

            byte[] payload = Convert.FromBase64String(parts[0]);
            byte[] signature = Convert.FromBase64String(parts[1]);

            using var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(PublicKeyPath));

            if (rsa.VerifyData(payload, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
            {
                string json = Encoding.UTF8.GetString(payload);
                var data = JsonConvert.DeserializeObject<LicenseData>(json);
                Console.WriteLine("✅ Valid Signature!");
                Console.WriteLine($"Customer: {data.CustomerName}");
                Console.WriteLine($"Expires: {data.ExpirationDate}");
                Console.WriteLine($"Max Emp: {data.MaxEmployees}");
            }
            else
            {
                Console.WriteLine("❌ Invalid Signature!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
