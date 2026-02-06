using System.Net;
using FluentAssertions;
using LaoHR.Tests.Helpers;
using Xunit;

namespace LaoHR.Tests.Integration.Controllers;

public class LicenseControllerTests : TestBase
{
    public LicenseControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetStatus_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/license/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
