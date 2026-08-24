using FluentAssertions;
using LaoHR.API.Data;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using System.Collections.Concurrent;

namespace LaoHR.Tests.Integration.Data;

/// <summary>
/// Phase 4B.2 — audit secret exclusion.
///
/// Proves the audit interceptor never persists sensitive values (passwords,
/// tokens, secrets, full bank account numbers, SWIFT) into audit JSON. The
/// audit still records that the field changed via a [REDACTED] marker.
/// </summary>
public class AuditSecretExclusionTests
{
    private static (ConcurrentQueue<AuditLog> queue, AuditLogInterceptor interceptor) CreateInterceptor(string username)
    {
        var mockHttp = new Mock<IHttpContextAccessor>();
        var contextUser = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, username) }, "mock"));
        mockHttp.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = contextUser });

        var queue = new System.Collections.Concurrent.ConcurrentQueue<AuditLog>();
        var mockChannel = new Mock<IAuditLogChannel>();
        mockChannel.Setup(x => x.TryWrite(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(entry => queue.Enqueue(entry))
            .Returns(true);
        var interceptor = new AuditLogInterceptor(mockHttp.Object, mockChannel.Object);
        return (queue, interceptor);
    }

    [Fact]
    public async Task AppUser_Add_RedactsPasswordHash()
    {
        var (queue, interceptor) = CreateInterceptor("audituser");
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        using var context = new LaoHRDbContext(options);
        context.Users.Add(new AppUser
        {
            Username = "secretuser",
            PasswordHash = "PBKDF2$super-secret-hash-value",
            PasswordHashVersion = 2,
            Role = "Employee",
            IsActive = true,
        });
        await context.SaveChangesAsync();

        var log = queue.Should().ContainSingle().Subject;
        log.EntityName.Should().Be("AppUser");
        log.NewValues.Should().Contain("[REDACTED]");
        log.NewValues.Should().NotContain("super-secret-hash-value");
    }

    [Fact]
    public async Task BankAccount_Add_RedactsAccountNumberAndSwift()
    {
        var (queue, interceptor) = CreateInterceptor("audituser");
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        using var context = new LaoHRDbContext(options);
        context.BankAccounts.Add(new BankAccount
        {
            BankName = "BCEL",
            AccountName = "Operating",
            AccountNumber = "1234567890123456",
            Swift = "COEBLALA",
            Currency = "LAK",
        });
        await context.SaveChangesAsync();

        var log = queue.Should().ContainSingle().Subject;
        log.EntityName.Should().Be("BankAccount");
        log.NewValues.Should().Contain("[REDACTED]");
        log.NewValues.Should().NotContain("1234567890123456");
        log.NewValues.Should().NotContain("COEBLALA");
        // Non-sensitive fields are still recorded.
        log.NewValues.Should().Contain("BCEL");
    }

    [Fact]
    public async Task RefreshToken_Add_RedactsTokenHash()
    {
        var (queue, interceptor) = CreateInterceptor("audituser");
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        using var context = new LaoHRDbContext(options);
        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = 1,
            TokenHash = "abcdef0123456789abcdef0123456789abcdef0123456789abcdef0123456789",
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        });
        await context.SaveChangesAsync();

        var log = queue.Should().ContainSingle().Subject;
        log.EntityName.Should().Be("RefreshToken");
        log.NewValues.Should().Contain("[REDACTED]");
        log.NewValues.Should().NotContain("abcdef0123456789");
    }
}
