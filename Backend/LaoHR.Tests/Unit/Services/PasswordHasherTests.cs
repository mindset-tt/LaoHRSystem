using FluentAssertions;
using LaoHR.API.Services;
using Xunit;

namespace LaoHR.Tests.Unit.Services;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_ProducesV2Format()
    {
        var hash = PasswordHasher.HashPassword("test-password");
        hash.Should().StartWith("v2:");
    }

    [Fact]
    public void HashPassword_UsesRandomSalt_TwoIdenticalPasswordsDiffer()
    {
        var hash1 = PasswordHasher.HashPassword("same-password");
        var hash2 = PasswordHasher.HashPassword("same-password");
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void VerifyPasswordWithMigration_V2CorrectPassword_Succeeds()
    {
        var hash = PasswordHasher.HashPassword("correct-password");
        var (isValid, needsRehash) = PasswordHasher.VerifyPasswordWithMigration("correct-password", hash, 2);
        isValid.Should().BeTrue();
        needsRehash.Should().BeFalse();
    }

    [Fact]
    public void VerifyPasswordWithMigration_V2WrongPassword_Fails()
    {
        var hash = PasswordHasher.HashPassword("correct-password");
        var (isValid, _) = PasswordHasher.VerifyPasswordWithMigration("wrong-password", hash, 2);
        isValid.Should().BeFalse();
    }

    [Fact]
    public void VerifyPasswordWithMigration_LegacyV1CorrectPassword_SucceedsAndNeedsRehash()
    {
        // Legacy SHA-256 hash of "legacy-password"
        var legacyHash = PasswordHasher.HashPassword("legacy-password");
        // Simulate a v1 user: the stored hash is the raw SHA-256 (not v2 format)
        // We compute the legacy SHA-256 directly to simulate old data.
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("legacy-password"));
        var rawLegacyHash = Convert.ToBase64String(bytes);

        var (isValid, needsRehash) = PasswordHasher.VerifyPasswordWithMigration("legacy-password", rawLegacyHash, 1);
        isValid.Should().BeTrue();
        needsRehash.Should().BeTrue();
    }

    [Fact]
    public void VerifyPasswordWithMigration_LegacyV1WrongPassword_Fails()
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes("legacy-password"));
        var rawLegacyHash = Convert.ToBase64String(bytes);

        var (isValid, _) = PasswordHasher.VerifyPasswordWithMigration("wrong-password", rawLegacyHash, 1);
        isValid.Should().BeFalse();
    }
}