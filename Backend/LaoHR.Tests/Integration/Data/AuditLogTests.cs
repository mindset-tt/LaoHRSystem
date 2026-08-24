using FluentAssertions;
using LaoHR.API.Data;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LaoHR.Tests.Integration.Data;

public class AuditLogTests
{
    [Fact]
    public async Task SaveChanges_WithAddedEntity_QueuesAuditLog()
    {
        // Arrange
        var mockHttp = new Mock<IHttpContextAccessor>();
        var contextUser = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "TestUser") }, "mock"));
        
        mockHttp.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = contextUser });
        
        var auditEntries = new System.Collections.Concurrent.ConcurrentQueue<AuditLog>();
        var mockChannel = new Moq.Mock<IAuditLogChannel>();
        mockChannel.Setup(x => x.TryWrite(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(entry => auditEntries.Enqueue(entry))
            .Returns(true);
        var interceptor = new AuditLogInterceptor(mockHttp.Object, mockChannel.Object);
        
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
            
        using var context = new LaoHRDbContext(options);
        
        // Act
        context.Departments.Add(new Department { DepartmentName = "AuditTest", DepartmentCode = "AT" });
        await context.SaveChangesAsync();
        
        // Assert — verify the interceptor queued the audit entry to the channel
        auditEntries.Should().HaveCount(1);
        var log = auditEntries.First();
        log.Action.Should().Be("ADDED");
        log.UserId.Should().Be("TestUser");
        log.EntityName.Should().Be("Department");
        log.NewValues.Should().Contain("AuditTest");
    }

    [Fact]
    public async Task SaveChanges_WithModifiedEntity_QueuesAuditLog_TrackingChanges()
    {
        // Arrange
        var mockHttp = new Mock<IHttpContextAccessor>();
        mockHttp.Setup(x => x.HttpContext.User.Identity.Name).Returns("EditorUser");
        
        var auditEntries = new System.Collections.Concurrent.ConcurrentQueue<AuditLog>();
        var mockChannel2 = new Moq.Mock<IAuditLogChannel>();
        mockChannel2.Setup(x => x.TryWrite(It.IsAny<AuditLog>()))
            .Callback<AuditLog>(entry => auditEntries.Enqueue(entry))
            .Returns(true);
        var interceptor = new AuditLogInterceptor(mockHttp.Object, mockChannel2.Object);
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(dbName)
            .AddInterceptors(interceptor)
            .Options;
            
        // Seed
        using (var seedContext = new LaoHRDbContext(options))
        {
            seedContext.Departments.Add(new Department { DepartmentId = 99, DepartmentName = "Original", DepartmentCode = "ORG" });
            await seedContext.SaveChangesAsync();
        }

        // Act
        using (var context = new LaoHRDbContext(options))
        {
            var dept = await context.Departments.FirstAsync(d => d.DepartmentId == 99);
            dept.DepartmentName = "Modified";
            await context.SaveChangesAsync();
        }
        
        // Assert — verify the interceptor queued audit entries to the channel
        // Should have 1 Insert (from seed) and 1 Update
        auditEntries.Should().HaveCount(2);
        
        var updateLog = auditEntries.Last();
        updateLog.Action.Should().Be("MODIFIED");
        updateLog.EntityName.Should().Be("Department");
        updateLog.OldValues.Should().Contain("Original");
        updateLog.NewValues.Should().Contain("Modified");
    }
}
