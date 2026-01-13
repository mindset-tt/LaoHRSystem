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
    public async Task SaveChanges_WithAddedEntity_CreatesAuditLog()
    {
        // Arrange
        var mockHttp = new Mock<IHttpContextAccessor>();
        var contextUser = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "TestUser") }, "mock"));
        
        mockHttp.Setup(x => x.HttpContext).Returns(new DefaultHttpContext { User = contextUser });
        
        var interceptor = new AuditLogInterceptor(mockHttp.Object);
        
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;
            
        using var context = new LaoHRDbContext(options);
        
        // Act
        context.Departments.Add(new Department { DepartmentName = "AuditTest", DepartmentCode = "AT" });
        await context.SaveChangesAsync();
        
        // Assert
        var log = await context.AuditLogs.FirstOrDefaultAsync();
        log.Should().NotBeNull();
        log.Action.Should().Be("ADDED");
        log.UserId.Should().Be("TestUser");
        log.EntityName.Should().Be("Department");
        log.NewValues.Should().Contain("AuditTest");
    }

    [Fact]
    public async Task SaveChanges_WithModifiedEntity_CreatesAuditLog_TrackingChanges()
    {
        // Arrange
        var mockHttp = new Mock<IHttpContextAccessor>();
        mockHttp.Setup(x => x.HttpContext.User.Identity.Name).Returns("EditorUser");
        
        var interceptor = new AuditLogInterceptor(mockHttp.Object);
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
        
        // Assert
        using (var verifyContext = new LaoHRDbContext(options))
        {
            var logs = await verifyContext.AuditLogs.ToListAsync();
            // Should have 1 Insert (from seed) and 1 Update
            logs.Count.Should().Be(2);
            
            var updateLog = logs.Last();
            updateLog.Action.Should().Be("MODIFIED");
            updateLog.EntityName.Should().Be("Department");
            updateLog.OldValues.Should().Contain("Original");
            updateLog.NewValues.Should().Contain("Modified");
        }
    }
}
