using FluentAssertions;
using LaoHR.API.Middleware;
using LaoHR.Shared.Data;
using LaoHR.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LaoHR.Tests.Unit.Middleware;

public class LicenseMiddlewareTests
{
    private readonly Mock<RequestDelegate> _next;
    private readonly Mock<ILogger<LicenseMiddleware>> _logger;
    private readonly LaoHRDbContext _dbContext;

    public LicenseMiddlewareTests()
    {
        _next = new Mock<RequestDelegate>();
        _logger = new Mock<ILogger<LicenseMiddleware>>();
        
        var options = new DbContextOptionsBuilder<LaoHRDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new LaoHRDbContext(options);
    }
    
    private DefaultHttpContext CreateContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        
        // Mock Service Provider for Scope
        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider
            .Setup(x => x.GetService(typeof(LaoHRDbContext)))
            .Returns(_dbContext);
            
        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider).Returns(serviceProvider.Object);
        
        var serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory
            .Setup(x => x.CreateScope())
            .Returns(serviceScope.Object);
            
        context.RequestServices = serviceProvider.Object;
        
        // This is tricky: DefaultHttpContext.RequestServices.CreateScope() relies on IServiceScopeFactory being registered.
        // We need to register IServiceScopeFactory in the provider.
        serviceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);
            
        return context;
    }

    [Theory]
    [InlineData("/swagger/index.html")]
    [InlineData("/api/auth/login")]
    [InlineData("/api/license/activate")]
    public async Task InvokeAsync_BypassEndpoints_ShouldCallNext(string path)
    {
        // Arrange
        var middleware = new LicenseMiddleware(_next.Object, _logger.Object);
        var context = CreateContext(path);
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        _next.Verify(x => x(context), Times.Once);
        context.Response.StatusCode.Should().Be(200); // Default
    }

    [Fact]
    public async Task InvokeAsync_ProtectedEndpoint_NoLicense_ShouldReturn402()
    {
        // Arrange
        var middleware = new LicenseMiddleware(_next.Object, _logger.Object);
        var context = CreateContext("/api/employees");
        
        // Act
        await middleware.InvokeAsync(context);
        
        // Assert
        _next.Verify(x => x(context), Times.Never);
        context.Response.StatusCode.Should().Be(402);
    }
    
    // Note: To test "Valid License", we'd need a valid RSA signed key.
    // Since LicenseService uses hardcoded Public Key (or file based), it's hard to mock verify without the private key.
    // However, we tested the "Invalid" path which covers the logic branch.
    // If I want to test success, I'd need to mock LicenseService, but Middleware creates it with `new()`.
    // It's not injected. This is a design flaw in the Middleware (Line 18: new LicenseService()).
    // I cannot mock it easily.
    // So coverage will hit the "Invalid" branch return. The "Success" branch (await next) is covered by the Bypass tests technically?
    // No, Bypass returns early.
    // Success path falls through to `await _next(context)`.
    // So lines 58-59 are not covered unless I have a valid license.
    // I can stick to testing what I can.
}
